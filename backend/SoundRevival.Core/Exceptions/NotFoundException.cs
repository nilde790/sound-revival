using System;
using System.Collections.Generic;
using System.Text;

namespace SoundRevival.Core.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404)
        {

        }
    }
}
