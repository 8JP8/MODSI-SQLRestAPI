using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.Company.Departments.Services;
using MODSI_SQLRestAPI.Company.KPIs.Services;
using MODSI_SQLRestAPI.Company.Roles.DTOs;
using MODSI_SQLRestAPI.Company.Services;
using MODSI_SQLRestAPI.UserAuth.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.Roles.Controllers
{
    public class RoleFunctions
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleFunctions> _logger;
        private readonly IKPIService _kpiService;

        public RoleFunctions(IRoleService roleService, IKPIService kpiService, ILogger<RoleFunctions> logger)
        {
            _roleService = roleService;
            _kpiService = kpiService;
            _logger = logger;
        }

        [Function("GetAllRoles")]
        public async Task<HttpResponseData> GetAllRoles(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "roles")] HttpRequestData req)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized.");
                return forbidden;
            }

            var roles = await _roleService.GetAllRolesAsync();
            var roleDTOs = roles.Select(role => new RoleDTO
            {
                Id = role.Id,
                Name = role.Name,
                DepartmentsWithReadAccess = role.RoleDepartmentPermissions?
                    .Where(rdp => rdp.CanRead)
                    .Select(rdp => rdp.Department?.Name)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct()
                    .ToList() ?? new List<string>(),
                DepartmentsWithWriteAccess = role.RoleDepartmentPermissions?
                    .Where(rdp => rdp.CanWrite)
                    .Select(rdp => rdp.Department?.Name)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct()
                    .ToList() ?? new List<string>()
            }).ToList();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(roleDTOs);
            return response;
        }

        [Function("GetRoleById")]
        public async Task<HttpResponseData> GetRoleById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "roles/{id}")] HttpRequestData req,
            int id)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized.");
                return forbidden;
            }

            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Role with ID {id} not found.");
                return notFoundResponse;
            }

            var roleDTO = new RoleDTO
            {
                Id = role.Id,
                Name = role.Name,
                DepartmentsWithReadAccess = role.RoleDepartmentPermissions?
                    .Where(rdp => rdp.CanRead)
                    .Select(rdp => rdp.Department?.Name)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct()
                    .ToList() ?? new List<string>(),
                DepartmentsWithWriteAccess = role.RoleDepartmentPermissions?
                    .Where(rdp => rdp.CanWrite)
                    .Select(rdp => rdp.Department?.Name)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct()
                    .ToList() ?? new List<string>()
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(roleDTO);
            return response;
        }

        [Function("GetRoleKPIs")]
        public async Task<HttpResponseData> GetRoleKPIs(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "roles/{roleId}/kpis")] HttpRequestData req,
            int roleId)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized.");
                return forbidden;
            }

            _logger.LogInformation($"GetRoleKPIs function processed a request for role {roleId}.");

            try
            {
                var role = await _roleService.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Role with ID {roleId} not found.");
                    return notFoundResponse;
                }

                var departmentIds = role.RoleDepartmentPermissions?
                    .Where(rdp => rdp.Department != null && (rdp.CanRead || rdp.CanWrite))
                    .Select(rdp => rdp.Department.Id)
                    .Distinct()
                    .ToList() ?? new List<int>();

                var kpiSet = new HashSet<int>();
                var kpiList = new List<object>();

                foreach (var deptId in departmentIds)
                {
                    var kpis = await _kpiService.GetKPIsByDepartmentIdAsync(deptId);
                    foreach (var kpi in kpis)
                    {
                        if (kpiSet.Add(kpi.Id))
                        {
                            kpiList.Add(new
                            {
                                kpi.Id,
                                kpi.Name,
                                kpi.Description,
                                kpi.Unit,
                                kpi.Value_1,
                                kpi.Value_2,
                                kpi.ByProduct
                            });
                        }
                    }
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(kpiList);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting KPIs for role {roleId}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("GetRoleDepartments")]
        public async Task<HttpResponseData> GetRoleDepartments(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "roles/{roleId}/departments")] HttpRequestData req,
            int roleId)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized.");
                return forbidden;
            }

            _logger.LogInformation($"GetRoleDepartments function processed a request for role {roleId}.");

            try
            {
                var role = await _roleService.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Role with ID {roleId} not found.");
                    return notFoundResponse;
                }

                var departments = role.RoleDepartmentPermissions?
                    .Where(rdp => rdp.Department != null && (rdp.CanRead || rdp.CanWrite))
                    .Select(rdp => new
                    {
                        rdp.Department.Id,
                        rdp.Department.Name,
                        rdp.CanRead,
                        rdp.CanWrite
                    })
                    .Distinct()
                    .ToList();

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(departments);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting departments for role {roleId}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("CreateRole")]
        public async Task<HttpResponseData> CreateRole(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "roles")] HttpRequestData req)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated || !principal.IsInGroup("ADMIN"))
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized: Only ADMIN can create roles.");
                return forbidden;
            }

            _logger.LogInformation("CreateRole function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var role = JsonConvert.DeserializeObject<Role>(requestBody);

                if (role == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid role data.");
                    return badRequestResponse;
                }

                var createdRole = await _roleService.CreateRoleAsync(role);
                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(createdRole);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("UpdateRole")]
        public async Task<HttpResponseData> UpdateRole(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "roles/{id}")] HttpRequestData req,
            int id)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated || !principal.IsInGroup("ADMIN"))
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized: Only ADMIN can update roles.");
                return forbidden;
            }

            _logger.LogInformation($"UpdateRole function processed a request for role {id}.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var role = JsonConvert.DeserializeObject<Role>(requestBody);

                if (role == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid role data.");
                    return badRequestResponse;
                }

                var updatedRole = await _roleService.UpdateRoleAsync(id, role);
                if (updatedRole == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Role with ID {id} not found.");
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(updatedRole);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating role {id}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("DeleteRole")]
        public async Task<HttpResponseData> DeleteRole(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "roles/{id}")] HttpRequestData req,
            int id)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated || !principal.IsInGroup("ADMIN"))
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized: Only ADMIN can delete roles.");
                return forbidden;
            }

            _logger.LogInformation($"DeleteRole function processed a request for role {id}.");

            try
            {
                await _roleService.DeleteRoleAsync(id);
                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting role {id}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }
    }
}
