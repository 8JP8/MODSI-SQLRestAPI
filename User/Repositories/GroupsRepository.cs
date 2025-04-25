using MODSI_SQLRestAPI.Infrastructure.Data;
using MODSI_SQLRestAPI.UserAuth.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.UserAuth.Repositories
{
    public class GroupsRepository
    {
        private readonly string _groups_DB;

        public GroupsRepository()
        {
            _groups_DB = ConfigurationManager.AppSettings["Groups_DBName"];
        }

        public async Task EnsureGroupsExistAsync(List<Groups> groups)
        {
            using (SqlConnection conn = new SqlConnection(ApplicationDbContext.ConnectionString))
            {
                await conn.OpenAsync();

                foreach (var group in groups)
                {
                    var query = $"IF NOT EXISTS (SELECT 1 FROM {_groups_DB} WHERE Name = @Name) " +
                                $"INSERT INTO {_groups_DB} (Name) VALUES (@Name)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", group.Name);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}