using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Hometech.Models;
using MySqlConnector;
namespace Hometech.Data
{
    public class UserStore: IUserEmailStore<user_info>, IUserPhoneNumberStore<user_info>,
        IUserPasswordStore<user_info>,IUserRoleStore<user_info>
    {
        private readonly string _connectionString;
        public UserStore(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        public async Task<IdentityResult> CreateAsync(user_info user,CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            var emailConfirm = user.email_confirm? 1:0;
            var phoneNumberConfirm = user.phone_number_confirm? 1:0;
            var sql = $"insert into hometech.user_info (login,norm_login,password_hash,email,norm_email,email_confirm,name,surname,phone_number,phone_number_confirm) values ('{user.login}','{user.norm_login}','{user.password_hash}','{user.email}','{user.norm_email}',{emailConfirm},'{user.name}','{user.surname}','{user.phone_number}',{phoneNumberConfirm})";
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(cToken);
            cmd.CommandText = "select last_insert_id()";
            user.id_user = (int)(ulong)cmd.ExecuteScalar();
            cmd.CommandText = $"insert into hometech.user_role (id_user,id_role) values ('{user.id_user}','2')";
            await cmd.ExecuteNonQueryAsync(cToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(user_info user, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"delete from hometech.user_info where id_user={user.id_user}";
            await cmd.ExecuteNonQueryAsync(cToken);
            return IdentityResult.Success;
        }

        public async Task<user_info> FindByIdAsync(string userId, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"select * from hometech.user_info where id_user={userId}";
            var reader= await cmd.ExecuteReaderAsync(cToken);
            if (!await reader.ReadAsync(cToken)) return null;
            var idUser=reader.GetInt32(reader.GetOrdinal("id_user"));
            var login=reader.GetString(reader.GetOrdinal("login"));
            var normLogin=reader.GetString(reader.GetOrdinal("norm_login"));
            var passwordHash=reader.GetString(reader.GetOrdinal("password_hash"));
            var email=reader.GetString(reader.GetOrdinal("email"));
            var normEmail=reader.GetString(reader.GetOrdinal("norm_email"));
            var name=reader.GetString(reader.GetOrdinal("name"));
            var surname=reader.GetString(reader.GetOrdinal("surname"));
            var phoneNumber=reader.GetString(reader.GetOrdinal("phone_number"));
            var emailConfirm=reader.GetBoolean(reader.GetOrdinal("email_confirm"));
            var phoneNumberConfirm=reader.GetBoolean(reader.GetOrdinal("phone_number_confirm"));
            var user = new user_info
            {
                id_user = idUser, login = login, norm_login = normLogin, password_hash = passwordHash, email = email, norm_email = normEmail, 
                name = name, surname = surname,phone_number = phoneNumber,
                phone_number_confirm = phoneNumberConfirm,email_confirm = emailConfirm 
            };
            return user;
        }

        public async Task<user_info> FindByNameAsync(string normalizedUserName, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"select * from hometech.user_info where norm_login='{normalizedUserName}'";
            var reader = await cmd.ExecuteReaderAsync(cToken);
            if (!await reader.ReadAsync(cToken)) return null;
            var idUser=reader.GetInt32(reader.GetOrdinal("id_user"));
            var login=reader.GetString(reader.GetOrdinal("login"));
            var normLogin=reader.GetString(reader.GetOrdinal("norm_login"));
            var passwordHash=reader.GetString(reader.GetOrdinal("password_hash"));
            var email=reader.GetString(reader.GetOrdinal("email"));
            var normEmail=reader.GetString(reader.GetOrdinal("norm_email"));
            var name=reader.GetString(reader.GetOrdinal("name"));
            var surname=reader.GetString(reader.GetOrdinal("surname"));
            var phoneNumber=reader.GetString(reader.GetOrdinal("phone_number"));
            var emailConfirm=reader.GetBoolean(reader.GetOrdinal("email_confirm"));
            var phoneNumberConfirm=reader.GetBoolean(reader.GetOrdinal("phone_number_confirm"));
            var user = new user_info
            {
                id_user = idUser, login = login, norm_login = normLogin, password_hash = passwordHash, email = email, norm_email = normEmail, 
                name = name, surname = surname,phone_number = phoneNumber,
                phone_number_confirm = phoneNumberConfirm,email_confirm = emailConfirm 
            };
            return user;
        }
        
        public Task<string> GetNormalizedUserNameAsync(user_info user, CancellationToken cToken)
        {
            return Task.FromResult(user.norm_login);
        }
        
        public Task<string> GetUserIdAsync(user_info user, CancellationToken cToken)
        {
            return Task.FromResult(user.id_user.ToString());
        }

        public Task<string> GetUserNameAsync(user_info user, CancellationToken cToken)
        {
            return Task.FromResult(user.login);
        }
        
        public Task SetNormalizedUserNameAsync(user_info user, string normalizedName, CancellationToken cToken)
        {
            user.norm_login = normalizedName;
            return Task.FromResult(0);
        }
        
        public Task SetUserNameAsync(user_info user, string userName, CancellationToken cToken)
        {
            user.login = userName;
            return Task.FromResult(0);
        }
        public async Task<IdentityResult> UpdateAsync(user_info user, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText =
                $"update hometech.user_info set login='{user.login}', norm_login='{user.norm_login}', password_hash='{user.password_hash}', email='{user.email}', email_confirm={user.email_confirm}, norm_email='{user.norm_email}', name='{user.name}', surname='{user.surname}', phone_number='{user.phone_number}', phone_number_confirm={user.phone_number_confirm} where id_user={user.id_user}";
            await cmd.ExecuteNonQueryAsync(cToken);
            return IdentityResult.Success;
        }
        
        public Task SetEmailAsync(user_info user, string email, CancellationToken cToken)
        {
            user.email = email;
            return Task.FromResult(0);
        }
        
        public Task<string> GetEmailAsync(user_info user, CancellationToken cToken)
        {
            return Task.FromResult(user.email);
        }
        
        public Task<bool> GetEmailConfirmedAsync(user_info user, CancellationToken cToken)
        {
            return Task.FromResult(user.email_confirm);
        }
        
        public Task SetEmailConfirmedAsync(user_info user, bool confirmed, CancellationToken cToken)
        {
            user.email_confirm = confirmed;
            return Task.FromResult(0);
        }

        public async Task<user_info> FindByEmailAsync(string normalizedEmail, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"select * from hometech.user_info where norm_email='{normalizedEmail}'";
            var reader= await cmd.ExecuteReaderAsync(cToken);
            if (!await reader.ReadAsync(cToken)) return null;
            var idUser=reader.GetInt32(reader.GetOrdinal("id_user"));
            var login=reader.GetString(reader.GetOrdinal("login"));
            var normLogin=reader.GetString(reader.GetOrdinal("norm_login"));
            var passwordHash=reader.GetString(reader.GetOrdinal("password_hash"));
            var email=reader.GetString(reader.GetOrdinal("email"));
            var normEmail=reader.GetString(reader.GetOrdinal("norm_email"));
            var emailConfirm=reader.GetBoolean(reader.GetOrdinal("email_confirm"));
            var name=reader.GetString(reader.GetOrdinal("name"));
            var surname=reader.GetString(reader.GetOrdinal("surname"));
            var phoneNumber=reader.GetString(reader.GetOrdinal("phone_number"));
            var phoneNumberConfirm=reader.GetBoolean(reader.GetOrdinal("phone_number_confirm"));
            var user = new user_info
            {
                id_user = idUser, login = login, norm_login = normLogin, password_hash = passwordHash, email = email, norm_email = normEmail, email_confirm = emailConfirm, 
                name = name, surname = surname,phone_number = phoneNumber,phone_number_confirm = phoneNumberConfirm
            };
            return user;
        }

        public Task<string> GetNormalizedEmailAsync(user_info user, CancellationToken cToken)
        {
            return Task.FromResult(user.norm_email);
        }

        public Task SetNormalizedEmailAsync(user_info user, string normalizedEmail, CancellationToken cToken)
        {
            user.norm_email = normalizedEmail;
            return Task.FromResult(0);
        }
        
        public Task SetPhoneNumberAsync(user_info user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.phone_number = phoneNumber;
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(user_info user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.phone_number);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(user_info user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.phone_number_confirm);
        }

        public Task SetPhoneNumberConfirmedAsync(user_info user, bool confirmed, CancellationToken cancellationToken)
        {
            user.phone_number_confirm = confirmed;
            return Task.FromResult(0);
        }
        
        public Task SetPasswordHashAsync(user_info user, string passwordHash, CancellationToken cToken)
        {
            user.password_hash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(user_info user, CancellationToken cToken)
        {
            return Task.FromResult(user.password_hash);
        }

        public Task<bool> HasPasswordAsync(user_info user, CancellationToken cToken)
        {
            return Task.FromResult(user.password_hash != null);
        }

        public async Task AddToRoleAsync(user_info user, string roleName, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            var normalizedName = roleName.ToUpper();
            cmd.CommandText = $"select id_role from hometech.role_info where norm_role='{normalizedName}'";
            if (await cmd.ExecuteNonQueryAsync(cToken) == 0)
            {
                cmd.CommandText=$"insert into role_info (name, norm_role) values ({roleName},{normalizedName})";
                await cmd.ExecuteNonQueryAsync(cToken);
            }
            cmd.CommandText = $"select id_role from hometech.role_info where norm_role='{normalizedName}'";
            var roleId = (int)(ulong) cmd.ExecuteScalar();
            cmd.CommandText=$"insert ignore into hometech.user_role (id_user,id_role) values ({user.id_user},{roleId})";
            await cmd.ExecuteNonQueryAsync(cToken);
        }

        public async Task RemoveFromRoleAsync(user_info user, string roleName, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"select id_role from hometech.role_info where norm_role='{roleName.ToUpper()}'";
            if (await cmd.ExecuteNonQueryAsync(cToken) == 1)
            {
                var roleId = (int)(ulong) cmd.ExecuteScalar();
                cmd.CommandText = $"delete from hometech.user_role where id_user={user.id_user} and id_role={roleId}";
                await cmd.ExecuteNonQueryAsync(cToken);
            }
        }

        public async Task<IList<string>> GetRolesAsync(user_info user, CancellationToken cToken)
        {
            List<string> userRoles = new();
            cToken.ThrowIfCancellationRequested();
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"select ri.name from hometech.role_info ri inner join hometech.user_role ur using (id_role) where id_user={user.id_user}";
            var reader = await cmd.ExecuteReaderAsync(cToken);
            while (await reader.ReadAsync(cToken))
            {
                userRoles.Add(reader.GetString(reader.GetOrdinal("name")));
            }
            return userRoles;
        }

        public async Task<bool> IsInRoleAsync(user_info user, string roleName, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            cmd.CommandText = $"select id_role from hometech.role_info where norm_role='{roleName.ToUpper()}'";
            if (await cmd.ExecuteNonQueryAsync(cToken) == 0) return false;
            var roleId = (int)(ulong) cmd.ExecuteScalar();
            cmd.CommandText =
                $"select count(*) from hometech.user_role where id_user={user.id_user} and id_role={roleId}";
            return (int)(ulong) cmd.ExecuteScalar() > 0;
        }

        public async Task<IList<user_info>> GetUsersInRoleAsync(string roleName, CancellationToken cToken)
        {
            cToken.ThrowIfCancellationRequested();
            await using var con = new MySqlConnection(_connectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync(cToken);
            List<user_info> usersInRole = new(); 
            cmd.CommandText = $"select u.* from hometech.user_info u inner join hometech.user_role using (id_user) inner join hometech.role_info using (id_role) where norm_role='{roleName.ToUpper()}'";
            var reader = await cmd.ExecuteReaderAsync(cToken);
            while (await reader.ReadAsync(cToken))
            {
                var idUser=reader.GetInt32(reader.GetOrdinal("id_user"));
                var login=reader.GetString(reader.GetOrdinal("login"));
                var normLogin=reader.GetString(reader.GetOrdinal("norm_login"));
                var passwordHash=reader.GetString(reader.GetOrdinal("password_hash"));
                var email=reader.GetString(reader.GetOrdinal("email"));
                var normEmail=reader.GetString(reader.GetOrdinal("norm_email"));
                var emailConfirm=reader.GetBoolean(reader.GetOrdinal("email_confirm"));
                var name=reader.GetString(reader.GetOrdinal("name"));
                var surname=reader.GetString(reader.GetOrdinal("surname"));
                var phoneNumber=reader.GetString(reader.GetOrdinal("phone_number"));
                var phoneNumberConfirm=reader.GetBoolean(reader.GetOrdinal("phone_number_confirm"));
                var user = new user_info
                {
                    id_user = idUser, login = login, norm_login = normLogin, password_hash = passwordHash, email = email, norm_email = normEmail, email_confirm = emailConfirm, 
                    name = name, surname = surname,phone_number = phoneNumber,phone_number_confirm = phoneNumberConfirm
                };
                usersInRole.Add(user);
            }
            return usersInRole;
        }
        public void Dispose(){}
    }
}