using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlatformWellSync.Models;

[Table("Platform")]
public class Platform
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string? PlatformName { get; set; }

    public ICollection<Well> Wells { get; set; } = new List<Well>();
}
