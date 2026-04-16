using Application.Common.Interfaces;
using AutoMapper;
using Domain.DTO;
using Domain.Entity.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Object.Commands
{
    public class EditObjectResult
    {
        public bool Succeeded { get; set; }
        public bool NotFound { get; set; }
        public bool InvalidStatus { get; set; }
        public string? Error { get; set; }
    }

    public class EditObjectCommand : ICommand<EditObjectResult>
    {
        public Guid Id { get; set; }
        public AddSocialObjectDto Model { get; set; } = null!;
        public Guid UserId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class EditObjectCommandHandler : IRequestHandler<EditObjectCommand, EditObjectResult>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;
        private readonly ILogger<EditObjectCommandHandler> logger;

        public EditObjectCommandHandler(ILocalDbContext context, IMapper mapper, ILogger<EditObjectCommandHandler> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<EditObjectResult> Handle(EditObjectCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new EditObjectResult();

            var socialObject = await context.SocialObject
                .FirstOrDefaultAsync(x => x.IdObject == request.Id && x.CreatorId == request.UserId, cancellationToken);

            if (socialObject == null)
            {
                viewResult.NotFound = true;
                return viewResult;
            }

            if (socialObject.Status != Status.Rejected)
            {
                viewResult.InvalidStatus = true;
                viewResult.Error = "Редактировать можно только отклоненные объекты";
                return viewResult;
            }

            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                var point = new Point(request.Longitude.Value, request.Latitude.Value)
                {
                    SRID = 4326
                };
                socialObject.Location = point;
            }

            mapper.Map(request.Model, socialObject);

            socialObject.Status = Status.Pending;
            socialObject.EditorId = request.UserId;
            socialObject.EditedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            viewResult.Succeeded = true;

            logger.LogInformation("Пользователь {UserId} отредактировал объект {ObjectId}",
                request.UserId, request.Id);

            return viewResult;
        }
    }
}
