using CSVParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSVParser.ColumnTransformers;

internal class TimeSpanTranformer : IColumnTransformer
{
    public PropertyInfo Property { get; init; }
    public string? format;

    public void TransformValue(object obj, string value)
    {
        TimeSpan date;
        try
        {
            if (format != null)
                date = TimeSpan.ParseExact(value, format, CultureInfo.InvariantCulture);
            else
                date = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            throw new ConvertException($"Cannot convert {value} to the type of {typeof(TimeSpan)}");
        }

        Property.SetValue(obj, date);
    }
}
