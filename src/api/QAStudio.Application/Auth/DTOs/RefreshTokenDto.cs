using System.ComponentModel.DataAnnotations;

namespace QAStudio.Application.Auth.DTOs;

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
