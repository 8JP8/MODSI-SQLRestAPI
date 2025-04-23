using MODSI_SQLRestAPI.UserAuth.DTO;
using MODSI_SQLRestAPI.UserAuth.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// O repositório é responsável por interagir com a base de dados, realizando operações CRUD (Create, Read, Update, Delete)
// Geralmete não se deve colocar aqui if statems relacionados a autenticação ou outros processos de negócio

namespace MODSI_SQLRestAPI.UserAuth.Repositories
{
    public class UserRepository
    {

        private readonly string _connectionString;
        private readonly string _user_DB;

        public UserRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
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
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                Group = reader.GetString(reader.GetOrdinal("Group")),
                Tel = !reader.IsDBNull(reader.GetOrdinal("Tel")) ? reader.GetString(reader.GetOrdinal("Tel")) : null,
                Photo = !reader.IsDBNull(reader.GetOrdinal("Photo")) ? (byte[])reader.GetValue(reader.GetOrdinal("Photo")) : null,
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
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                Group = reader.GetString(reader.GetOrdinal("Group")),
                Photo = !reader.IsDBNull(reader.GetOrdinal("Photo")) ? (byte[])reader.GetValue(reader.GetOrdinal("Photo")) : null
            };
        }

        internal async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = new List<UserDTO>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group], Photo FROM {_user_DB}";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                users.Add( MapReaderToUserDTO(reader));
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
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = $"INSERT INTO {_user_DB} (Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group], Salt, Tel, Photo) " +
                            "VALUES (@Name, @Email, @Password, @Username, @Role, @CreatedAt, @IsActive, @Group, @Salt, @Tel, @Photo)";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@Role", DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                    cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string identifier = username_or_email.Contains("@") ? "Email" : "Username";

                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group], Salt, Tel, Photo FROM {_user_DB} WHERE {identifier} = @{identifier}";
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"UPDATE {_user_DB} SET Name = @Name, Email = @Email, Password = @Password, Username = @Username, Role = @Role, CreatedAt = @CreatedAt, IsActive = @IsActive, [Group] = @Group, Salt = @Salt WHERE Id = @id";
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
                    cmd.Parameters.AddWithValue("@Salt", user.Salt);
                    try { await cmd.ExecuteNonQueryAsync(); } catch (Exception ex) { throw new Exception(ex.Message); }
                }
            }
        }

        internal async Task<User> GetUserByIdentifierAsync(string identifier, bool return_salt = false)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string column = identifier.Contains("@") ? "Email" : "Username";

                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group], Salt, Tel, Photo FROM {_user_DB} WHERE {column} = @{column}";
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
        // Username exists
        internal async Task<bool> UsernameUserExistsAsync(string username)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
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
        // U

        internal async Task DeleteUserByIdAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
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

        internal async Task<UserDTO> GetUserByIdAsync(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = $"SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group], Photo FROM {_user_DB} WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapReaderToUserDTO(reader);
                        }
                    }
                }
            }
            return null;
        }

    }
}