using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Utils
{
    public class UploadedFileResult
    {
        public int FileIndex { get; set; }

        public string FileName { get; set; }

        public List<long> ErrorNumbers { get; set; }

        public bool StopException { get; set; }

        public string Message { get; set; }

        public bool IsError { get; set; }
    }
    public class Log
    {
        public string FileName { get; set; }

        public string Description { get; set; }

        public int LineNumber { get; set; }

        public bool IsError { get; set; }
    }
}