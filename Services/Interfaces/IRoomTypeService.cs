// IRoomTypeService.cs
using HotelBookingAPI.DTOs.RoomType;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IRoomTypeService
    {
        Task<IEnumerable<RoomTypeResponseDto>> GetAllRoomTypesAsync();
        Task<RoomTypeResponseDto> GetRoomTypeByIdAsync(int id);
        Task<RoomTypeResponseDto> CreateRoomTypeAsync(CreateRoomTypeDto dto);
        Task<RoomTypeResponseDto> UpdateRoomTypeAsync(int id, UpdateRoomTypeDto dto);
        Task DeleteRoomTypeAsync(int id);
    }
}