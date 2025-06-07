// Company/KPIs/Services/ValueHistoryService.cs
using MODSI_SQLRestAPI.Company.KPIs.DTO;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.KPIs.Repositories;
using MODSI_SQLRestAPI.Company.Repositories;
using MODSI_SQLRestAPI.Company.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.KPIs.Services
{
    public class ValueHistoryService : IValueHistoryService
    {
        private readonly IValueHistoryRepository _repository;

        public ValueHistoryService(IValueHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ValueHistoryDTO>> GetHistoryAsync(int? kpiId = null, int? userId = null)
        {
            var histories = await _repository.GetAllAsync(kpiId, userId);
            return histories.Select(h => new ValueHistoryDTO
            {
                Id = h.Id,
                KPIId = h.KPIId,
                ChangedByUserId = h.ChangedByUserId,
                OldValue_1 = h.OldValue_1,
                NewValue_1 = h.NewValue_1,
                OldValue_2 = h.OldValue_2,
                NewValue_2 = h.NewValue_2,
                ChangedAt = h.ChangedAt,
                Unit = h.Unit
            }).ToList();

        }

        public async Task AddHistoryAsync(ValueHistoryDTO dto)
        {
            var entity = new ValueHistory
            {
                KPIId = dto.KPIId,
                ChangedByUserId = dto.ChangedByUserId,
                OldValue_1 = dto.OldValue_1,
                NewValue_1 = dto.NewValue_1,
                OldValue_2 = dto.OldValue_2,
                NewValue_2 = dto.NewValue_2,
                ChangedAt = dto.ChangedAt,
                Unit = dto.Unit
            };
            await _repository.AddAsync(entity);

        }
    }
}
