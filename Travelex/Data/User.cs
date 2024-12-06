using System.ComponentModel.DataAnnotations;

namespace Travelex.Data;

public class User {
    public int Id { get; set; }
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string UserName { get; set; }
    public string? Email { get; set; } = string.Empty;
    [Required]
    public required string Password { get; set; }
    public Image? ProfileImage { get; set; }
}