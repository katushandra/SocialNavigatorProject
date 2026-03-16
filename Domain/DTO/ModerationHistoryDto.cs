using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class ModerationHistoryDto
    {
        /// <summary>
        /// идентификатор записи
        /// </summary>
        public Guid IdModerationHistory { get; set; }

        /// <summary>
        /// Название объекта
        /// </summary>
        public string ObjectName { get; set; } = null!;

        /// <summary>
        /// Имя модератора
        /// </summary>
        public string ModeratorName { get; set; } = null!;

        /// <summary>
        /// Старый статус
        /// </summary>
        public string OldStatus { get; set; } = null!;

        /// <summary>
        /// Новый статус
        /// </summary>
        public string NewStatus { get; set; } = null!;

        /// <summary>
        /// Комментарий (причина отклонения)
        /// </summary>
        public string Comment { get; set; } = null!;

        /// <summary>
        /// Дата модерации
        /// </summary>
        public DateTime ModeratedAt { get; set; }
    }
}