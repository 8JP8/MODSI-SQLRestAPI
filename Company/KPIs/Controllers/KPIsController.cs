using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.Company.DTOs;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.KPIs.Services;
using MODSI_SQLRestAPI.Company.Services;
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
        private readonly ILogger<KPIFunctions> _logger;

        public KPIFunctions(IKPIService kpiService, IValueHistoryService valueHistoryService, ILogger<KPIFunctions> logger)
        {
            _kpiService = kpiService;
            _valueHistoryService = valueHistoryService;
            _logger = logger;
        }

        [Function("GetAllKPIs")]
        public async Task<HttpResponseData> GetAllKPIs(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kpis")] HttpRequestData req)
        {
            var kpis = await _kpiService.GetAllKPIsAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(kpis);
            return response;
        }

        [Function("GetKPIById")]
        public async Task<HttpResponseData> GetKPIById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kpis/byid/{id}")] HttpRequestData req, int id)
        {
            var kpi = await _kpiService.GetKPIByIdAsync(id);
            if (kpi == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"KPI with ID {id} not found.");
                return notFoundResponse;
            }
            ;

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(kpi);
            return response;
        }


        [Function("GetValueHistory")]
        public async Task<HttpResponseData> GetValueHistory(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kpis/valuehistory")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            int? kpiId = int.TryParse(query["kpiId"], out var kid) ? kid : (int?)null;
            int? userId = int.TryParse(query["userId"], out var uid) ? uid : (int?)null;

            var history = await _valueHistoryService.GetHistoryAsync(kpiId, userId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(history);
            return response;
        }








        [Function("CreateKPI")]
        public async Task<HttpResponseData> CreateKPI(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "kpis")] HttpRequestData req)
        {
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
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "kpis/{id}")] HttpRequestData req,
            int id)
        {
            _logger.LogInformation($"UpdateKPI function processed a request for KPI {id}.");

            try
            {
                // Obter o id do usuário autenticado do token JWT
                var retrieveToken = new MODSI_SQLRestAPI.UserAuth.Services.RetrieveToken();
                var principal = retrieveToken.GetPrincipalFromRequest(req);

                if (principal == null || !principal.Identity.IsAuthenticated)
                {
                    var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbiddenResponse.WriteStringAsync("Unauthorized or invalid token.");
                    return forbiddenResponse;
                }

                // O id do usuário está no claim "id"
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "id");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    var forbiddenResponse = req.CreateResponse(HttpStatusCode.Forbidden);
                    await forbiddenResponse.WriteStringAsync("User ID not found in token.");
                    return forbiddenResponse;
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var kpi = JsonConvert.DeserializeObject<KPI>(requestBody);

                if (kpi == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid KPI data.");
                    return badRequestResponse;
                }

                var updatedKPI = await _kpiService.UpdateKPIAsync(id, kpi, userId);
                if (updatedKPI == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"KPI with ID {id} not found.");
                    return notFoundResponse;
                }

                // Mapeie para DTO antes de retornar
                var updatedKPIDTO = new DTOMap().MapToKPIDetailDTO(updatedKPI);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(updatedKPIDTO);
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating KPI {id}");
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
