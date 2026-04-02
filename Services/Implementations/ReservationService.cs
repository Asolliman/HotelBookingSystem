// ReservationService.cs
using HotelBookingAPI.Data;
using HotelBookingAPI.DTOs.Reservation;
using HotelBookingAPI.Models.Entities;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services.Implementations
{
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;

        public ReservationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetAllReservationsAsync()
        {
            return await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Room)
                .ThenInclude(rm => rm.RoomType)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    RoomNumber = r.Room.RoomNumber,
                    RoomType = r.Room.RoomType.Name,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    Status = r.Status,
                    IsPaid = r.IsPaid,
                    TotalPrice = r.TotalPrice,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ReservationResponseDto> GetReservationByIdAsync(int id)
        {
            var reservation = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Room)
                .ThenInclude(rm => rm.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                throw new Exception("Reservation not found");
            }

            return new ReservationResponseDto
            {
                Id = reservation.Id,
                RoomNumber = reservation.Room.RoomNumber,
                RoomType = reservation.Room.RoomType.Name,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                Status = reservation.Status,
                IsPaid = reservation.IsPaid,
                TotalPrice = reservation.TotalPrice,
                CreatedAt = reservation.CreatedAt
            };
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetMyReservationsAsync(string userId)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .Include(r => r.Room)
                .ThenInclude(rm => rm.RoomType)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    RoomNumber = r.Room.RoomNumber,
                    RoomType = r.Room.RoomType.Name,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    Status = r.Status,
                    IsPaid = r.IsPaid,
                    TotalPrice = r.TotalPrice,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ReservationResponseDto> CreateReservationAsync(string userId, CreateReservationDto dto)
        {
            var room = await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.Id == dto.RoomId);
            if (room == null)
            {
                throw new Exception("Room not found");
            }

            if (!room.IsAvailable)
            {
                throw new Exception("Room is not available");
            }

            // Check for overlapping reservations
            var overlapping = await _context.Reservations
                .AnyAsync(r => r.RoomId == dto.RoomId && r.Status != ReservationStatus.Cancelled &&
                               ((dto.CheckInDate >= r.CheckInDate && dto.CheckInDate < r.CheckOutDate) ||
                                (dto.CheckOutDate > r.CheckInDate && dto.CheckOutDate <= r.CheckOutDate) ||
                                (dto.CheckInDate <= r.CheckInDate && dto.CheckOutDate >= r.CheckOutDate)));

            if (overlapping)
            {
                throw new Exception("Room is already reserved for the selected dates");
            }

            var nights = (dto.CheckOutDate - dto.CheckInDate).Days;
            var totalPrice = room.PricePerNight * nights;

            var reservation = new Reservation
            {
                UserId = userId,
                RoomId = dto.RoomId,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                TotalPrice = totalPrice,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return new ReservationResponseDto
            {
                Id = reservation.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType.Name,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                Status = reservation.Status,
                IsPaid = reservation.IsPaid,
                TotalPrice = reservation.TotalPrice,
                CreatedAt = reservation.CreatedAt
            };
        }

        public async Task CancelReservationAsync(int id, string userId)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (reservation == null)
            {
                throw new Exception("Reservation not found or access denied");
            }

            if (reservation.Status == ReservationStatus.Completed)
            {
                throw new Exception("Cannot cancel completed reservation");
            }

            reservation.Status = ReservationStatus.Cancelled;
            await _context.SaveChangesAsync();
        }

        public async Task ConfirmReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                throw new Exception("Reservation not found");
            }

            reservation.Status = ReservationStatus.Confirmed;
            await _context.SaveChangesAsync();
        }

        public async Task CancelUnpaidReservationsAsync()
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);
            var unpaidReservations = await _context.Reservations
                .Where(r => !r.IsPaid && r.Status == ReservationStatus.Pending && r.CreatedAt < cutoff)
                .ToListAsync();

            foreach (var reservation in unpaidReservations)
            {
                reservation.Status = ReservationStatus.Cancelled;
            }

            await _context.SaveChangesAsync();
        }
    }
}