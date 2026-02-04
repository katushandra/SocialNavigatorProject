using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    /// <summary>
    /// Таблица типов объектов
    /// </summary>
    public class ObjectType
    {
        /// <summary>
        /// идентификатор типа объекта
        /// </summary>
        public Guid IdObjectType { get; set; } = Guid.NewGuid();

        /// <summary>
        /// название типа объекта
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// описание объекта
        /// </summary>
        public string? Description { get; set; }

        public required ICollection<SocialObject> Objects { get; set; }
    }
}