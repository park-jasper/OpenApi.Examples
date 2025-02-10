using FluentAssertions;

namespace OpenApi.Examples.Usage.Test;

public class OpenApiDocumentTest
{
    [Fact]
    public async Task GetOpenApiDocument()
    {
        // ARRANGE
        var waf = new OpenApiExamplesUsageWebApplicationFactory();
        var client = waf.CreateClient();

        // ACT
        var openApiResponse = await client.GetAsync("openapi/v1.json", TestContext.Current.CancellationToken);

        // ASSERT
        openApiResponse.Should().BeSuccessful();
        var json = await openApiResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        await Verifier.VerifyJson(json);
    }
}