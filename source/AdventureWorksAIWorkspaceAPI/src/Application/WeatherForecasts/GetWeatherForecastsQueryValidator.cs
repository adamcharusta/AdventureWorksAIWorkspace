using FluentValidation;

namespace AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;

public sealed class GetWeatherForecastsQueryValidator : AbstractValidator<GetWeatherForecastsQuery>
{
    public GetWeatherForecastsQueryValidator()
    {
        RuleFor(query => query.Days)
            .InclusiveBetween(1, 10);
    }
}
