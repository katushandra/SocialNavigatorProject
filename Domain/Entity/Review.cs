using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class Review
    {
        /// <summary>
        /// идентификатор отзыва
        /// </summary>
        public Guid IdReview { get; set; } = Guid.NewGuid();

        /// <summary>
        /// идентификатор объекта
        /// </summary>
        public Guid ObjectId { get; set; }

        /// <summary>
        /// идентификтор пользователя
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// оценка объекта
        /// </summary>
        public decimal Score { get; set; }

        /// <summary>
        /// комментарий к оценке
        /// </summary>
        public string Comment { get; set; } = null!;

        /// <summary>
        /// дата создания отзыва
        /// </summary>
        public DateTime ReviewCreatedAt { get; set; } = DateTime.UtcNow;

        public SocialObject Object { get; set; } = null!;
        public AppUser User { get; set; } = null!;
    }
}
