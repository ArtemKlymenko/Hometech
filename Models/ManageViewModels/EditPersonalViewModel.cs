using System.ComponentModel.DataAnnotations;
using Hometech.ValidationAttributes;

namespace Hometech.Models.ManageViewModels
{
    public class EditPersonalViewModel
    {
        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Required(ErrorMessage = "Введите свой адрес электронной почты")]
        [EmailAddress]
        [UniqueEmail]
        public string Email { get; set; }
        [Required(ErrorMessage = "Введите своё имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Введите свою фамилию")]
        public string Surname { get; set; }

        [RegularExpression(@"\+380((50)|(63)|(66)|(67)|(68)|(73)|(93)|(95)|(96)|(97)|(98)|(99))\d{7}",
            ErrorMessage="Пример номера телефона: +380501234567")]
        [Required(ErrorMessage = "Введите свой номер телефона")]
        [UniquePhoneNumber]
        public string PhoneNumber { get; set; }

        public string StatusMessage { get; set; }
    }
}