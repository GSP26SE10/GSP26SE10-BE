using BookfetSystem.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookfetSystem.Services.Models.Request
{
    public class MenuSuggestionRequest
    {
        public int GuestCount { get; set; }
        public string PartyType { get; set; }
        public decimal Budget { get; set; }

        public List<string> FavoriteDishes { get; set; } = new();
        public List<string> Allergies { get; set; } = new();
    }
}
