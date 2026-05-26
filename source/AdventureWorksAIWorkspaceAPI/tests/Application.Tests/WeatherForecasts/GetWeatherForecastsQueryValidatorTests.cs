using AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.WeatherForecasts;

public sealed class GetWeatherForecastsQueryValidatorTests
{
    private readonly GetWeatherForecastsQueryValidator validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Validate_WhenDaysIsInAllowedRange_ShouldBeValid(int days)
    {
        var result = validator.Validate(new GetWeatherForecastsQuery(days));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Validate_WhenDaysIsOutsideAllowedRange_ShouldBeInvalid(int days)
    {
        var result = validator.Validate(new GetWeatherForecastsQuery(days));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(GetWeatherForecastsQuery.Days));
    }
}
