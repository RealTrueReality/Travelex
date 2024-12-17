using System.ComponentModel.DataAnnotations;

namespace Travelex.Models;

public class OnboardingModel
{
    [Required(ErrorMessage = "请输入标题")]
    [MaxLength(50, ErrorMessage = "标题不能超过50个字符")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "请输入描述")]
    [MaxLength(200, ErrorMessage = "描述不能超过200个字符")]
    public required string Description { get; set; }

    [Required(ErrorMessage = "请提供图片路径")]
    public required string ImageSource { get; set; }
}
