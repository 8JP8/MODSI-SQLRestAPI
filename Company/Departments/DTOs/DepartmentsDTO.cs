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

    public class DepartmentSummaryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> RolesWithReadAccess { get; set; }
        public List<string> RolesWithWriteAccess { get; set; }
    }

    public class DepartmentDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> RolesWithReadAccess { get; set; }
        public List<string> RolesWithWriteAccess { get; set; }
        public List<KPIDTO> KPIs { get; set; }
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
