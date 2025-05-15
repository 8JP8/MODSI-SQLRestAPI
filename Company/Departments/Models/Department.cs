using System.Collections.Generic;
using System;
using MODSI_SQLRestAPI.Company.KPIs.Models;

namespace MODSI_SQLRestAPI.Company.Departments.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigation properties
        public ICollection<RoleDepartmentPermission> RoleDepartmentPermissions { get; set; }
        public ICollection<DepartmentKPI> DepartmentKPIs { get; set; }

        public Department()
        {
            DepartmentKPIs = new List<DepartmentKPI>();
        }

        public Department(int id, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Department name cannot be null or empty.");
            Id = id;
            Name = name;
            DepartmentKPIs = new List<DepartmentKPI>();
        }

        public string GetName() => Name;
        public void SetName(string name) => Name = name;
    }
}