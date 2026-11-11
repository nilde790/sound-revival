using System;
using System.Collections.Generic;
using System.Text;

namespace SoundRevival.Dto.Listings
{
    public class ListingFilterRequestDto
    {
        public string? Category {  get; set; }

        public string? Condition { get; set; }

        public decimal? MinPrice { get; set; } 

        public decimal? MaxPrice { get; set; } 

        public string? Search { get; set; } 

        public string Status { get; set; } = "available";
    }
}
