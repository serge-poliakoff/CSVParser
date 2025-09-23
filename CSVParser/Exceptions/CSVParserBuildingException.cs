using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVParser.Exceptions;

class CSVParserBuildingException : Exception
{
    public CSVParserBuildingException(string msg) : base(msg)
    {
        
    }
}
