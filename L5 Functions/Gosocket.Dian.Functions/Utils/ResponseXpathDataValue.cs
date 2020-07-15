using System.Collections.Generic;

namespace Gosocket.Dian.Functions.Utils
{
    public class ResponseXpathDataValue
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> XpathsValues { get; set; }
    }
}
