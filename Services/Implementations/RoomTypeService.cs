// RoomTypeService.cs
using HotelBookingAPI.Data;
using HotelBookingAPI.DTOs.RoomType;
using HotelBookingAPI.Models.Entities;
using HotelBookingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingAPI.Services.Implementations
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly AppDbContext _context;

        public RoomTypeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoomTypeResponseDto>> GetAllRoomTypesAsync()
        {
            return await _context.RoomTypes
                .AsNoTracking()
                .Select(rt => new RoomTypeResponseDto
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    Description = rt.Description,
                    BasePrice = rt.BasePrice
                })
                .ToListAsync();
        }

        public async Task<RoomTypeResponseDto> GetRoomTypeByIdAsync(int id)
        {
            var roomType = await _context.RoomTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Id == id);

            if (roomType == null)
            {
                throw new Exception("Room type not found");
            }

            return new RoomTypeResponseDto
            {
                Id = roomType.Id,
                Name = roomType.Name,
                Description = roomType.Description,
                BasePrice = roomType.BasePrice
            };
        }

        public async Task<RoomTypeResponseDto> CreateRoomTypeAsync(CreateRoomTypeDto dto)
        {
            var roomType = new RoomType
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice
            };

            _context.RoomTypes.Add(roomType);
            await _context.SaveChangesAsync();

            return new RoomTypeResponseDto
            {
                Id = roomType.Id,
                Name = roomType.Name,
                Description = roomType.Description,
                BasePrice = roomType.BasePrice
            };
        }

        public async Task<RoomTypeResponseDto> UpdateRoomTypeAsync(int id, UpdateRoomTypeDto dto)
        {
            var roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == id);
            if (roomType == null)
            {
                throw new Exception("Room type not found");
            }

            if (dto.Name != null) roomType.Name = dto.Name;
            if (dto.Description != null) roomType.Description = dto.Description;
            if (dto.BasePrice.HasValue) roomType.BasePrice = dto.BasePrice.Value;

            await _context.SaveChangesAsync();

            return new RoomTypeResponseDto
            {
                Id = roomType.Id,
                Name = roomType.Name,
                Description = roomType.Description,
                BasePrice = roomType.BasePrice
            };
        }

        public async Task DeleteRoomTypeAsync(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null)
            {
                throw new Exception("Room type not found");
            }

            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();
        }
    }
}