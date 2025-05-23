using MODSI_SQLRestAPI.Company.DTOs;
using MODSI_SQLRestAPI.Company.KPIs.DTO;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.KPIs.Repositories;
using MODSI_SQLRestAPI.Company.Repositories;
using MODSI_SQLRestAPI.Company.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.KPIs.Services
{
    public class KPIService : IKPIService
    {
        private readonly DTOMap _mapper = new DTOMap();

        private readonly IKPIRepository _kpiRepository;

        private readonly IValueHistoryRepository _valueHistoryRepository;


        public KPIService(IKPIRepository kpiRepository, IValueHistoryRepository valueHistoryRepository)
        {
            _kpiRepository = kpiRepository ?? throw new ArgumentNullException(nameof(kpiRepository));
            _valueHistoryRepository = valueHistoryRepository ?? throw new ArgumentNullException(nameof(valueHistoryRepository));
        }


        public async Task<IEnumerable<KPIDetailDTO>> GetAllKPIsAsync()
        {
            var kpis = await _kpiRepository.GetAllAsync();
            return kpis.Select(kpi => _mapper.MapToKPIDetailDTO(kpi)).ToList();
        }

        public async Task<KPIDetailDTO> GetKPIByIdAsync(int id)
        {
            var kpi = await _kpiRepository.GetByIdAsync(id);
            if (kpi == null)
                return null;
            return _mapper.MapToKPIDetailDTO(kpi);
        }

        public async Task<IEnumerable<KPIDTO>> GetKPIsByDepartmentIdAsync(int departmentId)
        {
            var kpis = await _kpiRepository.GetKPIsByDepartmentIdAsync(departmentId);
            return kpis.Select(kpi => _mapper.MapToKPIDTO(kpi)).ToList();
        }

        public async Task<KPI> CreateKPIAsync(KPI kpi)
        {
            if (kpi == null)
                throw new ArgumentNullException(nameof(kpi));

            await _kpiRepository.AddAsync(kpi);
            return kpi;
        }


        public async Task<KPI> UpdateKPIAsync(int id, KPI kpi, int changedByUserId)
        {
            if (kpi == null)
                throw new ArgumentNullException(nameof(kpi));

            var existingKPI = await _kpiRepository.GetByIdAsync(id);
            if (existingKPI == null)
                return null;

            // Verifica alteração do Value_1 e registra histórico
            if (existingKPI.Value_1 != kpi.Value_1)
            {
                var valueHistory = new ValueHistory
                {
                    KPIId = existingKPI.Id,
                    ChangedByUserId = changedByUserId,
                    OldValue = existingKPI.Value_1,
                    NewValue = kpi.Value_1,
                    ChangedAt = DateTime.UtcNow
                };
                await _valueHistoryRepository.AddAsync(valueHistory);
            }

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
