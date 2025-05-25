using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.Infrastructure.Data;
using MODSI_SQLRestAPI.UserAuth.DTO;
using MODSI_SQLRestAPI.UserAuth.Models;
using MODSI_SQLRestAPI.UserAuth.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;


// Point between the controller and the database – essentially where you should place 'if' statements unrelated to authentication.
// In other words, this is where business logic goes, like password length checks, valid characters, etc.
// DTO retriever
namespace MODSI_SQLRestAPI.UserAuth.Services
{
    internal class UserService
    {
        private readonly ILogger _logger;
        private readonly UserRepository _databaseHandler;
        private readonly ApplicationDbContext _dbContext;

        public UserService(ILoggerFactory loggerFactory, ApplicationDbContext dbContext)
        {
            _logger = loggerFactory.CreateLogger<UserService>();
            _databaseHandler = new UserRepository();
            _dbContext = dbContext;
        }

        internal async Task<List<UserDTO>> GetAllUsers()
        {
            var users = await _databaseHandler.GetAllUsersAsync();

            if (users == null || users.Count == 0)
            {
                throw new NotFoundException("Nenhum usuário encontrado.");
            }

            return users;
        }


        internal async Task<bool> UserExistsByUsername(string username)
        {
            var exists = await _databaseHandler.UserExistsByUsernameAsync(username);
            return exists;
        }

        internal async Task<UserDTO> GetUserById(int id)
        {
            var user = await _databaseHandler.GetUserByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException($"Usuário com ID {id} não encontrado.");
            }
            return user;
        }

        internal async Task<UserDTO> CreateUser(User user)
        {

            // Check if the email exists
            var existingUser = await _databaseHandler.EmailUserExistsAsync(user.Email);
            if (existingUser == true)
            {
                throw new BadRequestException($"Usuário com email {user.Email} já existe.");
            }

            // Check if the username exists
            var existingUsername = await _databaseHandler.UsernameUserExistsAsync(user.Username);
            if (existingUsername == true)
            {
                throw new BadRequestException($"Usuário com nome de usuário {user.Username} já existe.");
            }
            await _databaseHandler.AddUserAsync(user);

            // Create DTO User
            var userDTO = new UserDTO(user.Name, user.Email, user.Username, user.Role, user.Group, user.Photo, user.Tel);

            return userDTO;
        }


