using Travelex.Data;
using Travelex.Entities;
using Travelex.Models;

namespace Travelex.Services;

public class TravelService {
    private readonly DatabaseContext _db;

    public TravelService(DatabaseContext db) {
        _db = db;
    }
    //获取所有旅行
    public async Task<ResultDataModel<List<Travel>?>> GetTripsAsync(int page=1,int count=10) {
        var travels = await _db.GetTableAsync<Travel>();
        var filteredTravels = await travels.OrderByDescending(t=> t.AddedOn)
            .Skip((page - 1) * count)
            .Take(count).ToListAsync();
        return filteredTravels is not null? ResultDataModel<List<Travel>?>.Success(data:filteredTravels) : ResultDataModel<List<Travel>?>.Failure("获取失败");
        
    }
    //根据id获取旅行
    public async Task<ResultDataModel<Travel>> GetTravelByIdAsync(int id) {
        var travel = await _db.GetItemByKeyAsync<Travel>(id);
        return travel is not null ? ResultDataModel<Travel>.Success(travel) : ResultDataModel<Travel>.Failure("获取失败");
    }
    
    //添加旅行
    public async Task<ResultDataModel<Travel>> AddTravelAsync(Travel travel) {
        try {
            var b = await _db.AddItemAsync(travel);
            return b ? ResultDataModel<Travel>.Success(travel) : ResultDataModel<Travel>.Failure("添加失败",travel);
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return ResultDataModel<Travel>.Failure("添加失败",travel);
        }
    }
    //修改旅行
    public async Task<ResultDataModel<Travel>> UpdateTravelAsync(Travel travel) {
        try {
            var b = await _db.UpdateItemAsync(travel);
            return b ? ResultDataModel<Travel>.Success(travel) : ResultDataModel<Travel>.Failure("更新失败",travel);
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return ResultDataModel<Travel>.Failure("更新失败",travel);
        }
    }
    //删除旅行
    public async Task<ResultDataModel<Travel>> DeleteTravelAsync(int id) {
        try {
            var travel = await _db.GetItemByKeyAsync<Travel>(id);
            if (travel is null) {
                return ResultDataModel<Travel>.Failure("不存在该旅行，删除失败");
            }
            var b = await _db.DeleteItemAsync(travel);
            return b ? ResultDataModel<Travel>.Success(travel) : ResultDataModel<Travel>.Failure("删除失败");
        }
        catch (Exception e) {
            Console.WriteLine(e);
            return ResultDataModel<Travel>.Failure("删除失败");
        }
       
    }
}