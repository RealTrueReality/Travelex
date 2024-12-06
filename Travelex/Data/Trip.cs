using System.ComponentModel.DataAnnotations;

namespace Travelex.Data;

public class Trip {
    public int Id { get; set; }
    [Required] public required string Title { get; set; }

    public string? Description { get; set; } = string.Empty;
    [Required] public required string LocationName { get; set; }

    public Location? Location { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime AddedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }

    public TripCategory? Category { get; set; }


    public TripStatus Status { get; set; }
}