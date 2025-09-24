using System.Collections;
using System.Reflection;

using CSVParser.ColumnTransformers;
using CSVParser.Exceptions;
using CSVParser.NameParsing;

namespace CSVParser;

public class CsvParser<T> where T : class
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

    private char delimiter = ',';

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

    /// <summary>
    /// Sets the delimiter character used to split columns in the CSV file.
    /// </summary>
    /// <param name="delimiter">The delimiter character (e.g., ',').</param>
    /// <returns>The current CsvParser instance for chaining. </returns>
    public CsvParser<T> WithDelimiter(char delimiter)
    {
        this.delimiter = delimiter;

        return this;
    }

    /// <summary>
    /// Configures the parser to write an auto-incremented (or auto-generated) Id property for each parsed entity.
    /// Throws if the type T does not have an "Id" property.
    /// </summary>
    /// <returns>The current CsvParser instance for chaining. </returns>
    public CsvParser<T> WriteId()
    {
        writeId = true;

        var idProp = props.Where(p => p.Name == "Id").FirstOrDefault();
        if (idProp == null)
        {
            throw new CSVParserBuildingException($"Id property not found on type {parseType}");
        }

        factory.AddMetadata("write_id", true);
        IdTransformer = factory.Create(idProp);
        props.Remove(idProp);   //this would be removed, so that we can feel free to iterate on
            //other props in order to find their columns etc.

        return this;
    }

    /// <summary>
    /// Specifies a custom format string for parsing properties of a given type (e.g., DateTime).
    /// </summary>
    /// <param name="type">The property type to apply the format to.</param>
    /// <param name="format">The format string.</param>
    /// <returns>The current CsvParser instance for chaining. </returns>
    public CsvParser<T> WithFormat(Type type, string format)
    {
        factory.AddMetadata($"{type.Name}", format);

        return this;
    }

    /// <summary>
    /// Configures a custom name parser for handling enum property names.
    /// </summary>
    /// <param name="configure">An action to configure the NameParser for enums.</param>
    /// <returns>The current CsvParser instance for chaining. </returns>
    public CsvParser<T> WithEnumParser(Action<NameParser> configure)
    {
        var enumParser = new NameParser();
        configure(enumParser);

        factory.AddMetadata("enum_parser", enumParser);

        return this;
    }

    /// <summary>
    /// Declares the column indexes to map to each property, in order of their declaration on T.
    /// Pay attention to use this method after WriteId() if you want use auto-generated Id's
    /// Throws if the number of indexes does not match the number of properties.
    /// </summary>
    /// <param name="colIndexes">Array of column indexes.</param>
    /// <returns>The current CsvParser instance for chaining. </returns>
    public CsvParser<T> WithExplicitColumnDeclaration(int[] colIndexes)
    {
        if (colIndexes.Length != props.Count)
        {
            throw new CSVParserBuildingException($"Provided indexes doesn't match to the count of properties on {parseType}");
        }
        columnIndexes = colIndexes;
        return this;
    }

    /// <summary>
    /// Declares the column names to map to each property, in order of their declaration on T.
    /// Throws if the number of names does not match the number of properties.
    /// Pay attention to use this method after WriteId() if you want use auto-generated Id's
    /// </summary>
    /// <param name="colNames">Array of column names.</param>
    /// <returns>The current CsvParser instance for chaining.</returns>
    public CsvParser<T> WithExplicitColumnDeclaration(string[] colNames)
    {
        if (colNames.Length != props.Count)
        {
            throw new CSVParserBuildingException($"Provided indexes doesn't match to the count of properties on {parseType}");
        }
        columnNames = colNames;
        useColumnNames = true;
        return this;
    }

    /// <summary>
    /// Configures the name parser, which is used to match property names to column names.
    /// </summary>
    /// <param name="configure">A function to configure the NameParser.</param>
    /// <returns>The current CsvParser instance for chaining.</returns>
    public CsvParser<T> WithHeaderParser(Func<NameParser, NameParser> configure)
    {
        configure(headerParser);

        return this;
    }

    #endregion


    #region Prepare parsing

    /// <summary>
    /// Prepares the parser by creating column transformers for each property.
    /// Must be called before parsing.
    /// </summary>
    /// <returns>The current CsvParser instance, ready to work.</returns>
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

    
    private void FitColumnTransformers((int Index, string Item)[] header)
    {
        if (columnIndexes != null)
        {
            for(int i = 0; i < propertyTransformers.Count; i++)
            {
                if (columnIndexes[i] >= header.Length)
                    throw new CsvParsingException($"Can not convert to {typeof(T).Name} - " +
                        $"no column found at index {columnIndexes[i]} for given csv table ({header.Length} columns)");
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
                throw new CsvParsingException($"Can not convert to {typeof(T).Name} - " +
                    $"no column found for {propName} property");

            columnTransformers.Add(new ()
            {
                transformer = propertyTransformers[i],
                colIndex = ind.GetValueOrDefault()  //ind can not be null here, however to cast from int?
            });
        }
    }

    #endregion

    #region Parse methodes

    /// <summary>
    /// Parses a CSV file from the specified file path and returns a list of parsed entities.
    /// </summary>
    /// <param name="path">The file path to the CSV file.</param>
    /// <returns>A list of parsed entities of type T.</returns>
    public IList<T> ParseFile(string path)
    {
        using var stream = new StreamReader(path);
        
        var result = Parse(stream);

        //do not refactor to return Parse, as stream would be then closed by "using" directive
        return result;
    }

    /// <summary>
    /// Parses a CSV file from the provided stream and returns a list of parsed entities.
    /// </summary>
    /// <param name="stream">The input stream containing CSV data.</param>
    /// <returns>A list of parsed entities of type T.</returns>
    public IList<T> ParseStream(Stream stream)
    {
        using var reader = new StreamReader(stream);

        var result = Parse(reader);

        return result;
    }

    /// <summary>
    /// Parses a CSV file from the provided StreamReader and returns a list of parsed entities.
    /// </summary>
    /// <param name="stream">The StreamReader for the CSV data.</param>
    /// <returns>A list of parsed entities of type T.</returns>
    public IList<T> Parse(StreamReader stream)
    {
        
        var header = stream.ReadLine()!.Split(delimiter).Index().ToArray();

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

    #endregion
}
