using Application.Models;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();

            // You can add custom mappings here if needed
            // For example, if property names don't match:
            // CreateMap<User, UserDto>()
            //     .ForMember(dest => dest.SomeProperty, opt => opt.MapFrom(src => src.OtherProperty));
        }
    }
}
