using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoundRevival.Dto.Listings
{
    public class UpdateListingStatusRequestDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

    }
}
