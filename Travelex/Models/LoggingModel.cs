using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Travelex.Models;

public class LoggingModel {
    [Required(ErrorMessage = "请输入用户名")]
    [MaxLength(30, ErrorMessage = "用户名不能超过30个字符")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "请输入密码")]
    [MaxLength(20, ErrorMessage = "密码不能超过20个字符")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "密码不能少于6个字符")]
    public string Password { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    //to json format
    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
    
    public static LoggingModel? ParseFromJson(string json) {
        return string.IsNullOrWhiteSpace(json) ? default: JsonSerializer.Deserialize<LoggingModel>(json);
    }
}