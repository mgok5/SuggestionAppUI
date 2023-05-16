using Microsoft.Extensions.Caching.Memory;

namespace SuggestionAppLibrary.DataAccess;
public class MongoCategoryData : ICategoryData
{
    private readonly IMongoCollection<CategoryModel> _categories;
    private readonly IMemoryCache _cache;
    private const string CacheName = "CategoryData"; //it uses this name to check from database if there's category data brings them

    public MongoCategoryData(IDbConnection db, IMemoryCache cache)
    {
        _cache = cache;
        _categories = db.CategoryCollection;
    }

    public async Task<List<CategoryModel>> GetAllCategories()
    {
        var output = _cache.Get<List<CategoryModel>>(CacheName); // goes to categories and finds the categorie matches with cacheName

        if (output == null || output.Count == 0)
        {
            var results = await _categories.FindAsync(_ => true);
            output = results.ToList();

            _cache.Set(CacheName, output, TimeSpan.FromDays(1)); // Everyday gets new lists of categories and holds the cache 1 day in the memory
        }

        return output;
    }

    public Task CreateCategory(CategoryModel category) //Creates category
    {
        return _categories.InsertOneAsync(category);
    }
}
