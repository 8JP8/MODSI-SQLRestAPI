using MODSI_SQLRestAPI.Functions;
using MODSI_SQLRestAPI.UserAuth.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UserAuthenticate.Repositories;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.UserAuth.DTO;


// Ponto entre controlador e Base de Dados , Basicamnete onde se deve colcar if statements não relacionados a autenticação
// Ou seja processos de negocio aqui como tamanho de password, letras válidas, etc
//DTO retriver
namespace MODSI_SQLRestAPI.UserAuth.Services
{
    class UserService
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private readonly UserRepository _databaseHandler;
        private readonly Mapper _dto_mapper;
        public UserService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UserService>();
            _databaseHandler = new UserRepository();
            _dto_mapper = new Mapper();
        }

        //GetAllUsers
        internal async Task<List<UserDTO>> GetAllUsers()
        {
            var users = await _databaseHandler.GetAllUsersAsync();

            if (users == null || users.Count == 0)
            {
                _logger.LogWarning("No users found in the database.");
                return new List<UserDTO>();
            }
            // Map User to UserDTO
            var userDTOs = _dto_mapper.UserListTOUserDTOList(users);

            return userDTOs;
        }




    }

}
