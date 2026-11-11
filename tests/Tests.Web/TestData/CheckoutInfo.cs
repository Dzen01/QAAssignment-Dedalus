namespace Tests.Web.TestData;

public record CheckoutInfo(string FirstName, string LastName, string PostalCode)
{
    public static CheckoutInfo Valid => new("Test", "User", "71000");
}