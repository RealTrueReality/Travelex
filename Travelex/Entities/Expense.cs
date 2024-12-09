using System.ComponentModel.DataAnnotations;
using SQLite;
using MaxLength = System.ComponentModel.DataAnnotations.MaxLengthAttribute;

namespace Travelex.Entities;

public class Expense {
    [PrimaryKey, AutoIncrement] public long Id { get; set; }
    public int TripId { get; set; }
    [Required, MaxLength(100)] public string? Description { get; set; } = string.Empty;
    [Required,Range(0.01,double.MaxValue)] public double Amount { get; set; }
    [Required] public string? Category { get; set; }
    public DateTime TimeOnSpend { get; set; }
}