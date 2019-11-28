using System.ServiceModel.Description;

namespace Gosocket.Dian.Web.Services.Wsdl.Utils
{
    static class Extensions
    {
        public static string GetHeaderType(this MessageHeaderDescription header)
        {
            return (string)ReflectionUtils.GetValue(header, "BaseType");
        }
    }
}
