using CSVParser.Exceptions;
using CSVParser.NameParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSVParser.ColumnTransformers;

internal class EnumTransformer : IColumnTransformer
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
            throw new ConvertException($"Enum convertion exception on {Property} property:" +
                $" haven't found the appropriate enum value for {value}\n" +
                $"The issue may be a wrong policy given for enum name parser");

        object result;
        try
        {
            result = Enum.Parse(Property.PropertyType, key);
        }
        catch (Exception e)
        {
            throw new ConvertException($"Cannot convert {value} to the type of {Property.PropertyType}");
        }
        Property.SetValue(obj, result);
    }
}
