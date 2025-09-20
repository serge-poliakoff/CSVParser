using System.Reflection;

namespace CSVParser.ColumnTransformers;

internal class TranformerFactory
{
    private Dictionary<string, object> metadata;
    
    //metadata conventions:
    //  - to indicate format use {TypeName}_format
    public void AddMetadata(string key, object value)
    {
        metadata.Add(key, value);
    }

    public IColumnTransformer Create(PropertyInfo property)
    {
        IColumnTransformer result;

        if (property.PropertyType == typeof(DateOnly))
        {
            result = new DateOnlyTransformer()
            {
                Property = property,
                format = GetFormat(typeof(DateOnly))
            };
            return result;
        }
        
        if(property.PropertyType == typeof(Guid))
        {
            object _;
            bool generator = property.Name == "Id"
                && metadata.TryGetValue("GenerateId", out _);

            result = new GuidTransformer()
            {
                Property = property,
                id_gen = generator
            };
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
