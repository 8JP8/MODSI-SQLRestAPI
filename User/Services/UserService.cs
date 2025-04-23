using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.UserAuth.DTO;
using MODSI_SQLRestAPI.UserAuth.Models;
using MODSI_SQLRestAPI.UserAuth.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;


// Point between the controller and the database – essentially where you should place 'if' statements unrelated to authentication.
// In other words, this is where business logic goes, like password length checks, valid characters, etc.
// DTO retriever
namespace MODSI_SQLRestAPI.UserAuth.Services
{
    class UserService
    {
        private readonly ILogger _logger;
        private readonly UserRepository _databaseHandler;
        public UserService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UserService>();
            _databaseHandler = new UserRepository();
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
            
            // Cehck if the username exists
            var existingUsername = await _databaseHandler.UsernameUserExistsAsync(user.Username);
            if (existingUsername == true)
            {
                throw new BadRequestException($"Usuário com nome de usuário {user.Username} já existe.");
            }
            await _databaseHandler.AddUserAsync(user);

            // Create DTO User
            var userDTO = new UserDTO(user.Name, user.Email, user.Username, user.Role, user.Group, user.Photo);

            return userDTO;
        }

    }

}
