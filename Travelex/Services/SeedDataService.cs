using Travelex.Data;

namespace Travelex.Services;

public class SeedDataService {
    private readonly DatabaseContext _db;

    public SeedDataService(DatabaseContext db) {
        _db = db;
    }

    public async Task SeedDataAsync() {
        // 检查并初始化 ExpenseCategory 表数据
        if (!await _db.TableItemExistsAsync<ExpenseCategory>()) {
            var expenseCategories = new List<ExpenseCategory> {
                new() { Name = "交通" },
                new() { Name = "美食" },
                new() { Name = "住宿" },
                new() { Name = "购物" },
                new() { Name = "运动" },
                new() { Name = "娱乐" },
                new() { Name = "其他" }
            };

            foreach (var category in expenseCategories) {
                await _db.AddItemAsync(category);
            }
        }

        // 检查并初始化 TripCategory 表数据
        if (!await _db.TableItemExistsAsync<TripCategory>()) {
            var tripCategories = new List<TripCategory> {
                new() { CategoryName = "海滩", CategoryImage = "/images/chair.png" },
                new() { CategoryName = "野外", CategoryImage = "/images/conservation.png" },
                new() { CategoryName = "爬山", CategoryImage = "/images/mountain.png" },
                new() { CategoryName = "City Walk", CategoryImage = "/images/city.png" },
                new() { CategoryName = "岛屿", CategoryImage = "/images/island.png" },
                new() { CategoryName = "宗教", CategoryImage = "/images/buddha.png" },
                new() { CategoryName = "出境", CategoryImage = "/images/abroad.png" },
                new() { CategoryName = "长途旅行", CategoryImage = "/images/roadtrip.png" },
                new() { CategoryName = "其他", CategoryImage = "/images/travels.png" }
            };

            foreach (var category in tripCategories) {
                await _db.AddItemAsync(category);
            }
        }
    }
}