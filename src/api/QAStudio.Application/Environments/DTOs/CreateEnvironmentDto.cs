using System.ComponentModel.DataAnnotations;

namespace QAStudio.Application.Environments.DTOs;

public class CreateEnvironmentDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? AuthToken { get; set; }

    public bool IsActive { get; set; } = true;
}
