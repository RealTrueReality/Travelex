using System.ComponentModel.DataAnnotations;
using SQLite;
using MaxLength = System.ComponentModel.DataAnnotations.MaxLengthAttribute;

namespace Travelex.Entities;

public class Travel {
    private TravelStatus _status = TravelStatus.Planning;
    [PrimaryKey, AutoIncrement] public int Id { get; set; }

    [Required, MaxLength(30)] public string Title { get; set; } = String.Empty;

    public string? Description { get; set; } = string.Empty;

    [Required, MaxLength(50)] public string LocationName { get; set; } = string.Empty;
    
    public double? Longitude { get; set; }
    
    public double? Latitude { get; set; }

    [Required(ErrorMessage = "请选择开始日期")]
    public DateTime? StartDate { get; set; }=DateTime.Today;

    [Required(ErrorMessage = "请选择结束日期")]
    public DateTime? EndDate { get; set; }=DateTime.Now;

    public DateTime AddedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string? CategoryName { get; set; }

    public TravelStatus Status {
        get => _status; 
        set {
            _status = value;
            StatusForDisplay = value switch {
                TravelStatus.Planning => "规划中",
                TravelStatus.Ongoing => "进行中",
                TravelStatus.Completed => "已完成",
                TravelStatus.Cancelled => "已取消",
                _ => "未知状态"
            };
        }
    }

    [Ignore]
    public string? StatusForDisplay { get; private set; } = "规划中";


    [Ignore] public List<Expense> Expense { get; set; } = [];

    [MaxLength(200)] public string? ImageUrl { get; set; }
}
