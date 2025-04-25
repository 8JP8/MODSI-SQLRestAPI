using MODSI_SQLRestAPI.Company.Departments.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.KPIs.DTO
{
    public class KPIDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value_1 { get; set; }
        public string Value_2 { get; set; }
    }

    public class KPIDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value_1 { get; set; }
        public string Value_2 { get; set; }
        public List<DepartmentDTO> Departments { get; set; } = new List<DepartmentDTO>();
    }

    public class CreateKPIDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value_1 { get; set; }
        public string Value_2 { get; set; }
    }

    public class UpdateKPIDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value_1 { get; set; }
        public string Value_2 { get; set; }
    }
}
