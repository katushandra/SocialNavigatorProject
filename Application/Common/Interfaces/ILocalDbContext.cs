using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface ILocalDbContext
    {
        DatabaseFacade Database { get; }
        DbSet<ObjectType> ObjectType { get; set; }
        DbSet<AppUser> User { get; set; }
        DbSet<SocialObject> Object { get; set; }
        DbSet<Review> Review { get; set; }
        DbSet<ModerationHistory> ModerationHistory { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
