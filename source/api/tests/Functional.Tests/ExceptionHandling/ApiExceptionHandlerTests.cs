using AdventureWorksAIWorkspace.Api.ExceptionHandling;
using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdventureWorksAIWorkspace.Functional.Tests.ExceptionHandling;

public sealed class ApiExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WhenClientAbortedRequest_ShouldSetClientClosedRequestStatus()
    {
        using var requestAborted = new CancellationTokenSource();
        await requestAborted.CancelAsync();

        var httpContext = new DefaultHttpContext
        {
            RequestAborted = requestAborted.Token
        };
        var problemDetailsService = new CapturingProblemDetailsService();
        var handler = new ApiExceptionHandler(
            Substitute.For<ILogger<ApiExceptionHandler>>(),
            problemDetailsService);

        bool handled = await handler.TryHandleAsync(
            httpContext,
            new OperationCanceledException(requestAborted.Token),
            CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(499);
        problemDetailsService.Context.Should().BeNull();
    }

    [Theory]
    [MemberData(nameof(ExceptionCases))]
    public async Task TryHandleAsync_ShouldMapKnownExceptionsToProblemDetails(
        Exception exception,
        int expectedStatusCode,
        string expectedTitle)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/reports/report-1";
        var problemDetailsService = new CapturingProblemDetailsService();
        var handler = new ApiExceptionHandler(
            Substitute.For<ILogger<ApiExceptionHandler>>(),
            problemDetailsService);

        bool handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(expectedStatusCode);
        problemDetailsService.Context.Should().NotBeNull();
        problemDetailsService.Context!.ProblemDetails.Status.Should().Be(expectedStatusCode);
        problemDetailsService.Context.ProblemDetails.Title.Should().Be(expectedTitle);
        problemDetailsService.Context.Exception.Should().Be(exception);
    }

    [Fact]
    public async Task TryHandleAsync_WhenValidationFails_ShouldWriteValidationProblemDetails()
    {
        var exception = new ValidationException(
            [
                new ValidationFailure("Title", "Title is required."),
                new ValidationFailure("Title", "Title is too long."),
                new ValidationFailure("Message", "Message is required.")
            ]);
        var httpContext = new DefaultHttpContext();
        var problemDetailsService = new CapturingProblemDetailsService();
        var handler = new ApiExceptionHandler(
            Substitute.For<ILogger<ApiExceptionHandler>>(),
            problemDetailsService);

        bool handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        handled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var validationProblem = problemDetailsService.Context!.ProblemDetails
            .Should()
            .BeOfType<ValidationProblemDetails>()
            .Subject;
        validationProblem.Errors["Title"].Should().Equal("Title is required.", "Title is too long.");
        validationProblem.Errors["Message"].Should().Equal("Message is required.");
    }

    public static TheoryData<Exception, int, string> ExceptionCases() =>
        new()
        {
            {
                new NotFoundException("Report was not found."),
                StatusCodes.Status404NotFound,
                "The specified resource was not found."
            },
            {
                new ForbiddenException("Access denied."),
                StatusCodes.Status403Forbidden,
                "Access to the requested resource is forbidden."
            },
            {
                new UnauthorizedException("Sign in first."),
                StatusCodes.Status401Unauthorized,
                "Authentication is required to access the resource."
            },
            {
                new InvalidOperationException("Unexpected failure."),
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred."
            }
        };

    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext? Context { get; private set; }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Context = context;
            return ValueTask.FromResult(true);
        }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Context = context;
            return ValueTask.CompletedTask;
        }
    }
}
