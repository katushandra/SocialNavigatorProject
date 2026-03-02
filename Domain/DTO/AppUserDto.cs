using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entity;

namespace Domain.DTO
{
    public class AppUserDto
    {
        /// <summary>
        /// идентификатор пользователя
        /// </summary>
        public Guid IdUser { get; set; }
        /// <summary>
        /// ФИО пользовтеля
        /// </summary>
        public string? FullName { get; set; }
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; } = null!;
        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; } = null!;
        /// <summary>
        /// Дата регистрации
        /// </summary>
        public DateTime UserCreatedAt { get; set; }
        /// <summary>
        /// Активен ли пользователь
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Роль пользователя
        /// </summary>
        public string Role { get; set; } = null!;
    }
}
