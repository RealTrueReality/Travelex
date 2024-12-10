using Travelex.Data;
using Travelex.Entities;
using Travelex.Models;

namespace Travelex.Services;

public class AuthService {
    private readonly DatabaseContext _db;
    private readonly string LoggedInKey = "logged";


    public AuthService(DatabaseContext db) {
        _db = db;
    }

    public bool IsUserLoggedIn => Preferences.ContainsKey("logged");


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
            Password = user.Password
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
    
}