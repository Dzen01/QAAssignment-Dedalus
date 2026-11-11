# Known failures: `KnownBug.ExpectFailureAsync`

The application under test has real defects. The assignment asks that a test which exposes one is **kept**, **marked as a known failure** and **explained**. This document describes how that is done in this suite and why.

## The problem

A test that exposes a known defect has two bad default outcomes:

- **Left failing:** the run is red every time. People get used to a red run and stop looking at failures, including new ones.
- **Ignored with `[Ignore]`:** the test never runs. It no longer exposes the defect, and nobody notices when the defect is fixed.

What is needed is a test that still runs, reports the defect clearly, keeps the run green, and complains once the defect is gone. This is known as a **strict expected failure** (Playwright Test: `test.fail()`, pytest: `xfail(strict=True)`). NUnit has no built-in equivalent, so this suite has a small helper.

## How it works

`Infrastructure/KnownBug.cs` wraps the part of a test that is expected to fail:

| What happens inside the block | Result | Meaning |
|---|---|---|
| Fails with a Playwright error (failed `Expect`, timeout) | **Warning** with the bug ID and the error message. Shown as *skipped* by `dotnet test`. | The defect is still there. The run stays green. |
| Passes | **Failed**: "BUG-x no longer reproduces..." | The defect looks fixed. Remove the marker so the test becomes a normal test. |
| Fails with any other error (e.g. a bug in the test code) | **Failed** as usual | Not the known defect, so it is not hidden. |

## Usage

```csharp
[Test]
public async Task ProblemUser_CanAddBoltTShirtToCart()
{
    var inventory = await LogInAsync(Users.Problem);   // setup: outside the block

    await KnownBug.ExpectFailureAsync("BUG-1: problem_user cannot add some products to the cart", async () =>
    {
        await inventory.AddToCartAsync(Products.BoltTShirt);
        await Expect(inventory.CartBadge).ToHaveTextAsync("1");   // asserts the correct behavior
    });
}
```

## Rules

1. **Assert the correct behavior.** The test describes what *should* happen. It is never rewritten to match the bug.
2. **Only the failing step goes inside the block.** Setup (login, navigation) stays outside, so a broken setup fails normally instead of being reported as the known bug.
3. **One known bug per test, each with its own ID.** Otherwise the first failure hides the second.
4. **Every known bug is listed below** with steps, expected and actual result.
5. **When the test reports "no longer reproduces"**, verify the fix manually, then remove the `KnownBug.ExpectFailureAsync` wrapper and keep the test as a normal one.

## Limitations

- **Custom code.** It is small, but it is one more thing to read and maintain.
- **Reported as "skipped".** NUnit reports the result as a *Warning*, and the NUnit adapter for `dotnet test` maps warnings to *Skipped*. The bug ID and message are shown with it.
- **Catching exceptions inside a test** can hide real failures. This is limited by catching only `PlaywrightException` and keeping setup outside the block.
- **Slower.** A failing `Expect` waits for its timeout (5 seconds by default) before the warning is reported.

## Known bugs

| ID | Steps | Expected | Actual | Covered by |
|---|---|---|---|---|
| BUG-1 | Log in as `problem_user`, click **Add to cart** on *Sauce Labs Bolt T-Shirt* | Button changes to **Remove**, cart badge shows 1 | Nothing changes. Same for *Sauce Labs Fleece Jacket* and *Test.allTheThings() T-Shirt (Red)* | `CartTests.ProblemUser_CanAddBoltTShirtToCart` |
| BUG-2 | Log in as `problem_user`, add *Sauce Labs Backpack*, click **Remove** on the products page | Button changes back to **Add to cart**, cart badge disappears | Nothing changes. Removing the item from the cart page works. | Not automated (removing items is outside the requested scope) |
| BUG-3 | Log in as `problem_user` and look at the products page | Each product shows its own image | All products show the same image | Not automated (needs visual comparison) |
| BUG-4 | Log in as `problem_user`, click a product name | The details page of that product opens | A different product's details page opens | Not automated (product details page is outside the requested scope) |
