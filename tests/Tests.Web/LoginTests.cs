using System.Text.RegularExpressions;
using Tests.Web.Pages;
using Tests.Web.TestData;
using Tests.Web.Infrastructure;

namespace Tests.Web;

public class LoginTests : WebTestBase
{
    [Test]
    public async Task StandardUser_CanLogIn()
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();

        await login.LoginAsync(Users.Standard, WebSettings.Current.Password);

        await Expect(Page).ToHaveURLAsync(new Regex(@"/inventory\.html$"));
    }

    [Test]
    public async Task LockedOutUser_SeesErrorAndStaysOnLogin()
    {
        var login = new LoginPage(Page);
        await login.OpenAsync();

        await login.LoginAsync(Users.LockedOut, WebSettings.Current.Password);

        await Expect(login.Error).ToContainTextAsync("this user has been locked out");
        await Expect(Page).Not.ToHaveURLAsync(new Regex(@"/inventory\.html$"));
    }
}