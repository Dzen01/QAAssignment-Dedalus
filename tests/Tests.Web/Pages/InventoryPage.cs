using Microsoft.Playwright;

namespace Tests.Web.Pages;

public class InventoryPage(IPage page)
{
    // The cart badge is in the header, so it works on every page after login.
    public ILocator CartBadge => page.Locator("[data-test='shopping-cart-badge']");
    private ILocator CartLink => page.Locator("[data-test='shopping-cart-link']");

    public async Task AddToCartAsync(string productName)
    {
        await page.Locator(".inventory_item")
            .Filter(new() { HasText = productName })
            .GetByRole(AriaRole.Button, new() { Name = "Add to cart" })
            .ClickAsync();
    }

    public async Task OpenCartAsync() => await CartLink.ClickAsync();
}