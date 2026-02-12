using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class FullSocialObjectDto
    {
        public SocialObjectDto SocialObjectDto { get; set; }
        /// <summary>
        /// описание объекта
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Все отзывы об объекте
        /// </summary>
        public ICollection<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    }
}
