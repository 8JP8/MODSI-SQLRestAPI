using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI
{
    public class DatabaseHandler
    {
        private readonly string _connectionString;

        private readonly string _3DPoints_DB;
        private readonly string _pieChart_DB;
        public DatabaseHandler()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _3DPoints_DB = ConfigurationManager.AppSettings["3DPoints_DBName"];
            _pieChart_DB = ConfigurationManager.AppSettings["PieChart_DBName"];
        }

        #region 3D Points Visualization
        public async Task<List<Point3D>> GetAllPointsAsync()
        {
            var points = new List<Point3D>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT Id, x, y, z FROM {_3DPoints_DB}";
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
                var query = $"SELECT Id, x, y, z FROM {_3DPoints_DB} WHERE Id = @id";
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
                    var query = $"INSERT INTO {_3DPoints_DB} (x, y, z) VALUES (@x, @y, @z)";
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
                var query = $"DELETE FROM {_3DPoints_DB} WHERE Id = @id";
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
                var query = $"UPDATE {_3DPoints_DB} SET x = @x, y = @y, z = @z WHERE Id = @id";
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

        #region PieChart Visualization
        public async Task<List<PieChart>> GetAllPieChartsAsync()
        {
            var pieCharts = new List<PieChart>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT Id, name, value FROM {_pieChart_DB}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            pieCharts.Add(new PieChart
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Value = (float)reader.GetDouble(2)
                            });
                        }
                    }
                }
            }

            return pieCharts;
        }

        public async Task<PieChart> GetPieChartByIdAsync(int id)
        {
            PieChart pieChart = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT Id, name, value FROM {_pieChart_DB} WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            pieChart = new PieChart
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Value = (float)reader.GetDouble(2)
                            };
                        }
                    }
                }
            }

            return pieChart;
        }

        public async Task AddPieChartsAsync(List<PieChart> pieCharts)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                foreach (var pieChart in pieCharts)
                {
                    var query = $"INSERT INTO {_pieChart_DB} (name, value) VALUES (@name, @value)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", pieChart.Name);
                        cmd.Parameters.AddWithValue("@value", pieChart.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task DeletePieChartByIdAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"DELETE FROM {_pieChart_DB} WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task ReplacePieChartByIdAsync(PieChart pieChart)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"UPDATE {_pieChart_DB} SET name = @name, value = @value WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", pieChart.Id);
                    cmd.Parameters.AddWithValue("@name", pieChart.Name);
                    cmd.Parameters.AddWithValue("@value", pieChart.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task SetRandomPieChartsAsync()
        {
            var random = new Random();
            var pieCharts = new List<PieChart>
            {
                new PieChart { Name = "Category A", Value = random.Next(0, 100) },
                new PieChart { Name = "Category B", Value = random.Next(0, 100) },
                new PieChart { Name = "Category C", Value = random.Next(0, 100) }
            };

            await AddPieChartsAsync(pieCharts);
        }
        #endregion

        #region User Management

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT ID, Name, Email, Password, Username, Role, CreatedAt, IsActive FROM [user]";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                                Password = reader.GetString(3),
                                Username = reader.GetString(4),
                                Role = reader.GetString(5),
                                CreatedAt = reader.GetDateTime(6),
                                IsActive = reader.GetString(7)
                            });
                        }
                    }
                }
            }
            return users;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            User user = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT ID, Name, Email, Password, Username, Role, CreatedAt, IsActive FROM [user] WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user = new User
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                                Password = reader.GetString(3),
                                Username = reader.GetString(4),
                                Role = reader.GetString(5),
                                CreatedAt = reader.GetDateTime(6),
                                IsActive = reader.GetString(7)
                            };
                        }
                    }
                }
            }
            return user;
        }

        public async Task AddUserAsync(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"INSERT INTO [user] (name, email, password, username, role, createdAt, isActive) 
                      VALUES (@Name, @Email, @Password, @Username, @Role, @CreatedAt, @IsActive)";
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@Role", user.Role);
                command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                command.Parameters.AddWithValue("@IsActive", user.IsActive);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteUserByIdAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"DELETE FROM [user] WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateUserByIdAsync(User user)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"UPDATE [user] SET Name = @name, Email = @email, Password = @password, Username = @username, Role = @role, CreatedAt = @createdAt, IsActive = @isActive WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", user.ID);
                    cmd.Parameters.AddWithValue("@name", user.Name);
                    cmd.Parameters.AddWithValue("@email", user.Email);
                    cmd.Parameters.AddWithValue("@password", user.Password);
                    cmd.Parameters.AddWithValue("@username", user.Username);
                    cmd.Parameters.AddWithValue("@role", user.Role);
                    cmd.Parameters.AddWithValue("@createdAt", user.CreatedAt);
                    cmd.Parameters.AddWithValue("@isActive", user.IsActive);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> EmailUserExistsAsync(string email)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT COUNT(*) FROM [user] WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }
        // Get user by email
        public async Task<User> GetUserByEmailAsync(string email)
        {
            User user = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT ID, Name, Email, Password, Username, Role, CreatedAt, IsActive FROM [user] WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user = new User
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                                Password = reader.GetString(3),
                                Username = reader.GetString(4),
                                Role = reader.GetString(5),
                                CreatedAt = reader.GetDateTime(6),
                                IsActive = reader.GetString(7)
                            };
                        }
                    }
                }
            }
            return user;
        }



        #endregion

    }
}