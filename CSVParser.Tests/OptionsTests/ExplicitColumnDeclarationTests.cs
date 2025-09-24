using CSVParser.Exceptions;

namespace CSVParser.Tests.OptionsTests;

public class ExplicitColumnDeclarationTests
{
    private record class TestEnt
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Height { get; set; }
    }

    private List<TestEnt> expected = new List<TestEnt>
        {
            new TestEnt { Name = "John", Age = 25, Height = 180 },
            new TestEnt { Name = "Alice", Age = 30, Height = 165 },
            new TestEnt { Name = "Bob", Age = 22, Height = 175 }
        };

    #region Functionality Tests

    [Theory]
    [InlineData("full_name,age,height (cm)","John,25,180", "Alice,30,165", "Bob,22,175")]
    [InlineData("age,full_name,height (cm)","25,John,180", "30,Alice,165", "22,Bob,175")]
    [InlineData("age,height (cm),full_name", "25,180,John", "30,165,Alice", "22,175,Bob")]
    public void ExplicitColumnDeclarationByNameTest(params string[] testStrings)
    {
        var parser = new CsvParser<TestEnt>()
            .WithHeaderParser(headerParser =>
                headerParser.AddNamingPolicy(NameParsing.NameParsingStandarts.ExactMatch))
            .WithExplicitColumnDeclaration(
                [
                    "full_name",
                    "age",
                    "height (cm)"
                ])
            .Build();
       
        var stream = CSVTestUtils.StreamFromStrings(testStrings);
        var result = parser.ParseStream(stream);

        Assert.Equal(3, result.Count);

        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(expected[i].Name, result[i].Name);
            Assert.Equal(expected[i].Age, result[i].Age);
            Assert.Equal(expected[i].Height, result[i].Height);
        }
    }

    [Theory]
    [InlineData("Id,Age,gthFO,Name,Weight,Height",
        "1,25,180,John,75,180", "2,30,165,Alice,60,165", "3,22,175,Bob,68,175")]
    public void ExplicitColumnDeclarationByIdTets(params string[] testStrings)
    {
        var parser =  new CsvParser<TestEnt>()
            .WithHeaderParser(headerParser =>
                headerParser.AddNamingPolicy(NameParsing.NameParsingStandarts.ExactMatch))
            .WithExplicitColumnDeclaration(
                [
                    3, //Name
                    1, //Age
                    2  //Height
                ])
            .Build();
        var stream = CSVTestUtils.StreamFromStrings(testStrings);

        var result = parser.ParseStream(stream);

        Assert.Equal(3, result.Count);
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(expected[i].Name, result[i].Name);
            Assert.Equal(expected[i].Age, result[i].Age);
            Assert.Equal(expected[i].Height, result[i].Height);
        }
    }

    #endregion

    #region Error Tests

    [Fact]
    public void ExplicitColumnDeclarationByName_WrongColumnName_Throws()
    {
        var parser = new CsvParser<TestEnt>()
            .WithHeaderParser(headerParser =>
                headerParser.AddNamingPolicy(NameParsing.NameParsingStandarts.ExactMatch))
            .WithExplicitColumnDeclaration(
                [
                    "full_name",
                    "age",
                    "heigt" //wrong column name
                ])
            .Build();
        var stream = CSVTestUtils.StreamFromStrings([
            "full_name,age,height (cm)",
            "John,25,180",
            "Alice,30,165",
            "Bob,22,175"]);
        
        Assert.Throws<CsvParsingException>(() => parser.ParseStream(stream));
    }

    [Fact]
    public void ExplicitColumnDeclarationById_WrongColumnIndex_Throws()
    {
        var parser = new CsvParser<TestEnt>()
            .WithHeaderParser(headerParser =>
                headerParser.AddNamingPolicy(NameParsing.NameParsingStandarts.ExactMatch))
            .WithExplicitColumnDeclaration(
                [
                    3, //Name
                    1, //Age
                    7  //wrong column index
                ])
            .Build();
        var stream = CSVTestUtils.StreamFromStrings([
            "Id,Age,height (cm),Name,Weight,Height",
            "1,25,180,John,75,180",
            "2,30,165,Alice,60,165",
            "3,22,175,Bob,68,175"]);
        
        Assert.Throws<CsvParsingException>(() => parser.ParseStream(stream));
    }

    #endregion
}
