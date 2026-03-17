using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO.Admin
{
    public class TypeStatisticDto
    {
        /// <summary>
        /// Название типа объекта
        /// </summary>
        public string TypeName { get; set; } = null!;

        /// <summary>
        /// Количество объектов данного типа
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Проценты
        /// </summary>
        public double Percentage { get; set; }
    }
}
