using System.Security.Claims;
using AdventureWorksAIWorkspace.Api.Authentication;
using AdventureWorksAIWorkspace.Api.Endpoints;
using AdventureWorksAIWorkspace.Application.Auth.Login;
using AdventureWorksAIWorkspace.Application.Auth.Refresh;
using AdventureWorksAIWorkspace.Application.Auth.SetFirstPassword;
using AdventureWorksAIWorkspace.Application.Reports;
using AdventureWorksAIWorkspace.Application.Reports.AddReportMessage;
using AdventureWorksAIWorkspace.Application.Reports.CreateReport;
using AdventureWorksAIWorkspace.Application.Reports.DeleteReport;
using AdventureWorksAIWorkspace.Application.Reports.GetReportDetails;
using AdventureWorksAIWorkspace.Application.Reports.GetReports;
using AdventureWorksAIWorkspace.Application.Reports.RenameReport;
using AdventureWorksAIWorkspace.Application.User.CreateUser;
using AdventureWorksAIWorkspace.Application.User.DeleteUser;
using AdventureWorksAIWorkspace.Application.User.GetAssignableRoles;
using AdventureWorksAIWorkspace.Application.User.GetUsers;
using AdventureWorksAIWorkspace.Application.User.UpdateUser;
using AdventureWorksAIWorkspace.Domain.Reports;
using Microsoft.AspNetCore.Http;
using Wolverine;

namespace AdventureWorksAIWorkspace.Functional.Tests.Endpoints;

