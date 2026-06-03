using AdventureWorksAIWorkspace.Application.Reports.GetReportDetails;

namespace AdventureWorksAIWorkspace.Application.Tests.Reports.GetReportDetails;

public sealed class GetReportDetailsQueryValidatorTests
{
    private readonly GetReportDetailsQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenReportIdIsProvided_ShouldPass()
    {
        var result = _validator.Validate(new GetReportDetailsQuery("report-1", "user-1"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenReportIdIsMissing_ShouldFail(string reportId)
    {
        var result = _validator.Validate(new GetReportDetailsQuery(reportId, "user-1"));

        result.IsValid.Should().BeFalse();
    }
}
