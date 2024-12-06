using SQLite;
using System.ComponentModel.DataAnnotations;
using MaxLength = System.ComponentModel.DataAnnotations.MaxLengthAttribute;

namespace Travelex.Data;

public class TripCategory {
    [PrimaryKey,MaxLength(100)]
    public string? CategoryName { get; set; }
    public string? CategoryImage { get; set; }
}