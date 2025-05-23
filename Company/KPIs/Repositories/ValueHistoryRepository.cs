using Microsoft.EntityFrameworkCore;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.Repositories;
using MODSI_SQLRestAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.KPIs.Repositories
{
    public class ValueHistoryRepository : IValueHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ValueHistoryRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<ValueHistory>> GetAllAsync()
        {
            return await _context.ValueHistories
                .Include(vh => vh.KPI)
                .OrderByDescending(vh => vh.ChangedAt)
                .ToListAsync();
        }

        public async Task<ValueHistory> GetByIdAsync(int id)
        {
            return await _context.ValueHistories
                .Include(vh => vh.KPI)
                .FirstOrDefaultAsync(vh => vh.Id == id);
        }

        public async Task<IEnumerable<ValueHistory>> FindAsync(Expression<Func<ValueHistory, bool>> predicate)
        {
            return await _context.ValueHistories
                .Include(vh => vh.KPI)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task AddAsync(ValueHistory entity)
        {
            await _context.ValueHistories.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ValueHistory entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var valueHistory = await _context.ValueHistories.FindAsync(id);
            if (valueHistory != null)
            {
                _context.ValueHistories.Remove(valueHistory);
                await _context.SaveChangesAsync();
            }
        }

        // Método específico para filtro por KPIId e UserId
        public async Task<IEnumerable<ValueHistory>> GetAllAsync(int? kpiId = null, int? userId = null)
        {
            var query = _context.ValueHistories
                .Include(vh => vh.KPI)
                .AsQueryable();

            if (kpiId.HasValue)
                query = query.Where(vh => vh.KPIId == kpiId.Value);

            if (userId.HasValue)
                query = query.Where(vh => vh.ChangedByUserId == userId.Value);

            return await query.OrderByDescending(vh => vh.ChangedAt).ToListAsync();
        }
    }
}
