using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI
{
    public class DatabaseHandler
    {
        private readonly string _connectionString;

        private readonly string _3DPoints_DB;
        private readonly string _pieChart_DB;

        private readonly string _user_DB;
        public DatabaseHandler()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _3DPoints_DB = ConfigurationManager.AppSettings["3DPoints_DBName"];
            _pieChart_DB = ConfigurationManager.AppSettings["PieChart_DBName"];
            _user_DB = ConfigurationManager.AppSettings["Users_DBName"];
        }

        #region 3D Points Visualization
        internal async Task<List<Point3D>> GetAllPointsAsync()
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
                                Id = reader.GetInt32(0),
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

        internal async Task<Point3D> GetPointByIdAsync(int id)
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
                                Id = reader.GetInt32(0),
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

        internal async Task AddPointsAsync(List<Point3D> points)
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

        internal async Task DeletePointByIdAsync(int id)
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

        internal async Task ReplacePointByIdAsync(Point3D point)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"UPDATE {_3DPoints_DB} SET x = @x, y = @y, z = @z WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", point.Id);
                    cmd.Parameters.AddWithValue("@x", point.X);
                    cmd.Parameters.AddWithValue("@y", point.Y);
                    cmd.Parameters.AddWithValue("@z", point.Z);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        #endregion

        #region PieChart Visualization
        internal async Task<List<PieChart>> GetAllPieChartsAsync()
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

        internal async Task<PieChart> GetPieChartByIdAsync(int id)
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

        internal async Task AddPieChartsAsync(List<PieChart> pieCharts)
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

        internal async Task DeletePieChartByIdAsync(int id)
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

        internal async Task ReplacePieChartByIdAsync(PieChart pieChart)
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

        internal async Task SetRandomPieChartsAsync()
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
        internal async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group] FROM {_user_DB}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                var role = reader.IsDBNull(5) ? "N.D." : reader.GetString(5);
                                if (role == "N.D.")
                                {
                                    Console.WriteLine($"User with ID {reader.GetInt32(0)} has a null Role.");
                                }

                                users.Add(new User
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Email = reader.GetString(2),
                                    Password = reader.GetString(3),
                                    Username = reader.GetString(4),
                                    Role = role,
                                    CreatedAt = reader.GetDateTime(6),
                                    IsActive = reader.GetBoolean(7),
                                    Group = reader.GetString(8)
                                });
                            }
                            catch { }
                        }
                    }
                }
            }
            return users;
        }

        internal async Task AddUserAsync(User user)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"INSERT INTO {_user_DB} (Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group], Salt) " +
                            "VALUES (@Name, @Email, @Password, @Username, @Role, @CreatedAt, @IsActive, @Group, @Salt)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@Role", user.Role);
                    cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                    cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                    cmd.Parameters.AddWithValue("@Group", user.Group);
                    cmd.Parameters.AddWithValue("@Salt", user.Salt);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        internal async Task<User> AuthenticateUserAsync(string username_or_email, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string identifier = username_or_email.Contains("@") ? "Email" : "Username";

                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group], Salt FROM {_user_DB} WHERE {identifier} = @{identifier}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue($"@{identifier}", username_or_email);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Obter os índices das colunas pelo nome
                                int idIndex = reader.GetOrdinal("Id");
                                int nameIndex = reader.GetOrdinal("Name");
                                int emailIndex = reader.GetOrdinal("Email");
                                int passwordIndex = reader.GetOrdinal("Password");
                                int usernameIndex = reader.GetOrdinal("Username");
                                int roleIndex = reader.GetOrdinal("Role");
                                int createdAtIndex = reader.GetOrdinal("CreatedAt");
                                int isActiveIndex = reader.GetOrdinal("IsActive");
                                int groupIndex = reader.GetOrdinal("Group");
                                int saltIndex = reader.GetOrdinal("Salt");

                                // Recuperar os valores das colunas
                                var storedHash = reader.GetString(passwordIndex);
                                var salt = reader.GetString(saltIndex);

                                if (storedHash == PasswordUtils.HashPassword(password, salt) || storedHash == password)
                                {
                                    return new User
                                    {
                                        Id = reader.GetInt32(idIndex),
                                        Name = reader.GetString(nameIndex),
                                        Email = reader.GetString(emailIndex),
                                        Username = reader.GetString(usernameIndex),
                                        Role = reader.GetString(roleIndex),
                                        CreatedAt = reader.GetDateTime(createdAtIndex),
                                        IsActive = reader.GetBoolean(isActiveIndex),
                                        Group = reader.GetString(groupIndex)
                                    };
                                }
                            }
                        }
                }
            }
            return null;
        }

        internal async Task UpdateUserByIdAsync(User user)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"UPDATE {_user_DB} SET Name = @Name, Email = @Email, Password = @Password, Username = @Username, Role = @Role, CreatedAt = @CreatedAt, IsActive = @IsActive, [Group] = @Group WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", user.Id);
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@Role", user.Role);
                    cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                    cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                    cmd.Parameters.AddWithValue("@Group", user.Group);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        internal async Task<User> GetUserByIdentifierAsync(string identifier, bool return_salt = false)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string column = identifier.Contains("@") ? "Email" : "Username";

                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group], Salt FROM {_user_DB} WHERE {column} = @{column}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue($"@{column}", identifier);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {

                            // Obter os índices das colunas pelo nome
                            int idIndex = reader.GetOrdinal("Id");
                            int nameIndex = reader.GetOrdinal("Name");
                            int emailIndex = reader.GetOrdinal("Email");
                            int passwordIndex = reader.GetOrdinal("Password");
                            int usernameIndex = reader.GetOrdinal("Username");
                            int roleIndex = reader.GetOrdinal("Role");
                            int createdAtIndex = reader.GetOrdinal("CreatedAt");
                            int isActiveIndex = reader.GetOrdinal("IsActive");
                            int groupIndex = reader.GetOrdinal("Group");
                            int saltIndex = reader.GetOrdinal("Salt");

                            // Recuperar os valores das colunas
                            var storedHash = reader.GetString(passwordIndex);
                            var salt = reader.GetString(saltIndex);

                            User returndata = new User
                            {
                                Id = reader.GetInt32(idIndex),
                                Name = reader.GetString(nameIndex),
                                Email = reader.GetString(emailIndex),
                                Username = reader.GetString(usernameIndex),
                                Role = reader.GetString(roleIndex),
                                CreatedAt = reader.GetDateTime(createdAtIndex),
                                IsActive = reader.GetBoolean(isActiveIndex),
                                Group = reader.GetString(groupIndex)
                            };
                            if (return_salt) returndata.Salt = reader.GetString(saltIndex);

                            return returndata;
                        }
                    }
                }
            }
            return null;
        }

        internal async Task<bool> EmailUserExistsAsync(string email)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT COUNT(*) FROM {_user_DB} WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    var count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        internal async Task DeleteUserByIdAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"DELETE FROM {_user_DB} WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        internal async Task<User> GetUserByIdAsync(int id)
        {
            User user = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group] FROM {_user_DB} WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            user = new User
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                                Password = reader.GetString(3),
                                Username = reader.GetString(4),
                                Role = reader.GetString(5),
                                CreatedAt = reader.GetDateTime(6),
                                IsActive = reader.GetBoolean(7),
                                Group = reader.GetString(8)
                            };
                        }
                    }
                }
            }
            return user;
        }

        internal static class PasswordUtils
        {
            internal static string HashPassword(string password, string salt)
            {
                using (var sha256 = SHA256.Create())
                {
                    var combined = Encoding.UTF8.GetBytes(password + salt);
                    var hash = sha256.ComputeHash(combined);
                    return Convert.ToBase64String(hash);
                }
            }

            internal static string GenerateSalt()
            {
                var saltBytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }
                return Convert.ToBase64String(saltBytes);
            }
        }
        #endregion

    }
}