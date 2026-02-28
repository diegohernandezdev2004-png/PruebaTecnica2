using System.Net;
using System.Text.Json;
using CursosAPI.Domain.DTOs;
using Microsoft.Data.SqlClient;

namespace CursosAPI.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error en el servidor.");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "Error interno del servidor.";

        if (exception is SqlException sqlEx)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = $"Error de base de datos: {sqlEx.Message}";
        }
        else if (exception is ArgumentException || exception is InvalidOperationException)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = exception.Message;
        }

        context.Response.StatusCode = (int)statusCode;
        var result = JsonSerializer.Serialize(ApiResponse<string>.Error(message));
        return context.Response.WriteAsync(result);
    }
}
