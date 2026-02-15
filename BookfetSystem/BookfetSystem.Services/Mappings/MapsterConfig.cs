using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Mappings
{
    public static class MapsterConfig
    {
        public static void RegisterMappings()
        {
            var config = TypeAdapterConfig.GlobalSettings;

            // Scan toàn bộ assembly để lấy tất cả class IRegister
            config.Scan(Assembly.GetExecutingAssembly());
        }
    }
}
