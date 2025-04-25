using MODSI_SQLRestAPI.Company.DTOs;
using MODSI_SQLRestAPI.Company.KPIs.DTO;
using System.Collections.Generic;

namespace MODSI_SQLRestAPI.Company.Departments.DTO
{
    public class DepartmentDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<RoleDepartmentPermissionDTO> RoleDepartmentPermissions { get; internal set; } = new List<RoleDepartmentPermissionDTO>();
    }

    public class DepartmentDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<KPIDTO> KPIs { get; set; } = new List<KPIDTO>();
        public List<RoleDepartmentPermissionDTO> Permissions { get; set; } = new List<RoleDepartmentPermissionDTO>();
    }

    public class CreateDepartmentDTO
    {
        public string Name { get; set; }
    }

    public class UpdateDepartmentDTO
    {
        public string Name { get; set; }
    }
}
