using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.Company.Departments.Services;
using MODSI_SQLRestAPI.Company.DTOs;
using MODSI_SQLRestAPI.Company.KPIs.DTO;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.KPIs.Services;
using MODSI_SQLRestAPI.Company.Services;
using MODSI_SQLRestAPI.UserAuth.Models;
using MODSI_SQLRestAPI.UserAuth.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Company.KPIs.Controllers
{
    public class KPIFunctions
    {
        private readonly IKPIService _kpiService;
        private readonly IValueHistoryService _valueHistoryService;
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<KPIFunctions> _logger;

        public KPIFunctions(IKPIService kpiService, IValueHistoryService valueHistoryService, IDepartmentService departmentService, ILogger<KPIFunctions> logger)
        {
            _kpiService = kpiService;
            _valueHistoryService = valueHistoryService;
            _departmentService = departmentService;
            _logger = logger;
        }

        [Function("GetAllKPIs")]
        public async Task<HttpResponseData> GetAllKPIs(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kpis")] HttpRequestData req)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated || !principal.IsInGroup("ADMIN"))
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized.");
                return forbidden;
            }

            var kpis = await _kpiService.GetAllKPIsAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(kpis);
            return response;
        }

        [Function("GetKPIById")]
        public async Task<HttpResponseData> GetKPIById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kpis/byid/{id}")] HttpRequestData req, int id)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized.");
                return forbidden;
            }

            var (canRead, _) = await _departmentService.GetUserKPIAccess(id, principal);
            if (!canRead && !principal.IsInGroup("ADMIN"))
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized: You do not have read access to this KPI.");
                return forbidden;
            }

            var kpi = await _kpiService.GetKPIByIdAsync(id);
            if (kpi == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"KPI with ID {id} not found.");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(kpi);
            return response;
        }

        [Function("GetValueHistory")]
        public async Task<HttpResponseData> GetValueHistory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kpis/valuehistory")] HttpRequestData req)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized.");
                return forbidden;
            }

            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string kpiIdStr = query["kpiId"];
            int? kpiId = null;
            if (!string.IsNullOrEmpty(kpiIdStr))
            {
                if (!int.TryParse(kpiIdStr, out int parsed))
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync("O parâmetro 'kpiId' deve ser um número inteiro.");
                    return badRequest;
                }
                kpiId = parsed;
            }

            int? userId = int.TryParse(query["userId"], out var uid) ? uid : (int?)null;

            // Se não foi passado kpiId, só ADMIN pode acessar
            if (!kpiId.HasValue)
            {
                if (!principal.IsInGroup("ADMIN"))
                {
                    var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbidden.WriteStringAsync("Unauthorized: Only ADMIN can access all value history.");
                    return forbidden;
                }
            }
            else
            {
                // Se foi passado kpiId, ADMIN ou quem tem read access pode acessar
                var (canRead, _) = await _departmentService.GetUserKPIAccess(kpiId.Value, principal);
                if (!canRead)
                {
                    var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbidden.WriteStringAsync("Unauthorized: You do not have read access to this KPI.");
                    return forbidden;
                }
            }

            var history = await _valueHistoryService.GetHistoryAsync(kpiId, userId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(history);
            return response;
        }

        [Function("ChangeKPIAvailableDepartments")]
        public async Task<HttpResponseData> ChangeKPIAvailableDepartments(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "kpis/{kpiId}/departments")] HttpRequestData req,
            int kpiId)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated || !principal.IsInGroup("ADMIN"))
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized: Only ADMIN can change KPI departments.");
                return forbidden;
            }

            _logger.LogInformation($"ChangeKPIAvailableDepartments function processed a request for KPI {kpiId}.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var dto = JsonConvert.DeserializeObject<KPIAvailableDepartmentsDTO>(requestBody);

                if (dto == null || dto.AvailableInDepartments == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid departments data.");
                    return badRequestResponse;
                }

                var allDepartments = await _departmentService.GetAllDepartmentsAsync();

                foreach (var department in allDepartments)
                {
                    await _departmentService.RemoveKPIFromDepartmentAsync(department.Id, kpiId);
                }

                foreach (var departmentName in dto.AvailableInDepartments)
                {
                    var department = allDepartments.FirstOrDefault(d => d.Name == departmentName);
                    if (department != null)
                    {
                        await _departmentService.AddKPIFromDepartmentAsync(department.Id, kpiId);
                    }
                }

                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing available departments for KPI {kpiId}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("CreateKPIWithDepartments")]
        public async Task<HttpResponseData> CreateKPIWithDepartments(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "kpis/withdepartments")] HttpRequestData req)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated || !principal.IsInGroup("ADMIN"))
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized: Only ADMIN can create KPIs.");
                return forbidden;
            }

            _logger.LogInformation("CreateKPIWithDepartments function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var dto = JsonConvert.DeserializeObject<CreateKPIWithDepartmentsDTO>(requestBody);

                if (dto == null || dto.KPI == null || dto.AvailableInDepartments == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid KPI or departments data.");
                    return badRequestResponse;
                }

                var createdKPI = await _kpiService.CreateKPIAsync(dto.KPI);

                var allDepartments = await _departmentService.GetAllDepartmentsAsync();

                foreach (var departmentName in dto.AvailableInDepartments)
                {
                    var department = allDepartments.FirstOrDefault(d => d.Name == departmentName);
                    if (department != null)
                    {
                        await _departmentService.AddKPIFromDepartmentAsync(department.Id, createdKPI.Id);
                    }
                }

                var createdKPIDTO = new DTOMap().MapToKPIDetailDTO(createdKPI);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(createdKPIDTO);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating KPI with departments");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("CreateKPI")]
        public async Task<HttpResponseData> CreateKPI(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "kpis")] HttpRequestData req)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated || !principal.IsInGroup("ADMIN"))
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized: Only ADMIN can create KPIs.");
                return forbidden;
            }

            _logger.LogInformation("CreateKPI function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var kpi = JsonConvert.DeserializeObject<KPI>(requestBody);

                if (kpi == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid KPI data.");
                    return badRequestResponse;
                }

                var createdKPI = await _kpiService.CreateKPIAsync(kpi);
                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(createdKPI);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating KPI");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("UpdateKPI")]
        public async Task<HttpResponseData> UpdateKPI(
             [HttpTrigger(AuthorizationLevel.Function, "put", Route = "kpis/{kpiid}")] HttpRequestData req,
             int kpiid)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("Unauthorized.");
                return forbiddenResponse;
            }

            (_, bool canWrite) = await _departmentService.GetUserKPIAccess(kpiid, principal);
            if (!canWrite && !principal.IsInGroup("ADMIN"))
            {
                var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteStringAsync("Unauthorized: You do not have write access to this KPI.");
                return forbiddenResponse;
            }

            _logger.LogInformation($"UpdateKPI function processed a request for KPI {kpiid}.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var updateDto = JsonConvert.DeserializeObject<UpdateKPIDTO>(requestBody);

                if (updateDto == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid KPI data.");
                    return badRequestResponse;
                }

                int userId = 0;
                var userIdClaim = principal?.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    userId = parsedUserId;
                }

                var updatedKPI = await _kpiService.UpdateKPIFieldsAsync(kpiid, updateDto, userId);
                if (updatedKPI == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"KPI with ID {kpiid} not found.");
                    return notFoundResponse;
                }

                var updatedKPIDTO = new DTOMap().MapToKPIDetailDTO(updatedKPI);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(updatedKPIDTO);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating KPI {kpiid}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("DeleteKPI")]
        public async Task<HttpResponseData> DeleteKPI(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "kpis/{id}")] HttpRequestData req,
            int id)
        {
            var principal = new RetrieveToken().GetPrincipalFromRequest(req);
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized.");
                return forbidden;
            }

            var (_, canWrite) = await _departmentService.GetUserKPIAccess(id, principal);
            if (!canWrite && !principal.IsInGroup("ADMIN"))
            {
                var forbidden = req.CreateResponse(HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Unauthorized: You do not have write access to this KPI.");
                return forbidden;
            }

            _logger.LogInformation($"DeleteKPI function processed a request for KPI {id}.");

            try
            {
                await _kpiService.DeleteKPIAsync(id);
                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting KPI {id}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }
    }
}
