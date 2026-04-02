// IRoomService.cs
using HotelBookingAPI.DTOs.Room;

namespace HotelBookingAPI.Services.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomResponseDto>> GetAllRoomsAsync();
        Task<RoomResponseDto> GetRoomByIdAsync(int id);
        Task<RoomResponseDto> CreateRoomAsync(CreateRoomDto dto);
        Task<RoomResponseDto> UpdateRoomAsync(int id, UpdateRoomDto dto);
        Task DeleteRoomAsync(int id);
    }
}