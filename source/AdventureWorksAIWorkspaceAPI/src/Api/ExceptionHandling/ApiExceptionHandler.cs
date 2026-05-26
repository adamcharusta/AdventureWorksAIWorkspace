using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AdventureWorksAIWorkspaceAPI.Api.ExceptionHandling;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(exception);
        var statusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        LogException(httpContext, exception, statusCode);

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            NotFoundException notFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Title = "The specified resource was not found.",
                Detail = notFoundException.Message
            },
            ForbiddenException forbiddenException => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
                Title = "Access to the requested resource is forbidden.",
                Detail = forbiddenException.Message
            },
            ValidationException validationException => CreateValidationProblemDetails(validationException),
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Title = "An unexpected error occurred.",
                Detail = "An unexpected error occurred while processing the request."
            }
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Detail = exception.Message
        };
    }

    private void LogException(HttpContext httpContext, Exception exception, int statusCode)
    {
        var logLevel = statusCode >= StatusCodes.Status500InternalServerError
            ? LogLevel.Error
            : LogLevel.Warning;

        logger.Log(
            logLevel,
            exception,
            "Request {Method} {Path} failed with status code {StatusCode}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            statusCode);
    }
}
