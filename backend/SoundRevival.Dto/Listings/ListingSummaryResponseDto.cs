using System;
using System.Collections.Generic;
using System.Text;

namespace SoundRevival.Dto.Listings
{
    public class ListingSummaryResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }
}
