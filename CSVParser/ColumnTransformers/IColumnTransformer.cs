using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSVParser.ColumnTransformers;

internal interface IColumnTransformer
{
    PropertyInfo Property { get; init; }
    Type ColumnType { get => Property.PropertyType; }
    void TransformValue(object obj, string value);
}
