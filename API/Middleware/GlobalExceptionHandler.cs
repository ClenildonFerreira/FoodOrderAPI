using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderAPI.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path,
            Extensions = { ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier }
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Erro de validação";
            problemDetails.Detail = "Um ou mais erros de validação ocorreram.";
            
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }
        else
        {
            (problemDetails.Status, problemDetails.Title, problemDetails.Detail) = exception switch
            {
                ArgumentException argEx => (StatusCodes.Status400BadRequest, "Argumento inválido", argEx.Message),
                InvalidOperationException invEx => (StatusCodes.Status400BadRequest, "Operação inválida", invEx.Message),
                KeyNotFoundException keyEx => (StatusCodes.Status404NotFound, "Recurso não encontrado", keyEx.Message),
                _ => (StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro interno no servidor.")
            };
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);

        return true; 
    }
}