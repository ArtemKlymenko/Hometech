using System.ComponentModel.DataAnnotations;
using Hometech.ValidationAttributes;

namespace Hometech.Models.OrderViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Введите своё имя")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Введите свою фамилию")]
        public string Surname { get; set; }
        
        [Required(ErrorMessage = "Введите свой номер телефона")]
        [RegularExpression(@"\+380((50)|(63)|(66)|(67)|(68)|(73)|(93)|(95)|(96)|(97)|(98)|(99))\d{7}",
            ErrorMessage="Пример номера телефона: +380501234567")]
        [UniquePhoneNumber]
        public string PhoneNumber { get; set; }
        
        [Required(ErrorMessage = "Введите адрес электронной почты")]
        [EmailAddress]
        [UniqueEmail]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Введите название города")]
        public string City { get; set; }
        
        [Required(ErrorMessage = "Введите название улицы")]
        public string Street { get; set; }
        
        [Required(ErrorMessage = "Введите номер дома")]
        [Range(1,400, ErrorMessage = "Некорректный номер дома!")]
        public int HomeNumber { get; set; }
        
        public bool RememberPersonal { get; set; }
        public bool RememberAddress { get; set; }
    }
}