using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Utils
{
    public class ResponseXpathDataValue
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> XpathsValues { get; set; }
    }
}
