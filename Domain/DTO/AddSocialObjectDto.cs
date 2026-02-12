using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class AddSocialObjectDto
    {
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
        /// тип объекта
        /// </summary>
        public Guid ObjectTypeId { get; set; }

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
