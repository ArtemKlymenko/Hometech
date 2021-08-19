namespace Hometech.Models
{
    public class user_info
    {
        public int id_user { get; set; }
        public string login { get; set; }
        public string norm_login { get; set; }
        public string password_hash { get; set; }
        public string email { get; set; }
        public string norm_email { get; set; }
        public bool email_confirm { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string phone_number { get; set; }
        public bool phone_number_confirm { get; set; }
    }
}