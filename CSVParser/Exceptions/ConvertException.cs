using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVParser.Exceptions;

public class ConvertException : Exception
{
    public ConvertException(string msg) : base(msg)
    {
        
    }
}
