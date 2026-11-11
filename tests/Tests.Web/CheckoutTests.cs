using System.Text.RegularExpressions;
using Tests.Web.Infrastructure;
using Tests.Web.Pages;
using Tests.Web.TestData;

namespace Tests.Web;

public class CheckoutTests : WebTestBase
{
    [Test]
    public async Task Checkout_WithValidInfo_CompletesOrder()
    {
        var inventory = await LogInAsync();
        await inventory.AddToCartAsync(Products.Backpack);
        await inventory.AddToCartAsync(Products.BikeLight);
        await Expect(inventory.CartBadge).ToHaveTextAsync("2");
        await inventory.OpenCartAsync();

        var cart = new CartPage(Page);
        await Expect(cart.ItemNames).ToHaveTextAsync(new[] { Products.Backpack, Products.BikeLight });
        await cart.CheckoutAsync();

        var checkout = new CheckoutPage(Page);
        await checkout.SubmitInfoAsync(CheckoutInfo.Valid);

        await Expect(checkout.ItemTotal).ToHaveTextAsync("Item total: $39.98");
        await checkout.FinishAsync();

        await Expect(checkout.CompleteHeader).ToHaveTextAsync("Thank you for your order!");
        await Expect(inventory.CartBadge).ToBeHiddenAsync();
    }

    [Test]
    public async Task Checkout_WithoutPostalCode_ShowsErrorAndStays()
    {
        var inventory = await LogInAsync();
        await inventory.AddToCartAsync(Products.Backpack);
        await inventory.OpenCartAsync();
        await new CartPage(Page).CheckoutAsync();

        var checkout = new CheckoutPage(Page);
        await checkout.SubmitInfoAsync(CheckoutInfo.Valid with { PostalCode = "" });

        await Expect(checkout.Error).ToContainTextAsync("Postal Code is required");
        await Expect(Page).ToHaveURLAsync(new Regex(@"/checkout-step-one\.html$"));
    }
}