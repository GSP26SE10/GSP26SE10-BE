using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class IngredientMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<IngredientFilterRequest, Ingredient>()
                  .IgnoreNullValues(true);

            config.NewConfig<Ingredient, IngredientResponse>();
        }
    }
}
