using System.ComponentModel.DataAnnotations;
using SQLite;

namespace Travelex.Entities;

public class User {
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; } = string.Empty;
    
}