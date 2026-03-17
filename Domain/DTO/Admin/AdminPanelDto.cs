using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Admin
{
    public class AdminPanelDto
    {
        /// <summary>
        /// Общее количество объектов 
        /// </summary>
        public int TotalObjects { get; set; }

        /// <summary>
        /// Количество одобренных объектов 
        /// </summary>
        public int TotalApprovedObjects { get; set; }

        /// <summary>
        /// Общее количество зарегистрированных пользователей
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Средний рейтинг всех объектов 
        /// </summary>
        public decimal AverageRating { get; set; }

        /// <summary>
        /// Статистика распределения объектов по типам 
        /// </summary>
        public List<TypeStatisticDto> ObjectsByType { get; set; } = new();

        /// <summary>
        /// Топ самых активных пользователей 
        /// </summary>
        public List<UserActivityDto> TopActiveUsers { get; set; } = new();
    }
}
