using Application.Common.Interfaces;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #region Services
        builder.Services.AddControllers();
        builder.Services.AddDbContext<LocalDbContext>(options =>
            options.UseNpgsql(builder
            .Configuration.GetConnectionString("DefaultConnection"),
             npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));
        builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<LocalDbContext>()
            .AddDefaultTokenProviders();
        #endregion

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.Run();
    }
}