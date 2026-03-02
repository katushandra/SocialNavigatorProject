using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class RegisterDto
    {
        [StringLength(150, ErrorMessage = "ФИО не должно превышать 150 символов")]
        [Display(Name = "ФИО")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(16, MinimumLength = 3, ErrorMessage = "Логин должен содержать от 3 до 16 символов")]
        [Display(Name = "Логин")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтвеждение пароля")]
        public string PasswordConfirm { get; set; } = null!;
    }
}


