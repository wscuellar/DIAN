using Gosocket.Dian.Infrastructure;

namespace Gosocket.Dian.Services.Utils.Common
{
    public static class Schemas
    {
        private static FileManager SchemasFileManager = new FileManager("schemas");
        public static byte[] GetSchema(string schemaName)
        {
            return SchemasFileManager.GetBytes(schemaName);
        }
    }
}
