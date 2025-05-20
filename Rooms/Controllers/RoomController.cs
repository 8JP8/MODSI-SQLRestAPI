using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.Rooms.Models;
using MODSI_SQLRestAPI.Rooms.Services;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Rooms.Controllers
{
    public class RoomController
    {
        private readonly ILogger _logger;
        private readonly RoomService _service;

        public RoomController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RoomController>();
            _service = new RoomService();
        }

        [Function("AddRoom")]
        public async Task<HttpResponseData> AddRoom(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Room/Add")] HttpRequestData req)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonSerializer.Deserialize<Room>(body);

                if (string.IsNullOrWhiteSpace(data.Id) || data.Id.Length != 5)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync("Id deve ter 5 caracteres alfanuméricos.");
                    return badRequest;
                }

                await _service.AddRoom(data);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteStringAsync("Room adicionado com sucesso.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar Room");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Erro interno ao adicionar Room: " + ex.Message);
                return errorResponse;
            }
        }


        [Function("GetRoomById")]
        public async Task<HttpResponseData> GetRoomById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Room/Get/{id}")] HttpRequestData req, string id)
        {
            var room = await _service.GetRoomById(id);
            if (room == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Room não encontrado.");
                return notFound;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(room.JsonData);
            return response;
        }
    }
}
