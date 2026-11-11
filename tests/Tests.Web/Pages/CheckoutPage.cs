using Microsoft.Playwright;
using Tests.Web.TestData;

namespace Tests.Web.Pages;

// Checkout is three short screens in one flow, so they share one page object.
public class CheckoutPage(IPage page)
{
    // Step one: your information
    public ILocator Error => page.Locator("[data-test='error']");

    public async Task SubmitInfoAsync(CheckoutInfo info)
    {
        await page.Locator("[data-test='firstName']").FillAsync(info.FirstName);
        await page.Locator("[data-test='lastName']").FillAsync(info.LastName);
        await page.Locator("[data-test='postalCode']").FillAsync(info.PostalCode);
        await page.Locator("[data-test='continue']").ClickAsync();
    }

    // Step two: overview
    public ILocator ItemTotal => page.Locator("[data-test='subtotal-label']");

    public async Task FinishAsync() => await page.Locator("[data-test='finish']").ClickAsync();

    // Complete
    public ILocator CompleteHeader => page.Locator("[data-test='complete-header']");
}