using Microsoft.Playwright;

namespace Tests.Web.Pages;

public class LoginPage(IPage page)
{
    private ILocator Username => page.Locator("[data-test='username']");
    private ILocator Password => page.Locator("[data-test='password']");
    private ILocator LoginButton => page.Locator("[data-test='login-button']");

    public ILocator Error => page.Locator("[data-test='error']");

    public async Task OpenAsync() => await page.GotoAsync("/");

    public async Task LoginAsync(string username, string password)
    {
        await Username.FillAsync(username);
        await Password.FillAsync(password);
        await LoginButton.ClickAsync();
    }
}