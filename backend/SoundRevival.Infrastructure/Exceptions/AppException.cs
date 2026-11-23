using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace SoundRevival.Infrastructure.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode {  get; }

        protected AppException(string message, int statusCode) : base(message) 
        {
            StatusCode = statusCode;         
        }


    }
}
