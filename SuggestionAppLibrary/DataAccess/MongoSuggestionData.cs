using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver.Linq;

namespace SuggestionAppLibrary.DataAccess;
public class MongoSuggestionData : ISuggestionData
// this is a primary collection for our data
{
    private readonly IDbConnection _db;
    private readonly IUserData _userData;
    private readonly IMemoryCache _cache;
    private readonly IMongoCollection<SuggestionModel> _suggestions;
    private const string CacheName = "SuggestionData";

    public MongoSuggestionData(IDbConnection db, IUserData userData, IMemoryCache cache)
    {
        _db = db;
        _userData = userData;
        _cache = cache;
        _suggestions = db.SuggestionCollection; //never forget to define injections
    }

    public async Task<List<SuggestionModel>> GetAllSuggestions()
    {
        var output = _cache.Get<List<SuggestionModel>>(CacheName);

        if (output is null || output.Count == 0)
        {
            var results = await _suggestions.FindAsync(s => !s.Archived );

            output = results.ToList();

            _cache.Set(CacheName, output, TimeSpan.FromMinutes(1)); // absolutely not days!! Because we want our suggestion model always
                                                                    // updated because it is most important collection which is in use
                                                                    // for all other collections
        }

        return output;
    }

    public async Task<List<SuggestionModel>> GetUsersSuggestions(string userId)  //this added later and if you add new method dont forget to inject it with "ctrl + ." command.
    {
        var output = _cache.Get<List<SuggestionModel>>(userId);

        if (output is null || output.Count == 0)
        {
            var results = await _suggestions.FindAsync(s => s.Author.Id == userId);
            output = results.ToList();

            _cache.Set(userId, output, TimeSpan.FromMinutes(1));
        }


        return output;
    }

    public async Task<List<SuggestionModel>> GetAllApprovedSuggestions()  // we use Task<List<SuggestionModel>> because we want all suggestions
    {
        var output = await GetAllSuggestions();


        return output.Where(x => x.ApprovedForRelease).ToList();
    }

    public async Task<SuggestionModel> GetSuggestion(string id) // we use Task<SuggestionModel> because we want only one suggestion
    {
        var results = await _suggestions.FindAsync(s => s.Id == id);

        return results.FirstOrDefault();
    }

    public async Task<List<SuggestionModel>> GetAllSuggestionsWaitingForApproval()
    {
        var output = await GetAllSuggestions();

        return output.Where(x => x.ApprovedForRelease == false &&
                                                x.Rejected == false).ToList(); // it brings the suggestions which are
                                                                               // not approved yet and not rejected as well.
    }

    public async Task UpdateSuggestions(SuggestionModel suggestion)
    {
        await _suggestions.ReplaceOneAsync(s => s.Id == suggestion.Id, suggestion);
        _cache.Remove(CacheName);
    }

    public async Task UpvoteSuggetion(string suggestionId, string userId)
    {
        var client = _db.Client;

        using var session = await client.StartSessionAsync();

        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var suggestionInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
            var suggestion = (await suggestionInTransaction.FindAsync(s => s.Id == suggestionId)).First();

            bool isUpvote = suggestion.UserVotes.Add(userId);

            if (isUpvote == false)   // what does this? it at previous line checks whether user upvoted before if its true it means it is first
                                     // time but if its false which means user have already voted because hashset gets only one data it doesnt
                                     // duplicate data. so now he wants to remove his vote.
            {
                suggestion.UserVotes.Remove(userId);
            }

            await suggestionInTransaction.ReplaceOneAsync(session,s => s.Id == suggestionId, suggestion); // here updates suggestion with upvoted version
                                                                                                  // or downvoted version

            var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
            var user = await _userData.GetUser(userId);

            if (isUpvote) // if it is true it will add a voted suggestion model
            {
                user.VotedOnSuggestions.Add(new BasicSuggestionModel(suggestion));
            }
            else // if it is false it will remove previously created basic suggestion model
            {
                var suggestionToRemove = user.VotedOnSuggestions.Where(s => s.Id == suggestionId).First();
                user.VotedOnSuggestions.Remove(suggestionToRemove);
            }

            await usersInTransaction.ReplaceOneAsync(session,u => u.Id == userId, user);

            await session.CommitTransactionAsync();

            _cache.Remove(CacheName); // after all this we need to remove the cache to get refreshed version of database 

        }
        catch (Exception ex) //normally this ex variable is used to log to see what is the problem
        {
            await session.AbortTransactionAsync(); // with this we catch if there upvote is not succesful

            throw;
        }
    }

    public async Task CreateSuggestion(SuggestionModel suggestion) // again we create new data in suggestions which is important part of database so
                                                                   // we connect to database and start transaction
    {
        var client = _db.Client;

        using var session = await client.StartSessionAsync();

        session.StartTransaction();

        try
        {
            var db = client.GetDatabase(_db.DbName);
            var suggestionInTransaction = db.GetCollection<SuggestionModel>(_db.SuggestionCollectionName);
            await suggestionInTransaction.InsertOneAsync(suggestion); // here we insert new suggestion to main suggestion model

            var usersInTransaction = db.GetCollection<UserModel>(_db.UserCollectionName);
            var user = await _userData.GetUser(suggestion.Author.Id);
            user.AuthoredSuggestions.Add(new BasicSuggestionModel(suggestion));  // But here we are adding new suggestion only to basicsuggestion which
                                                                                 // belongs to this user
            await usersInTransaction.ReplaceOneAsync(u => u.Id == user.Id, user);

            await session.CommitTransactionAsync(); // we dont remove the cache because it doesnt effect the cache because user
                                                    // wont see it until admin approves the suggestions

        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync(); //normally this ex variable is used to log to see what is the problem
            Console.WriteLine(ex);
            throw;
        }
    }
}
