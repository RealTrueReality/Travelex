using Travelex.Data;
using Travelex.Entities;
using Travelex.Models;

namespace Travelex.Services;

public class SeedDataService {
    private readonly DatabaseContext _db;

    public SeedDataService(DatabaseContext db) {
        _db = db;
    }

    public async Task SeedDataAsync() {
        // 检查并初始化 ExpenseCategory 表数据
        if (!await _db.TableExistsAsync<ExpenseCategory>()) {
            var expenseCategories = new List<ExpenseCategory> {
                new() { Name = "交通" },
                new() { Name = "美食" },
                new() { Name = "住宿" },
                new() { Name = "购物" },
                new() { Name = "运动" },
                new() { Name = "娱乐" },
                new() { Name = "其他" }
            };

            foreach (var category in expenseCategories) await _db.AddItemAsync(category);
        }

        // 检查并初始化 TravelCategory 表数据
        if (!await _db.TableExistsAsync<TravelCategory>()) {
            var tripCategories = new List<TravelCategory> {
                new() { CategoryName = "海滩", CategoryImage = "/images/chair.png" },
                new() { CategoryName = "野外", CategoryImage = "/images/conservation.png" },
                new() { CategoryName = "爬山", CategoryImage = "/images/mountain.png" },
                new() { CategoryName = "City Walk", CategoryImage = "/images/city.png" },
                new() { CategoryName = "岛屿", CategoryImage = "/images/island.png" },
                new() { CategoryName = "宗教", CategoryImage = "/images/buddha.png" },
                new() { CategoryName = "出境", CategoryImage = "/images/abroad.png" },
                new() { CategoryName = "长途旅行", CategoryImage = "/images/roadtrip.png" }
            };

            foreach (var category in tripCategories) await _db.AddItemAsync(category);
        }

        // 检查并初始化 Travel 表数据
        if (!await _db.TableExistsAsync<Travel>()) {
            var travels = new List<Travel> {
                new() {
                    Id = 1,
                    Title = "特拉基之旅",
                    LocationName = "特拉基",
                    StartDate = new DateTime(2022, 1, 7),
                    EndDate = new DateTime(2022, 1, 10),
                    ImageUrl = "images/travelImages/Truckee.png",
                    Status = TravelStatus.Completed
                },
                new() {
                    Id = 2,
                    Title = "洛杉矶之旅",
                    LocationName = "洛杉矶",
                    StartDate = new DateTime(2021, 8, 9),
                    EndDate = new DateTime(2021, 8, 13),
                    ImageUrl = "images/travelImages/LosAngeles.png",
                    Status = TravelStatus.Completed
                },
                new() {
                    Id = 3,
                    Title = "凯卢阿之旅",
                    LocationName = "凯卢阿",
                    StartDate = new DateTime(2021, 7, 9),
                    EndDate = new DateTime(2021, 7, 13),
                    ImageUrl = "images/travelImages/KailuaKona.png",
                    Status = TravelStatus.Completed
                },
                new() {
                    Id = 4,
                    Title = "小狼的秘密旅行",
                    LocationName = "小狼家",
                    StartDate = new DateTime(2024, 12, 19),
                    EndDate = new DateTime(2024, 12, 24),
                    ImageUrl = "images/travelImages/furnace.jpg",
                    Status = TravelStatus.Planning
                },
                
                
            };
            foreach (var travel in travels) await _db.AddItemAsync(travel);
        }
    }
}