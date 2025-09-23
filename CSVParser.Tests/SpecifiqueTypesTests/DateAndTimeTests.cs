using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVParser.Tests.SpecifiqueTypesTests;

public class DateAndTimeTests
{
    public string[] testString =
        ["DateOnly,TimeSpan,DateTime",
        "2023-12-31,15:30:45,2023-12-31 23:59:59"];
    public DateOnly expectedDateOnly = new DateOnly(2023, 12, 31);
    public TimeSpan expectedTimeSpan = new TimeSpan(15, 30, 45);
    public DateTime expectedDateTime = new DateTime(2023, 12, 31, 23, 59, 59);

    private record TestDateAndTimeEntity
    {
        public DateOnly DateOnly { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public DateTime DateTime { get; set; }
    }

    [Fact]
    public void DateAndTimeParsingWithFormatSpecified()
    {
        var parser = new CSVParser.CsvParser<TestDateAndTimeEntity>()
            .WithFormat(typeof(DateOnly), "yyyy-MM-dd")
            .WithFormat(typeof(TimeSpan), "hh\\:mm\\:ss")
            .WithFormat(typeof(DateTime), "yyyy-MM-dd HH:mm:ss")
            .WithHeaderParser(headerParser =>
                headerParser.AddNamingPolicy(CSVParser.NameParsing.NameParsingStandarts.ExactMatch))
            .Build();

        var stream = CSVTestUtils.StreamFromStrings(testString);

        var result = parser.ParseStream(stream);
        
        Assert.Single(result);
        var entity = result[0];
        Assert.Equal(expectedDateOnly, entity.DateOnly);
        Assert.Equal(expectedTimeSpan, entity.TimeSpan);
        Assert.Equal(expectedDateTime, entity.DateTime);
    }
}
