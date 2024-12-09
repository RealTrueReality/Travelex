using System.ComponentModel.DataAnnotations;
using SQLite;
using MaxLength = System.ComponentModel.DataAnnotations.MaxLengthAttribute;

namespace Travelex.Entities;

public class ExpenseCategory {
    [PrimaryKey, MaxLength(100)] public string? Name { get; set; }
}