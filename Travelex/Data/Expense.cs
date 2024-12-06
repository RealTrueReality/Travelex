using System.ComponentModel.DataAnnotations;

namespace Travelex.Data;

public class Expense {
    public long Id { get; set; }
    public int TripId { get; set; }
    public string? Description { get; set; } = string.Empty;
    [Required]
    public double Amount { get; set; }
    public string? Category { get; set; }
    public DateTime TimeOnSpend { get; set; }
}