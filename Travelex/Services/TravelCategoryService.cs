using Travelex.Data;
using Travelex.Entities;

namespace Travelex.Services;

public class TravelCategoryService {
    private readonly DatabaseContext _db;
    public TravelCategoryService(DatabaseContext db) {
        _db = db;
    }
    //获取所有的分类
    public async Task<List<TravelCategory>> GetCategoriesAsync() {
        var travelCategories = await _db.GetAllAsync<TravelCategory>();
        return travelCategories.ToList();
    }
}