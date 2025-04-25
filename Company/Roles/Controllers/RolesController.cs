using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.Company.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using MODSI_SQLRestAPI.Company.Roles.DTOs;
using System.Linq;
using MODSI_SQLRestAPI.Company.DTOs;

namespace MODSI_SQLRestAPI.Company.Roles.Controllers
{
    public class RoleFunctions
    {
        private readonly ILogger<RoleFunctions> _logger;
        private readonly IRoleService _roleService;

        public RoleFunctions(ILogger<RoleFunctions> logger, IRoleService roleService)
        {
            _logger = logger;
            _roleService = roleService;
        }

        [Function("GetAllRoles")]
        public async Task<HttpResponseData> GetAllRoles(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "roles")] HttpRequestData req)
        {
            _logger.LogInformation("GetAllRoles function processed a request.");

            try
            {
                var roles = await _roleService.GetAllRolesAsync();
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(roles);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("GetRoleById")]
        public async Task<HttpResponseData> GetRoleById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "roles/{id}")] HttpRequestData req,
            int id)
        {
            _logger.LogInformation($"GetRoleById function processed a request for role {id}.");

            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Role with ID {id} not found.");
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(role);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting role {id}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("GetRoleWithPermissions")]
        public async Task<HttpResponseData> GetRoleWithPermissions(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "roles/{id}/permissions")] HttpRequestData req,
            int id)
        {
            _logger.LogInformation($"Getting role {id} with permissions.");

            try
            {
                var role = await _roleService.GetRoleWithPermissionsAsync(id);
                if (role == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Role with ID {id} not found.");
                    return notFoundResponse;
                }

                // Map to DTO to avoid serialization cycles
                var roleDTO = new RoleDetailDTO
                {
                    Id = role.Id,
                    Name = role.Name,
                    Permissions = role.RoleDepartmentPermissions.Select(rdp => new RoleDepartmentPermissionDTO
                    {
                        RoleId = rdp.RoleId,
                        DepartmentId = rdp.DepartmentId,
                        DepartmentName = rdp.Department?.Name ?? "Unknown",
                        CanRead = rdp.CanRead,
                        CanWrite = rdp.CanWrite
                    }).ToList()
                };

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(roleDTO);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting role {id} with permissions");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("CreateRole")]
        public async Task<HttpResponseData> CreateRole(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "roles")] HttpRequestData req)
        {
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
