using Tests.Web.Infrastructure;
using Tests.Web.Pages;
using Tests.Web.TestData;

namespace Tests.Web;

public class CartTests : WebTestBase
{
    [Test]
    public async Task AddedItem_AppearsInCart()
    {
        var inventory = await LogInAsync();

        await inventory.AddToCartAsync(Products.Backpack);
        await Expect(inventory.CartBadge).ToHaveTextAsync("1");

        await inventory.OpenCartAsync();
        var cart = new CartPage(Page);
        await Expect(cart.ItemNames).ToHaveTextAsync(new[] { Products.Backpack });
    }
}