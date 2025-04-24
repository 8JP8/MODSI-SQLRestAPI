using MODSI_SQLRestAPI.Company.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.Roles.DTOs
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RoleDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<RoleDepartmentPermissionDto> Permissions { get; set; } = new List<RoleDepartmentPermissionDto>();
    }

    public class CreateRoleDto
    {
        public string Name { get; set; }
    }

    public class UpdateRoleDto
    {
        public string Name { get; set; }
    }
}
