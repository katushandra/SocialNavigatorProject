using Domain.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace Domain.Entity
{
    public class SocialObject
    {
        /// <summary>
        /// идентификатор объекта
        /// </summary>
        public Guid IdObject { get; set; } = Guid.NewGuid();

        /// <summary>
        /// название объекта
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// описание объекта
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// адрес объекта
        /// </summary>
        public string Address { get; set; } = null!;

        /// <summary>
        /// координаты объекта
        /// </summary>
        public Point? Location { get; set; }

        /// <summary>
        /// схема проезда
        /// </summary>
        public string? RouteDescription { get; set; }

        /// <summary>
        /// идентификатор типа объекта
        /// </summary>
        public Guid ObjectTypeId { get; set; }

        /// <summary>
        /// для людей, передвигающихся на креслах-колясках
        /// </summary>
        public bool Wheelchairs { get; set; } = false;

        /// <summary>
        /// для людей с нарушениями зрения
        /// </summary>
        public bool BlindAccess { get; set; } = false;

        /// <summary>
        /// для людей с нарушениями слуха
        /// </summary>
        public bool DeafAccess { get; set; } = false;

        /// <summary>
        /// для людей с нарушениями речи
        /// </summary>
        public bool SpeechAccess { get; set; } = false;

        /// <summary>
        /// для людей с нарушениями опорно-двигательного аппарата
        /// </summary>
        public bool MobilityAccess { get; set; } = false;

        /// <summary>
        /// для людей с умственными нарушениями
        /// </summary>
        public bool IntellectualAccess { get; set; } = false;

        /// <summary>
        /// для людей с нарушениями поведения и общения 
        /// </summary>
        public bool AutismAccess { get; set; } = false;

        /// <summary>
        /// индентификатор пользователя, создавшего объект
        /// </summary>
        public Guid CreatorId { get; set; }

        /// <summary>
        /// дата создания объекта
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// статус объекта
        /// </summary>
        public Status Status { get; set; } = Status.Pending;

        /// <summary>
        /// идентификатор пользователя, отредактировавшего объект
        /// </summary>
        public Guid? EditorId { get; set; }

        /// <summary>
        /// дата редактирования объекта
        /// </summary>
        public DateTime? EditedAt { get; set; }

        /// <summary>
        /// общая оценка объекта
        /// </summary>
        public decimal ScoreObject { get; set; } = 0.0m;

        public ObjectType ObjectType { get; set; } = null!;
        public AppUser Creator { get; set; } = null!;
        public AppUser? Editor { get; set; }
        public required ICollection<Review> Reviews { get; set; } 
        public required ICollection<ModerationHistory> ModerationHistories { get; set; }
    }
}
