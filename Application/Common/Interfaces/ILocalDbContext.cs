using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Common.Interfaces
{
    public interface ILocalDbContext
    {
        DatabaseFacade Database { get; }
        DbSet<ObjectType> ObjectType { get; set; }
        DbSet<SocialObject> SocialObject { get; set; }
        DbSet<Review> Review { get; set; }
        DbSet<ModerationHistory> ModerationHistory { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
