﻿using AutoMapper;
using EG.IdentityManagement.Microservice.Entities.Identity;
using EG.IdentityManagement.Microservice.Entities.ViewModels;
using EG.IdentityManagement.Microservice.Identity;
using System;

namespace EG.IdentityManagement.Microservice.Entities.Mapper
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<IdentityViewModel, User>()
                .ForMember(dest => dest.Password, act => act.MapFrom(src => src.Password))
                .ForMember(dest => dest.Id, act => act.MapFrom(src => Guid.NewGuid()))
                .ReverseMap();

            CreateMap<RoleViewModel, Role>()
                .ForMember(dest => dest.Name, act => act.MapFrom(src => src.roleName))
                .ForMember(dest => dest.Id, act => act.MapFrom(src => Guid.NewGuid()))
                .ReverseMap();
        }
    }
}