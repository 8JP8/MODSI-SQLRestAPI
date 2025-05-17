using Microsoft.EntityFrameworkCore;
using MODSI_SQLRestAPI.Company.Departments.Models;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.Repositories;
using MODSI_SQLRestAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace MODSI_SQLRestAPI.Company.Departments.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

        public DepartmentRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Departments
                .Include(d => d.RoleDepartmentPermissions)
                    .ThenInclude(rdp => rdp.Role)
                .ToListAsync();
        }

        public async Task<Department> GetByIdAsync(int id)
        {
            return await _context.Departments
                .Include(d => d.RoleDepartmentPermissions)
                    .ThenInclude(rdp => rdp.Role)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Department>> FindAsync(Expression<Func<Department, bool>> predicate)
        {
            return await _context.Departments.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(Department entity)
        {
            await _context.Departments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Department>> GetDepartmentAndKPIsAsync()
        {
            return await _context.Departments
                .Include(d => d.DepartmentKPIs)
                    .ThenInclude(dk => dk.KPI)
                .Include(d => d.RoleDepartmentPermissions)
                    .ThenInclude(rdp => rdp.Role)
                .ToListAsync();
        }

        public async Task<Department> GetDepartmentAndKPIsAsync(int id)
        {
            return await _context.Departments
                .Include(d => d.DepartmentKPIs)
                    .ThenInclude(dk => dk.KPI)
                .Include(d => d.RoleDepartmentPermissions)
                    .ThenInclude(rdp => rdp.Role)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Department>> GetDepartmentsByRoleIdAsync(int roleId)
        {
            return await _context.Departments
                .Where(d => d.RoleDepartmentPermissions.Any(rdp => rdp.RoleId == roleId))
                .ToListAsync();
        }


        public async Task AddKPIFromDepartmentAsync(int departmentId, int kpiId)
        {
            var exists = await _context.DepartmentKPIs
                .AnyAsync(dk => dk.DepartmentId == departmentId && dk.KPIId == kpiId);

            if (!exists)
            {
                await _context.DepartmentKPIs.AddAsync(new DepartmentKPI
                {
                    DepartmentId = departmentId,
                    KPIId = kpiId
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveKPIFromDepartmentAsync(int departmentId, int kpiId)
        {
            var departmentKPI = await _context.DepartmentKPIs
                .FirstOrDefaultAsync(dk => dk.DepartmentId == departmentId && dk.KPIId == kpiId);

            if (departmentKPI != null)
            {
                _context.DepartmentKPIs.Remove(departmentKPI);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePermissionsAsync(int roleId, int departmentId, bool canRead, bool canWrite)
        {
            var permission = await _context.RoleDepartmentPermissions
                .FirstOrDefaultAsync(rdp => rdp.RoleId == roleId && rdp.DepartmentId == departmentId);

            if (permission != null)
            {
                permission.CanRead = canRead;
                permission.CanWrite = canWrite;
            }
            else
            {
                await _context.RoleDepartmentPermissions.AddAsync(new RoleDepartmentPermission
                {
                    RoleId = roleId,
                    DepartmentId = departmentId,
                    CanRead = canRead,
                    CanWrite = canWrite
                });
            }

            await _context.SaveChangesAsync();
        }


    }
}
