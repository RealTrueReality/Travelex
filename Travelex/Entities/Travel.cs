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

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime AddedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string? CategoryName { get; set; }

    public TravelStatus Status {
        get => _status;
        set {
            StuatusForDisplay = value.ToString();
            _status = value;
        }
    }

    [Ignore]
    public string? StuatusForDisplay { get; set; }

    [MaxLength(200)] public string? ImageUrl { get; set; }
}