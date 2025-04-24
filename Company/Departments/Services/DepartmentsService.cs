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
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            return await _departmentRepository.GetAllAsync();
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            return await _departmentRepository.GetByIdAsync(id);
        }

        public async Task<Department> GetDepartmentWithKPIsAsync(int id)
        {
            return await _departmentRepository.GetDepartmentWithKPIsAsync(id);
        }

        public async Task<IEnumerable<Department>> GetDepartmentsByRoleIdAsync(int roleId)
        {
            return await _departmentRepository.GetDepartmentsByRoleIdAsync(roleId);
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));

            await _departmentRepository.AddAsync(department);
            return department;
        }

        public async Task<Department> UpdateDepartmentAsync(int id, Department department)
        {
            if (department == null)
                throw new ArgumentNullException(nameof(department));

            var existingDepartment = await _departmentRepository.GetByIdAsync(id);
            if (existingDepartment == null)
                return null;

            existingDepartment.Name = department.Name;
            await _departmentRepository.UpdateAsync(existingDepartment);
            return existingDepartment;
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            await _departmentRepository.DeleteAsync(id);
        }

        public async Task AddKPIToDepartmentAsync(int departmentId, int kpiId)
        {
            await _departmentRepository.AddKPIToDepartmentAsync(departmentId, kpiId);
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
