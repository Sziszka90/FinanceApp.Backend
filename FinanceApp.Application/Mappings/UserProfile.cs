using AutoMapper;
using FinanceApp.Application.Dtos.UserDtos;

namespace FinanceApp.Application.Mappings;

public class UserProfile : Profile
{
  #region Constructors

  public UserProfile()
  {
    CreateMap<Domain.Entities.User, GetUserDto>();
    CreateMap<UpdateUserDto, Domain.Entities.User>();
    CreateMap<CreateUserDto, Domain.Entities.User>();
  }

  #endregion
}
