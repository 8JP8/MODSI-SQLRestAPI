﻿using MODSI_SQLRestAPI.Company.Departments.Models;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }

    public interface IDepartmentRepository : IRepository<Departments.Models.Department>
    {
        Task<Department> GetDepartmentAndKPIsAsync(int id);
        Task<IEnumerable<Department>> GetDepartmentAndKPIsAsync();
        Task<IEnumerable<Department>> GetDepartmentsByRoleIdAsync(int roleId);
        Task<IEnumerable<Department>> GetDepartmentsByKPIIdAsync(int kpiId);
        Task AddKPIFromDepartmentAsync(int departmentId, int kpiId);
        Task RemoveKPIFromDepartmentAsync(int departmentId, int kpiId);
        Task UpdatePermissionsAsync(int roleId, int departmentId, bool canRead, bool canWrite);
        Task<IEnumerable<RoleDepartmentPermission>> GetRoleDepartmentPermissionsByRoleIdAsync(int roleId);
    }

    public interface IKPIRepository : IRepository<KPIs.Models.KPI>
    {
        Task<IEnumerable<KPI>> GetKPIsByDepartmentIdAsync(int departmentId);
    }

    public interface IRoleRepository : IRepository<Role>
    {
    }

    public interface IValueHistoryRepository : IRepository<ValueHistory>
    {
        Task<IEnumerable<ValueHistory>> GetAllAsync(int? kpiId = null, int? userId = null);
        new Task AddAsync(ValueHistory valueHistory);
    }
}
