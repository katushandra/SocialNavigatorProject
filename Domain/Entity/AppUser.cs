using Domain.Entity.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class AppUser : IdentityUser
    {
        /// <summary>
        /// идентификатор пользователя
        /// </summary>
        public Guid IdUser { get; set; } = Guid.NewGuid();

        /// <summary>
        /// ФИО пользовтеля
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// логин пользователя
        /// </summary>
        public string Login { get; set; } = null!;

        /// <summary>
        /// email пользователя
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// пароль
        /// </summary>
        public string Password { get; set; } = null!;

        /// <summary>
        /// роль пользователя
        /// </summary>
        public Role Role { get; set; } = Role.User;

        /// <summary>
        /// дата создания пользовтаеля
        /// </summary>
        public DateTime UserCreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// дата редактирования пользовтаеля
        /// </summary>
        public DateTime UserEditedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// cтатус активности 
        /// </summary>
        public bool Active { get; set; } = true;

        public required ICollection<SocialObject> CreatedObjects { get; set; } 
        public required ICollection<SocialObject> EditedObjects { get; set; } 
        public required ICollection<Review> Reviews { get; set; }
        public required ICollection<ModerationHistory> ModerationHistories { get; set; }
    }
}
