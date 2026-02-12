using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class ObjectTypeDto
    {
        /// <summary>
        /// идентификатор типа объекта
        /// </summary>
        public Guid IdObjectType { get; set; }

        /// <summary>
        /// название типа объекта
        /// </summary>
        public string NameObjectType { get; set; } = null!;
    }
}
