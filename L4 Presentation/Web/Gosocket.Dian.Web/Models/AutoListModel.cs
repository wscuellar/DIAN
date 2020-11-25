using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    public class AutoListModel
    {
        public string text;
        public int value;
        public AutoListModel(int value, string text)
        {
            this.value = value;
            this.text = text;
        }
    }
}