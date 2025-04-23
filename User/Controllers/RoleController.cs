using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.UserAuth.Models;
using MODSI_SQLRestAPI.UserAuth.Repositories;

namespace MODSI_SQLRestAPI.UserAuth.Controllers
{
    public class RoleController
    {
        private readonly ILogger _logger;
        private readonly RolesRepository _rolesRepository;

        public RoleController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RoleController>();
            _rolesRepository = new RolesRepository();
        }

        // Initialize predefined roles
        [Function("InitializeRoles")]
        public async Task<HttpResponseData> InitializeRolesAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Roles/Initialize")] HttpRequestData req)
        {
            try
            {
                var predefinedRoles = new List<Roles>
                {
                    new Roles(1, "CEO"),
                    new Roles(2, "CFO"),
                    new Roles(3, "CTO"),
                    new Roles(4, "Accountant"),
                    new Roles(5, "RH"),
                    new Roles(6, "Logistics"),
                    new Roles(7, "Production"),
                    new Roles(8, "Marketing")
                };

                await _rolesRepository.EnsureRolesExistAsync(predefinedRoles);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("Roles initialized successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing roles.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        // Get all roles
        [Function("GetAllRoles")]
        public async Task<HttpResponseData> GetAllRoles([HttpTrigger(AuthorizationLevel.Function, "get", Route = "Roles/GetAll")] HttpRequestData req)
        {
            try
            {
                var roles = await _rolesRepository.GetAllRolesAsync();
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(roles));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving roles.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        // Add a new role
        [Function("AddRole")]
        public async Task<HttpResponseData> AddRole([HttpTrigger(AuthorizationLevel.Function, "post", Route = "Roles/Add")] HttpRequestData req)
        {
            try
            {
                var requestBody = await req.ReadAsStringAsync();
                var role = JsonSerializer.Deserialize<Roles>(requestBody);

                await _rolesRepository.AddRoleAsync(role);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteStringAsync("Role added successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a role.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        // Delete a role by ID
        [Function("DeleteRoleById")]
        public async Task<HttpResponseData> DeleteRoleById([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Roles/Delete/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                await _rolesRepository.DeleteRoleByIdAsync(id);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Role with ID {id} deleted successfully.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting role with ID {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}