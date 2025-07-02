using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace NoviCode.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception for request {Method} {Path}",
                         context.Request.Method, context.Request.Path);

        var problem = new ProblemDetails
        {
            Type   = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title  = "An internal server error occurred.",
            Status = (int)HttpStatusCode.InternalServerError,
            Instance = context.Request.Path
        };

        if (_env.IsDevelopment())
        {
            problem.Detail = exception.ToString();
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = problem.Status.Value;

        var json = JsonSerializer.Serialize(problem);
        return context.Response.WriteAsync(json);
    }
}