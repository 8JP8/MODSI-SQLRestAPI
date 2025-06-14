﻿using MODSI_SQLRestAPI.Company.KPIs.DTO;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using System.Collections.Generic;
using System.Linq;

namespace MODSI_SQLRestAPI.Company.DTOs
{
    internal class DTOMap
    {
        public KPIDTO MapToKPIDTO(KPI kpi)
        {
            return new KPIDTO
            {
                Id = kpi.Id,
                Name = kpi.Name,
                Description = kpi.Description,
                Unit = kpi.Unit,
                Value_1 = kpi.Value_1,
                Value_2 = kpi.Value_2,
                ByProduct = kpi.ByProduct
            };
        }

        public KPIDetailDTO MapToKPIDetailDTO(KPI kpi)
        {
            return new KPIDetailDTO
            {
                Id = kpi.Id,
                Name = kpi.Name,
                Description = kpi.Description,
                Unit = kpi.Unit,
                Value_1 = kpi.Value_1,
                Value_2 = kpi.Value_2,
                ByProduct = kpi.ByProduct,
                AvailableInDepartments = kpi.DepartmentKPIs?
                    .Where(dk => dk.Department != null)
                    .Select(dk => dk.Department.Name)
                    .ToList() ?? new List<string>()
            };
        }
    }
}
