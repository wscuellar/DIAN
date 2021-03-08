using System.IO;
using System.Text;
using System.Xml;

namespace Gosocket.Dian.Services.Utils.Common
{
    public static class Formatter
    {
        public static Encoding ISO_8859_1
        {
            get
            {
                return Encoding.GetEncoding("ISO-8859-1");
            }
        }

        public static string LineBreak
        {
            get
            {
                return "\r\n";
            }
        }

        public static Encoding GetEncoding(byte[] bytes)
        {
            Encoding encoding = Encoding.Default;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    using (XmlTextReader xmlTextReader = new XmlTextReader((Stream)memoryStream))
                    {
                        int content = (int)xmlTextReader.MoveToContent();
                        encoding = xmlTextReader.Encoding;
                    }
                }
                return encoding;
            }
            catch
            {
                return encoding;
            }
        }

        public static string StandardBase64(string base64)
        {
            int num1 = base64.Length / 64;
            int num2 = 0; 

            StringBuilder str = new StringBuilder();
            str.Append("\r\n");
            for (; num2 < num1; ++num2)
                str.Append(base64.Substring(num2 * 64, 64) + "\r\n");
            if (base64.Length % 64 == 0)
                return str.ToString();
            int length = base64.Length - base64.Length / 64 * 64;
            return str + base64.Substring(base64.Length - length, length) + "\r\n";
        }
    }
}
