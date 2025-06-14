using MODSI_SQLRestAPI.Infrastructure.Data;
using MODSI_SQLRestAPI.UserAuth.DTO;
using MODSI_SQLRestAPI.UserAuth.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// O reposit�rio � respons�vel por interagir com a base de dados, realizando opera��es CRUD (Create, Read, Update, Delete)
// Geralmete n�o se deve colocar aqui if statems relacionados a autentica��o ou outros processos de neg�cio

namespace MODSI_SQLRestAPI.UserAuth.Repositories
{
    public class UserRepository
    {
        private readonly string _user_DB;

        public UserRepository()
        {
            _user_DB = ConfigurationManager.AppSettings["Users_DBName"];
        }

        private User MapReaderToUser(SqlDataReader reader, bool includePasswordAndSalt = false)
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Role = !reader.IsDBNull(reader.GetOrdinal("Role")) ? reader.GetString(reader.GetOrdinal("Role")) : "n.d.",
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                IsVerified = reader.GetBoolean(reader.GetOrdinal("IsVerified")),
                Group = !reader.IsDBNull(reader.GetOrdinal("Group")) ? reader.GetString(reader.GetOrdinal("Group")) : null,
                Tel = !reader.IsDBNull(reader.GetOrdinal("Tel")) ? reader.GetString(reader.GetOrdinal("Tel")) : null,
                Photo = !reader.IsDBNull(reader.GetOrdinal("Photo")) ? (string)reader.GetValue(reader.GetOrdinal("Photo")) : null,
                Password = includePasswordAndSalt ? reader.GetString(reader.GetOrdinal("Password")) : null,
                Salt = includePasswordAndSalt ? reader.GetString(reader.GetOrdinal("Salt")) : null
            };
        }

        private UserDTO MapReaderToUserDTO(SqlDataReader reader)
        {
            return new UserDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Role = !reader.IsDBNull(reader.GetOrdinal("Role")) ? reader.GetString(reader.GetOrdinal("Role")) : "n.d.",
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                IsVerified = reader.GetBoolean(reader.GetOrdinal("IsVerified")),
                Group = !reader.IsDBNull(reader.GetOrdinal("Group")) ? reader.GetString(reader.GetOrdinal("Group")) : null,
                Tel = !reader.IsDBNull(reader.GetOrdinal("Tel")) ? reader.GetString(reader.GetOrdinal("Tel")) : null,
                Photo = !reader.IsDBNull(reader.GetOrdinal("Photo")) ? (string)reader.GetValue(reader.GetOrdinal("Photo")) : null
            };
        }


        internal async Task<bool> UserExistsByUsernameAsync(string username)
        {
            using (SqlConnection conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT COUNT(*) FROM {_user_DB} WHERE Username = @Username";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    var count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        internal async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = new List<UserDTO>();
            using (SqlConnection conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsVerified, [Group], Photo, Tel FROM {_user_DB}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                users.Add(MapReaderToUserDTO(reader));
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
            using (var connection = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await connection.OpenAsync();
                var query = $"INSERT INTO {_user_DB} (Name, Email, Password, Username, Role, CreatedAt, IsVerified, [Group], Salt, Tel, Photo) " +
                            "VALUES (@Name, @Email, @Password, @Username, @Role, @CreatedAt, @IsVerified, @Group, @Salt, @Tel, @Photo)";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@Role", DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                    cmd.Parameters.AddWithValue("@IsVerified", user.IsVerified);
                    cmd.Parameters.AddWithValue("@Group", user.Group);
                    cmd.Parameters.AddWithValue("@Salt", user.Salt);
                    cmd.Parameters.AddWithValue("@Tel", (object)user.Tel ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Photo", (object)user.Photo ?? DBNull.Value);

                    try { await cmd.ExecuteNonQueryAsync(); } catch (Exception ex) { throw new Exception(ex.Message); }
                }
            }
        }

        internal async Task<User> AuthenticateUserAsync(string username_or_email, string password)
        {
            using (SqlConnection conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                string identifier = username_or_email.Contains("@") ? "Email" : "Username";

                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsVerified, [Group], Salt, Tel, Photo FROM {_user_DB} WHERE {identifier} = @{identifier}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue($"@{identifier}", username_or_email);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var storedHash = reader.GetString(reader.GetOrdinal("Password"));
                            var salt = reader.GetString(reader.GetOrdinal("Salt"));

                            if (storedHash == PasswordUtils.HashPassword(password, salt) || storedHash == password)
                            {
                                return MapReaderToUser(reader, includePasswordAndSalt: true);
                            }
                        }
                    }
                }
            }
            return null;
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

        internal async Task UpdateUserByIdAsync(User user)
        {
            using (SqlConnection conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = $"UPDATE {_user_DB} SET Name = @Name, Email = @Email, Password = @Password, Username = @Username, Role = @Role, CreatedAt = @CreatedAt, IsVerified = @IsVerified, [Group] = @Group, Salt = @Salt, Tel= @Tel , Photo=@Photo WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", user.Id);
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@Role", user.Role);
                    cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                    cmd.Parameters.AddWithValue("@IsVerified", user.IsVerified);
                    cmd.Parameters.AddWithValue("@Group", user.Group);
                    cmd.Parameters.AddWithValue("@Salt", user.Salt);
                    cmd.Parameters.AddWithValue("@Tel", (object)user.Tel ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Photo", (object)user.Photo ?? DBNull.Value);
                    try { await cmd.ExecuteNonQueryAsync(); } catch (Exception ex) { throw new Exception(ex.Message); }
                }
            }
        }

        internal async Task<User> GetUserByIdentifierAsync(string identifier, bool return_salt = false)
        {
            using (SqlConnection conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                string column = identifier.Contains("@") ? "Email" : "Username";

                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsVerified, [Group], Salt, Tel, Photo FROM {_user_DB} WHERE {column} = @{column}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue($"@{column}", identifier);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {

                            User returndata = MapReaderToUser(reader, includePasswordAndSalt: true);

                            if (return_salt) returndata.Salt = reader.GetString(reader.GetOrdinal("Salt")) ?? "";

                            return returndata;
                        }
                    }
                }
            }
            return null;
        }

        internal async Task<bool> EmailUserExistsAsync(string email)
        {
            using (SqlConnection conn = new SqlConnection(ApplicationDbContext.ConnectionString))
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

        internal async Task<bool> UsernameUserExistsAsync(string username)
        {
            using (SqlConnection conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT COUNT(*) FROM {_user_DB} WHERE Username = @Username";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    var count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        internal async Task DeleteUserByIdAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = $"DELETE FROM {_user_DB} WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    try { await cmd.ExecuteNonQueryAsync(); } catch (Exception ex) { throw new Exception(ex.Message); }
                }
            }
        }

        internal async Task<object> GetUserByIdAsync(int id, bool asDto = false)
        {
            using (var conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT * FROM {_user_DB} WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (asDto)
                                return MapReaderToUserDTO(reader);
                            else
                                return MapReaderToUser(reader, true);
                        }
                    }
                }
            }
            return null;
        }

        internal async Task<User> GetUserByIdEntityAsync(int id)
        {
            using (var conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT * FROM Users WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapReaderToUser(reader, true); // true para incluir senha e salt
                        }
                    }
                }
            }
            return null;
        }

        internal async Task<bool> ChangeUserRoleAsync(int userId, string role)
        {
            using (var conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = $"UPDATE {_user_DB} SET Role = @Role WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Role", role);
                    cmd.Parameters.AddWithValue("@Id", userId);
                    var rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }

        internal async Task<bool> ChangeUserGroupAsync(int userId, string group)
        {
            using (var conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = $"UPDATE {_user_DB} SET [Group] = @Group WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Group", group);
                    cmd.Parameters.AddWithValue("@Id", userId);
                    var rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }
    }
}