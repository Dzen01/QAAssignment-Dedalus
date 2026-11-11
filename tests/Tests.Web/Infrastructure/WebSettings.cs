using QaFramework.Core.Config;

namespace Tests.Web.Infrastructure;

public sealed class WebSettings
{
    public string BaseUrl { get; set; } = "";
    public string Password { get; set; } = "";

    public static WebSettings Current { get; } = ConfigLoader.Load<WebSettings>("Web");
}