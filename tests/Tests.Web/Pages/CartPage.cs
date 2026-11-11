using Microsoft.Playwright;

namespace Tests.Web.Pages;

public class CartPage(IPage page)
{
    public ILocator ItemNames => page.Locator(".cart_item .inventory_item_name");

    public async Task CheckoutAsync() => await page.Locator("[data-test='checkout']").ClickAsync();
}