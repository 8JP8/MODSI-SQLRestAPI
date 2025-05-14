using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.Company.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;

namespace MODSI_SQLRestAPI.Company.KPIs.Controllers
{
    public class KPIFunctions
    {
        private readonly ILogger<KPIFunctions> _logger;
        private readonly IKPIService _kpiService;

        public KPIFunctions(ILogger<KPIFunctions> logger, IKPIService kpiService)
        {
            _logger = logger;
            _kpiService = kpiService;
        }

        [Function("GetAllKPIs")]
        public async Task<HttpResponseData> GetAllKPIs(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kpis")] HttpRequestData req)
        {
            _logger.LogInformation("GetAllKPIs function processed a request.");

            try
            {
                var kpis = await _kpiService.GetAllKPIsAsync();
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(kpis);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all KPIs");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
        }

        [Function("GetKPIById")]
        public async Task<HttpResponseData> GetKPIById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "kpis/{id}")] HttpRequestData req,
            int id)
        {
            _logger.LogInformation($"GetKPIById function processed a request for KPI {id}.");

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting KPI {id}");
                var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while processing your request.");
                return response;
            }
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
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var kpi = JsonConvert.DeserializeObject<KPI>(requestBody);

                if (kpi == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid KPI data.");
                    return badRequestResponse;
                }

                var updatedKPI = await _kpiService.UpdateKPIAsync(id, kpi);
                if (updatedKPI == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"KPI with ID {id} not found.");
                    return notFoundResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(updatedKPI);
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
