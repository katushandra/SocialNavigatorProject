using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.DTO;
using Domain.DTO.Admin;
using Domain.Entity;
using Domain.Entity.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SocialNavigator.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<AppUser> userManager;
        private readonly ILogger<AdminController> logger;

        public AdminController(ILocalDbContext context, IMapper mapper, UserManager<AppUser> userManager, ILogger<AdminController> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
            this.logger = logger;
        }

        #region AdminPanel Панель администратора
        [HttpGet]
        public async Task<IActionResult> AdminPanel(CancellationToken cancellationToken)
        {
            var statistics = await GetStatistics(cancellationToken);
            return View(statistics);
        }

        private async Task<AdminPanelDto> GetStatistics(CancellationToken cancellationToken)
        {
            var totalObjects = await context.SocialObject.CountAsync(cancellationToken);

            var totalApprovedObjects = await context.SocialObject
                .CountAsync(x => x.Status == Status.Approved, cancellationToken);

            var totalUsers = await userManager.Users.CountAsync(cancellationToken);

            var averageRating = await context.Review
                .AverageAsync(x => (double?)x.Score, cancellationToken) ?? 0;

            var objectsByType = await context.SocialObject
                .Include(x => x.ObjectType)
                .Where(x => x.Status == Status.Approved)
                .GroupBy(x => x.ObjectType.Name)
                .Select(g => new TypeStatisticDto
                {
                    TypeName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync(cancellationToken);

            foreach (var type in objectsByType)
            {
                if (totalApprovedObjects > 0)
                {
                    type.Percentage = Math.Round((double)type.Count / totalApprovedObjects * 100, 1);
                }
                else
                {
                    type.Percentage = 0;
                }
            }

            var topUsers = await context.SocialObject
              .Include(x => x.Creator)
              .Where(x => x.Creator != null)
              .GroupBy(x => new { x.Creator.Id, x.Creator.UserName, x.Creator.FullName })
              .Select(g => new UserActivityDto
              {
                  UserId = g.Key.Id,
                  UserName = g.Key.UserName,
                  FullName = g.Key.FullName,
                  ObjectsCount = g.Count(),
                  ReviewsCount = context.Review.Count(r => r.UserId == g.Key.Id)
              })
              .OrderByDescending(u => u.ObjectsCount)
              .Take(10)
              .ToListAsync(cancellationToken);

            return new AdminPanelDto
            {
                TotalObjects = totalObjects,
                TotalApprovedObjects = totalApprovedObjects,
                TotalUsers = totalUsers,
                AverageRating = Math.Round((decimal)averageRating, 1),
                ObjectsByType = objectsByType,
                TopActiveUsers = topUsers
            };
        }
        #endregion        

        #region AdminAllObjects Управление объектами
        [HttpGet]
        public async Task<IActionResult> AdminAllObjects(string searchTerm = "", string status = "", string type = "", CancellationToken cancellationToken = default)
        {
            var query = context.SocialObject
                .Include(x => x.ObjectType)
                .Include(x => x.Creator)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(searchTerm) ||
                    x.Address.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                if (Enum.TryParse<Status>(status, true, out var statusEnum))
                {
                    query = query.Where(x => x.Status == statusEnum);
                }
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(x => x.ObjectType.Name == type);
            }

            var objects = await query
                .OrderByDescending(x => x.CreatedAt)
                .ProjectTo<ModerationSocialObjectDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            var objectTypes = await context.ObjectType
                .Select(x => x.Name)
                .ToListAsync(cancellationToken);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = status;
            ViewBag.TypeFilter = type;
            ViewBag.ObjectTypes = objectTypes;

            return View(objects);
        }

        // Удаление объекта
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteObject(Guid id)
        {
            var socialObject = await context.SocialObject
                .FirstOrDefaultAsync(x => x.IdObject == id);

            if (socialObject == null)
            {
                return NotFound();
            }

            context.SocialObject.Remove(socialObject);
            await context.SaveChangesAsync();

            logger.LogInformation("Администратор удалил объект {ObjectId}", id);
            TempData["SuccessMessage"] = "Объект успешно удален";

            return RedirectToAction(nameof(AdminAllObjects));
        }
        #endregion

        #region AdminModerationHistory История модерации
        [HttpGet]
        public async Task<IActionResult> AdminModerationHistory(string searchTerm = "", CancellationToken cancellationToken = default)
        {
            var query = context.ModerationHistory
                .Include(x => x.Object)
                .Include(x => x.Moderator)
                .OrderByDescending(x => x.ModeratedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    x.Object.Name.ToLower().Contains(searchTerm) ||
                    (x.Comment != null && x.Comment.ToLower().Contains(searchTerm)));
            }

            var history = await query

                .ProjectTo<ModerationHistoryDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            ViewBag.SearchTerm = searchTerm;

            return View(history);
        }
        #endregion

        #region AdminUsers Управление пользователями
        [HttpGet]
        public async Task<IActionResult> AdminUsers(string searchTerm = "", string role = "", bool? active = null, CancellationToken cancellationToken = default)
        {
            var query = userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    u.UserName.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(searchTerm)));
            }

            if (active.HasValue)
            {
                query = query.Where(u => u.Active == active.Value);
            }

            var users = await query
                .OrderByDescending(u => u.UserCreatedAt)
                .ToListAsync(cancellationToken);

            var userList = new List<AppUserDto>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                var userDto = mapper.Map<AppUserDto>(user);
                userDto.Role = roles.FirstOrDefault() ?? "User";
                userList.Add(userDto);
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                userList = userList.Where(u => u.Role == role).ToList();
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = role;
            ViewBag.ActiveFilter = active;
            ViewBag.Roles = new Dictionary<string, string>
            {
                { "Admin", "Администратор" },
                { "Moderator", "Модератор" },
                { "User", "Пользователь" }
            };

            return View(userList);
        }

        // Изменение статуса пользователя
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserStatus(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }

            user.Active = !user.Active;
            user.UserEditedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                logger.LogInformation("Администратор изменил статус пользователя {UserName} на {Active}", user.UserName, user.Active);

                if (!user.Active)
                {
                    await userManager.UpdateSecurityStampAsync(user);
                }

                if (user.Active)
                {
                    TempData["SuccessMessage"] = "Пользователь активирован";
                }
                else
                {
                    TempData["SuccessMessage"] = "Пользователь деактивирован";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при изменении статуса";
            }

            return RedirectToAction(nameof(AdminUsers));
        }

        // Изменение роли пользователя
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(Guid userId, string newRole)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await userManager.AddToRoleAsync(user, newRole);

            if (result.Succeeded)
            {
                user.UserEditedAt = DateTime.UtcNow;
                await userManager.UpdateAsync(user);

                logger.LogInformation("Администратор изменил роль пользователя {UserName} на {Role}", user.UserName, newRole);
                TempData["SuccessMessage"] = "Роль пользователя изменена";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при изменении роли";
            }

            return RedirectToAction(nameof(AdminUsers));
        }
        #endregion
    }
}