using Application.Common.Interfaces;
using Domain.Entity;
using Domain.Entity.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class LocalDbContext : IdentityDbContext<AppUser>, ILocalDbContext
    {
        public IDbConnection Connection => Database.GetDbConnection();
        public override DatabaseFacade Database => base.Database;

        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
        {

        }

        public DbSet<ObjectType> ObjectType { get; set; }
        public DbSet<AppUser> User { get; set; }
        public DbSet<SocialObject> Object { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<ModerationHistory> ModerationHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region AppUser
            builder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUser");
                entity.HasKey(e => e.IdUser);

                entity.Property(e => e.IdUser)
                    .HasColumnName("IdUser")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.FullName)
                    .HasColumnName("FullName")
                    .HasMaxLength(150);

                entity.Property(e => e.Login)
                    .HasColumnName("Login")
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Password)
                    .HasColumnName("Password")
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Role)
                    .HasColumnName("Role")
                    .IsRequired()
                    .HasConversion<string>();

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
            #endregion

        }
    }
}