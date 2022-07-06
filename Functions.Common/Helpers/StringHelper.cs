using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Common.Helpers
{
    static public class StringHelper
    {
        public static string ToCleanString(this object value)
        {
            return value.ToString().Replace("_", " ");
        }
    }
}
