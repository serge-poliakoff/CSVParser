using CSVParser.Exceptions;
using System.Globalization;
using System.Reflection;

namespace CSVParser.ColumnTransformers;

class DateTimeTransformer : IColumnTransformer
{
    public PropertyInfo Property { get; init; }
    public string? format;

    public void TransformValue(object obj, string value)
    {
        DateTime date;
        try
        {
            if (format != null)
                date = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
            else
                date = DateTime.Parse(value, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            throw new ConvertException($"Cannot convert {value} to the type of {typeof(DateTime)}");
        }

        Property.SetValue(obj, date);
    }
}
