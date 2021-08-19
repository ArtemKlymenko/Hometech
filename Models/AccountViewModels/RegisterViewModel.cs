using System.ComponentModel.DataAnnotations;
using Hometech.ValidationAttributes;
namespace Hometech.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите адрес электронной почты")]
        [EmailAddress]
        [UniqueEmail]
        public string Email { get; set; }
        [Required(ErrorMessage = "Введите своё имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Введите свою фамилию")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Введите имя пользователя")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Введите свой номер телефона")]
        [RegularExpression(@"\+380((50)|(63)|(66)|(67)|(68)|(73)|(93)|(95)|(96)|(97)|(98)|(99))\d{7}",
            ErrorMessage="Пример номера телефона: +380501234567")]
        [UniquePhoneNumber]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен быть длиной от {2} до {1} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Введите пароль ещё раз")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Введите название города")]
        public string City { get; set; }
        [Required(ErrorMessage = "Введите название улицы")]
        public string Street { get; set; }
        [Required(ErrorMessage = "Введите номер дома")]
        [Range(1,400, ErrorMessage = "Некорректный номер дома!")]
        public int HomeNumber { get; set; }
    }
}