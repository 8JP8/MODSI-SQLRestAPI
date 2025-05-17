using System.Collections.Generic;

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
