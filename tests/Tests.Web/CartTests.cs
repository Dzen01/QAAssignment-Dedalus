using Tests.Web.Infrastructure;
using Tests.Web.TestData;

namespace Tests.Web;

public class CartTests : WebTestBase
{
    [Test]
    [Category("KnownBug")]
    public async Task ProblemUser_CanAddBoltTShirtToCart()
    {
        var inventory = await LogInAsync(Users.Problem);

        await KnownBug.ExpectFailureAsync("BUG-1: problem_user cannot add some products to the cart", async () =>
        {
            await inventory.AddToCartAsync(Products.BoltTShirt);
            await Expect(inventory.CartBadge).ToHaveTextAsync("1");
        });
    }
}