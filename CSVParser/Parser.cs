using System.Collections;
using System.Globalization;
using System.Reflection;

using CSVParser.ColumnTransformers;
using CSVParser.NameParsing;
namespace CSVParser;

public class CsvParser<T>
{
    private TranformerFactory factory;
    private List<(IColumnTransformer transformer, int colIndex)> columnTransformers;
    private NameParser headerParser;
    private List<IColumnTransformer> propertyTransformers;

    private Type parseType;
    private List<PropertyInfo> props;

    private bool writeId = false;
    private IColumnTransformer IdTransformer;

    private int[] columnIndexes;
    private string[] columnNames;
    private bool useColumnNames = false;

    public CsvParser()
    {
        parseType = typeof(T);
        props = parseType.GetProperties().ToList();

        factory = new TranformerFactory();
        propertyTransformers = new List<IColumnTransformer>();
        columnTransformers = new List<(IColumnTransformer transformer, int colIndex)>();
        headerParser = new NameParser();
    }

    #region Options
    public CsvParser<T> WriteId()
    {
        writeId = true;

        var idProp = props.Where(p => p.Name == "Id").FirstOrDefault();
        if (idProp == null)
        {
            throw new Exception($"Id property not found on type {parseType}");
            //change to custom CSVParserBuildException
        }

        factory.AddMetadata("write_id", true);
        IdTransformer = factory.Create(idProp);
        props.Remove(idProp);   //this would be removed, so that we can feel free to iterate on
            //other props in order to find their columns etc.

        return this;
    }

    public CsvParser<T> WithFormat(Type type, string format)
    {
        factory.AddMetadata($"{type.Name}", format);

        return this;
    }

    public CsvParser<T> WithImplicitColumnDeclaration(int[] colIndexes)
    {
        if (colIndexes.Length != props.Count)
        {
            throw new Exception($"Provided indexes doesn't match to the count of properties on {parseType}");
            //change to custom CSVParserBuildException
        }
        columnIndexes = colIndexes;
        return this;
    }

    public CsvParser<T> WithImplicitColumnDeclaration(string[] colNames)
    {
        if (colNames.Length != props.Count)
        {
            throw new Exception($"Provided indexes doesn't match to the count of properties on {parseType}");
            //change to custom CSVParserBuildException
        }
        columnNames = colNames;
        useColumnNames = true;
        return this;
    }

    public CsvParser<T> WithHeaderParser(Func<NameParser, NameParser> configure)
    {
        configure(headerParser);

        return this;
    }

    #endregion

    // creates column columnTransformers for each property
    public CsvParser<T> Build()
    {
        
        foreach(var prop in props)
        {
            propertyTransformers.Add(
                factory.Create(prop)
            );
        }
        return this;
    }

    //creates columnTransformers
    private void FitColumnTransformers((int Index, string Item)[] header)
    {
        if (columnIndexes != null)
        {
            for(int i = 0; i < propertyTransformers.Count; i++)
            {
                columnTransformers.Add(new()
                {
                    colIndex = columnIndexes[i],
                    transformer = propertyTransformers[i]
                });
            }

            return;
        }

        for (int i = 0; i < propertyTransformers.Count; i++)
        {
            var propName = propertyTransformers[i].Property.Name;

            //searching index of the column corresponding to given property
            int? ind = null;
            if (useColumnNames)
            {
                ind = header.Where(col => col.Item == columnNames[i])
                    .Select(col => col.Index)
                    .Cast<int?>()
                    .FirstOrDefault();
            }
            else
            {
                foreach (var nameParcerFunc in headerParser)
                {
                    var probableName = nameParcerFunc(propName);
                    ind = header.Where(col => col.Item == probableName)
                        .Select(col => col.Index)
                        .Cast<int?>()
                        .FirstOrDefault();
                    if (ind != null) break;
                }
            }

            if (ind == null)
                throw new Exception($"Can not convert to {typeof(T).Name} - " +
                    $"no column found for {propName} property");

            columnTransformers.Add(new ()
            {
                transformer = propertyTransformers[i],
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
            for (int i = 0; i < columnTransformers.Count; i++)
            {
                var columnIndex = columnTransformers[i].colIndex;   //index of a column with value
                columnTransformers[i].transformer.TransformValue(ent, data[columnIndex]);
            }
            if (writeId)
                IdTransformer.TransformValue(ent, ind.ToString());
            result.Add(ent);
            ind++;
        }

        return result as IList<T>;
    }
}
