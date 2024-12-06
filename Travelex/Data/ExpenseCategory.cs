using System.ComponentModel.DataAnnotations;
using SQLite;
using MaxLength = System.ComponentModel.DataAnnotations.MaxLengthAttribute;

namespace Travelex.Data;

public class ExpenseCategory {
    [PrimaryKey, MaxLength(100)] public string? Name { get; set; }
}