using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Configuration;
using System;

namespace MODSI_SQLRestAPI
{
    public class KeepAlive
    {
        private readonly ILogger<KeepAlive> _logger;
        private readonly HttpClient _httpClient;

        public KeepAlive(ILogger<KeepAlive> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [Function("CheckAPI")]
        public HttpResponseData CheckApi([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("CheckApi function executed at: {time}", System.DateTime.Now);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString("{\"Status\": \"Healthy\", \"timestamp\": \"" + System.DateTime.UtcNow + "\"}");

            return response;
        }

        
    }
}
