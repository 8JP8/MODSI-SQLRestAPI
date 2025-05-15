using MODSI_SQLRestAPI.Company.Departments.Models;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.Repositories;
using MODSI_SQLRestAPI.Company.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.Departments.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            // Garante que as coleções de navegação são carregadas
            return await _departmentRepository.GetDepartmentKPIsAsync();
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            // Garante que as coleções de navegação são carregadas
            return await _departmentRepository.GetDepartmentKPIsAsync(id);
        }

        public async Task<Department> GetDepartmentKPIsAsync(int id)
        {
            // Garante que as coleções de navegação são carregadas
            return await _departmentRepository.GetDepartmentKPIsAsync(id);
        }

        public async Task<IEnumerable<Department>> GetDepartmentsByRoleIdAsync(int roleId)
        {
            return await _departmentRepository.GetDepartmentsByRoleIdAsync(roleId);
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            await _departmentRepository.AddAsync(department);
            return department;
        }

        public async Task<Department> UpdateDepartmentAsync(int id, Department department)
        {
            var existing = await _departmentRepository.GetByIdAsync(id);
            if (existing == null)
                return null;

            existing.Name = department.Name;

            await _departmentRepository.UpdateAsync(existing);
            return existing;
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            await _departmentRepository.DeleteAsync(id);
        }

        public async Task AddKPIFromDepartmentAsync(int departmentId, int kpiId)
        {
            await _departmentRepository.AddKPIFromDepartmentAsync(departmentId, kpiId);
        }

        public async Task RemoveKPIFromDepartmentAsync(int departmentId, int kpiId)
        {
            await _departmentRepository.RemoveKPIFromDepartmentAsync(departmentId, kpiId);
        }

        public async Task UpdatePermissionsAsync(int roleId, int departmentId, bool canRead, bool canWrite)
        {
            await _departmentRepository.UpdatePermissionsAsync(roleId, departmentId, canRead, canWrite);
        }
    }
}
