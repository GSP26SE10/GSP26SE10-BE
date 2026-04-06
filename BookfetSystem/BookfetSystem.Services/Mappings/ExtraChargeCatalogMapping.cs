using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Models.Request;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class ExtraChargeCatalogMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ExtraChargeCatalogFilterRequest, ExtraChargeCatalog>()
                .IgnoreNullValues(true)
                .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString().ToUpperInvariant() : null);
        }
    }
}
