using CSVParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVParser.Tests.OptionsTests;

public class WrittingIdTests
{
    private record TestIdEntInt
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private record TestIdEntGuid
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    private record TestEntNoId
    {
        public string Name { get; set; }
    }

    [Theory]
    [InlineData("Name", "James", "Abraham", "Sarah", "Olga")]
    [InlineData("Id,Name", "5,James","6,Abraham","7,Sarah","8,Olga")]
    public void WritingIntId(params string[] testStrings)
    {
        var parser = new CsvParser<TestIdEntInt>()
            .WithHeaderParser(headerParser =>
                headerParser.AddNamingPolicy(NameParsing.NameParsingStandarts.ExactMatch))
            .WriteId()
            .Build();

        var stream = CSVTestUtils.StreamFromStrings(testStrings);
        var result = parser.ParseStream(stream);

        Assert.Equal(4, result.Count);
        for (int i = 0; i < result.Count; i++)
        {
            Assert.Equal(i, result[i].Id);
        }
    }

    [Theory]
    [InlineData("Name", "James", "Abraham", "Sarah", "Olga")]
    [InlineData("Id,Name", "5,James", "6,Abraham", "7,Sarah", "8,Olga")]
    public void WritingGuidId(params string[] testStrings)
    {
        var parser = new CsvParser<TestIdEntGuid>()
            .WithHeaderParser(headerParser =>
                headerParser.AddNamingPolicy(NameParsing.NameParsingStandarts.ExactMatch))
            .WriteId()
            .Build();
        var stream = CSVTestUtils.StreamFromStrings(testStrings);

        var result = parser.ParseStream(stream);
        Assert.Equal(4, result.Count);
        for (int i = 0; i < result.Count; i++)
        {
            Assert.NotEqual(Guid.Empty, result[i].Id);
        }
    }

    [Fact]
    public void ThrowsOnNoIdFound()
    {
        Assert.Throws<CSVParserBuildingException>(() => new CsvParser<TestEntNoId>()
            .WithHeaderParser(headerParser =>
                headerParser.AddNamingPolicy(NameParsing.NameParsingStandarts.ExactMatch))
            .WriteId()
            .Build());
    }

    [Theory]
    [InlineData("Id,Name", "5,James", "6,Abraham", "7,Sarah", "8,Olga")]
    public void DoesNotOverrideIdIfPresent(params string[] testStrings)
    {
        var parser = new CsvParser<TestIdEntInt>()
            .WithHeaderParser(headerParser =>
                headerParser.AddNamingPolicy(NameParsing.NameParsingStandarts.ExactMatch))
            .Build();
        var stream = CSVTestUtils.StreamFromStrings(testStrings);

        var result = parser.ParseStream(stream);

        Assert.Equal(4, result.Count);
        for (int i = 0; i < result.Count; i++)
        {
            Assert.Equal(i + 5, result[i].Id);
        }
    }
}
