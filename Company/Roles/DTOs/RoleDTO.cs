using MODSI_SQLRestAPI.Company.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.Roles.DTOs
{
    public class RoleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RoleDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<RoleDepartmentPermissionDTO> Permissions { get; set; } = new List<RoleDepartmentPermissionDTO>();
    }

    public class CreateRoleDTO
    {
        public string Name { get; set; }
    }

    public class UpdateRoleDTO
    {
        public string Name { get; set; }
    }
}