public sealed class EndpointMappingTests
{
    [Fact]
    public async Task Login_ShouldInvokeMessageBusWithSuppliedCommand()
    {
        var command = new LoginCommand("admin@example.com", "Password123!");
        var response = AuthTokens<LoginResponse>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<LoginResponse>(command, Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await AuthEndpoints.Login(command, messageBus, CancellationToken.None);

        result.Value.Should().Be(response);
        await messageBus.Received(1).InvokeAsync<LoginResponse>(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetFirstPassword_ShouldInvokeMessageBusWithSuppliedCommand()
    {
        var command = new SetFirstPasswordCommand("new-user", "Password123!", "Password123!");
        var response = AuthTokens<SetFirstPasswordResponse>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<SetFirstPasswordResponse>(command, Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await AuthEndpoints.SetFirstPassword(command, messageBus, CancellationToken.None);

        result.Value.Should().Be(response);
        await messageBus
            .Received(1)
            .InvokeAsync<SetFirstPasswordResponse>(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Refresh_ShouldInvokeMessageBusWithSuppliedCommand()
    {
        var command = new RefreshCommand("refresh-token");
        var response = AuthTokens<RefreshResponse>();
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<RefreshResponse>(command, Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await AuthEndpoints.Refresh(command, messageBus, CancellationToken.None);

        result.Value.Should().Be(response);
        await messageBus.Received(1).InvokeAsync<RefreshResponse>(command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetReports_ShouldUseCurrentUserIdFromSubjectClaim()
    {
        var response = new GetReportsResponse([ReportSummary()]);
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<GetReportsResponse>(
                Arg.Is<GetReportsQuery>(query => query.CurrentUserId == "user-1"),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await ReportsEndpoint.GetReports(
            HttpContextWithSubject("user-1"),
            messageBus,
            CancellationToken.None);

        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task GetReportDetails_ShouldPassReportIdAndCurrentUserId()
    {
        var response = ReportDetails("report-1");
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<ReportDetailsDto>(
                Arg.Is<GetReportDetailsQuery>(query =>
                    query.ReportId == "report-1" && query.CurrentUserId == "user-1"),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await ReportsEndpoint.GetReportDetails(
            "report-1",
            HttpContextWithSubject("user-1"),
            messageBus,
            CancellationToken.None);

        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task CreateReport_ShouldConvertRequestToCommandWithCurrentUserId()
    {
        var response = ReportChatResponse("report-1");
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<ReportChatResponse>(
                Arg.Is<CreateReportCommand>(command =>
                    command.Message == "Show sales by category" && command.CurrentUserId == "user-1"),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await ReportsEndpoint.CreateReport(
            new CreateReportRequest("Show sales by category"),
            HttpContextWithSubject("user-1"),
            messageBus,
            CancellationToken.None);

        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task AddReportMessage_ShouldConvertRequestToCommandWithReportIdAndCurrentUserId()
    {
        var response = ReportChatResponse("report-1");
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<ReportChatResponse>(
                Arg.Is<AddReportMessageCommand>(command =>
                    command.ReportId == "report-1" &&
                    command.Message == "Add margin" &&
                    command.CurrentUserId == "user-1"),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await ReportsEndpoint.AddReportMessage(
            "report-1",
            new AddReportMessageRequest("Add margin"),
            HttpContextWithSubject("user-1"),
            messageBus,
            CancellationToken.None);

        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task RenameReport_ShouldConvertRequestToCommandWithReportIdAndCurrentUserId()
    {
        var response = ReportSummary("report-1");
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<ReportSummaryDto>(
                Arg.Is<RenameReportCommand>(command =>
                    command.ReportId == "report-1" &&
                    command.Title == "Updated report" &&
                    command.CurrentUserId == "user-1"),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await ReportsEndpoint.RenameReport(
            "report-1",
            new RenameReportRequest("Updated report"),
            HttpContextWithSubject("user-1"),
            messageBus,
            CancellationToken.None);

        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task DeleteReport_ShouldReturnNoContentAfterCommandCompletes()
    {
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<DeleteReportResponse>(
                Arg.Is<DeleteReportCommand>(command =>
                    command.ReportId == "report-1" && command.CurrentUserId == "user-1"),
                Arg.Any<CancellationToken>())
            .Returns(new DeleteReportResponse("report-1"));

        var result = await ReportsEndpoint.DeleteReport(
            "report-1",
            HttpContextWithSubject("user-1"),
            messageBus,
            CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAssignableRoles_ShouldInvokeMessageBusWithQuery()
    {
        var response = new GetAssignableRolesResponse(["Admin", "User"]);
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<GetAssignableRolesResponse>(
                Arg.Any<GetAssignableRolesQuery>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await UserEndpoints.GetAssignableRoles(messageBus, CancellationToken.None);

        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task GetUsers_ShouldInvokeMessageBusWithQuery()
    {
        var response = new GetUsersResponse([]);
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<GetUsersResponse>(Arg.Any<GetUsersQuery>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await UserEndpoints.GetUsers(messageBus, CancellationToken.None);

        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task CreateUser_ShouldInvokeMessageBusWithSuppliedCommand()
    {
        var command = new CreateUserCommand("analyst", "analyst@example.com", "User");
        var response = new CreateUserResponse("user-1", "analyst", "analyst@example.com", "User");
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<CreateUserResponse>(command, Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await UserEndpoints.CreateUser(command, messageBus, CancellationToken.None);

        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task UpdateUser_ShouldPreferRouteUserIdOverBodyUserId()
    {
        var command = new UpdateUserCommand("body-user", "analyst", "analyst@example.com", "User");
        var response = new UpdateUserResponse("route-user", "analyst", "analyst@example.com", "User");
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync<UpdateUserResponse>(
                Arg.Is<UpdateUserCommand>(sent => sent.UserId == "route-user"),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await UserEndpoints.UpdateUser(
            "route-user",
            command,
            messageBus,
            CancellationToken.None);

        result.Value.Should().Be(response);
    }

    [Fact]
    public async Task DeleteUser_ShouldUseNameIdentifierWhenSubjectClaimIsMissing()
    {
        var messageBus = Substitute.For<IMessageBus>();
        messageBus
            .InvokeAsync(
                Arg.Is<DeleteUserCommand>(command =>
                    command.UserId == "deleted-user" && command.CurrentUserId == "admin-user"),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await UserEndpoints.DeleteUser(
            "deleted-user",
            HttpContextWithNameIdentifier("admin-user"),
            messageBus,
            CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetCurrentUserId_WhenNoKnownClaimExists_ShouldReturnNull()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        httpContext.GetCurrentUserId().Should().BeNull();
    }

    private static TResponse AuthTokens<TResponse>() =>
        (TResponse)(object)(typeof(TResponse).Name switch
        {
            nameof(LoginResponse) => new LoginResponse(
                "access-token",
                DateTime.UtcNow.AddMinutes(15),
                "refresh-token",
                DateTime.UtcNow.AddDays(7)),
            nameof(SetFirstPasswordResponse) => new SetFirstPasswordResponse(
                "access-token",
                DateTime.UtcNow.AddMinutes(15),
                "refresh-token",
                DateTime.UtcNow.AddDays(7)),
            nameof(RefreshResponse) => new RefreshResponse(
                "access-token",
                DateTime.UtcNow.AddMinutes(15),
                "refresh-token",
                DateTime.UtcNow.AddDays(7)),
            _ => throw new InvalidOperationException($"Unexpected response type {typeof(TResponse).Name}.")
        });

    private static DefaultHttpContext HttpContextWithSubject(string userId) =>
        HttpContextWithClaim("sub", userId);

    private static DefaultHttpContext HttpContextWithNameIdentifier(string userId) =>
        HttpContextWithClaim(ClaimTypes.NameIdentifier, userId);

    private static DefaultHttpContext HttpContextWithClaim(string claimType, string value) =>
        new()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(claimType, value)], "Test"))
        };

    private static ReportSummaryDto ReportSummary(string id = "report-1") =>
        new(id, "Sales report", ReportStatus.Ready, false, DateTime.UtcNow, DateTime.UtcNow);

    private static ReportDetailsDto ReportDetails(string id) =>
        new(
            id,
            "Sales report",
            "Show sales",
            "Sales are stable.",
            null,
            ReportStatus.Ready,
            false,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            [],
            [],
            [],
            []);

    private static ReportChatResponse ReportChatResponse(string reportId)
    {
        var report = ReportDetails(reportId);
        var userMessage = new ReportMessageDto(
            "message-user",
            ReportMessageRole.User,
            "Show sales",
            1,
            null,
            DateTime.UtcNow);
        var assistantMessage = new ReportMessageDto(
            "message-assistant",
            ReportMessageRole.Assistant,
            "Sales are stable.",
            2,
            null,
            DateTime.UtcNow);

        return new ReportChatResponse(
            report,
            userMessage,
            assistantMessage,
            null,
            ReportOutcome.Executed,
            "Sales are stable.",
            null,
            []);
    }
}
