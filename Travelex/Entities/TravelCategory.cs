using System.ComponentModel.DataAnnotations;
using SQLite;
using MaxLength = System.ComponentModel.DataAnnotations.MaxLengthAttribute;

namespace Travelex.Entities;

public class TravelCategory {
    [PrimaryKey,MaxLength(100)]
    public string? CategoryName { get; set; }
}