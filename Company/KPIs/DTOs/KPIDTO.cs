using System.Collections.Generic;

namespace MODSI_SQLRestAPI.Company.KPIs.DTO
{
    public class KPIDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value_1 { get; set; }
        public string Value_2 { get; set; }
    }

    public class KPIDetailDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value_1 { get; set; }
        public string Value_2 { get; set; }
        public List<string> AvailableInDepartments { get; set; }
    }

    public class CreateKPIDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value_1 { get; set; }
        public string Value_2 { get; set; }
    }

    public class UpdateKPIDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value_1 { get; set; }
        public string Value_2 { get; set; }
    }


}
