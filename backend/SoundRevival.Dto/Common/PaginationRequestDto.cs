using System;
using System.Collections.Generic;
using System.Text;

namespace SoundRevival.Dto.Common
{
    public class PaginationRequestDto
    {

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
