using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StrideGuard.Domain.Exceptions;

namespace StrideGuard.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Violação de Regra de Negócio"),
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso Não Encontrado"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Acesso Não Autorizado"),
            _ => (StatusCodes.Status500InternalServerError, "Erro Interno do Servidor")
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
