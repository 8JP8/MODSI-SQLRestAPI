using MODSI_SQLRestAPI.Company.Departments.Models;
using MODSI_SQLRestAPI.Company.KPIs.DTO;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task<Department> GetDepartmentByIdAsync(int id);
        Task<Department> GetDepartmentAndKPIsAsync(int id);
        Task<IEnumerable<Department>> GetDepartmentsByRoleIdAsync(int roleId);
        Task<Department> CreateDepartmentAsync(Department department);
        Task<Department> UpdateDepartmentAsync(int id, Department department);
        Task DeleteDepartmentAsync(int id);
        Task AddKPIFromDepartmentAsync(int departmentId, int kpiId);
        Task RemoveKPIFromDepartmentAsync(int departmentId, int kpiId);
        Task UpdatePermissionsAsync(int roleId, int departmentId, bool canRead, bool canWrite);
    }

    public interface IKPIService
    {
        Task<IEnumerable<KPIDetailDTO>> GetAllKPIsAsync();
        Task<KPIDetailDTO> GetKPIByIdAsync(int id);
        Task<IEnumerable<KPIDTO>> GetKPIsByDepartmentIdAsync(int departmentId);
        Task<KPI> CreateKPIAsync(KPI kpi);
        Task<KPI> UpdateKPIAsync(int id, KPI kpi, int changedByUserId);

        Task DeleteKPIAsync(int id);

        Task<KPI> UpdateKPIFieldsAsync(int id, UpdateKPIDTO updateDto, int changedByUserId);
    }

    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(int id);
        Task<Role> CreateRoleAsync(Role role);
        Task<Role> UpdateRoleAsync(int id, Role role);
        Task DeleteRoleAsync(int id);
    }
    public interface IValueHistoryService
    {

        Task<IEnumerable<ValueHistoryDTO>> GetHistoryAsync(int? kpiId = null, int? userId = null);
        Task AddHistoryAsync(ValueHistoryDTO dto);

    }
}
