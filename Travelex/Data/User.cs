using System.ComponentModel.DataAnnotations;
using SQLite;

namespace Travelex.Data;

public class User {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Required] public required string Name { get; set; }
    [Required] public required string UserName { get; set; }
    public string? Email { get; set; } = string.Empty;
    [Required] public required string Password { get; set; }
    public string? ProfileImageUrl { get; set; }=string.Empty;
}