using Application.Common.Interfaces;
using Application.Mapping;
using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialNavigator.Helpers;

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

        builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true; // хотя бы одна цифра
            options.Password.RequireLowercase = true; // хотя бы одна строчная буква
            options.Password.RequireNonAlphanumeric = true; // хотя бы один специальный символ
            options.Password.RequireUppercase = true; // хотя бы одна заглавная буква
            options.Password.RequiredLength = 6; // минимальная длина 6 символов

            options.Lockout.MaxFailedAccessAttempts = 8; // максимальное количество неудачных попыток входа до блокировки

            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+"; // символы в имени пользователя 
            options.User.RequireUniqueEmail = true; // уникальный email

            options.SignIn.RequireConfirmedAccount = false; // подтверждение учетной записи
        })
            .AddRoles<IdentityRole<Guid>>()
            .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
            .AddSignInManager<SignInManager<AppUser>>()
            .AddEntityFrameworkStores<LocalDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            //options.Cookie.HttpOnly = true; // куки не могут быть изменены через код страницы
            options.ExpireTimeSpan = TimeSpan.FromMinutes(120); // время действия куки аутентификации
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/accessdenied";
            options.SlidingExpiration = true; // автоматическое продление срока действия куки
        });

        builder.Services.AddAutoMapper(typeof(MappingProfile));


        #endregion

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await SeedData.Initialize(services);
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
           name: "default",
           pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "about",
            pattern: "about",
            defaults: new { controller = "Home", action = "About" });

        app.MapRazorPages();

        app.Run();
    }
}