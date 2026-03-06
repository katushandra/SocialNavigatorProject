using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Текущий пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Новый пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтвеждение пароля")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
