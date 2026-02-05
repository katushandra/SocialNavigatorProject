using Domain.Entity.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class AppUser : IdentityUser<Guid>
    {
        /// <summary>
        /// ФИО пользовтеля
        /// </summary>
        public string? FullName { get; set; }

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
