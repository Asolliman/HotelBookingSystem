// IGuestProfileService.cs
using HotelBookingAPI.DTOs.Profile;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IGuestProfileService
    {
        Task<GuestProfileResponseDto> GetProfileAsync(string userId);
        Task<GuestProfileResponseDto> CreateProfileAsync(string userId, CreateGuestProfileDto dto);
        Task<GuestProfileResponseDto> UpdateProfileAsync(string userId, CreateGuestProfileDto dto);
    }
}