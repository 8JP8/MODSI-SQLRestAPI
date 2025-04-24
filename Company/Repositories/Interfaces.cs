using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
        Task<IEnumerable<Departments.Models.Department>> GetDepartmentsWithKPIsAsync();
        Task<Departments.Models.Department> GetDepartmentWithKPIsAsync(int id);
        Task<IEnumerable<Departments.Models.Department>> GetDepartmentsByRoleIdAsync(int roleId);
        Task AddKPIToDepartmentAsync(int departmentId, int kpiId);
        Task RemoveKPIFromDepartmentAsync(int departmentId, int kpiId);
        Task UpdatePermissionsAsync(int roleId, int departmentId, bool canRead, bool canWrite);
    }

    public interface IKPIRepository : IRepository<MODSI_SQLRestAPI.Company.KPIs.Models.KPI>
    {
        Task<IEnumerable<KPIs.Models.KPI>> GetKPIsByDepartmentIdAsync(int departmentId);
        Task<IEnumerable<KPIs.Models.KPI>> GetKPIsWithDepartmentsAsync();
        Task<KPIs.Models.KPI> GetKPIWithDepartmentsAsync(int id);
    }

    public interface IRoleRepository : IRepository<Role>
    {
        Task<IEnumerable<Role>> GetRolesWithPermissionsAsync();
        Task<Role> GetRoleWithPermissionsAsync(int id);
    }
}
