using BookfetSystem.Services.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Interface
{
    public interface IAIRecommendationService
    {
        Task<string> GetRecommendationAsync(string prompt);
    }
}
