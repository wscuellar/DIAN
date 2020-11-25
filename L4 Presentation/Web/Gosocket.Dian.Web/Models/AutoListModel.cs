using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    public class AutoListModel
    {
        public string label;
        public string value;
        public AutoListModel(string value, string label)
        {
            this.value = value;
            this.label = label;
        }
    }
}