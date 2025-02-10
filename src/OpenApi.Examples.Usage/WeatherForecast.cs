using Microsoft.AspNetCore.Mvc;
using PolyType;

namespace OpenApi.Examples.Usage;

[GenerateShape<ProblemDetails>]
[GenerateShape]
public partial record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(this.TemperatureC / 0.55556);
}