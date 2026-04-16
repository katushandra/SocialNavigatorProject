using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.Internal;
using Domain.DTO;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace Application.Handlers.Admin.Queries
{
    public class AdminUsersResult
    {
        public List<AppUserDto> Users { get; set; } = new();
        public Dictionary<string, string> Roles { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string RoleFilter { get; set; } = "";
        public bool? ActiveFilter { get; set; }
    }
    public class GetAdminUsersQuery : IQuery<AdminUsersResult>
    {
        public string SearchTerm { get; set; } = "";
        public string Role { get; set; } = "";
        public bool? Active { get; set; }
    }
    public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, AdminUsersResult>
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IMapper mapper;

        public GetAdminUsersQueryHandler(UserManager<AppUser> userManager, IMapper mapper)
        {
            this.userManager = userManager;
            this.mapper = mapper;
        }

        public async Task<AdminUsersResult> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
        {
            var query = userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                request.SearchTerm = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.UserName.ToLower().Contains(request.SearchTerm) ||
                    u.Email.ToLower().Contains(request.SearchTerm) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(request.SearchTerm)));
            }

            if (request.Active.HasValue)
            {
                query = query.Where(u => u.Active == request.Active.Value);
            }

            var users = await query
                .OrderByDescending(u => u.UserCreatedAt)
                .ToListAsync(cancellationToken);

            var userList = new List<AppUserDto>();
            foreach (var user in users)
            {
                var role = await userManager.GetRolesAsync(user);
                var userDto = mapper.Map<AppUserDto>(user);
                userDto.Role = role.FirstOrDefault() ?? "User";
                userList.Add(userDto);
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                userList = userList.Where(u => u.Role == request.Role).ToList();
            }

            var roles = new Dictionary<string, string>
            {
                { "Admin", "Администратор" },
                { "Moderator", "Модератор" },
                { "User", "Пользователь" }
            };

            return new AdminUsersResult
            {                
                SearchTerm = request.SearchTerm,
                RoleFilter = request.Role,
                ActiveFilter = request.Active,
                Users = userList,
                Roles = roles,
            };
        }
    }
}