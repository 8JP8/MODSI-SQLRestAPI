using MODSI_SQLRestAPI.Infrastructure.Data;
using MODSI_SQLRestAPI.Rooms.Models;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Rooms.Repositories
{
    public class RoomRepository
    {
        public async Task AddRoomAsync(Room room)
        {
            using (var conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("INSERT INTO Rooms (Id, JsonData) VALUES (@Id, @JsonData)", conn);
                cmd.Parameters.AddWithValue("@Id", room.Id);
                cmd.Parameters.AddWithValue("@JsonData", room.JsonData);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<Room> GetRoomByIdAsync(string id)
        {
            using (var conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT Id, JsonData FROM Rooms WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Room
                        {
                            Id = reader.GetString(0),
                            JsonData = reader.GetString(1)
                        };
                    }
                }
            }
            return null;
        }
    }
}
