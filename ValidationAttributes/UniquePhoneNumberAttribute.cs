using System.ComponentModel.DataAnnotations;
using MySqlConnector;

namespace Hometech.ValidationAttributes
{
    public class UniquePhoneNumberAttribute:ValidationAttribute
    {
        private const string ConnectionString = "Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36;";
        public override bool IsValid(object value)
        {
            if (value != null)
            {
                var phone = value.ToString();
                using var con = new MySqlConnection(ConnectionString);
                using var cmd = con.CreateCommand();
                con.Open();
                cmd.CommandText = $"select count(*) from hometech.user_info where phone_number='{phone}'";
                var count = (long)cmd.ExecuteScalar();
                if (count == 0) 
                    return true;
                ErrorMessage="Пользователь с таким номером телефона уже существует";
            }
            return false;
        }
    }
}