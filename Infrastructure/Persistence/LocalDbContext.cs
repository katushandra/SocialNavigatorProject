using Application.Common.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;

namespace Infrastructure.Persistence
{
    public class LocalDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>, ILocalDbContext
    {
        public override DatabaseFacade Database => base.Database;

        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
        {

        }

        public DbSet<ObjectType> ObjectType { get; set; }
        public DbSet<SocialObject> SocialObject { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<ModerationHistory> ModerationHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasPostgresExtension("postgis");

            #region ObjectType
            builder.Entity<ObjectType>(entity =>
            {
                entity.ToTable("ObjectType");
                entity.HasKey(e => e.IdObjectType);

                entity.Property(e => e.IdObjectType)
                .HasColumnName("IdObjectType")
                .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.Name)
                .HasColumnName("Name")
                .IsRequired()
                .HasMaxLength(100);

                entity.Property(e => e.Description)
                .HasColumnName("Description");

                entity.HasMany(e => e.Objects)
                .WithOne(o => o.ObjectType)
                .HasForeignKey(o => o.ObjectTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            });
            #endregion

            #region AppUser
            builder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUser");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("IdUser")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.FullName)
                    .HasColumnName("FullName")
                    .HasMaxLength(150);

                entity.Property(e => e.UserName)
                    .HasColumnName("UserName")
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PasswordHash)
                    .HasColumnName("PasswordHash")
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UserCreatedAt)
                    .HasColumnName("UserCreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UserEditedAt)
                    .HasColumnName("UserEditedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Active)
                    .HasColumnName("Active")
                    .HasDefaultValue(true);

                entity.HasMany(e => e.CreatedObjects)
                    .WithOne(o => o.Creator)
                    .HasForeignKey(o => o.CreatorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.EditedObjects)
                    .WithOne(o => o.Editor)
                    .HasForeignKey(o => o.EditorId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.Reviews)
                    .WithOne(r => r.User)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.ModerationHistories)
                    .WithOne(m => m.Moderator)
                    .HasForeignKey(m => m.ModeratorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaim");
            builder.Entity<IdentityRole<Guid>>().ToTable("AppRole");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaim");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogin");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("AppUserRole");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserToken");

            #endregion
            #region SocialObject
            builder.Entity<SocialObject>(entity =>
            {
                entity.ToTable("SocialObject");
                entity.HasKey(e => e.IdObject);

                entity.Property(e => e.IdObject)
                    .HasColumnName("IdObject")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.Name)
                    .HasColumnName("Name")
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasColumnName("Description");

                entity.Property(e => e.Address)
                    .HasColumnName("Address")
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Location)
                    .HasColumnName("Location")
                    .HasColumnType("geography (point, 4326)");

                entity.Property(e => e.RouteDescription)
                    .HasColumnName("RouteDescription");

                entity.Property(e => e.ObjectTypeId)
                    .HasColumnName("ObjectTypeId");

                entity.Property(e => e.Wheelchairs)
                    .HasColumnName("Wheelchairs")
                    .HasDefaultValue(false);

                entity.Property(e => e.BlindAccess)
                    .HasColumnName("BlindAccess")
                    .HasDefaultValue(false);

                entity.Property(e => e.DeafAccess)
                    .HasColumnName("DeafAccess")
                    .HasDefaultValue(false);

                entity.Property(e => e.SpeechAccess)
                    .HasColumnName("SpeechAccess")
                    .HasDefaultValue(false);

                entity.Property(e => e.MobilityAccess)
                    .HasColumnName("MobilityAccess")
                    .HasDefaultValue(false);

                entity.Property(e => e.IntellectualAccess)
                    .HasColumnName("IntellectualAccess")
                    .HasDefaultValue(false);

                entity.Property(e => e.AutismAccess)
                    .HasColumnName("AutismAccess")
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatorId)
                    .HasColumnName("CreatorId");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Status)
                    .HasColumnName("Status")
                    .HasConversion<string>()
                    .HasDefaultValue(Domain.Entity.Enums.Status.Pending);

                entity.Property(e => e.EditorId)
                    .HasColumnName("EditorId");

                entity.Property(e => e.EditedAt)
                    .HasColumnName("EditedAt");

                entity.Property(e => e.ScoreObject)
                    .HasColumnName("ScoreObject")
                    .HasColumnType("decimal(3,1)")
                    .HasDefaultValue(0.0m);
            });
            #endregion

            #region Review
            builder.Entity<Review>(entity =>
            {
                entity.ToTable("Review");
                entity.HasKey(e => e.IdReview);

                entity.Property(e => e.IdReview)
                    .HasColumnName("IdReview")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.ObjectId)
                    .HasColumnName("ObjectId");

                entity.Property(e => e.UserId)
                    .HasColumnName("UserId");

                entity.Property(e => e.Score)
                    .HasColumnName("Score")
                    .HasColumnType("decimal(3,1)")
                    .IsRequired();

                entity.Property(e => e.Comment)
                    .HasColumnName("Comment")
                    .IsRequired();

                entity.Property(e => e.ReviewCreatedAt)
                    .HasColumnName("ReviewCreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Object)
                    .WithMany(o => o.Reviews)
                    .HasForeignKey(e => e.ObjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion

            #region ModerationHistory
            builder.Entity<ModerationHistory>(entity =>
            {
                entity.ToTable("ModerationHistory");
                entity.HasKey(e => e.IdModerationHistory);

                entity.Property(e => e.IdModerationHistory)
                    .HasColumnName("IdModerationHistory")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.ObjectId)
                    .HasColumnName("ObjectId");

                entity.Property(e => e.ModeratorId)
                    .HasColumnName("ModeratorId");

                entity.Property(e => e.OldStatus)
                    .HasColumnName("OldStatus")
                    .HasConversion<string?>();

                entity.Property(e => e.NewStatus)
                    .HasColumnName("NewStatus")
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(e => e.Comment)
                    .HasColumnName("Comment")
                    .IsRequired();

                entity.Property(e => e.ModeratedAt)
                    .HasColumnName("ModeratedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Object)
                    .WithMany(o => o.ModerationHistories)
                    .HasForeignKey(e => e.ObjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Moderator)
                    .WithMany(u => u.ModerationHistories)
                    .HasForeignKey(e => e.ModeratorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            #endregion
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
