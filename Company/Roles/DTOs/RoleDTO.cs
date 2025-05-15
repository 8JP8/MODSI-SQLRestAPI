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
        public List<string> DepartmentsWithReadAccess { get; set; }
        public List<string> DepartmentsWithWriteAccess { get; set; }
    }

    public class CreateRoleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UpdateRoleDTO
    {
        public string Name { get; set; }
    }
}
