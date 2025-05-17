using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.Company.Departments.DTO;
using MODSI_SQLRestAPI.Company.Departments.Models;
using MODSI_SQLRestAPI.Company.KPIs.DTO;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.Departments.Controllers
{
    public class DepartmentFunctions
    {
        private readonly ILogger<DepartmentFunctions> _logger;
        private readonly IDepartmentService _departmentService;
        private readonly IKPIService _kpiService;

        public DepartmentFunctions(ILogger<DepartmentFunctions> logger, IDepartmentService departmentService, IKPIService kpiService)
        {
            _logger = logger;
            _departmentService = departmentService;
            _kpiService = kpiService;
        }

        [Function("GetAllDepartments")]
        public async Task<HttpResponseData> GetAllDepartments(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "departments")] HttpRequestData req)
        {
            _logger.LogInformation("GetAllDepartments function processed a request.");

            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var departmentDTOs = departments.Select(department => new DepartmentSummaryDTO
                {
                    Id = department.Id,
                    Name = department.Name,
                    RolesWithReadAccess = (department.RoleDepartmentPermissions ?? new List<RoleDepartmentPermission>())
                        .Where(rdp => rdp.CanRead)
                        .Select(rdp => rdp.Role != null ? rdp.Role.Name : null)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Distinct()
                        .ToList(),
                    RolesWithWriteAccess = (department.RoleDepartmentPermissions ?? new List<RoleDepartmentPermission>())
                        .Where(rdp => rdp.CanWrite)
                        .Select(rdp => rdp.Role != null ? rdp.Role.Name : null)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Distinct()
                        .ToList()
                }).ToList();

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(departmentDTOs);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all departments");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }


        [Function("GetDepartmentById")]
        public async Task<HttpResponseData> GetDepartmentById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "departments/{id}")] HttpRequestData req,
            int id)
        {
            _logger.LogInformation($"GetDepartmentById function processed a request for department {id}.");

            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Department with ID {id} not found.");
                    return notFoundResponse;
                }

                var departmentDTO = new DepartmentSummaryDTO
                {
                    Id = department.Id,
                    Name = department.Name,
                    RolesWithReadAccess = (department.RoleDepartmentPermissions ?? new List<RoleDepartmentPermission>())
                        .Where(rdp => rdp.CanRead)
                        .Select(rdp => rdp.Role != null ? rdp.Role.Name : null)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Distinct()
                        .ToList(),
                    RolesWithWriteAccess = (department.RoleDepartmentPermissions ?? new List<RoleDepartmentPermission>())
                        .Where(rdp => rdp.CanWrite)
                        .Select(rdp => rdp.Role != null ? rdp.Role.Name : null)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Distinct()
                        .ToList()
                };

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(departmentDTO);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting department {id}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("GetDepartmentKPIs")]
        public async Task<HttpResponseData> GetDepartmentKPIs(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "departments/{id}/kpis")] HttpRequestData req,
          int id)
        {
            _logger.LogInformation($"GetDepartmentKPIs function processed a request for department {id}.");

            try
            {
                var departmentKPIs = await _kpiService.GetKPIsByDepartmentIdAsync(id);

                if (departmentKPIs == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Department with ID {id} not found.");
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(departmentKPIs);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting department {id} with KPIs");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("GetDepartmentAndKPIs")]
        public async Task<HttpResponseData> GetDepartmentAndKPIs(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "DepartmentAndKPIs/{id}")] HttpRequestData req,
          int id)
        {
            _logger.LogInformation($"GetDepartmentAndKPIs function processed a request for department {id}.");

            try
            {
                var department = await _departmentService.GetDepartmentAndKPIsAsync(id);

                if (department == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Department with ID {id} not found.");
                    return notFoundResponse;
                }

                var departmentDTO = new DepartmentDetailDTO()
                {
                    Id = department.Id,
                    Name = department.Name,
                    RolesWithReadAccess = (department.RoleDepartmentPermissions ?? new List<RoleDepartmentPermission>())
                        .Where(rdp => rdp.CanRead)
                        .Select(rdp => rdp.Role != null ? rdp.Role.Name : null)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Distinct()
                        .ToList(),
                    RolesWithWriteAccess = (department.RoleDepartmentPermissions ?? new List<RoleDepartmentPermission>())
                        .Where(rdp => rdp.CanWrite)
                        .Select(rdp => rdp.Role != null ? rdp.Role.Name : null)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Distinct()
                        .ToList(),
                    KPIs = (department.DepartmentKPIs ?? new List<DepartmentKPI>())
                        .Where(dk => dk.KPI != null)
                        .Select(dk => new KPIDTO
                        {
                            Id = dk.KPI.Id,
                            Name = dk.KPI.Name,
                            Description = dk.KPI.Description,
                            Unit = dk.KPI.Unit,
                            Value_1 = dk.KPI.Value_1,
                            Value_2 = dk.KPI.Value_2
                        })
                        .ToList()
                };

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(departmentDTO);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting department {id} with KPIs");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("CreateDepartment")]
        public async Task<HttpResponseData> CreateDepartment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "departments")] HttpRequestData req)
        {
            _logger.LogInformation("CreateDepartment function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var department = JsonConvert.DeserializeObject<Department>(requestBody);

                if (department == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid department data.");
                    return badRequestResponse;
                }

                var createdDepartment = await _departmentService.CreateDepartmentAsync(department);
                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(createdDepartment);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("UpdateDepartment")]
        public async Task<HttpResponseData> UpdateDepartment(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "departments/{id}")] HttpRequestData req,
            int id)
        {
            _logger.LogInformation($"UpdateDepartment function processed a request for department {id}.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var department = JsonConvert.DeserializeObject<Department>(requestBody);

                if (department == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid department data.");
                    return badRequestResponse;
                }

                var updatedDepartment = await _departmentService.UpdateDepartmentAsync(id, department);
                if (updatedDepartment == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Department with ID {id} not found.");
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(updatedDepartment);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating department {id}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("DeleteDepartment")]
        public async Task<HttpResponseData> DeleteDepartment(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "departments/{id}")] HttpRequestData req,
            int id)
        {
            _logger.LogInformation($"DeleteDepartment function processed a request for department {id}.");

            try
            {
                await _departmentService.DeleteDepartmentAsync(id);
                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting department {id}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("AddKPIFromDepartment")]
        public async Task<HttpResponseData> AddKPIFromDepartment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "departments/{departmentId}/kpis/{kpiId}")] HttpRequestData req,
            int departmentId,
            int kpiId)
        {
            _logger.LogInformation($"AddKPIFromDepartment function processed a request for department {departmentId} and KPI {kpiId}.");

            try
            {
                await _departmentService.AddKPIFromDepartmentAsync(departmentId, kpiId);
                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding KPI {kpiId} to department {departmentId}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("RemoveKPIFromDepartment")]
        public async Task<HttpResponseData> RemoveKPIFromDepartment(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "departments/{departmentId}/kpis/{kpiId}")] HttpRequestData req,
            int departmentId,
            int kpiId)
        {
            _logger.LogInformation($"RemoveKPIFromDepartment function processed a request for department {departmentId} and KPI {kpiId}.");

            try
            {
                await _departmentService.RemoveKPIFromDepartmentAsync(departmentId, kpiId);
                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing KPI {kpiId} from department {departmentId}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("UpdatePermissions")]
        public async Task<HttpResponseData> UpdatePermissions(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "roles/{roleId}/departments/{departmentId}/permissions")] HttpRequestData req,
            int roleId,
            int departmentId)
        {
            _logger.LogInformation($"UpdatePermissions function processed a request for role {roleId} and department {departmentId}.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var permissions = JsonConvert.DeserializeObject<Dictionary<string, bool>>(requestBody);

                if (permissions == null || !permissions.TryGetValue("canRead", out bool canRead) || !permissions.TryGetValue("canWrite", out bool canWrite))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid permissions data. Required fields: canRead, canWrite");
                    return badRequestResponse;
                }

                await _departmentService.UpdatePermissionsAsync(roleId, departmentId, canRead, canWrite);
                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating permissions for role {roleId} and department {departmentId}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }
    }
}