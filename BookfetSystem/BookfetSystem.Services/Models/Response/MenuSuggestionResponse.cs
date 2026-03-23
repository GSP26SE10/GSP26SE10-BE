using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Models.Response
{
    public class MenuSuggestionResponse
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public List<string> RecommendedDishes { get; set; }
        public List<string> ExtraDishes { get; set; }
        public string Reason { get; set; }
    }
}
