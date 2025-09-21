namespace CSVParser.Tests;

static class CSVTestUtils
{
    public static Stream StreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);

        writer.Write(s);
        writer.Flush();

        stream.Position = 0;
        return stream;
    }

    public static Stream StreamFromStrings(string[] ss)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);

        foreach(var s in ss)
            writer.WriteLine(s);
        writer.Flush();

        stream.Position = 0;
        return stream;
    }
}
