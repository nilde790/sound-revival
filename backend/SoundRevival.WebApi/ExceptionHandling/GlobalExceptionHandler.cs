using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SoundRevival.Infrastructure.Exceptions;

namespace SoundRevival.WebApi.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {

            if(exception is not AppException appException)
            {
                return false;
            }

            var problem = new ProblemDetails
            {
                Status = appException.StatusCode,
                Title = ReasonPhrases.GetReasonPhrase(appException.StatusCode),
                Detail = appException.Message,
            };

            httpContext.Response.StatusCode = appException.StatusCode;

            await httpContext.Response.WriteAsJsonAsync(problem,options:null,"application/problem+json", cancellationToken);

            return true;
        }
    }
}
