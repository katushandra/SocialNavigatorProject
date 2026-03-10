using Domain.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class MyObjectDto
    {
        /// <summary>
        /// идентификатор объекта
        /// </summary>
        public Guid IdObject { get; set; }

        /// <summary>
        /// название объекта
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// адрес объекта
        /// </summary>
        public string Address { get; set; } = null!;

        /// <summary>
        /// Тип объекта
        /// </summary>
        public ObjectTypeDto ObjectType { get; set; }

        /// <summary>
        /// Статус объекта
        /// </summary>
        public Status Status { get; set; }
                 
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// общая оценка объекта
        /// </summary>
        public decimal ScoreObject { get; set; } = 0.0m;
    }

}
