using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.UserAuth.DTO;
using MODSI_SQLRestAPI.UserAuth.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;


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


    }

}
