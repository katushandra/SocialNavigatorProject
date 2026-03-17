using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Admin
{
    public class UserActivityDto
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Имя пользователя 
        /// </summary>
        public string UserName { get; set; } = null!;

        /// <summary>
        /// ФИО
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Количество отзывов, оставленных пользователем
        /// </summary>
        public int ReviewsCount { get; set; }

        /// <summary>
        /// Количество объектов, созданных пользователем
        /// </summary>
        public int ObjectsCount { get; set; }
    }
}
