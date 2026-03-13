using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required(ErrorMessage = "Название объекта обязательно")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Название должно содержать от 3 до 200 символов")]
        [Display(Name = "Название объекта")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// описание объекта
        /// </summary>
        [StringLength(2000, ErrorMessage = "Описание не может превышать 2000 символов")]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        /// <summary>
        /// адрес объекта
        /// </summary>
        [Required(ErrorMessage = "Адрес объекта обязателен")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Адрес должен содержать от 5 до 255 символов")]
        [Display(Name = "Адрес")]
        public string Address { get; set; } = null!;

        /// <summary>
        /// координаты объекта
        /// </summary>
        public Point? Location { get; set; }

        /// <summary>
        /// схема проезда
        /// </summary>
        [StringLength(2000, ErrorMessage = "Схема проезда не может превышать 2000 символов")]
        [Display(Name = "Схема проезда")]
        public string? RouteDescription { get; set; }

        /// <summary>
        /// тип объекта
        /// </summary>
        public Guid ObjectTypeId { get; set; }

        /// <summary>
        /// для людей, передвигающихся на креслах-колясках
        /// </summary>
        [Display(Name = "Доступно для кресел-колясок")]
        public bool Wheelchairs { get; set; }

        /// <summary>
        /// для людей с нарушениями зрения
        /// </summary>
        [Display(Name = "Доступно для людей с нарушениями зрения")]
        public bool BlindAccess { get; set; }

        /// <summary>
        /// для людей с нарушениями слуха
        /// </summary>
        [Display(Name = "Доступно для людей с нарушениями слуха")]
        public bool DeafAccess { get; set; }

        /// <summary>
        /// для людей с нарушениями речи
        /// </summary>
        [Display(Name = "Доступно для людей с нарушениями речи")]
        public bool SpeechAccess { get; set; }

        /// <summary>
        /// для людей с нарушениями опорно-двигательного аппарата
        /// </summary>
        [Display(Name = "Доступно для людей с нарушениями ОДА")]
        public bool MobilityAccess { get; set; }

        /// <summary>
        /// для людей с умственными нарушениями
        /// </summary>
        [Display(Name = "Доступно для людей с умственными нарушениями")]
        public bool IntellectualAccess { get; set; }

        /// <summary>
        /// для людей с нарушениями поведения и общения 
        /// </summary>
        [Display(Name = "Доступно для людей с нарушениями поведения")]
        public bool AutismAccess { get; set; }
    }
}
