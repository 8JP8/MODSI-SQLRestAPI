using System.Collections.Generic;

namespace MODSI_SQLRestAPI.Company.DTOs
{
    public class DepartmentKPIDTO
    {
        public int DepartmentId { get; set; }
        public int KPIId { get; set; }
    }

    public class KPIAvailableDepartmentsDTO
    {
        public List<string> AvailableInDepartments { get; set; }
    }
}
