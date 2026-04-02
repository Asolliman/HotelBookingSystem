// GuestProfileService.cs
using HotelBookingAPI.Data;
using HotelBookingAPI.DTOs.Profile;
using HotelBookingAPI.Models.Entities;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services.Implementations
{
    public class GuestProfileService : IGuestProfileService
    {
        private readonly AppDbContext _context;

        public GuestProfileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GuestProfileResponseDto> GetProfileAsync(string userId)
        {
            var profile = await _context.GuestProfiles
                .Include(gp => gp.User)
                .FirstOrDefaultAsync(gp => gp.UserId == userId);

            if (profile == null)
            {
                throw new Exception("Profile not found");
            }

            return new GuestProfileResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                NationalId = profile.NationalId,
                DateOfBirth = profile.DateOfBirth,
                FullName = profile.User.FullName,
                Email = profile.User.Email
            };
        }

        public async Task<GuestProfileResponseDto> CreateProfileAsync(string userId, CreateGuestProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var existingProfile = await _context.GuestProfiles.FirstOrDefaultAsync(gp => gp.UserId == userId);
            if (existingProfile != null)
            {
                throw new Exception("Profile already exists");
            }

            var profile = new GuestProfile
            {
                UserId = userId,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                NationalId = dto.NationalId,
                DateOfBirth = dto.DateOfBirth
            };

            _context.GuestProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return new GuestProfileResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                NationalId = profile.NationalId,
                DateOfBirth = profile.DateOfBirth,
                FullName = user.FullName,
                Email = user.Email
            };
        }

        public async Task<GuestProfileResponseDto> UpdateProfileAsync(string userId, CreateGuestProfileDto dto)
        {
            var profile = await _context.GuestProfiles
                .Include(gp => gp.User)
                .FirstOrDefaultAsync(gp => gp.UserId == userId);

            if (profile == null)
            {
                throw new Exception("Profile not found");
            }

            profile.PhoneNumber = dto.PhoneNumber;
            profile.Address = dto.Address;
            profile.NationalId = dto.NationalId;
            profile.DateOfBirth = dto.DateOfBirth;

            await _context.SaveChangesAsync();

            return new GuestProfileResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                NationalId = profile.NationalId,
                DateOfBirth = profile.DateOfBirth,
                FullName = profile.User.FullName,
                Email = profile.User.Email
            };
        }
    }
}