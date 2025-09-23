using CSVParser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSVParser.ColumnTransformers;

class GuidTransformer : IColumnTransformer
{
    public PropertyInfo Property { get; init; }

    public bool id_gen = false; //if the transformer is used to generate id's rather to translate column value
    public void TransformValue(object obj, string value)
    {
        Guid guid;
        if (id_gen)
            guid = Guid.NewGuid();
        else
        {
            try { 
                guid = Guid.Parse(value);
            }
            catch (Exception e)
            {
                throw new ConvertException($"Cannot convert {value} to the type of {Property.PropertyType}");
            }
        }

        Property.SetValue(obj, guid);
    }
}
