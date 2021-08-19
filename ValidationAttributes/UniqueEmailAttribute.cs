using System.ComponentModel.DataAnnotations;
using MySqlConnector;

namespace Hometech.ValidationAttributes
{
    public class UniqueEmailAttribute:ValidationAttribute
    {
        private const string ConnectionString = "Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36;";
        public override bool IsValid(object value)
        {
            if (value != null)
            {
                var email = value.ToString();
                using var con = new MySqlConnection(ConnectionString);
                using var cmd = con.CreateCommand();
                con.Open();
                cmd.CommandText = $"select count(*) from hometech.user_info where email='{email}'";
                var count = (long)cmd.ExecuteScalar();
                if (count == 0) 
                    return true;
                ErrorMessage="Пользователь с таким e-mail уже существует";
            }
            return false;
        }
    }
}