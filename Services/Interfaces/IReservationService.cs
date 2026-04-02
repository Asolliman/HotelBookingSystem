// IReservationService.cs
using HotelBookingAPI.DTOs.Reservation;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationResponseDto>> GetAllReservationsAsync();
        Task<ReservationResponseDto> GetReservationByIdAsync(int id);
        Task<IEnumerable<ReservationResponseDto>> GetMyReservationsAsync(string userId);
        Task<ReservationResponseDto> CreateReservationAsync(string userId, CreateReservationDto dto);
        Task CancelReservationAsync(int id, string userId);
        Task ConfirmReservationAsync(int id);
        Task CancelUnpaidReservationsAsync();
    }
}