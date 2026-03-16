using Domain.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class ModerationSocialObjectDto
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
        /// Автор объекта
        /// </summary>
        public string CreatorName { get; set; } = null!;

        /// <summary>
        /// Email автора
        /// </summary>
        public string CreatorEmail { get; set; } = null!;

        /// <summary>
        /// Количество отзывов
        /// </summary>
        public int ReviewsCount { get; set; }

        /// <summary>
        /// для людей, передвигающихся на креслах-колясках
        /// </summary>
        public bool Wheelchairs { get; set; }

        /// <summary>
        /// для людей с нарушениями зрения
        /// </summary>
        public bool BlindAccess { get; set; }

        /// <summary>
        /// для людей с нарушениями слуха
        /// </summary>
        public bool DeafAccess { get; set; }

        /// <summary>
        /// для людей с нарушениями речи
        /// </summary>
        public bool SpeechAccess { get; set; }

        /// <summary>
        /// для людей с нарушениями опорно-двигательного аппарата
        /// </summary>
        public bool MobilityAccess { get; set; }

        /// <summary>
        /// для людей с умственными нарушениями
        /// </summary>
        public bool IntellectualAccess { get; set; }

        /// <summary>
        /// для людей с нарушениями поведения и общения 
        /// </summary>
        public bool AutismAccess { get; set; }
    }
}
