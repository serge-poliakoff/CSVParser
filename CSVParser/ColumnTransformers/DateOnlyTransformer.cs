using System.Reflection;
using System.Globalization;

namespace CSVParser.ColumnTransformers;

class DateOnlyTransformer : IColumnTransformer
{
    public PropertyInfo Property { get; init; }
    public string? format;

    public void TransformValue(object obj, string value)
    {
        DateOnly date;
        if (format != null)
            date = DateOnly.ParseExact(value, format);
        else
            date = DateOnly.Parse(value, CultureInfo.InvariantCulture);

        Property.SetValue(obj, date);
    }
}
