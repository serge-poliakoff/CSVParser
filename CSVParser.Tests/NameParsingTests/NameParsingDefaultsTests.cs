

using CSVParser.NameParsing;

namespace CSVParser.Tests.NameParsingTests;

public class NameParsingDefaultsTests
{
    [Fact]
    public void AllNameParsersReturnNullOnNullInput()
    {
        var nameParsingDefaults = typeof(NameParsingDefaults);
        var methods = nameParsingDefaults.GetMethods()
            .Where(m => m.IsStatic && m.ReturnType == typeof(string) && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string))
            .ToArray();

        foreach(var method in methods)
        {
            var del = method.CreateDelegate(typeof(Func<string, string>));
            var result = del.DynamicInvoke((string?)null);
            Assert.Null(result);
        }
    }


    [Theory]
    [InlineData("CardType", "card_type")]
    [InlineData("TimeOfDay", "time_of_day")]
    public void PascalToSnakeCase(string input, string expected)
    {
        var actual = NameParsingDefaults.PascalToSnakeCase(input);

        Assert.Equal(actual, expected);
    }

    [Theory]
    [InlineData("card_type", "CardType")]
    [InlineData("coffee_with_milk", "CoffeeWithMilk")]
    public void SnakeCaseToPascal(string input, string expected)
    {
        var actual = NameParsingDefaults.SnakeCaseToPascal(input);

        Assert.Equal(actual, expected);
    }

    [Theory]
    [InlineData("Societe Generale", "SocieteGenerale")]
    [InlineData("Double Expresso", "DoubleExpresso")]
    public void SupressSpaces(string input, string expected)
    {
        var actual = NameParsingDefaults.SupressSpaces(input);

        Assert.Equal(actual, expected);
    }

    [Theory]
    [InlineData("societe generale", "SocieteGenerale")]
    [InlineData("Americano with Milk", "AmericanoWithMilk")]
    public void SupressCapitalise(string input, string expected)
    {
        var actual = NameParsingDefaults.SupressCapitalise(input);
        Assert.Equal(actual, expected);
    }
}
