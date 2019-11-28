using System;
using System.Linq;

namespace Gosocket.Dian.Functions.Utils
{
    public static class StringUtils
    {
        public static string NormalizeInput(string input)
        {
            var output = input?.Replace("\n", "")
                               .Replace("\r", "")
                               .Replace("\t", "");
            return output;
        }

        public static bool HasOnlyNumbers(string input)
        {
            return input.All(char.IsDigit);
        }
    }
}
