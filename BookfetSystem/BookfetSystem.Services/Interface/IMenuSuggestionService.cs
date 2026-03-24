using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IMenuSuggestionService
    {
        Task<MenuSuggestionResponse> SuggestMenu(MenuSuggestionRequest request);
    }
}
