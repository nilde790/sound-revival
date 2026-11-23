using SoundRevival.Dto.Constants;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SoundRevival.Infrastructure.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = Roles.User;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    }
}
