using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;
using Tests.Web.Pages;
using Tests.Web.TestData;

namespace Tests.Web.Infrastructure;

[Parallelizable(ParallelScope.Self)]
public abstract class WebTestBase : PageTest
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        BaseURL = WebSettings.Current.BaseUrl,
    };

    [SetUp]
    public async Task StartTracing()
    {
        await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
    }

    [TearDown]
    public async Task StopTracing()
    {
        var test = TestContext.CurrentContext;
        var failed = test.Result.Outcome.Status == TestStatus.Failed;

        string? path = null;
        if (failed)
        {
            var safeName = string.Join("_", test.Test.Name.Split(Path.GetInvalidFileNameChars()));
            path = Path.Combine(test.WorkDirectory, "playwright-traces", $"{safeName}.zip");
        }

        await Context.Tracing.StopAsync(new() { Path = path });

        if (path != null)
            TestContext.AddTestAttachment(path);
    }
    protected async Task<InventoryPage> LogInAsync(string username = Users.Standard)
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();
        await login.LoginAsync(username, WebSettings.Current.Password);
        return new InventoryPage(Page);
    }
}