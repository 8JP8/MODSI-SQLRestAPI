using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                })
                .Build();

            host.Run();
        }
    }
}