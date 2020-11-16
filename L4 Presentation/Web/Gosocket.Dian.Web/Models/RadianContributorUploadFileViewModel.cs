using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    public class RadianContributorUploadFileViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public Guid FileId { get; set; }
        public int FileTypeId { get; set; }
        public string FileTypeName { get; set; }
    }
}