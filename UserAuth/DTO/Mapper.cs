using MODSI_SQLRestAPI.UserAuth.Models;
using System.Collections.Generic;

namespace MODSI_SQLRestAPI.UserAuth.DTO
{
    class Mapper
    {
        /// Map User to UserDTO
        public List<UserDTO> UserListTOUserDTOList(List<User> users)
        {
            List<UserDTO> userDTOs = new List<UserDTO>();
            foreach (var user in users)
            {
                var userDTO = new UserDTO
                {
                    Name = user.Name,
                    Email = user.Email,
                    Username = user.Username,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive,
                    Group = user.Group
                };
                userDTOs.Add(userDTO);
            }
            return userDTOs;
        }
    }
}
