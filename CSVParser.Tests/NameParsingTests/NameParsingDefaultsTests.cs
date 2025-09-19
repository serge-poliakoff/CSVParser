

using CSVParser.NameParsing;

namespace CSVParser.Tests.NameParsingTests;

public class NameParsingDefaultsTests
{
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
}
