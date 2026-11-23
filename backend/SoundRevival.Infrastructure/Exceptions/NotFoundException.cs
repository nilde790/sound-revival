using System;
using System.Collections.Generic;
using System.Text;

namespace SoundRevival.Infrastructure.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404)
        {

        }
    }
}
