using AutoMapper;
using Domain.DTO;
using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    public class MappingProfile:Profile
    {
        public MappingProfile() 
        {
            #region AddSocialObjectDto
            CreateMap<AddSocialObjectDto, SocialObject>()
                .ForMember(x => x.IdObject, opt => opt.Ignore())
                .ForMember(x => x.CreatorId, opt => opt.Ignore())
                .ForMember(x => x.CreatedAt, opt => opt.Ignore())
                .ForMember(x => x.Status, opt => opt.Ignore())
                .ForMember(x => x.EditorId, opt => opt.Ignore())
                .ForMember(x => x.EditedAt, opt => opt.Ignore())
                .ForMember(x => x.ScoreObject, opt => opt.Ignore())
                .ForMember(x => x.ObjectType, opt => opt.Ignore())
                .ForMember(x => x.Creator, opt => opt.Ignore())
                .ForMember(x => x.Editor, opt => opt.Ignore())
                .ForMember(x => x.Reviews, opt => opt.Ignore())
                .ForMember(x => x.ModerationHistories, opt => opt.Ignore());
            #endregion

            #region FullSocialObjectDto
            CreateMap<SocialObject, FullSocialObjectDto>()
                .ForMember(x => x.SocialObjectDto, opt => opt.MapFrom(src => src))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.Reviews, opt => opt.MapFrom(src => src.Reviews));
            #endregion

            #region AppUser 
            CreateMap<AppUser, AppUserDto>()
                .ForMember(x => x.IdUser, opt => opt.MapFrom(src => src.Id));
            #endregion

            #region ObjectTypeDto
            CreateMap<ObjectType, ObjectTypeDto>()
                .ForMember(x => x.NameObjectType, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.IdObjectType, opt => opt.MapFrom(src => src.IdObjectType));
            #endregion

            #region ReviewDto
            CreateMap<Review, ReviewDto>()
                .ForMember(x => x.IdReview, opt => opt.MapFrom(src => src.IdReview))
                .ForMember(x => x.AppUser, opt => opt.MapFrom(src => src.User));
            #endregion

            #region SocialObjectDto
            CreateMap<SocialObject, SocialObjectDto>();
            CreateMap<SocialObject, AddSocialObjectDto>();
            #endregion

            #region MyObjectDto
            CreateMap<SocialObject, MyObjectDto>()
                .ForMember(x => x.Status, opt => opt.MapFrom(src => src.Status));
            #endregion

            #region  ModerationSocialObjectDto
            CreateMap<SocialObject, ModerationSocialObjectDto>()
                .ForMember(x => x.ObjectType, opt => opt.MapFrom(src => src.ObjectType))
                .ForMember(x => x.Status, opt => opt.MapFrom(src => src.Status)) 
                .ForMember(x => x.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? (src.Creator.FullName ?? src.Creator.UserName) : " "))
                .ForMember(x => x.CreatorEmail, opt => opt.MapFrom(src => src.Creator.Email))
                .ForMember(x => x.ReviewsCount, opt => opt.MapFrom(src => src.Reviews.Count));
            #endregion

            #region  ModerationHistoryDto
            CreateMap<ModerationHistory, ModerationHistoryDto>()
                .ForMember(x => x.ObjectName, opt => opt.MapFrom(src => src.Object.Name))
                .ForMember(x => x.ModeratorName, opt => opt.MapFrom(src => src.Moderator != null ? (src.Moderator.FullName ?? src.Moderator.UserName) : " "));
            #endregion
        }
    }
}
