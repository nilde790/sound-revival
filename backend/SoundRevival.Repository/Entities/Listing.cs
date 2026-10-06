using System;
using System.Collections.Generic;
using System.Text;

namespace SoundRevival.Repository.Entities
{
    public class Listing
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string Status { get; set; } = "available";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Image> Images { get; set; } = new List<Image>();
    }

}
