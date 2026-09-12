using Xunit;

namespace DotWebToPdf.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IntegrationFactAttribute : FactAttribute
{
    internal const string EnvironmentVariableName = "DOTWEBTOPDF_RUN_INTEGRATION";

    public IntegrationFactAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable(EnvironmentVariableName), "1", StringComparison.Ordinal))
        {
            Skip = $"Set {EnvironmentVariableName}=1 to run integration tests. Chromium must be installed.";
        }
    }
}