        internal async Task DeleteUser(int id)
        {
            var user = await _databaseHandler.GetUserByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException($"Usuário com ID {id} não encontrado.");
            }
            await _databaseHandler.DeleteUserByIdAsync(id);

        }

        internal async Task<UserDTO> UpdateUser(int id, User user)
        {
            var existingUser = await _databaseHandler.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                throw new NotFoundException($"Usuário com ID {id} não encontrado.");
            }

            await _databaseHandler.UpdateUserByIdAsync(user);

            return new UserDTO(user.Name, user.Email, user.Username, user.Role, user.Group, user.Photo, user.Tel);
        }



        internal async Task<bool> EmailUserExists(string email)
        {
            var exists = await _databaseHandler.EmailUserExistsAsync(email);
            return exists;

        }

        internal async Task<User> GetUserByIdentifier(string identifier, bool return_salt = false)
        {
            var user = await _databaseHandler.GetUserByIdentifierAsync(identifier, return_salt);
            if (user == null)
            {
                throw new NotFoundException($"Usuário com username/email {identifier} não encontrado.");
            }
            return user;
        }

        internal async Task<UserDTO> ChangeUserRole(int userId, string roleNameOrId)
        {
            // Verifica se o usuário existe
            var user = await _databaseHandler.GetUserByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"Usuário com ID {userId} não encontrado.");

            string roleName = roleNameOrId;

            // Se for um número, trata como ID
            if (int.TryParse(roleNameOrId, out int roleId))
            {
                roleName = await GetRoleNameByIdAsync(roleId);
                if (string.IsNullOrEmpty(roleName))
                    throw new NotFoundException($"Role com ID {roleId} não encontrado.");
            }
            else
            {
                // Confirma se existe a role pelo nome
                using (var conn = new SqlConnection(ApplicationDbContext.ConnectionString))
                {
                    await conn.OpenAsync();
                    var query = "SELECT COUNT(*) FROM Roles WHERE Name = @Name";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", roleNameOrId);
                        var count = (int)await cmd.ExecuteScalarAsync();
                        if (count == 0)
                            throw new NotFoundException($"Role com nome '{roleNameOrId}' não encontrado.");
                    }
                }
            }

            var updated = await _databaseHandler.ChangeUserRoleAsync(userId, roleName);
            if (!updated)
                throw new System.Exception("Falha ao atualizar o papel do usuário.");

            // Retorna o usuário atualizado
            var updatedUser = await _databaseHandler.GetUserByIdAsync(userId);
            return updatedUser;
        }

        internal async Task<UserDTO> ChangeUserGroup(int userId, string groupNameOrId)
        {
            // Verifica se o usuário existe
            var user = await _databaseHandler.GetUserByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"Usuário com ID {userId} não encontrado.");

            string groupName = groupNameOrId;

            // Se for um número, trata como ID
            if (int.TryParse(groupNameOrId, out int groupId))
            {
                groupName = await GetGroupNameByIdAsync(groupId);
                if (string.IsNullOrEmpty(groupName))
                    throw new NotFoundException($"Grupo com ID {groupId} não encontrado.");
            }
            else
            {
                // Confirma se existe o grupo pelo nome
                using (var conn = new SqlConnection(ApplicationDbContext.ConnectionString))
                {
                    await conn.OpenAsync();
                    var query = "SELECT COUNT(*) FROM Groups WHERE Name = @Name";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", groupNameOrId);
                        var count = (int)await cmd.ExecuteScalarAsync();
                        if (count == 0)
                            throw new NotFoundException($"Grupo com nome '{groupNameOrId}' não encontrado.");
                    }
                }
            }

            var updated = await _databaseHandler.ChangeUserGroupAsync(userId, groupName);
            if (!updated)
                throw new System.Exception("Falha ao atualizar o grupo do usuário.");

            // Retorna o usuário atualizado
            var updatedUser = await _databaseHandler.GetUserByIdAsync(userId);
            return updatedUser;
        }

        private async Task<string> GetRoleNameByIdAsync(int roleId)
        {
            using (var conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT Name FROM Roles WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", roleId);
                    var result = await cmd.ExecuteScalarAsync();
                    return result as string;
                }
            }
        }

        private async Task<string> GetGroupNameByIdAsync(int groupId)
        {
            using (var conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT Name FROM Groups WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", groupId);
                    var result = await cmd.ExecuteScalarAsync();
                    return result as string;
                }
            }
        }

        private static readonly Dictionary<string, PasswordResetCodeEntry> PasswordResetCodes = new Dictionary<string, PasswordResetCodeEntry>();

        public Task StorePasswordResetCode(int userId, string code, DateTime expiration)
        {
            lock (PasswordResetCodes)
            {
                PasswordResetCodes[code] = new PasswordResetCodeEntry
                {
                    UserId = userId,
                    Code = code,
                    Expiration = expiration
                };
            }
            return Task.CompletedTask;
        }

        public Task<PasswordResetCodeEntry> GetPasswordResetCodeEntry(string code)
        {
            lock (PasswordResetCodes)
            {
                PasswordResetCodes.TryGetValue(code, out var entry);
                return Task.FromResult(entry);
            }
        }

        public async Task<bool> ChangePassword(string identifier, string currentPasswordHash, string newPasswordHash, string newSalt)
        {
            // Buscar usuário pelo identificador (email ou username)
            var user = await _databaseHandler.GetUserByIdentifierAsync(identifier, true);
            if (user == null)
                return false;

            // Verificar se o hash da senha atual confere
            if (user.Password != currentPasswordHash)
                return false;

            // Atualizar senha e salt
            user.Password = newPasswordHash;
            user.Salt = newSalt;
            await _databaseHandler.UpdateUserByIdAsync(user);

            return true;
        }

    }

}
