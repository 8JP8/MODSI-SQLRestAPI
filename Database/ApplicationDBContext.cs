using Microsoft.EntityFrameworkCore;
using MODSI_SQLRestAPI.Company.Departments.Models;
using MODSI_SQLRestAPI.Company.KPIs.Models;
using MODSI_SQLRestAPI.UserAuth.Models;
using System.Configuration;
using System.Reflection.Emit;

namespace MODSI_SQLRestAPI.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {  }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<KPI> KPIs { get; set; }

        
        public DbSet<RoleDepartmentPermission> RoleDepartmentPermissions { get; set; }
        public DbSet<DepartmentKPI> DepartmentKPIs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DepartmentKPI composite key
            modelBuilder.Entity<DepartmentKPI>()
                .HasKey(dk => new { dk.DepartmentId, dk.KPIId });

            // Configure DepartmentKPI relationship
            modelBuilder.Entity<DepartmentKPI>()
                .HasOne(dk => dk.Department)
                .WithMany(d => d.DepartmentKPIs)
                .HasForeignKey(dk => dk.DepartmentId);

            modelBuilder.Entity<DepartmentKPI>()
                .HasOne(dk => dk.KPI)
                .WithMany(k => k.DepartmentKPIs)
                .HasForeignKey(dk => dk.KPIId);

            // Configure RoleDepartmentPermission composite key
            modelBuilder.Entity<RoleDepartmentPermission>()
                .HasKey(rdp => new { rdp.RoleId, rdp.DepartmentId });

            // Configure RoleDepartmentPermission relationship
            modelBuilder.Entity<RoleDepartmentPermission>()
                .HasOne(rdp => rdp.Role)
                .WithMany(r => r.RoleDepartmentPermissions)
                .HasForeignKey(rdp => rdp.RoleId);

            modelBuilder.Entity<RoleDepartmentPermission>()
                .HasOne(rdp => rdp.Department)
                .WithMany(d => d.RoleDepartmentPermissions)
                .HasForeignKey(rdp => rdp.DepartmentId);

            // Configure User to Role relationship
            //modelBuilder.Entity<User>()
            //    .HasOne(u => u.Role)
            //    .WithMany(r => r.Users)
            //    .HasForeignKey(u => u.RoleId);
        }
    }
}