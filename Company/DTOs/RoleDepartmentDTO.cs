using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.DTOs
{
    public class RoleDepartmentPermissionDTO
    {
        public int RoleId { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
    }

    public class UpdatePermissionDTO
    {
        public int RoleId { get; set; }
        public int DepartmentId { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
    }
}
