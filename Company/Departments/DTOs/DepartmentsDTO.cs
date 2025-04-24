using MODSI_SQLRestAPI.Company.DTOs;
using MODSI_SQLRestAPI.Company.KPIs.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.Departments.DTO
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DepartmentDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<KPIDto> KPIs { get; set; } = new List<KPIDto>();
        public List<RoleDepartmentPermissionDto> Permissions { get; set; } = new List<RoleDepartmentPermissionDto>();
    }

    public class CreateDepartmentDto
    {
        public string Name { get; set; }
    }

    public class UpdateDepartmentDto
    {
        public string Name { get; set; }
    }
}
