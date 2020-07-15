using System;
using System.Security.Cryptography;
using System.Text;

namespace Gosocket.Dian.Functions.Common
{
    public static class StringExtensions
    {
        public static string EncryptSHA256(this string input)
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

        public static Guid ToGuid(this string code)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(code);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return new Guid(hashBytes);
            }
        }
    }
}
