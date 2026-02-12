using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
