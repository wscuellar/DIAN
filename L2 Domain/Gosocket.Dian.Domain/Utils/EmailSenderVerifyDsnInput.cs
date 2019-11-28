using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Domain.Utils
{
    public class EmailSenderVerifyDsnInput
    {
        public string Application { get; set; }
        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string EmailID { get; set; }
    }
}
