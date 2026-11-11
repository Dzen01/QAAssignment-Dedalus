# QA Assignment: Clinic Appointments and Sauce Demo

## Part A answers

### 3 questions for the product owner

1. *How exactly do the time rules work, and which timezone do they use?*
   Rules 1, 2, 5 and 6 all depend on "now". Is "future" judged in the clinic's timezone or the patient's? Is a slot starting in the next minute bookable? Is cancelling exactly 24:00h before free or late? Time boundaries are edge cases where most bugs hide, and the ticket gives no precise rule.

2. *How does cancellation work, and how does the fee work?*
   Rules 5 and 6 contradict each other. Rule 5 suggests you can't cancel after the 24h mark, and rule 6 suggests you can, for a fee. If you can: how much is the fee, is a card stored at booking, what happens if the charge fails, and is the slot released? It involves money and is the least specified part of the ticket.

3. *What does "load fast" mean in measurable numbers?*
   Rule 7 is untestable as written. I would need a defined target (for example, under 1s for `GET /slots` and the booking page on 4G). An important follow-up question is the expected load (concurrent users, peak hours). Without all of this, I can't say pass or fail.

Since only 3 questions can be asked, I would also try to resolve these unclear points:
- What happens if the email or push notification fails, since both rely on third-party services? And how do web users get a push notification, since a browser has no app?
- Can only the patient cancel, or can the doctor cancel too?

### Top 5 risks and where I'd test them

Both clients call the same API, so I test the business rules once at API and unit level. UI tests cover only what the UI itself adds: display, the happy path and error handling.

#### Security
A patient cancels or views someone else's appointment via `DELETE /appointments/{id}`, or calls the API without logging in.

