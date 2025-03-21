using System.ComponentModel.DataAnnotations;
using SQLite;
using MaxLength = System.ComponentModel.DataAnnotations.MaxLengthAttribute;

namespace Travelex.Entities;

public class Expense {
    [PrimaryKey, AutoIncrement] public long Id { get; set; }
    public int TripId { get; set; }
    [Required(ErrorMessage = "请输入标题")] public string? Title { get; set; }= string.Empty;
    [MaxLength(100,ErrorMessage = "请输入不超过100个字符的描述  ")] public string? Description { get; set; } = string.Empty;
    [Required(ErrorMessage = "请输入金额"),Range(0.01,double.MaxValue,ErrorMessage = "请输入正确的金额")] public double? Amount { get; set; }
    [Required(ErrorMessage = "请选择消费类别")] public string? Category { get; set; }
    public DateTime TimeOnSpend { get; set; }=DateTime.Now;
}