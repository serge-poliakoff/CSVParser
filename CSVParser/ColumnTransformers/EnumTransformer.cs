using CSVParser.NameParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSVParser.ColumnTransformers;

class EnumTransformer : IColumnTransformer
{
    public PropertyInfo Property { get; init; }
    public NameParser NameParser { get; init; }

    private string[] enumValues;

    public EnumTransformer(PropertyInfo property, NameParser nameParser)
    {
        this.Property = property;
        this.NameParser = nameParser;

        enumValues = Enum.GetNames(Property.PropertyType);
    }

    public void TransformValue(object obj, string value)
    {
        string? key = null;
        foreach(var parser in NameParser)
        {
            key = enumValues.Where(v => v == parser(value)).FirstOrDefault();
            if (key != null) break;
        }

        //change to custom ConvertException
        if (key == null)
            throw new Exception($"Enum convertion exception on {Property} property: cannot convert {value}");

        object result = Enum.Parse(Property.PropertyType, key);
        Property.SetValue(obj, result);
    }
}
