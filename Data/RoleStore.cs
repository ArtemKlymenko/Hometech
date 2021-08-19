using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Hometech.Models;
using MySqlConnector;
namespace Hometech.Data
{
    public class RoleStore : IRoleStore<role_info>
    {
        private readonly string _connectionString;

        public RoleStore(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<IdentityResult> CreateAsync(role_info role, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            using var con = new MySqlConnection(_connectionString);
            using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"insert into hometech.role_info (name,norm_role) values ({role.name},{role.norm_role})";
            await cmd.ExecuteNonQueryAsync(cToken);
            cmd.CommandText = "select last_insert_id()";
            role.id_role = (int)(ulong)cmd.ExecuteScalar();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(role_info role, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            using var con = new MySqlConnection(_connectionString);
            using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText =$"update hometech.role_info set name='{role.name}' norm_name='{role.norm_role}' where id_role={role.id_role}";
            await cmd.ExecuteNonQueryAsync(cToken);
            return IdentityResult.Success;
        }
        public async Task<IdentityResult> DeleteAsync(role_info role, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            using var con = new MySqlConnection(_connectionString);
            using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"delete from hometech.role_info where id_role={role.id_role}";
            await cmd.ExecuteNonQueryAsync(cToken);
            return IdentityResult.Success;
        }
        public Task<string> GetRoleIdAsync(role_info role, CancellationToken cToken)
        {
            return Task.FromResult(role.id_role.ToString());
        }

        public Task<string> GetRoleNameAsync(role_info role, CancellationToken cToken)
        {
            return Task.FromResult(role.name);
        }

        public Task SetRoleNameAsync(role_info role, string roleName, CancellationToken cToken)
        {
            role.name = roleName;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedRoleNameAsync(role_info role, CancellationToken cToken)
        {
            return Task.FromResult(role.norm_role);
        }

        public Task SetNormalizedRoleNameAsync(role_info role, string normalizedName, CancellationToken cToken)
        {
            role.norm_role = normalizedName;
            return Task.FromResult(0);
        }

        public async Task<role_info> FindByIdAsync(string roleId, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            using var con = new MySqlConnection(_connectionString);
            using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"select * from hometech.role_info where id_role={roleId}";
            var reader= await cmd.ExecuteReaderAsync(cToken);
            int idRole = reader.GetInt32(reader.GetOrdinal("id_role"));
            string name = reader.GetString(reader.GetOrdinal("name"));
            string normRole = reader.GetString(reader.GetOrdinal("norm_role"));
            var role=new role_info{id_role=idRole,name=name,norm_role=normRole};
            return role;
        }

        public async Task<role_info> FindByNameAsync(string normalizedRoleName, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            using var con = new MySqlConnection(_connectionString);
            using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"select * from hometech.role_info where norm_role='{normalizedRoleName}'";
            var reader= await cmd.ExecuteReaderAsync(cToken);
            int idRole = reader.GetInt32(reader.GetOrdinal("id_role"));
            string name = reader.GetString(reader.GetOrdinal("name"));
            string normRole = reader.GetString(reader.GetOrdinal("norm_role"));
            var role=new role_info{id_role=idRole,name=name,norm_role=normRole};
            return role;
        }
        public void Dispose(){}
    }
}