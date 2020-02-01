using System;
using System.Linq;

namespace ProxyStation.Util
{
    public class Misc
    {
        public static string KebabCase2PascalCase(string kebabCase)
        {
            var words = kebabCase
                .Split('-')
                .Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1));

            return String.Join("", words);
        }
    }
}