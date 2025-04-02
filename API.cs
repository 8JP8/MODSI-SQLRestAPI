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

        #region 3D Points Visualization
        [Function("GetAllPoints")]
        public async Task<HttpResponseData> GetAllPoints([HttpTrigger(AuthorizationLevel.Function, "get", Route = "3DPoints/GetAll")] HttpRequestData req)
        {
            try
            {
                _logger.LogInformation("Retrieving all 3D points.");

                var points = await _databaseHandler.GetAllPointsAsync();
                _logger.LogInformation($"Retrieved points: {JsonSerializer.Serialize(points)}");

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(points));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving points.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("GetPointById")]
        public async Task<HttpResponseData> GetPointById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "3DPoints/Get/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving point with ID: {id}");

                var point = await _databaseHandler.GetPointByIdAsync(id);
                if (point == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonSerializer.Serialize(point));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving point with ID {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("AddPoints")]
        public async Task<HttpResponseData> AddPoints([HttpTrigger(AuthorizationLevel.Function, "post", Route = "3DPoints/Add")] HttpRequestData req)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding points.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("SendRandomPoints")]
        public async Task<HttpResponseData> SendRandomPoints([HttpTrigger(AuthorizationLevel.Function, "post", Route = "3DPoints/SendRandom")] HttpRequestData req)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending random points.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("DeletePointById")]
        public async Task<HttpResponseData> DeletePointById([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "3DPoints/Delete/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Deleting point with ID: {id}");

                await _databaseHandler.DeletePointByIdAsync(id);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"Point with ID {id} deleted successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting point with ID {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("ReplacePointById")]
        public async Task<HttpResponseData> ReplacePointById([HttpTrigger(AuthorizationLevel.Function, "put", Route = "3DPoints/Replace/{id:int}")] HttpRequestData req, int id)
        {
            try
            {
                _logger.LogInformation($"Replacing point with ID: {id}");

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var point = JsonSerializer.Deserialize<Point3D>(requestBody);
                point.ID = id;

                await _databaseHandler.ReplacePointByIdAsync(point);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                await response.WriteStringAsync($"Point with ID {id} replaced successfully.");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while replacing point with ID {id}.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
        #endregion
    }
}