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

    public async Task<ResultDataModel<Expense>> DeleteExpenseAsync(long expenseId) {
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
        var itemByKeyAsync = await _db.GetItemByKeyAsync<ExpenseCategory>(category.Name);
        if (itemByKeyAsync is not null) {
            return ResultDataModel<ExpenseCategory>.Failure($"类别{category.Name}已存在");
        }
        try {
            var b = await _db.AddItemAsync(category);
            return b ? ResultDataModel<ExpenseCategory>.Success(category) : ResultDataModel<ExpenseCategory>.Failure("添加失败");
        }
        catch (Exception ex) {
            return ResultDataModel<ExpenseCategory>.Failure(ex.Message);
        }
    }

    public async Task<ResultDataModel<ExpenseCategory>> GetExpenseCategoryAsync(string categoryName)
    {
        try
        {
            var category = await _db.GetItemByKeyAsync<ExpenseCategory>(categoryName);
            return category is not null 
                ? ResultDataModel<ExpenseCategory>.Success(category) 
                : ResultDataModel<ExpenseCategory>.Failure($"未找到类别：{categoryName}");
        }
        catch (Exception ex)
        {
            return ResultDataModel<ExpenseCategory>.Failure(ex.Message);
        }
    }

    public async Task<ResultDataModel<ExpenseCategory>> UpdateExpenseCategoryAsync(string oldName, ExpenseCategory category)
    {
        try
        {
            // 检查新名称是否已存在（如果更改了名称）
            if (oldName != category.Name)
            {
                var existing = await _db.GetItemByKeyAsync<ExpenseCategory>(category.Name);
                if (existing is not null)
                {
                    return ResultDataModel<ExpenseCategory>.Failure($"类别{category.Name}已存在");
                }
            }

            // 获取原有类别
            var oldCategory = await _db.GetItemByKeyAsync<ExpenseCategory>(oldName);
            if (oldCategory is null)
            {
                return ResultDataModel<ExpenseCategory>.Failure($"未找到类别：{oldName}");
            }

            // 如果类别名称改变，需要更新所有使用该类别的支出记录
            if (oldName != category.Name)
            {
                var expenses = await _db.GetTableAsync<Expense>();
                var relatedExpenses = await expenses.Where(e => e.Category == oldName).ToListAsync();
                foreach (var expense in relatedExpenses)
                {
                    expense.Category = category.Name;
                    await _db.UpdateItemAsync(expense);
                }
            }

            // 删除旧类别并添加新类别（因为类别名是主键）
            await _db.DeleteItemAsync(oldCategory);
            var success = await _db.AddItemAsync(category);

            return success 
                ? ResultDataModel<ExpenseCategory>.Success(category) 
                : ResultDataModel<ExpenseCategory>.Failure("更新失败");
        }
        catch (Exception ex)
        {
            return ResultDataModel<ExpenseCategory>.Failure(ex.Message);
        }
    }

    public async Task<ResultDataModel<ExpenseCategory>> DeleteExpenseCategoryAsync(string categoryName)
    {
        try
        {
            var category = await _db.GetItemByKeyAsync<ExpenseCategory>(categoryName);
            if (category is null)
            {
                return ResultDataModel<ExpenseCategory>.Failure($"未找到类别：{categoryName}");
            }

            // 检查是否有使用该类别的支出记录
            var expenses = await _db.GetTableAsync<Expense>();
            var relatedExpenses = await expenses.FirstAsync(e => e.Category == categoryName);
            if (relatedExpenses is not null)
            {
                return ResultDataModel<ExpenseCategory>.Failure("该类别下有支出记录，无法删除");
            }

            var success = await _db.DeleteItemAsync(category);
            return success 
                ? ResultDataModel<ExpenseCategory>.Success(category) 
                : ResultDataModel<ExpenseCategory>.Failure("删除失败");
        }
        catch (Exception ex)
        {
            return ResultDataModel<ExpenseCategory>.Failure(ex.Message);
        }
    }

    public async Task<ResultDataModel<List<Expense>>> GetExpensesByCategoryAsync(string categoryName)
    {
        try
        {
            var expenses = await _db.GetTableAsync<Expense>();
            var filteredList = await expenses.Where(e => e.Category == categoryName)
                .OrderByDescending(e => e.TimeOnSpend)
                .ToListAsync();
            return ResultDataModel<List<Expense>>.Success(filteredList);
        }
        catch (Exception ex)
        {
            return ResultDataModel<List<Expense>>.Failure(ex.Message);
        }
    }

    public async Task<ResultDataModel<Expense>> GetExpenseByIdAsync(long expenseId)
    {
        try
        {
            var expense = await _db.GetItemByKeyAsync<Expense>(expenseId);
            return expense is not null 
                ? ResultDataModel<Expense>.Success(expense) 
                : ResultDataModel<Expense>.Failure($"未找到支出记录：{expenseId}");
        }
        catch (Exception ex)
        {
            return ResultDataModel<Expense>.Failure(ex.Message);
        }
    }
}