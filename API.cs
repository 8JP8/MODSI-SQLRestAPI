using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace MODSI_SQLRestAPI
{
    public class Points
    {
        private readonly ILogger _logger;
        private readonly DatabaseHandler _databaseHandler;

        public Points(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Points>();
            _databaseHandler = new DatabaseHandler();
        }

        [Function("GetPoints")]
        public async Task<HttpResponseData> GetPoints([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("Retrieving all 3D points.");

            var points = await _databaseHandler.GetAllPointsAsync();
            _logger.LogInformation($"Retrieved points: {JsonSerializer.Serialize(points)}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(JsonSerializer.Serialize(points));

            return response;
        }

        [Function("AddPoints")]
        public async Task<HttpResponseData> AddPoints([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Adding new 3D points.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var points = JsonSerializer.Deserialize<List<Point3D>>(requestBody);

            await _databaseHandler.AddPointsAsync(points);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync("Points added successfully.");

            return response;
        }

        [Function("SendRandomPoints")]
        public async Task<HttpResponseData> SendRandomPoints([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Sending random 3D points.");

            var random = new Random();
            var points = new List<Point3D>();

            for (int i = 0; i < 10; i++)
            {
                points.Add(new Point3D
                {
                    X = random.Next(-100, 100),
                    Y = random.Next(-100, 100),
                    Z = random.Next(-100, 100)
                });
            }

            await _databaseHandler.AddPointsAsync(points);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(JsonSerializer.Serialize(points));

            return response;
        }
    }
}