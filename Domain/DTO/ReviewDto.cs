using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class ReviewDto
    {
        /// <summary>
        /// идентификатор отзыва
        /// </summary>
        public Guid IdReview { get; set; }

        /// <summary>
        /// идентификатор объекта
        /// </summary>
        public Guid ObjectId { get; set; }

        /// <summary>
        /// пользователь
        /// </summary>
        public AppUserDto AppUser { get; set; }

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
        public DateTime ReviewCreatedAt { get; set; }

    }
}
