using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using MODSI_SQLRestAPI.Company.Services;
using Microsoft.Azure.Functions.Worker.Http;
using MODSI_SQLRestAPI.Company.Departments.Models;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json;
using MODSI_SQLRestAPI.Company.Departments.DTO;
using System.Linq;
using MODSI_SQLRestAPI.Company.KPIs.DTO;
using MODSI_SQLRestAPI.Company.DTOs;

namespace MODSI_SQLRestAPI.Company.Departments.Controllers
{
    public class DepartmentFunctions
    {
        private readonly ILogger<DepartmentFunctions> _logger;
        private readonly IDepartmentService _departmentService;

        public DepartmentFunctions(ILogger<DepartmentFunctions> logger, IDepartmentService departmentService)
        {
            _logger = logger;
            _departmentService = departmentService;
        }

        [Function("GetAllDepartments")]
        public async Task<HttpResponseData> GetAllDepartments(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "departments")] HttpRequestData req)
        {
            _logger.LogInformation("GetAllDepartments function processed a request.");

            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(departments);
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

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(department);
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

        [Function("GetDepartmentWithKPIs")]
        public async Task<HttpResponseData> GetDepartmentWithKPIs(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "departments/{id}/detail")] HttpRequestData req,
           int id)
        {
            _logger.LogInformation($"GetDepartmentWithKPIs function processed a request for department {id}.");

            try
            {
                var department = await _departmentService.GetDepartmentWithKPIsAsync(id);

                if (department == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Department with ID {id} not found.");
                    return notFoundResponse;
                }

                // Mapear para DTO para evitar ciclos de serialização
                var departmentDTO = new DepartmentDetailDTO
                {
                    Id = department.Id,
                    Name = department.Name,

                    // Mapear KPIs associados
                    KPIs = department.DepartmentKPIs
                        .Select(dk => new KPIDTO
                        {
                            Id = dk.KPI.Id,
                            Name = dk.KPI.Name,
                            Description = dk.KPI.Description,
                            Unit = dk.KPI.Unit,
                            Value_1 = dk.KPI.Value_1,
                            Value_2 = dk.KPI.Value_2
                        })
                        .ToList(),

                    // Mapear permissões de função
                    Permissions = department.RoleDepartmentPermissions
                        .Select(rdp => new RoleDepartmentPermissionDTO
                        {
                            RoleId = rdp.RoleId,
                            DepartmentId = rdp.DepartmentId,
                            CanRead = rdp.CanRead,
                            CanWrite = rdp.CanWrite
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

        [Function("AddKPIToDepartment")]
        public async Task<HttpResponseData> AddKPIToDepartment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "departments/{departmentId}/kpis/{kpiId}")] HttpRequestData req,
            int departmentId,
            int kpiId)
        {
            _logger.LogInformation($"AddKPIToDepartment function processed a request for department {departmentId} and KPI {kpiId}.");

            try
            {
                await _departmentService.AddKPIToDepartmentAsync(departmentId, kpiId);
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