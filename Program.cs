using Microsoft.Azure.Functions.Worker;
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
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Microsoft.Azure.WebJobs;

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

                })
                .Build();

            host.Run();
        }
    }

    public class KeepWarmFunction
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        [Function("KeepWarm")]
        public async Task Run(FunctionContext context)
        {
            var logger = context.GetLogger("KeepWarm");
            logger.LogInformation($"KeepWarm function executed at: {DateTime.UtcNow}");

            try
            {
                var url = Environment.GetEnvironmentVariable("KEEPWARM_URL") ?? "https://<SEU-URL-DA-API>/api/health";
                var response = await _httpClient.GetAsync(url);
                logger.LogInformation($"KeepWarm ping status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"KeepWarm ping failed: {ex.Message}");
            }
        }
    }

    // Classe auxiliar para o TimerTrigger (pode ser vazia)
    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }
        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }
        public DateTime Next { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}