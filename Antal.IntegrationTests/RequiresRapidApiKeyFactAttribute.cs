using System;
using Xunit;

namespace Antal.IntegrationTests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiresRapidApiKeyFactAttribute : FactAttribute
{
    public RequiresRapidApiKeyFactAttribute()
    {
        var apiKey = Environment.GetEnvironmentVariable("RAPIDAPI_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Skip = "RAPIDAPI_KEY is not set; skipping external API integration test.";
        }
    }
}
