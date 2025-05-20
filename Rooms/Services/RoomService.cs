using MODSI_SQLRestAPI.Rooms.Models;
using MODSI_SQLRestAPI.Rooms.Repositories;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Rooms.Services
{
    public class RoomService
    {
        private readonly RoomRepository _repo;
        public RoomService()
        {
            _repo = new RoomRepository();
        }

        public async Task AddRoom(Room room)
        {
            await _repo.AddRoomAsync(room);
        }

        public async Task<Room> GetRoomById(string id)
        {
            return await _repo.GetRoomByIdAsync(id);
        }
    }
}