**Should be tested at: API layer (auth and ownership checks, invalid IDs and other users' IDs).**

#### Late-cancellation fee
Charged wrongly (wrong side of 24h, charged twice, charged on a free cancel).

**Should be tested at all layers: unit for the fee decision, API with the payment provider mocked, UI for the fee warning shown before confirming.**

#### Parallel/double booking
Two or more patients book the same slot at the same moment (rule 3).

**Should be tested at: API (parallel requests).**

#### Wrong time logic
Past slots bookable, timezone errors, wrong 24h cutoff.

**Should be tested at all layers: unit tests with a fixed clock and boundaries such as 23:59 / 24:00 / 24:01, then API. Web and mobile UI: slot times shown in the right timezone.**

#### Limit of 3 upcoming
Check whether cancelled or past bookings are counted, or whether the limit can be bypassed with parallel requests. Stale slots should also be verified.

**Should be tested at: unit layer (logic) and API layer for real behavior.**

UI tests would only verify that UI elements are displayed correctly (for example, stale slots).

### Two things I would NOT automate

1. **Real email and push delivery on real devices, including other components that rely on third-party services.** It depends on third-party services, device settings and the network, so tests would be flaky and slow. I cover the logic with mocks at API level and check real delivery manually before each release.
   This also includes real money transfers, since tests should not move real money. A manual test can make a real transaction and then refund it.
2. **Some business rules through the UI.** Both clients call the same API, so rules like the limit of 3 bookings, slot availability and the 24h cutoff are tested at unit and API level. Repeating each one in web, iOS and Android UI tests would double or triple the suite and add flakiness without finding new bugs. UI automation stays limited to a happy path and cancel on each client, plus how errors are displayed.

## Part B: Web automation (Sauce Demo)

Track: **Web**. Tests for [Sauce Demo](https://www.saucedemo.com) covering login, adding items to the cart and checkout, written in C# with .NET 10, NUnit and Playwright.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Playwright's Chromium browser. **Install it once before the first test run**, otherwise every test fails with an "Executable doesn't exist" error. The install script is created by the build, so build first:

```powershell
dotnet build

# PowerShell 7 (Windows, macOS, Linux)
pwsh tests/Tests.Web/bin/Debug/net10.0/playwright.ps1 install chromium

# Windows PowerShell (if pwsh is not installed)
powershell -ExecutionPolicy Bypass -File tests/Tests.Web/bin/Debug/net10.0/playwright.ps1 install chromium
```

### How to run

From the repository root:

```powershell
dotnet test
```

Expected result: **4 passed, 1 skipped**. The skipped test is a known bug, explained below.
When a test fails, a Playwright trace is saved to `tests/Tests.Web/bin/Debug/net10.0/playwright-traces/`. Open it with:

```powershell
pwsh tests/Tests.Web/bin/Debug/net10.0/playwright.ps1 show-trace <path-to-trace.zip>
```

The alternatives considered and all bugs found: [docs/KnownBug.md](docs/KnownBug.md).

## Part C: Write-up

### 1. Why these tests

- They cover the requested scope end to end: log in, add items to the cart, check out.
- Each test protects something a user would notice: getting in, being blocked when locked out, completing an order, and being stopped when information is missing.
- The checkout test checks more than navigation: the cart badge, the cart contents, the item total and that the cart is empty after the order.
- The two negative tests check that the app refuses bad input **and** that the user does not get any further.
- The fifth test covers a real defect found while exploring the app (BUG-1).

**Left out:**
- Sorting, the product details page, removing items and logout: outside the requested scope.
- Missing first name and missing last name: same validation as the postal code, so one negative test represents them.
- Wrong product images: needs visual comparison tooling, which doesn't fit the time box.
- Other browsers: tests run on Chromium only. Firefox and WebKit are a configuration change in Playwright.
- The other special users (`error_user`, `performance_glitch_user`, `visual_user`): not explored in depth.

### 2. Defects and doubts

All defects were found manually as `problem_user`. Removing an item from the cart page works, which shows BUG-2 is limited to the products page.

| Finding | Type | How sure | Automated |
|---|---|---|---|
| **BUG-1:** Add to cart does nothing for *Bolt T-Shirt*, *Fleece Jacket* and *Test.allTheThings() T-Shirt (Red)* | Bug | High: reproduced manually and by the test | Yes, as a known failure |
| **BUG-2:** Remove on the products page does nothing | Bug | High: reproduced manually | No, removing items is outside the scope |
| **BUG-3:** All products show the same image | Bug | High: visible on every product | No, visual check |
| **BUG-4:** Clicking a product opens a different product's details page | Bug | High: reproduced manually | No, outside the scope |

**Doubts:**
- **Spec gap:** Sauce Demo has no specification, so "expected" behavior is taken from `standard_user`. That is an assumption.
- **Test design:** the checkout test expects an item total of $39.98. If the shop changes its prices, the test fails although nothing is broken. Acceptable for a demo app; on a real product, prices would come from test data.
- **Test design:** products and cart items are found by CSS classes (`.inventory_item`, `.cart_item`) because they have no `data-test` attribute. These can break when the styling changes.
- **Environment:** the tests run against the public demo site, so an outage or slow network shows up as a failed test. That would be an environment problem, not an app bug.

### 3. AI usage

Used for summary of .md file. 
Creation of KnownBug.md based on the implementation
Used as aid in setting up the repository (gave PS script and commands)
Validation of the solution and its structure (Instructed to give strict and fair criticism if implementation is fitting requirements).

### 4. Next steps

If this suite ran daily in CI for a team, the first three changes would be:

1. **Run it in a pipeline** on every pull request and on a daily schedule. Publish the test results, upload Playwright traces when a test fails, and notify the team.
2. **Link known bugs to real tickets.** Every `KnownBug` ID points to a ticket in the team's tracker, and known failures are reviewed regularly, so warnings don't become noise nobody reads.
3. **Make the suite faster.** Tests that are not about login reuse a saved login session instead of logging in through the UI, and tests run in parallel.

Later: cross-browser runs (Firefox, WebKit), and moving `QaFramework.Core` to its own NuGet package if a second repository needs it.
