using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class AddReviewDto
    {
        /// <summary>
        /// идентификатор объекта
        /// </summary>
        public Guid ObjectId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// оценка объекта
        /// </summary>
        [Required(ErrorMessage = "Оценка обязательна")]
        [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5")]
        public decimal Score { get; set; }

        /// <summary>
        /// комментарий к оценке
        /// </summary>
        [Required(ErrorMessage = "Комментарий обязателен")]
        [StringLength(1000, MinimumLength = 3, ErrorMessage = "Комментарий должен содержать от 3 до 1000 символов")]
        public string Comment { get; set; } = null!;

        /// <summary>
        /// дата создания отзыва
        /// </summary>
        public DateTime ReviewCreatedAt { get; set; } = DateTime.UtcNow;
    }
}
