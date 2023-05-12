namespace SuggestionAppLibrary.DataAccess;
public class MongoUserData : IUserData
{
    private readonly IMongoCollection<UserModel> _users;  // we are doing this to ask dbconnection for users

    public MongoUserData(IDbConnection _db)
    {
        _users = _db.UserCollection;   // with this we dont make a copy of actual object we just make a refrence to the object so
                                       // therefor we dont use any extra memory by making this copy here

    }

    public async Task<List<UserModel>> GetUsersAsync() // Brings all users
    {
        var results = await _users.FindAsync(_ => true);
        return results.ToList();
    }

    public async Task<UserModel> GetUser(string id) //Brings user with provided id
    {
        var results = await _users.FindAsync(u => u.Id == id);  // make sure it is await
        return results.FirstOrDefault();
    }

    public async Task<UserModel> GetUserFromAuthentication(string objectid) //this matches provided object id with ObjectIdentifier from azure B2C
    {
        var results = await _users.FindAsync(u => u.ObjectIdentifier == objectid);
        return results.FirstOrDefault();
    }

    public Task CreateUser(UserModel user) //Creates user in database
    {
        return _users.InsertOneAsync(user);
    }

    public Task UpdateUser(UserModel user)  //Updates the user with provided id
    {
        var filter = Builders<UserModel>.Filter.Eq("Id", user.Id);
        return _users.ReplaceOneAsync(filter, user, new ReplaceOptions { IsUpsert = true });
    }
}
