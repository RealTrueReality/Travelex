﻿using Travelex.Data;
using Travelex.Entities;
using Travelex.Models;

namespace Travelex.Services;

public class TravelService {
    private readonly DatabaseContext _db;

    public TravelService(DatabaseContext db) {
        _db = db;
    }

    //获取所有旅行
    public async Task<ResultDataModel<List<Travel>>> GetTravelsAsync(bool includeExpenses = false) {
        var travels = await _db.GetTableAsync<Travel>();
        var filteredTravels = await travels.OrderByDescending(t => t.AddedOn).ToListAsync();
        if (filteredTravels is null) {
            return ResultDataModel<List<Travel>>.Failure("获取失败");
        }

        if (includeExpenses) {
            var expenses = await _db.GetTableAsync<Expense>();
            foreach (var travel in filteredTravels) {
                var filteredExpenses = await expenses.Where(e => e.TripId == travel.Id).ToListAsync();
                travel.Expense = filteredExpenses;
            }
        }

        return ResultDataModel<List<Travel>>.Success(data: filteredTravels);
    }

    //根据id获取旅行
    public async Task<ResultDataModel<Travel>> GetTravelByIdAsync(int id, bool includeExpenses = false) {
        var travel = await _db.GetItemByKeyAsync<Travel>(id);
        if (travel is null) {
            return ResultDataModel<Travel>.Failure("获取行程失败");
        }

        if (includeExpenses) {
            var expenses = await _db.GetTableAsync<Expense>();
            var filteredExpenses = await expenses.Where(e => e.TripId == travel.Id).ToListAsync();
            travel.Expense = filteredExpenses;
        }

        return ResultDataModel<Travel>.Success(travel);
    }

    //添加旅行
    public async Task<ResultDataModel<Travel>> AddTravelAsync(Travel travel) {
        try {
            var b1 = await _db.AddItemAsync(travel);
            //把travel的类别存到数据库
            var itemByKeyAsync = await _db.GetItemByKeyAsync<TravelCategory>(travel.CategoryName ?? string.Empty);
            if (itemByKeyAsync is null) {
                var b2 = await _db.AddItemAsync(new TravelCategory {
                    CategoryName = travel.CategoryName,
                });
                return b1 && b2
                    ? ResultDataModel<Travel>.Success(travel)
                    : ResultDataModel<Travel>.Failure("添加失败", travel);
            }

            return b1 ? ResultDataModel<Travel>.Success(travel) : ResultDataModel<Travel>.Failure("添加失败", travel);
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return ResultDataModel<Travel>.Failure("添加失败", travel);
        }
    }

    //修改旅行
    public async Task<ResultDataModel<Travel>> UpdateTravelAsync(Travel travel) {
        try {
            var b1 = await _db.UpdateItemAsync(travel);
            var b2 = await _db.DeleteItemByKeyAsync<TravelCategory>(travel.CategoryName ?? string.Empty);
            var b3 = await _db.AddItemAsync(new TravelCategory {
                CategoryName = travel.CategoryName,
            });

            return b1 && b2 && b3
                ? ResultDataModel<Travel>.Success(travel)
                : ResultDataModel<Travel>.Failure("更新失败", travel);
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return ResultDataModel<Travel>.Failure("更新失败", travel);
        }
    }

    //删除旅行
    public async Task<ResultDataModel<Travel>> DeleteTravelAsync(int id, bool includeExpenses = false) {
        try {
            var travel = await _db.GetItemByKeyAsync<Travel>(id);
            if (travel is null) {
                return ResultDataModel<Travel>.Failure("不存在该旅行，删除失败");
            }

            var b = await _db.DeleteItemAsync(travel);
            if (includeExpenses) {
                var expenses = await _db.GetTableAsync<Expense>();
                var filteredExpenses = await expenses.Where(e => e.TripId == travel.Id).ToListAsync();
                foreach (var expense in filteredExpenses) {
                    await _db.DeleteItemAsync(expense);
                }
            }
            if (b) {
                await _db.DeleteItemByKeyAsync<TravelCategory>(travel.CategoryName ?? string.Empty);
            }
            return b ? ResultDataModel<Travel>.Success(travel) : ResultDataModel<Travel>.Failure("删除失败");
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return ResultDataModel<Travel>.Failure("删除失败");
        }
    }

    public async Task<ResultDataModel<List<TravelCategory>>> GetTravelCategoriesAsync() {
        var travelCategoriesTable = await _db.GetTableAsync<TravelCategory>();
        var travelCategories = await travelCategoriesTable.ToListAsync();
        return travelCategories is not null
            ? ResultDataModel<List<TravelCategory>>.Success(travelCategories)
            : ResultDataModel<List<TravelCategory>>.Failure("获取行程类别失败");
    }
}