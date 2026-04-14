using QAStudio.Application.Auth.DTOs;
using QAStudio.Application.Auth.Interfaces;
using QAStudio.Application.Common.Exceptions;
using QAStudio.Domain.Entities;
using QAStudio.Domain.Interfaces;
using QAStudio.Infrastructure.Configuration;

namespace QAStudio.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtService _jwtService;
    private readonly JwtConfig _jwtConfig;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        JwtService jwtService,
        JwtConfig jwtConfig)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _jwtConfig = jwtConfig;
    }

    public async Task<User> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        if (await _userRepository.ExistsByEmailAsync(dto.Email, ct))
        {
            throw new BadRequestException($"A user with email '{dto.Email}' already exists.");
        }

        var user = new User
        {
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Name = dto.Name,
            Role = Domain.Enums.UserRole.QA
        };

        return await _userRepository.AddAsync(user, ct);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, ct);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return await CreateAuthResponseAsync(user, ct);
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, ct);
        if (storedToken == null || !storedToken.IsActive)
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        // Revoke the old token (rotation)
        var user = await _userRepository.GetByIdAsync(storedToken.UserId, ct)
            ?? throw new NotFoundException("User", storedToken.UserId);

        await _refreshTokenRepository.RevokeAllUserTokensAsync(storedToken.UserId, refreshToken, ct);

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        var newToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = _jwtService.GetRefreshTokenExpiry()
        };
        await _refreshTokenRepository.AddAsync(newToken, ct);

        return new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpirationMinutes)
        };
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User", userId);

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    private async Task<AuthResponseDto> CreateAuthResponseAsync(User user, CancellationToken ct)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var token = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = _jwtService.GetRefreshTokenExpiry()
        };
        await _refreshTokenRepository.AddAsync(token, ct);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpirationMinutes),
            User = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        };
    }
}
