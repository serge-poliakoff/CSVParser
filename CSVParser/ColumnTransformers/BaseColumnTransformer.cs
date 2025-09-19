using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSVParser.ColumnTransformers;

internal class BaseColumnTransformer : IColumnTransformer
{
    public required PropertyInfo Property { get; init; }
    public Type ColumnType { get => Property.PropertyType; }

    public void TransformValue(object obj, string value)
    {
        var result = Convert.ChangeType(value, ColumnType,
            CultureInfo.InvariantCulture);
        Property.SetValue(obj, result);
    }
}
