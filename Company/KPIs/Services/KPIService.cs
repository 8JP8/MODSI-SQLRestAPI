using MODSI_SQLRestAPI.Company.Departments.DTO;
using MODSI_SQLRestAPI.Company.KPIs.DTO;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.Repositories;
using MODSI_SQLRestAPI.Company.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.KPIs.Services
{
    public class KPIService : IKPIService
    {
        private readonly IKPIRepository _kpiRepository;

        public KPIService(IKPIRepository kpiRepository)
        {
            _kpiRepository = kpiRepository ?? throw new ArgumentNullException(nameof(kpiRepository));
        }

        public async Task<IEnumerable<KPIDTO>> GetAllKPIsAsync()
        {
            var kpis = await _kpiRepository.GetKPIsWithDepartmentsAsync();
            return kpis.Select(kpi => new KPIDTO
            {
                Id = kpi.Id,
                Name = kpi.Name,
                Description = kpi.Description,
                Unit = kpi.Unit,
                Value_1 = kpi.Value_1,
                Value_2 = kpi.Value_2,
                AvailableInDepartments = kpi.DepartmentKPIs?
                    .Where(dk => dk.Department != null)
                    .Select(dk => new DepartmentDTO
                    {
                        Id = dk.Department.Id,
                        Name = dk.Department.Name
                    }).ToList() ?? new List<DepartmentDTO>()
            }).ToList();
        }

        public async Task<KPIDTO> GetKPIByIdAsync(int id)
        {
            var kpi = await _kpiRepository.GetKPIWithDepartmentsAsync(id);
            if (kpi == null)
                return null;

            return new KPIDTO
            {
                Id = kpi.Id,
                Name = kpi.Name,
                Description = kpi.Description,
                Unit = kpi.Unit,
                Value_1 = kpi.Value_1,
                Value_2 = kpi.Value_2,
                AvailableInDepartments = kpi.DepartmentKPIs?
                    .Where(dk => dk.Department != null)
                    .Select(dk => new DepartmentDTO
                    {
                        Id = dk.Department.Id,
                        Name = dk.Department.Name
                    }).ToList() ?? new List<DepartmentDTO>()
            };
        }

        public async Task<IEnumerable<KPI>> GetKPIsByDepartmentIdAsync(int departmentId)
        {
            return await _kpiRepository.GetKPIsByDepartmentIdAsync(departmentId);
        }

        public async Task<KPI> CreateKPIAsync(KPI kpi)
        {
            if (kpi == null)
                throw new ArgumentNullException(nameof(kpi));

            await _kpiRepository.AddAsync(kpi);
            return kpi;
        }

        public async Task<KPI> UpdateKPIAsync(int id, KPI kpi)
        {
            if (kpi == null)
                throw new ArgumentNullException(nameof(kpi));

            var existingKPI = await _kpiRepository.GetByIdAsync(id);
            if (existingKPI == null)
                return null;

            existingKPI.Name = kpi.Name;
            existingKPI.Description = kpi.Description;
            existingKPI.Unit = kpi.Unit;
            existingKPI.Value_1 = kpi.Value_1;
            existingKPI.Value_2 = kpi.Value_2;

            await _kpiRepository.UpdateAsync(existingKPI);
            return existingKPI;
        }

        public async Task DeleteKPIAsync(int id)
        {
            await _kpiRepository.DeleteAsync(id);
        }
    }
}
