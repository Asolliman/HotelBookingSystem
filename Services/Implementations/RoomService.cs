// RoomService.cs
using HotelBookingAPI.Data;
using HotelBookingAPI.DTOs.Room;
using HotelBookingAPI.Models.Entities;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoomResponseDto>> GetAllRoomsAsync()
        {
            return await _context.Rooms
                .AsNoTracking()
                .Include(r => r.RoomType)
                .Select(r => new RoomResponseDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    Floor = r.Floor,
                    PricePerNight = r.PricePerNight,
                    IsAvailable = r.IsAvailable,
                    RoomTypeName = r.RoomType.Name
                })
                .ToListAsync();
        }

        public async Task<RoomResponseDto> GetRoomByIdAsync(int id)
        {
            var room = await _context.Rooms
                .AsNoTracking()
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                throw new Exception("Room not found");
            }

            return new RoomResponseDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                Floor = room.Floor,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable,
                RoomTypeName = room.RoomType.Name
            };
        }

        public async Task<RoomResponseDto> CreateRoomAsync(CreateRoomDto dto)
        {
            var roomType = await _context.RoomTypes.FindAsync(dto.RoomTypeId);
            if (roomType == null)
            {
                throw new Exception("Room type not found");
            }

            var room = new Room
            {
                RoomNumber = dto.RoomNumber,
                Floor = dto.Floor,
                PricePerNight = dto.PricePerNight,
                IsAvailable = dto.IsAvailable,
                RoomTypeId = dto.RoomTypeId
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return new RoomResponseDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                Floor = room.Floor,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable,
                RoomTypeName = roomType.Name
            };
        }

        public async Task<RoomResponseDto> UpdateRoomAsync(int id, UpdateRoomDto dto)
        {
            var room = await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.Id == id);
            if (room == null)
            {
                throw new Exception("Room not found");
            }

            if (dto.RoomNumber != null) room.RoomNumber = dto.RoomNumber;
            if (dto.Floor.HasValue) room.Floor = dto.Floor.Value;
            if (dto.PricePerNight.HasValue) room.PricePerNight = dto.PricePerNight.Value;
            if (dto.IsAvailable.HasValue) room.IsAvailable = dto.IsAvailable.Value;
            if (dto.RoomTypeId.HasValue)
            {
                var roomType = await _context.RoomTypes.FindAsync(dto.RoomTypeId.Value);
                if (roomType == null)
                {
                    throw new Exception("Room type not found");
                }
                room.RoomTypeId = dto.RoomTypeId.Value;
            }

            await _context.SaveChangesAsync();

            return new RoomResponseDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                Floor = room.Floor,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable,
                RoomTypeName = room.RoomType.Name
            };
        }

        public async Task DeleteRoomAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                throw new Exception("Room not found");
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }
}