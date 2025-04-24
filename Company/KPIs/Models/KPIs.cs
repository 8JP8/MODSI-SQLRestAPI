using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.KPIs.Models
{
    public class KPI
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value_1 { get; set; }
        public string Value_2 { get; set; }

        // Navigation properties
        public ICollection<DepartmentKPI> DepartmentKPIs { get; set; }

        public KPI()
        {
            DepartmentKPIs = new List<DepartmentKPI>();
        }

        public KPI(int id, string name, string description, string unit, string value1, string value2)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("KPI name cannot be null or empty.");
            Id = id;
            Name = name;
            Description = description;
            Unit = unit;
            Value_1 = value1;
            Value_2 = value2;
            DepartmentKPIs = new List<DepartmentKPI>();
        }

        // Getters and setters
        public string GetName() => Name;
        public void SetName(string name) => Name = name;

        public string GetDescription() => Description;
        public void SetDescription(string description) => Description = description;

        public string GetUnit() => Unit;
        public void SetUnit(string unit) => Unit = unit;

        public string GetValue1() => Value_1;
        public void SetValue1(string value1) => Value_1 = value1;

        public string GetValue2() => Value_2;
        public void SetValue2(string value2) => Value_2 = value2;
    }
}
