using Microsoft.Extensions.Configuration;

namespace QaFramework.Core.Config;

public static class ConfigLoader
{
    public static T Load<T>(string section) where T : new()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        return config.GetSection(section).Get<T>()
            ?? throw new InvalidOperationException($"Config section '{section}' is missing.");
    }
}