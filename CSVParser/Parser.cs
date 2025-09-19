using System.Collections;
using System.Globalization;
using System.Reflection;

using CSVParser.ColumnTransformers;
using CSVParser.NameParsing;
namespace CSVParser;

public class CsvParser<T>
{
    private List<(IColumnTransformer transformer, int colIndex)> transformers;
    private NameParser headerParser;
    private List<IColumnTransformer> columnTransformers;

    private Type parseType;

    private bool writeId = true;
    private IColumnTransformer IdTransformer;
    public CsvParser()
    {
        columnTransformers = new List<IColumnTransformer>();
        transformers = new List<(IColumnTransformer transformer, int colIndex)>();
        headerParser = new NameParser();
    }

    public CsvParser<T> WithHeaderParser(Func<NameParser, NameParser> configure)
    {
        configure(headerParser);

        return this;
    }

    public CsvParser<T> Build()
    {
        parseType = typeof(T);
        var props = parseType.GetProperties().ToList();
        foreach(var prop in props)
        {
            if (prop.Name == "Id" && writeId)
            {
                IdTransformer = new BaseColumnTransformer() { Property = prop };
                continue;     //do not ad id column if it would be overwritten
            }
            columnTransformers.Add(
                new BaseColumnTransformer() { Property = prop }
            );
        }
        return this;
    }

    //extract to HeaderParser (which would contain naming policies etc)
    private void FitColumnTransformers((int Index, string Item)[] header)
    {
        for (int i = 0; i < columnTransformers.Count; i++)
        {
            var propName = columnTransformers[i].Property.Name;

            int? ind = null;
            foreach(var nameParcerFunc in headerParser)
            {
                var probableName = nameParcerFunc(propName);
                ind = header.Where(col => col.Item == probableName)
                    .Select(col => col.Index)
                    .Cast<int?>()
                    .FirstOrDefault();
                if (ind != null) break;
            }

            if (ind == null)
                throw new Exception($"Can not convert to {typeof(T).Name} - " +
                    $"no column found for {propName} property");

            transformers.Add(new ()
            {
                transformer = columnTransformers[i],
                colIndex = ind.GetValueOrDefault()  //ind can not be null here, however to cast from int?
            });
        }
    }

    public IList<T> Parse(string path)
    {
        using var stream = new StreamReader(path);
        var header = stream.ReadLine()!.Split(',').Index().ToArray();

        FitColumnTransformers(header);

        var resultListType = typeof(List<>).MakeGenericType(parseType);
        var result = (IList)Activator.CreateInstance(resultListType)!;

        int ind = 0;
        string line;
        while ((line = stream.ReadLine()!) != null)
        {
            var data = line.Split(',');
            var ent = Activator.CreateInstance(parseType);
            for (int i = 0; i < transformers.Count; i++)
            {
                var columnIndex = transformers[i].colIndex;   //index of a column with value
                transformers[i].transformer.TransformValue(ent, data[columnIndex]);
            }
            if (writeId)
                IdTransformer.TransformValue(ent, ind.ToString());
            result.Add(ent);
            ind++;
        }

        return result as IList<T>;
    }

    private IList ParseObsolete(string path)
    {
        using var stream = new StreamReader(path);
        //var lines = File.ReadAllLines(path);
        //int lines_count = lines.Length - 1; //minus one for header

        var header = stream.ReadLine()!.Split(',').Index();
        int columns_count = header.Count();
        
        
        //checking that T is a correct type for parsing: a property per column name
        Type parseType = typeof(T);
        var props = parseType.GetProperties().ToList();
        PropertyInfo idProp = null!;
        foreach (var prop in props)
        {
            if (prop.Name == "Id")
            {
                idProp = prop;
                if (writeId) continue;
            }
            if (header.Where(col => col.Item == prop.Name).Count() == 0)
            {
                throw new Exception($"Can not convert to {parseType.Name} - no column found for {prop.Name} property");
            }
        }
        if (idProp != null & writeId)
        {
            props.Remove(idProp);
        }
        var columnProperties = props
            .Select(prop => header.Where(col => col.Item == prop.Name).First().Index)
            .ToArray();     //map between properties and columns' indexes

        

        var resultListType = typeof(List<>).MakeGenericType(parseType);
        var result = (IList)Activator.CreateInstance(resultListType)!;

        int ind = 0;
        string line;
        while ((line = stream.ReadLine()) != null)
        {
            var data = line.Split(',');
            var ent = Activator.CreateInstance(parseType);
            for (int i = 0; i < props.Count; i++)
            {
                var valueIndex = columnProperties[i];   //index of a column with value
                var value = Convert.ChangeType(data[valueIndex], props[i].PropertyType,
                    CultureInfo.InvariantCulture);
                props[i].SetValue(ent, value);
            }
            if (writeId)
                idProp.SetValue(ent, ind);
            result.Add(ent);
            ind++;
        }

        return result;
    }
}
