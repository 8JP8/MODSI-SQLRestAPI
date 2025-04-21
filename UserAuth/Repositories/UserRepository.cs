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

        internal async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = new List<UserDTO>();
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
                            var role = reader.IsDBNull(5) ? "N.D." : reader.GetString(5);
                            if (role == "N.D.")
                            {
                                Console.WriteLine($"User with ID {reader.GetInt32(0)} has a null Role.");
                            }

                            try
                            {

                                // Obter os Ãndices das colunas pelo nome
                                int idIndex = reader.GetOrdinal("Id");
                                int nameIndex = reader.GetOrdinal("Name");
                                int emailIndex = reader.GetOrdinal("Email");
                                int usernameIndex = reader.GetOrdinal("Username");
                                int roleIndex = reader.GetOrdinal("Role");
                                int createdAtIndex = reader.GetOrdinal("CreatedAt");
                                int isActiveIndex = reader.GetOrdinal("IsActive");
                                int groupIndex = reader.GetOrdinal("Group");

                                users.Add(new UserDTO
                                {
                                    Id = reader.GetInt32(idIndex),
                                    Name = reader.GetString(nameIndex),
                                    Username = reader.GetString(usernameIndex),
                                    Email = reader.GetString(emailIndex),
                                    Role = !reader.IsDBNull(roleIndex) ? reader.GetString(roleIndex) : "n.d.",
                                    CreatedAt = reader.GetDateTime(createdAtIndex),
                                    IsActive = reader.GetBoolean(isActiveIndex),
                                    Group = reader.GetString(groupIndex)
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
                    cmd.Parameters.AddWithValue("@Role", DBNull.Value);
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
                                    Role = !reader.IsDBNull(roleIndex) ? reader.GetString(roleIndex) : "n.d.",
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

                            // Obter os Ãndices das colunas pelo nome
                            int idIndex = reader.GetOrdinal("Id");
                            int nameIndex = reader.GetOrdinal("Name");
                            int emailIndex = reader.GetOrdinal("Email");
                            int usernameIndex = reader.GetOrdinal("Username");
                            int roleIndex = reader.GetOrdinal("Role");
                            int createdAtIndex = reader.GetOrdinal("CreatedAt");
                            int isActiveIndex = reader.GetOrdinal("IsActive");
                            int groupIndex = reader.GetOrdinal("Group");
                            int saltIndex = reader.GetOrdinal("Salt");

                            User returndata = new User
                            {
                                Id = reader.GetInt32(idIndex),
                                Name = reader.GetString(nameIndex),
                                Email = reader.GetString(emailIndex),
                                Username = reader.GetString(usernameIndex),
                                Role = !reader.IsDBNull(roleIndex) ? reader.GetString(roleIndex) : "n.d.",
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

        internal async Task<UserDTO> GetUserByIdAsync(int id)
        {
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
                            // Obter os Ãndices das colunas pelo nome
                            int idIndex = reader.GetOrdinal("Id");
                            int nameIndex = reader.GetOrdinal("Name");
                            int emailIndex = reader.GetOrdinal("Email");
                            int usernameIndex = reader.GetOrdinal("Username");
                            int roleIndex = reader.GetOrdinal("Role");
                            int createdAtIndex = reader.GetOrdinal("CreatedAt");
                            int isActiveIndex = reader.GetOrdinal("IsActive");
                            int groupIndex = reader.GetOrdinal("Group");

                            UserDTO returndata = new UserDTO
                            {
                                Id = reader.GetInt32(idIndex),
                                Name = reader.GetString(nameIndex),
                                Email = reader.GetString(emailIndex),
                                Username = reader.GetString(usernameIndex),
                                Role = !reader.IsDBNull(roleIndex) ? reader.GetString(roleIndex) : "n.d.",
                                CreatedAt = reader.GetDateTime(createdAtIndex),
                                IsActive = reader.GetBoolean(isActiveIndex),
                                Group = reader.GetString(groupIndex)
                            };

                            return returndata;
                        }
                    }
                }
            }
            return null;
        }






        public async Task<User> GetByCredentialsAsync(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Consulta para buscar o usuário pelo nome de usuário ou e-mail
                var query = @"
                SELECT Id, Name, Email, Password, Username, Role, CreatedAt, IsActive, [Group], Salt
                FROM Users
                WHERE Email = @Username";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var user = new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Password = reader.GetString(reader.GetOrdinal("Password")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                Role = reader.GetString(reader.GetOrdinal("Role")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                Group = reader.GetString(reader.GetOrdinal("Group")),
                                Salt = reader.GetString(reader.GetOrdinal("Salt")),
                            };

                            // Verifica a senha usando o utilitário de hash
                            var hashedPassword = PasswordUtils.HashPassword(password, user.Salt);
                            if (user.Password == hashedPassword)
                            {
                                return user;
                            }
                        }
                    }
                }
            }

            return null; // Retorna null se as credenciais forem inválidas
        }


    }
}