using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Services.Utils
{
    public class StringUtil
    {
        public static readonly Encoding ISO_8859_1 = Encoding.GetEncoding("ISO-8859-1");
        public static readonly Encoding utf8 = Encoding.UTF8;
        public static string TransformToIso88591(string xml)
        {
            byte[] isoBytes = ISO_8859_1.GetBytes(xml);
            return ISO_8859_1.GetString(isoBytes);
        }

        public static string ToCSV<T>(IEnumerable<T> list)
        {
            var type = typeof(T);
            var props = type.GetProperties();

            //Setup expression constants
            var param = Expression.Parameter(type, "x");
            var doublequote = Expression.Constant("\"");
            var doublequoteescape = Expression.Constant("\"\"");
            var comma = Expression.Constant(",");

            //Convert all properties to strings, escape and enclose in double quotes
            var propq = (from prop in props
                         let tostringcall = Expression.Call(Expression.Property(param, prop), prop.ReflectedType.GetMethod("ToString", new Type[0]))
                         let replacecall = Expression.Call(tostringcall, typeof(string).GetMethod("Replace", new Type[] { typeof(string), typeof(string) }), doublequote, doublequoteescape)
                         select Expression.Call(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string) }), doublequote, replacecall, doublequote)
                         ).ToArray();

            var concatLine = propq[0];
            for (int i = 1; i < propq.Length; i++)
                concatLine = Expression.Call(typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string) }), concatLine, comma, propq[i]);

            var method = Expression.Lambda<Func<T, string>>(concatLine, param).Compile();
            return string.Join(Environment.NewLine, list.Select(method).ToArray());
            //var header = string.Join(",", props.Select(p => p.Name).ToArray());
            //return header + Environment.NewLine + string.Join(Environment.NewLine, list.Select(method).ToArray());
        }

        public static string StringToBinary(string data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }

        public static string BinaryToString(string data)
        {
            List<Byte> byteList = new List<Byte>();

            for (int i = 0; i < data.Length; i += 8)
            {
                byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
            }
            return Encoding.ASCII.GetString(byteList.ToArray());
        }

        public static string EncryptSHA256(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                StringBuilder Sb = new StringBuilder();

                using (var hash = SHA256.Create())
                {
                    Encoding enc = Encoding.UTF8;
                    byte[] result = hash.ComputeHash(enc.GetBytes(input));

                    foreach (byte b in result)
                        Sb.Append(b.ToString("x2"));
                }

                return Sb.ToString();

            }
        }

        public static string GenerateRandomString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GenerateString(10, true));
            builder.Append(GenerateNumber(1000, 9999));
            builder.Append(GenerateString(10, false));
            return builder.ToString();
        }

        public static string GenerateIdentifierSHA256(string input)
        {
            var sha256 = EncryptSHA256(input);
            return sha256;

        }
        // Generate a random number between two numbers    
        public static int GenerateNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        // Generate a random string with a given size    
        public static string GenerateString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        public static string TextAfter(string value, string search)
        {
            try
            {
                search = search ?? "";
                //var length = search != null ? search.Length : 0;
                return value.Substring(value.IndexOf(search) + search.Length);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_text"></param>
        /// <returns></returns>
        public static string FormatStringDian(string _text)
        {
            if ((string.IsNullOrEmpty(_text)) || !_text.Contains("Ã"))
                return _text;

            StringBuilder sb = new StringBuilder(_text.Length);
            string _cadena = "";
            List<Tuple<string, string>> _items = new List<Tuple<string, string>>();
            Tuple<string, string> itemChar = null;
            _items.Add(new Tuple<string, string>("Ã¡", "á"));
            _items.Add(new Tuple<string, string>("Ã©", "é"));
            _items.Add(new Tuple<string, string>("Ã­", "í"));
            _items.Add(new Tuple<string, string>("Ã³", "ó"));
            _items.Add(new Tuple<string, string>("Ãº", "ú"));
            _items.Add(new Tuple<string, string>("Ã\u0081", "Á"));
            _items.Add(new Tuple<string, string>("Ã\u0089", "É"));
            _items.Add(new Tuple<string, string>("Ã\u008d", "Í"));
            _items.Add(new Tuple<string, string>("Ã\u0093", "Ó"));
            _items.Add(new Tuple<string, string>("Ã\u009a", "Ú"));
            _items.Add(new Tuple<string, string>("Ã\u0091", "Ñ"));
            _items.Add(new Tuple<string, string>("Ã±", "ñ"));

            foreach (char c in _text)
            {
                switch (c.ToString())
                {
                    case "Ã":
                        _cadena = "Ã";
                        break;
                    default:
                        _cadena += c.ToString();
                        break;
                }
                if (!_cadena.StartsWith("Ã"))
                {
                    sb.Append(c.ToString());
                }
                else
                {
                    itemChar = _items.FirstOrDefault(i => i.Item1 == _cadena);
                    if (itemChar != null)
                    {
                        sb.Append(itemChar.Item2);
                        _cadena = String.Empty;
                    }
                    else if (_cadena != "Ã")
                        _cadena = String.Empty;
                }
            }

            return sb.ToString();
        }

    }
}
