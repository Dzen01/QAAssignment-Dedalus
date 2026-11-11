using Microsoft.Playwright;

namespace Tests.Web.Infrastructure;

public static class KnownBug
{
    // Runs a test that is expected to fail because of a known defect.
    // Fails as expected -> reported as a warning, the run stays green.
    // Passes -> the test fails, so we notice the bug was fixed and remove the marker.
    public static async Task ExpectFailureAsync(string bugId, Func<Task> test)
    {
        try
        {
            await test();
        }
        catch (PlaywrightException ex)
        {
            Assert.Warn($"Known bug {bugId}: {ex.Message}");
            return;
        }

        Assert.Fail($"{bugId} no longer reproduces. Check if it was fixed and remove the known-bug marker.");
    }
}