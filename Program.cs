using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MODSI_SQLRestAPI.Company.Departments.Services;
using MODSI_SQLRestAPI.Company.KPIs.Services;
using MODSI_SQLRestAPI.Company.Roles.Services;
using MODSI_SQLRestAPI.Company.Services;
using MODSI_SQLRestAPI.Infrastructure.Data;
using MODSI_SQLRestAPI.Company.Departments.Repositories;
using MODSI_SQLRestAPI.Company.Repositories;

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
                    services.AddScoped<IDepartmentService, DepartmentService>();
                    services.AddScoped<IKPIService, KPIService>();
                    services.AddScoped<IRoleService, RoleService>();
                    services.AddScoped<IDepartmentRepository, DepartmentRepository>();
                    services.AddScoped<IDepartmentService, DepartmentService>();
                })
                .Build();

            host.Run();
        }
    }
}