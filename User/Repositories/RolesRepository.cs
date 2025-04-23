using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MODSI_SQLRestAPI.UserAuth.Models;

namespace MODSI_SQLRestAPI.UserAuth.Repositories
{
    public class RolesRepository
    {
        private readonly string _connectionString;

        public RolesRepository()
        {
            // Replace with your actual connection string
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        // Ensure predefined roles exist in the database
        public async Task EnsureRolesExistAsync(List<Roles> predefinedRoles)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var role in predefinedRoles)
                {
                    var query = "IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = @Id) " +
                                "INSERT INTO Roles (Id, Name) VALUES (@Id, @Name)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", role.Id);
                        command.Parameters.AddWithValue("@Name", role.Name);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        // Get all roles from the database
        public async Task<List<Roles>> GetAllRolesAsync()
        {
            var roles = new List<Roles>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT Id, Name FROM Roles";
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        roles.Add(new Roles(reader.GetInt32(0), reader.GetString(1)));
                    }
                }
            }

            return roles;
        }

        // Add a new role to the database
        public async Task AddRoleAsync(Roles role)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "INSERT INTO Roles (Id, Name) VALUES (@Id, @Name)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", role.Id);
                    command.Parameters.AddWithValue("@Name", role.Name);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Delete a role by ID
        public async Task DeleteRoleByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "DELETE FROM Roles WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
