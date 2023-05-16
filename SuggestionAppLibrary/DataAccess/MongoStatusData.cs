using Microsoft.Extensions.Caching.Memory;

namespace SuggestionAppLibrary.DataAccess;
public class MongoStatusData : IStatusData
{
    private readonly IMongoCollection<StatusModel> _statuses;
    private readonly IMemoryCache _cache;
    private const string CacheName = "StatusData";

    public MongoStatusData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _statuses = db.StatusCollection;
    }

    public async Task<List<StatusModel>> GetAllStatuses()
    {
        List<StatusModel> output = _cache.Get<List<StatusModel>>(CacheName);  // goes to database and finds the statusmodel matches with cacheName

        if (output == null || output.Count == 0)
        {
            var results = await _statuses.FindAsync(_ => true);
            output = results.ToList();

            _cache.Set(CacheName, output, TimeSpan.FromDays(1)); // Everyday gets new lists of categories and holds the cache 1 day in the memory
        }

        return output;
    }

    public Task CreateStatuses(StatusModel status) // it will use this method only at initialization of our database or testing and it wont use it again
    {
        return _statuses.InsertOneAsync(status);
    }
}
