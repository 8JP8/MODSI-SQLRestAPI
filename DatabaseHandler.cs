using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI
{
    public class DatabaseHandler
    {
        private readonly string _connectionString;

        public DatabaseHandler()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        #region 3D Points Visualization
        public async Task<List<Point3D>> GetAllPointsAsync()
        {
            var points = new List<Point3D>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT Id, x, y, z FROM Pontos3D";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            points.Add(new Point3D
                            {
                                ID = reader.GetInt32(0),
                                X = (double)reader.GetDouble(1),
                                Y = (double)reader.GetDouble(2),
                                Z = (double)reader.GetDouble(3)
                            });
                        }
                    }
                }
            }

            return points;
        }

        public async Task<Point3D> GetPointByIdAsync(int id)
        {
            Point3D point = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT Id, x, y, z FROM Pontos3D WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            point = new Point3D
                            {
                                ID = reader.GetInt32(0),
                                X = (double)reader.GetDouble(1),
                                Y = (double)reader.GetDouble(2),
                                Z = (double)reader.GetDouble(3)
                            };
                        }
                    }
                }
            }

            return point;
        }

        public async Task AddPointsAsync(List<Point3D> points)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                foreach (var point in points)
                {
                    var query = "INSERT INTO Pontos3D (x, y, z) VALUES (@x, @y, @z)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@x", point.X);
                        cmd.Parameters.AddWithValue("@y", point.Y);
                        cmd.Parameters.AddWithValue("@z", point.Z);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task DeletePointByIdAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "DELETE FROM Pontos3D WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task ReplacePointByIdAsync(Point3D point)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "UPDATE Pontos3D SET x = @x, y = @y, z = @z WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", point.ID);
                    cmd.Parameters.AddWithValue("@x", point.X);
                    cmd.Parameters.AddWithValue("@y", point.Y);
                    cmd.Parameters.AddWithValue("@z", point.Z);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        #endregion
    }
}
