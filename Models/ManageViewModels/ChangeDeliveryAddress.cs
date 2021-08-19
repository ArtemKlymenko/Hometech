using System.ComponentModel.DataAnnotations;

namespace Hometech.Models.ManageViewModels
{
    public class ChangeDeliveryAddress
    {
        [Required(ErrorMessage = "Введите название города")]
        public string City { get; set; }
        [Required(ErrorMessage = "Введите название улицы")]
        public string Street { get; set; }
        [Required(ErrorMessage = "Введите номер дома")]
        [Range(1,400, ErrorMessage = "Некорректный номер дома!")]
        public int HomeNumber { get; set; }
        public string StatusMessage { get; set; }
    }
}