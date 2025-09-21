using System.IO;

namespace CSVParser.Tests;

public class ParseMethodTests
{
    internal record class SimpleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Surname { get; set; }
    }

    private SimpleEntity expected = new SimpleEntity()
    {
        Id = 7,
        Name = "James",
        Surname = "Bond"
    };

    string[] mockCsv = ["Id,Name,Surname", "7,James,Bond"];

    [Fact]
    public void DefaultSettingsParsingStream()
    {
        var stream = CSVTestUtils.StreamFromStrings(mockCsv);

        var parser = new CsvParser<SimpleEntity>().Build();

        var actual = parser.ParseStream(stream);

        //assert result
        Assert.True(actual.Count == 1);
        Assert.Equal(expected, actual[0]);

        //assert stream is closed
        Assert.True(stream.CanRead == false);
        Assert.True(stream.CanWrite == false);
        Assert.True(stream.CanSeek == false);
    }

    [Fact]
    public void DefaultSettingsParsingStreamReader()
    {
        var streamReader = new StreamReader(CSVTestUtils.StreamFromStrings(mockCsv));

        var parser = new CsvParser<SimpleEntity>().Build();

        var actual = parser.Parse(streamReader);

        //assert result
        Assert.True(actual.Count == 1);
        Assert.Equal(expected, actual[0]);

        //assert stream is still open
        Assert.True(streamReader.BaseStream.CanRead == true);
        Assert.True(streamReader.BaseStream.CanWrite == true);
        Assert.True(streamReader.BaseStream.CanSeek == true);

        streamReader.Close();
    }

    [Fact]
    public void DefaultSettingsFileParsing()
    {
        //create file with mockCsv's data
        const string fileLocation = "./mockCsv.csv";

        File.WriteAllLines(fileLocation, mockCsv);

        //read file using CSVParser
        var parser = new CsvParser<SimpleEntity>().Build();

        var actual = parser.ParseFile(fileLocation);

        //assert result
        Assert.True(actual.Count == 1);
        Assert.Equal(expected, actual[0]);
    }
}
