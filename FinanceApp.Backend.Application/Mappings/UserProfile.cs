using AutoMapper;
using FinanceApp.Backend.Application.Dtos.UserDtos;

namespace FinanceApp.Backend.Application.Mappings;

public class UserProfile : Profile
{
  public UserProfile()
  {
    CreateMap<Domain.Entities.User, GetUserDto>();
    CreateMap<UpdateUserRequest, Domain.Entities.User>();
    CreateMap<CreateUserDto, Domain.Entities.User>();
  }
}
