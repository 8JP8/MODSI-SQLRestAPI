using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MODSI_SQLRestAPI.Company.Departments.Repositories;
using MODSI_SQLRestAPI.Company.Departments.Services;
using MODSI_SQLRestAPI.Company.KPIs.Repositories;
using MODSI_SQLRestAPI.Company.KPIs.Services;
using MODSI_SQLRestAPI.Company.Repositories;
using MODSI_SQLRestAPI.Company.Roles.Repositories;
using MODSI_SQLRestAPI.Company.Roles.Services;
using MODSI_SQLRestAPI.Company.Services;
using MODSI_SQLRestAPI.Infrastructure.Data;
using MODSI_SQLRestAPI.UserAuth.Services;
using System.Net;

namespace MODSI_SQLRestAPI
{
    internal class Program
    {
        static void Main()
        {
            FunctionsDebugger.Enable();

            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    // Registrar o contexto do banco de dados
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(ApplicationDbContext.ConnectionString));

                    // Registrar os serviços
                    services.AddScoped<UserService>();
                    services.AddScoped<IDepartmentService, DepartmentService>();
                    services.AddScoped<IKPIService, KPIService>();
                    services.AddScoped<IRoleService, RoleService>();
                    services.AddScoped<IDepartmentRepository, DepartmentRepository>();
                    services.AddScoped<IKPIRepository, KPIRepository>();
                    services.AddScoped<IRoleRepository, RoleRepository>();
                    services.AddScoped<IValueHistoryRepository, ValueHistoryRepository>();
                    services.AddScoped<IValueHistoryService, ValueHistoryService>();
                    services.AddScoped<Rooms.Services.RoomService>();

                    services.AddHttpClient();
                })
                .Build();

            host.Run();
        }
    }

    public class APICheck
    {
        private readonly ILogger<APICheck> _logger;

        public APICheck(ILogger<APICheck> logger)
        {
            _logger = logger;
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