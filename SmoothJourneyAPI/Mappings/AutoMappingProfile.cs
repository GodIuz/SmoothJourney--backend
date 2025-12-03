using AutoMapper;
using SmoothJourneyAPI.Models;
using SmoothJourneyAPI.Dtos;

namespace SmoothJourneyAPI.Mappings
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<Users, UserResponseDto>();
            CreateMap<RegisterDto, Users>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());
            CreateMap<UpdateUserDto, Users>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
