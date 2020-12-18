using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models.FreeBiller
{
    public class ProfileFreeBillerModel
    {

        public int ProfileId { get; set; }

        public string Name { get; set; }

        public string[,] MenuOptionsByProfile { get; set; }
    }
}