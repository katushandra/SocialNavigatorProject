using Application.Common.Interfaces;
using AutoMapper;
using Domain.DTO;
using Domain.Entity;
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
    public class AddObjectResult
    {
        public bool Succeeded { get; set; }
        public Guid ObjectId { get; set; }
        public string? Error { get; set; }
    }
    public class AddCommand : ICommand<AddObjectResult>
    {
        public AddSocialObjectDto Model { get; set; } = null!;
        public Guid UserId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
    public class AddObjectCommandHandler : IRequestHandler<AddCommand, AddObjectResult>
    {
        private readonly ILocalDbContext context;
        private readonly IMapper mapper;
        private readonly ILogger<AddObjectCommandHandler> logger;

        public AddObjectCommandHandler(ILocalDbContext context, IMapper mapper, ILogger<AddObjectCommandHandler> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<AddObjectResult> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            var viewResult = new AddObjectResult();

            var objectType = await context.ObjectType
                .FirstOrDefaultAsync(x => x.IdObjectType == request.Model.ObjectTypeId, cancellationToken);

            if (objectType == null)
            {
                viewResult.Succeeded = false;
                viewResult.Error = "Выберите тип объекта";
                return viewResult;
            }

            var socialObject = mapper.Map<SocialObject>(request.Model);

            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                var point = new Point(request.Longitude.Value, request.Latitude.Value)
                {
                    SRID = 4326
                };
                socialObject.Location = point;
            }

            socialObject.CreatorId = request.UserId;
            socialObject.Status = Status.Pending;  
            socialObject.CreatedAt = DateTime.UtcNow;

            context.SocialObject.Add(socialObject);
            await context.SaveChangesAsync(cancellationToken);

            viewResult.Succeeded = true;
            viewResult.ObjectId = socialObject.IdObject;

            logger.LogInformation("Пользователь {UserId} добавил новый объект {ObjectName} id - {ObjectId}",
                request.UserId, request.Model.Name, socialObject.IdObject);

            return viewResult;
        }
    }
}