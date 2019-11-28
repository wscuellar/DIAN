using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Gosocket.Dian.Services.Utils
{
    public static class Extensions
    {
        public static string RemoveSpecialCharacters(this string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "-", RegexOptions.Compiled);
        }

        public static bool ValidateEmail(this string email)
        {
            if (Regex.IsMatch(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*"))
                return Regex.Replace(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*", string.Empty).Length == 0;
            return false;
        }

        public static bool StartsWith(this byte[] original, byte[] subString)
        {
            for (int index = 0; index < subString.Length; ++index)
            {
                if ((int)original[index] != (int)subString[index])
                    return false;
            }
            return true;
        }

        public static byte[] SubArray(this byte[] original, int start)
        {
            return original.SubArray(start, original.Length);
        }

        public static byte[] SubArray(this byte[] original, int start, int end)
        {
            int length = end - start;
            byte[] numArray = new byte[length];
            for (int index = 0; index < length; ++index)
                numArray[index] = original[start + index];
            return numArray;
        }

        public static byte[] ReadFully(this Stream input)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                input.CopyTo((Stream)memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static string EncodeToBase64(this string toEncode, Encoding encoding = null)
        {
            if (encoding != null)
                return Convert.ToBase64String(encoding.GetBytes(toEncode));
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(toEncode));
        }

        public static string DecodeFromBase64(this string encodeData, Encoding encoding = null)
        {
            return (encoding != null ? encoding.GetString(Convert.FromBase64String(encodeData)) : (string)null) ?? Encoding.UTF8.GetString(Convert.FromBase64String(encodeData));
        }

        public static T ToObject<T>(this IDictionary<string, object> source)
        where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
            {
                var property = someObjectType.GetProperty(item.Key);
                if(property != null)
                {
                    TypeCode typeCode = Type.GetTypeCode(property.PropertyType);
                    switch (typeCode)
                    {
                        case TypeCode.DateTime:
                            DateTime.TryParse(item.Value.ToString(), out DateTime date);
                            someObjectType.GetProperty(item.Key).SetValue(someObject, date, null);
                            break;
                        case TypeCode.Decimal:
                            decimal.TryParse(item.Value.ToString(), out decimal decimalVal);
                            someObjectType.GetProperty(item.Key).SetValue(someObject, decimalVal, null);
                            break;
                        case TypeCode.Double:
                            double.TryParse(item.Value.ToString(), out double doubleVal);
                            someObjectType.GetProperty(item.Key).SetValue(someObject, doubleVal, null);
                            break;
                        case TypeCode.Int32:
                            int.TryParse(item.Value.ToString(), out int intVal);
                            someObjectType.GetProperty(item.Key).SetValue(someObject, intVal, null);
                            break;
                        case TypeCode.Int64:
                            long.TryParse(item.Value.ToString(), out long longVal);
                            someObjectType.GetProperty(item.Key).SetValue(someObject, longVal, null);
                            break;
                        case TypeCode.String:
                            someObjectType.GetProperty(item.Key).SetValue(someObject, item.Value, null);
                            break;
                        default:
                            break;
                    }
                }
            }

            return someObject;
        }
    }
}
