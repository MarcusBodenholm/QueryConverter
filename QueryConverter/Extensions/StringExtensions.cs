using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConverter.Extensions;
public static class StringExtensions
{
    public static string Capitalize(this string str)
    {
        var firstChar = str.Substring(0, 1).ToUpper();
        return firstChar + str.Substring(1);
    }

}
