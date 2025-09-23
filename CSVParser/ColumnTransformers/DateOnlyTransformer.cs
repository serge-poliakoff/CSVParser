using System.Reflection;
using System.Globalization;
using CSVParser.Exceptions;

namespace CSVParser.ColumnTransformers;

internal class DateOnlyTransformer : IColumnTransformer
{
    public PropertyInfo Property { get; init; }
    public string? format;

    public void TransformValue(object obj, string value)
    {
        DateOnly date;
        try
        {
            if (format != null)
                date = DateOnly.ParseExact(value, format);
            else
                date = DateOnly.Parse(value, CultureInfo.InvariantCulture);
        }catch(Exception e)
        {
            throw new ConvertException($"Cannot convert {value} to the type of {typeof(DateOnly)}");
        }
        Property.SetValue(obj, date);
    }
}
