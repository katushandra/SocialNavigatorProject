using Domain.Entity;
using Microsoft.AspNetCore.Identity;


namespace SocialNavigator.Helpers
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                await CreateRoles(roleManager);
                await CreateUsers(userManager);
            }
        }

        private static async Task CreateRoles(RoleManager<IdentityRole<Guid>> roleManager)
        {
            string[] role = { "Admin", "Moderator", "User" };

            foreach (var roleName in role)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    });
                }
            }
        }
        private static async Task CreateUsers(UserManager<AppUser> userManager)
        {
            await CreateUserIfNotExists(userManager, new AppUser
            {
                FullName = "Системный администратор",
                UserCreatedAt = DateTime.UtcNow,
                UserEditedAt = DateTime.UtcNow,
                Active = true,
                UserName = "admin",
                Email = "admin@mail.ru",
                EmailConfirmed = true
            }, "Admin123!", new[] { "Admin", "Moderator" });

            await CreateUserIfNotExists(userManager, new AppUser
            {
                FullName = "Модератор",
                UserCreatedAt = DateTime.UtcNow,
                UserEditedAt = DateTime.UtcNow,
                Active = true,
                UserName = "moderator",
                Email = "moderator@mail.ru",
                EmailConfirmed = true
            }, "Moder123!", new[] { "Moderator" });
        }

        private static async Task CreateUserIfNotExists(
            UserManager<AppUser> userManager,
            AppUser user,
            string password,
            string[] roles)
        {
            var existingUser = await userManager.FindByEmailAsync(user.Email);
            if (existingUser == null)
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, roles);
                }
            }
        }
    }
}