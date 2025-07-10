using AutoMapper;
using JwtAuthDemo.Model.Entity;
using JwtAuthDemo.Model.DTO;
namespace JwtAuthDemo.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserRegistrationDto, AppUser>()
             .ForMember(dest => dest.Passwordhash, opt => opt.Ignore());
            CreateMap<AppUser, UserDto>();
        }
    }
}
