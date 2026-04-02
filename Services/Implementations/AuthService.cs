// AuthService.cs
using HotelBookingAPI.Data;
using HotelBookingAPI.DTOs.Auth;
using HotelBookingAPI.Helpers;
using HotelBookingAPI.Models.Entities;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context, JwtHelper jwtHelper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _jwtHelper = jwtHelper;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                throw new Exception("Registration failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(user, "Guest");

            var tokens = await _jwtHelper.GenerateTokensAsync(user);
            return new AuthResponseDto
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                ExpiresAt = tokens.ExpiresAt,
                FullName = user.FullName,
                Email = user.Email,
                Role = "Guest"
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new Exception("Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
            {
                throw new Exception("Invalid credentials");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Guest";

            var tokens = await _jwtHelper.GenerateTokensAsync(user);
            return new AuthResponseDto
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                ExpiresAt = tokens.ExpiresAt,
                FullName = user.FullName,
                Email = user.Email,
                Role = role
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

            if (token == null)
            {
                throw new Exception("Invalid refresh token");
            }

            // Revoke old token
            token.IsRevoked = true;
            await _context.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(token.User);
            var role = roles.FirstOrDefault() ?? "Guest";

            var newTokens = await _jwtHelper.GenerateTokensAsync(token.User);
            return new AuthResponseDto
            {
                AccessToken = newTokens.AccessToken,
                RefreshToken = newTokens.RefreshToken,
                ExpiresAt = newTokens.ExpiresAt,
                FullName = token.User.FullName,
                Email = token.User.Email,
                Role = role
            };
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (token != null)
            {
                token.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}