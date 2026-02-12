using Application.Common.Interfaces;
using Application.Mapping;
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
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddDbContext<LocalDbContext>(options =>
            options.UseNpgsql(builder
            .Configuration.GetConnectionString("DefaultConnection"),
             npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));
        builder.Services.AddScoped<ILocalDbContext, LocalDbContext>();
        builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<LocalDbContext>()
            .AddDefaultTokenProviders();
        builder.Services.AddAutoMapper(typeof(MappingProfile));
        #endregion

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication(); 
        app.UseAuthorization(); 
        app.MapControllerRoute(
           name: "default",
           pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.MapControllerRoute(
            name: "about",
            pattern: "about",
            defaults: new { controller = "Home", action = "About" });

        app.Run();
    }
}