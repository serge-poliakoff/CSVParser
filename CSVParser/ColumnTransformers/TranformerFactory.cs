using CSVParser.NameParsing;
using System.Reflection;

namespace CSVParser.ColumnTransformers;

internal class TranformerFactory
{
    private Dictionary<string, object> metadata;

    public TranformerFactory()
    {
        metadata = new Dictionary<string, object>();
    }

    //metadata conventions:
    //  - to indicate format use {TypeName}_format
    public void AddMetadata(string key, object value)
    {
        metadata.Add(key, value);
    }

    public IColumnTransformer Create(PropertyInfo property)
    {
        IColumnTransformer result;

        var format = GetFormat(property.PropertyType);

        if (property.PropertyType == typeof(DateOnly))
        {
            result = new DateOnlyTransformer()
            {
                Property = property,
                format = format
            };
            return result;
        }

        if (property.PropertyType == typeof(DateTime))
        {
            result = new DateTimeTransformer()
            {
                Property = property,
                format = format
            };
            return result;
        }

        if (property.PropertyType == typeof(TimeSpan))
        {
            result = new TimeSpanTranformer()
            {
                Property = property,
                format = format
            };
            return result;
        }

        if (property.PropertyType == typeof(Guid))
        {
            object _;
            bool generator = property.Name == "Id"
                && metadata.TryGetValue("write_id", out _);

            result = new GuidTransformer()
            {
                Property = property,
                id_gen = generator
            };
            return result;
        }

        if (property.PropertyType.IsEnum)
        {
            object enumParser;
            if (!metadata.TryGetValue("enum_parser", out enumParser))
                enumParser = new NameParser();

            result = new EnumTransformer(
                property,
                (NameParser)enumParser);
            return result;
        }

        result = new BaseColumnTransformer()
        {
            Property = property
        };
        return result;
    }

    private string? GetFormat(Type type)
    {
        object format;
        if (metadata.TryGetValue($"{type.Name}_format", out format))
            return format as string;
        else
            return null;
    }
}
