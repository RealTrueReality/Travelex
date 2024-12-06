using System.ComponentModel.DataAnnotations;
using SQLite;
using MaxLength = System.ComponentModel.DataAnnotations.MaxLengthAttribute;

namespace Travelex.Data;

public class Trip {
    [PrimaryKey, AutoIncrement] 
    public int Id { get; set; }
    
    [Required, MaxLength(30)] 
    public required string Title { get; set; }

    public string? Description { get; set; } = string.Empty;
    
    [Required, MaxLength(50)] 
    public required string LocationName { get; set; }

    public Location? Location { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public DateTime AddedOn { get; set; }
    
    public DateTime? ModifiedOn { get; set; }

    public TripCategory? Category { get; set; }

    public TripStatus Status { get; set; }=TripStatus.Planning;
}