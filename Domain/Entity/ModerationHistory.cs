using Domain.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class ModerationHistory
    {
        /// <summary>
        /// идентификатор записи
        /// </summary>
        public Guid IdModerationHistory { get; set; } = Guid.NewGuid();

        /// <summary>
        /// идентификатор объекта
        /// </summary>
        public Guid ObjectId { get; set; }


        /// <summary>
        /// идентификатор модератора
        /// </summary>
        public Guid ModeratorId { get; set; }

        /// <summary>
        /// предыдущий статус
        /// </summary>
        public Status? OldStatus { get; set; }

        /// <summary>
        /// новый статус
        /// </summary>
        public Status NewStatus { get; set; }

        /// <summary>
        /// причина отклонения или комментарий модератора
        /// </summary>
        public string Comment { get; set; } = null!;

        /// <summary>
        /// дата модерации
        /// </summary>
        public DateTime ModeratedAt { get; set; } = DateTime.UtcNow;

        public SocialObject Object { get; set; } = null!;
        public AppUser Moderator { get; set; } = null!;
    }
}
