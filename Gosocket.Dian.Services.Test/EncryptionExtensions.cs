using System.Security.Cryptography;
using System.Text;

namespace Gosocket.Dian.Services.Test
{
    static class EncryptionExtensions
    {
        public static string EncryptSHA384(this string input)
        {
            using (SHA384 shaM = new SHA384Managed())
            {
                StringBuilder Sb = new StringBuilder();

                using (var hash = SHA384.Create())
                {
                    Encoding enc = Encoding.UTF8;
                    byte[] result = hash.ComputeHash(enc.GetBytes(input));

                    foreach (byte b in result)
                        Sb.Append(b.ToString("x2"));
                }

                return Sb.ToString();

            }
        }
    }
}
