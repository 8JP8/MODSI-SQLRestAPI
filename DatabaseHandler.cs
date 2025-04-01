using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI
{
    public class DatabaseHandler
    {
        private readonly string _connectionString;

        public DatabaseHandler()
        {
            _connectionString = "Server=tcp:modsi-project.database.windows.net,1433;Initial Catalog=modsi-project;Persist Security Info=False;User ID=MODSIPROJECT;Password=#modsi2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

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
                                X = reader.GetInt32(1),
                                Y = reader.GetInt32(2),
                                Z = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }

            return points;
        }

        public async Task AddPointsAsync(List<Point3D> points)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                foreach (var point in points)
                {
                    var query = "INSERT INTO Pontos3D (Id, x, y, z) VALUES (@Id, @x, @y, @z)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", point.ID);
                        cmd.Parameters.AddWithValue("@x", point.X);
                        cmd.Parameters.AddWithValue("@y", point.Y);
                        cmd.Parameters.AddWithValue("@z", point.Z);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}