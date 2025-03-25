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
                new() { Name = "交通", Icon = "car" },
                new() { Name = "餐饮", Icon = "restaurant" },
                new() { Name = "住宿", Icon = "hotel" },
                new() { Name = "购物", Icon = "store" },
                new() { Name = "运动", Icon = "run" },
                new() { Name = "娱乐", Icon = "joystick" },
                new() { Name = "办公", Icon = "expense" },
            };

            foreach (var category in expenseCategories) await _db.AddItemAsync(category);
        }

        // 检查并初始化 TravelCategory 表数据
        if (!await _db.TableExistsAsync<TravelCategory>()) {
            var tripCategories = new List<TravelCategory> {
                new() { CategoryName = "海滩", },
                new() { CategoryName = "野外" },
                new() { CategoryName = "爬山" },
                new() { CategoryName = "City Walk" },
                new() { CategoryName = "岛屿" },
                new() { CategoryName = "宗教" },
                new() { CategoryName = "出境" },
                new() { CategoryName = "长途旅行" }
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
                    Status = TravelStatus.Completed,
                    CategoryName = "海滩",
                    Description = "itrake 是一个位于美国加州的 beach，它位于美国西雅图州西雅图市附近，是西雅图州最大的 beach，并且是西雅图州唯一一个没有水草的 beach。",
                    AddedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now,
                    Longitude = -122.5,
                    Latitude = 47.5,
                },
                new() {
                    Id = 2,
                    Title = "洛杉矶之旅",
                    LocationName = "洛杉矶",
                    StartDate = new DateTime(2021, 8, 9),
                    EndDate = new DateTime(2021, 8, 13),
                    ImageUrl = "images/travelImages/LosAngeles.png",
                    Status = TravelStatus.Completed,
                    Description = "洛杉矶是美国加州的著名城市，以好莱坞和美丽的海滩闻名，是全球娱乐产业的中心。",
                },
                new() {
                    Id = 3,
                    Title = "凯卢阿之旅",
                    LocationName = "凯卢阿",
                    StartDate = new DateTime(2021, 7, 9),
                    EndDate = new DateTime(2021, 7, 13),
                    ImageUrl = "images/travelImages/KailuaKona.png",
                    Status = TravelStatus.Completed,
                    Description = "凯卢阿位于夏威夷，是一个以冲浪、海滩和温暖气候闻名的度假胜地，适合家庭和朋友共度假期。",
                },
                new() {
                    Id = 4,
                    Title = "小狼的秘密旅行",
                    LocationName = "小狼家",
                    StartDate = new DateTime(2024, 12, 19),
                    EndDate = new DateTime(2024, 12, 24),
                    ImageUrl = "images/travelImages/furnace.jpg",
                    Status = TravelStatus.Planning,
                    Description = "一次特殊的旅行，前往小狼家探访乖小狼天壤，享受温馨的安心时光。",
                },
            };
            foreach (var travel in travels) await _db.AddItemAsync(travel);
        }

        // 检查并初始化 Expense 表数据
        if (!await _db.TableExistsAsync<Expense>()) {
            var expenses = new List<Expense> {
                new Expense {
                    Id = 1,
                    TripId = 1, // 特拉基之旅
                    Title = "住宿费",
                    Description = "入住酒店费用",
                    Amount = 500.00,
                    Category = "住宿",
                    TimeOnSpend = new DateTime(2024, 1, 7)
                },
                new Expense {
                    Id = 2,
                    TripId = 1,
                    Title = "餐饮费",
                    Description = "餐馆午餐",
                    Amount = 150.00,
                    Category = "餐饮",
                    TimeOnSpend = new DateTime(2024, 1, 8)
                },
                new Expense {
                    Id = 3,
                    TripId = 1,
                    Title = "景区门票",
                    Description = "特拉基滑雪场门票",
                    Amount = 300.00,
                    Category = "娱乐",
                    TimeOnSpend = new DateTime(2024, 1, 9)
                },
                new Expense {
                    Id = 4,
                    TripId = 2, // 洛杉矶之旅
                    Title = "机票",
                    Description = "飞往洛杉矶的机票",
                    Amount = 1200.00,
                    Category = "交通",
                    TimeOnSpend = new DateTime(2024, 8, 9)
                },
                new Expense {
                    Id = 5,
                    TripId = 2,
                    Title = "景点门票",
                    Description = "参观好莱坞的门票",
                    Amount = 200.00,
                    Category = "娱乐",
                    TimeOnSpend = new DateTime(2024, 8, 10)
                },
                new Expense {
                    Id = 6,
                    TripId = 2,
                    Title = "晚餐",
                    Description = "洛杉矶海鲜餐厅晚餐",
                    Amount = 180.00,
                    Category = "餐饮",
                    TimeOnSpend = new DateTime(2024, 8, 11)
                },
                new Expense {
                    Id = 7,
                    TripId = 2,
                    Title = "租车费",
                    Description = "洛杉矶市区租车费用",
                    Amount = 350.00,
                    Category = "交通",
                    TimeOnSpend = new DateTime(2024, 8, 9)
                },
                new Expense {
                    Id = 8,
                    TripId = 3, // 凯卢阿之旅
                    Title = "租车费",
                    Description = "租车自驾游",
                    Amount = 400.00,
                    Category = "交通",
                    TimeOnSpend = new DateTime(2024, 7, 9)
                },
                new Expense {
                    Id = 9,
                    TripId = 3,
                    Title = "午餐费",
                    Description = "凯卢阿海滩餐厅午餐",
                    Amount = 120.00,
                    Category = "餐饮",
                    TimeOnSpend = new DateTime(2024, 7, 10)
                },
                new Expense {
                    Id = 10,
                    TripId = 3,
                    Title = "冲浪课程",
                    Description = "参加凯卢阿冲浪培训班",
                    Amount = 250.00,
                    Category = "娱乐",
                    TimeOnSpend = new DateTime(2024, 7, 11)
                },
                new Expense {
                    Id = 11,
                    TripId = 4, // 小狼的秘密旅行
                    Title = "礼物开销",
                    Description = "给朋友的礼物",
                    Amount = 300.00,
                    Category = "购物",
                    TimeOnSpend = new DateTime(2024, 12, 20)
                },
                new Expense {
                    Id = 12,
                    TripId = 4,
                    Title = "零食采购",
                    Description = "购买旅行零食",
                    Amount = 50.00,
                    Category = "餐饮",
                    TimeOnSpend = new DateTime(2024, 12, 19)
                },
                new Expense {
                    Id = 13,
                    TripId = 4,
                    Title = "交通费",
                    Description = "前往小狼家的火车票",
                    Amount = 120.00,
                    Category = "交通",
                    TimeOnSpend = new DateTime(2024, 12, 19)
                },
                new Expense {
                    Id = 14,
                    TripId = 4,
                    Title = "晚餐开销",
                    Description = "家庭晚餐食材",
                    Amount = 180.00,
                    Category = "餐饮",
                    TimeOnSpend = new DateTime(2024, 12, 22)
                },
                new Expense {
                    Id = 15,
                    TripId = 4,
                    Title = "住宿费",
                    Description = "小狼之家住宿费用",
                    Amount = 500.00,
                    Category = "住宿",
                    TimeOnSpend = new DateTime(2024, 12, 23)
                },
                new Expense {
                    Id = 16,
                    TripId = 4,
                    Title = "购物费",
                    Description = "购买旅行用品",
                    Amount = 80.00,
                    Category = "购物",
                    TimeOnSpend = new DateTime(2024, 12, 24)
                },
                new Expense {
                    Id = 17,
                    TripId = 4,
                    Title = "其他开销",
                    Description = "其他旅行费用",
                    Amount = 100.00,
                    Category = "其他",
                    TimeOnSpend = new DateTime(2024, 12, 25)
                },
                new Expense {
                    Id = 18,
                    TripId = 4,
                    Title = "旅行活动",
                    Description = "参加小狼的秘密旅行活动",
                    Amount = 350.00,
                    Category = "娱乐",
                    TimeOnSpend = new DateTime(2024, 12, 26)
                },
                new Expense {
                    Id = 19,
                    TripId = 4,
                    Title = "旅行保险",
                    Description = "购买旅行保险",
                    Amount = 50.00,
                    Category = "保险",
                    TimeOnSpend = new DateTime(2025, 1, 10)
                },
                new Expense {
                    Id = 20,
                    TripId = 4,
                    Title = "旅行活动",
                    Description = "参加小狼的秘密旅行活动",
                    Amount = 350.00,
                    Category = "娱乐",
                    TimeOnSpend = new DateTime(2025, 3, 5)
                },
                new Expense {
                    Id = 21,
                    TripId = 4,
                    Title = "旅行活动",
                    Description = "出去吃饭",
                    Amount = 350.00,
                    Category = "娱乐",
                    TimeOnSpend = new DateTime(2025, 4, 15)
                }
            };
            foreach (var expense in expenses) {
                await _db.AddItemAsync(expense);
            }
        }
    }
}