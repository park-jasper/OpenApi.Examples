using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using OpenApi.Examples;
using OpenApi.Examples.Usage;
using PolyType.SourceGenerator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(
    options =>
    {
        options.AddOperationTransformer(async (operation, context, cancellationToken) =>
        {
            if (operation.OperationId == "Weather")
            {
                var ok = operation.Responses["200"];
                ok.Content["application/json"].Examples.Add(
                    "Example Weather Forecast",
                    new OpenApiExample()
                    {
                        Value = OpenApiConverter.Parse(new WeatherForecast(new DateOnly(2025, 02, 09), 2, "Clear")),
                    });
                var error = operation.Responses["400"];
                error.Content["application/problem+json"].Examples.Add(
                    "Example Problem",
                    new OpenApiExample()
                    {
                        Value = OpenApiConverter.Parse(
                            new ProblemDetails()
                            {
                                Title = "Empty parameter",
                                Detail = "city",
                                Status = StatusCodes.Status400BadRequest,
                            },
                            ShapeProvider_OpenApi_Examples_Usage.Default.ProblemDetails)
                    });
            }
        });
    });

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

var api = app.MapGroup("api");
api
    .MapGet(
        "weather/{city}",
        (string city) =>
        {
            if (string.IsNullOrEmpty(city))
            {
                return Results.Problem(new ProblemDetails()
                {
                    Title = "Empty parameter",
                    Detail = nameof(city),
                    Status = StatusCodes.Status400BadRequest,
                });
            }
            return Results.Ok(new WeatherForecast(new DateOnly(2025, 02, 10), 4, "cloudy"));
        })
    .WithName("Weather")
    .Produces<WeatherForecast>()
    .ProducesProblem(StatusCodes.Status400BadRequest);

app.MapOpenApi();

app.Run();
