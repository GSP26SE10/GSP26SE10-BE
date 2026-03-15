using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Mappings
{
    public class UserMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            //filter request -> entity (Status: enum số -> string để query DB)
            config.NewConfig<UserFilterRequest, User>()
                  .IgnoreNullValues(true)
                  .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null);
            config.NewConfig<User, UserResponse>()
                  .Map(dest => dest.RoleName,
                      src => src.Role != null ? src.Role.RoleName : null)
                  .Map(dest => dest.Status,
                      src => EnumHelper.TryParseToInt<UserStatus>(src.Status));
            config.NewConfig<User, LoginResponse>()
                  .Map(dest => dest.RoleName,
                      src => src.Role != null ? src.Role.RoleName : null);
        }
    }
}
