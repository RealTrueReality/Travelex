using SQLite;
using Travelex.Data;
using Travelex.Entities;
using Travelex.Models;
using Travelex.Utils;

namespace Travelex.Services;

public class ExpenseService {
    private readonly DatabaseContext _db;

    public ExpenseService(DatabaseContext db) {
        _db = db;
    }


    public async Task<ResultDataModel<List<Expense>?>> GetExpensesAsync() {
        var expenses = await _db.GetTableAsync<Expense>();
        var filteredExpenses = await expenses.OrderByDescending(t => t.TimeOnSpend).ToListAsync();
        if (filteredExpenses is null) {
            return ResultDataModel<List<Expense>>.Failure("获取失败");
        }

        return ResultDataModel<List<Expense>>.Success(filteredExpenses);
    }

    public async Task<ResultDataModel<List<Expense>>> GetExpensesByTripIdAsync(int tripId) {
        try {
            var expenses = await _db.GetTableAsync<Expense>();
            var filteredList = await expenses.Where(e => e.TripId == tripId)
                .OrderByDescending(e => e.TimeOnSpend)
                .ToListAsync();
            return ResultDataModel<List<Expense>>.Success(filteredList);
        }
        catch (Exception ex) {
            return ResultDataModel<List<Expense>>.Failure(ex.Message);
        }
    }

    public async Task<ResultDataModel<Expense>> AddExpenseAsync(Expense expense) {
        try {
            var b = await _db.AddItemAsync(expense);
            return b? ResultDataModel<Expense>.Success(expense) : ResultDataModel<Expense>.Failure("添加失败",expense);
        }
        catch (Exception ex) {
            return ResultDataModel<Expense>.Failure(ex.Message);
        }
    }

    //修改支出
    public async Task<ResultDataModel<Expense>> UpdateExpenseAsync(Expense expense) {
        try {
            var b = await _db.UpdateItemAsync(expense);
            return b ? ResultDataModel<Expense>.Success(expense) : ResultDataModel<Expense>.Failure("修改失败");
        }
        catch (Exception ex) {
            return ResultDataModel<Expense>.Failure(ex.Message);
        }
    }

    public async Task<ResultDataModel<Expense>> DeleteExpenseAsync(int expenseId) {
        try {
            var expense = await _db.GetItemByKeyAsync<Expense>(expenseId);
            if (expense is null) {
                return ResultDataModel<Expense>.Failure("不存在该支出，删除失败");
            }

            var b = await _db.DeleteItemAsync(expense);
            return b ? ResultDataModel<Expense>.Success(expense) : ResultDataModel<Expense>.Failure("删除失败");
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return ResultDataModel<Expense>.Failure("删除失败");
        }
    }

    public async Task<ResultDataModel<List<ExpenseCategory>>> GetExpenseCategoriesAsync() {
        var categories = await _db.GetTableAsync<ExpenseCategory>();
        var list = await categories.ToListAsync();
        return list is not null ? ResultDataModel<List<ExpenseCategory>>.Success(list) : ResultDataModel<List<ExpenseCategory>>.Failure("获取类别失败");
    }
    
    public async Task<ResultDataModel<ExpenseCategory>> AddExpenseCategoryAsync(ExpenseCategory category) {
        try {
            var b = await _db.AddItemAsync(category);
            return b ? ResultDataModel<ExpenseCategory>.Success(category) : ResultDataModel<ExpenseCategory>.Failure("添加失败");
        }
        catch (Exception ex) {
            return ResultDataModel<ExpenseCategory>.Failure(ex.Message);
        }
    }
}