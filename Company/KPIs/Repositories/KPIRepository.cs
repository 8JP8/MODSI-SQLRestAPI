using Microsoft.EntityFrameworkCore;
using MODSI_SQLRestAPI.Company.Departments.Models;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.Repositories;
using MODSI_SQLRestAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.KPIs.Repositories
{
    public class KPIRepository : IKPIRepository
    {
        private readonly ApplicationDbContext _context;

        public KPIRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<KPI>> GetAllAsync()
        {
            return await _context.KPIs
                .Include(k => k.DepartmentKPIs)
                   .ThenInclude(dk => dk.Department)
                .ToListAsync();
        }

        public async Task<KPI> GetByIdAsync(int id)
        {
            return await _context.KPIs
                .Include(k => k.DepartmentKPIs)
                    .ThenInclude(dk => dk.Department)
                .FirstOrDefaultAsync(k => k.Id == id);
        }

        public async Task<IEnumerable<KPI>> FindAsync(Expression<Func<KPI, bool>> predicate)
        {
            return await _context.KPIs.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(KPI entity)
        {
            await _context.KPIs.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(KPI entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var kpi = await _context.KPIs.FindAsync(id);
            if (kpi != null)
            {
                _context.KPIs.Remove(kpi);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<KPI>> GetKPIsByDepartmentIdAsync(int departmentId)
        {
            return await _context.DepartmentKPIs
                .Where(dk => dk.DepartmentId == departmentId)
                .Select(dk => dk.KPI)
                .ToListAsync();
        }
    }
}
