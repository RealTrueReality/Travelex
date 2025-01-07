using Travelex.Data;
using Travelex.Entities;
using Travelex.Models;

namespace Travelex.Services;

public class AuthService {
    private readonly DatabaseContext _db;
    private const string LoggedInKey = "logged";
    

    public AuthService(DatabaseContext db) {
        _db = db;
    }

    public bool IsUserLoggedIn => Preferences.ContainsKey(LoggedInKey);
    public LoggingModel? CurrentUser => LoggingModel.ParseFromJson(Preferences.Get(LoggedInKey,string.Empty));
    

    public async Task<ResultModel> LogInAsync(LoggingModel loggingModel) {
        var usersFiltered = await _db.GetFileteredAsync<User>(u =>
            u.UserName == loggingModel.UserName && u.Password == loggingModel.Password);
        var user = usersFiltered.FirstOrDefault();
        if (user is not null) {
            SetUserAsLoggedIn(user);
            return ResultModel.Success();
        }

        return ResultModel.Failure("用户名或密码不正确");
    }

    private void SetUserAsLoggedIn(User user) {
        var loggingModel = new LoggingModel() {
            UserName = user.UserName,
            Password = user.Password,
            Name = user.Name
        };
        Preferences.Set(LoggedInKey, loggingModel.ToJson());
    }


    public void LogdOut() => Preferences.Remove(LoggedInKey);

    public async Task<ResultModel> SignUpAsync(SignUpModel signUpModel) {
        
        var user = new User() {
            Name = signUpModel.Name,
            UserName = signUpModel.UserName,
            Password = signUpModel.Password
        };

        var result = await _db.AddItemAsync(user);
        if (result) {
            SetUserAsLoggedIn(user);
            return ResultModel.Success();
        }

        return ResultModel.Failure("注册失败");
    }
    
    public async Task ChangeNameAsync(string newName) {
        var users = await _db.GetFileteredAsync<User>(u => u.UserName == CurrentUser!.UserName);
        var user = users.FirstOrDefault();
        if (user != null) {
            user.Name = newName;
            await _db.UpdateItemAsync(user);
            SetUserAsLoggedIn(user);
        }
    }
    
    public async Task<ResultModel> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        try
        {
            var users = await _db.GetFileteredAsync<User>(u => u.UserName == CurrentUser!.UserName);
            var user = users.FirstOrDefault();
            if (user == null)
            {
                return ResultModel.Failure("用户不存在");
            }

            // 验证当前密码
            if (user.Password != currentPassword)
            {
                return ResultModel.Failure("当前密码错误");
            }

            // 更新密码
            user.Password = newPassword;
            var success = await _db.UpdateItemAsync(user);
            if (success)
            {
                SetUserAsLoggedIn(user);
                return ResultModel.Success();
            }

            return ResultModel.Failure("修改密码失败");
        }
        catch (Exception ex)
        {
            return ResultModel.Failure($"修改密码失败：{ex.Message}");
        }
    }
    
    public async Task<ResultModel> UpdateProfileImageAsync(string imageUrl)
    {
        try
        {
            var users = await _db.GetFileteredAsync<User>(u => u.UserName == CurrentUser!.UserName);
            var user = users.FirstOrDefault();
            if (user != null)
            {
                user.ProfileImageUrl = imageUrl;
                var success = await _db.UpdateItemAsync(user);
                if (success)
                {
                    SetUserAsLoggedIn(user);
                    return ResultModel.Success();
                }
            }
            return ResultModel.Failure("更新头像失败");
        }
        catch (Exception ex)
        {
            return ResultModel.Failure($"更新头像失败：{ex.Message}");
        }
    }
    
    public async Task<string?> GetProfileImageAsync()
    {
        try
        {
            var users = await _db.GetFileteredAsync<User>(u => u.UserName == CurrentUser!.UserName);
            var user = users.FirstOrDefault();
            return user?.ProfileImageUrl;
        }
        catch
        {
            return null;
        }
    }
}