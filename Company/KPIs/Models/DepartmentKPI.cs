using MODSI_SQLRestAPI.Company.Departments.Models;

namespace MODSI_SQLRestAPI.Company.KPIs.Models
{
    public class DepartmentKPI
    {
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public int KPIId { get; set; }
        public KPI KPI { get; set; }

        public DepartmentKPI() { }

        public DepartmentKPI(int departmentId, int kpiId)
        {
            DepartmentId = departmentId;
            KPIId = kpiId;
        }
    }
}
