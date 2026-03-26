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
        public int NumberOfGuests { get; set; }
        public decimal Budget { get; set; }
        public int PartyCategoryId { get; set; }
        public DateTime EventDate { get; set; }

        public List<string>? FavoriteDishes { get; set; }
        public List<string>? AllergyDishes { get; set; }
    }
}
