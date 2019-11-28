using Gosocket.Dian.Infrastructure;

namespace Gosocket.Dian.Functions.Global.Cryptography.Common
{
    public static class Schemas
    {
        public static byte[] GetSchema(string schemaName)
        {
            return new FileManager().GetBytes("schemas", schemaName);
        }
    }
}
