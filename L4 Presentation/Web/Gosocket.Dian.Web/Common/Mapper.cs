using Newtonsoft.Json;

namespace Gosocket.Dian.Web.Common
{
    public class Mapper
    {
        public static T1 Map<T, T1>(T sourceObj)
        {
            string json = JsonConvert.SerializeObject(sourceObj);
            T1 m = JsonConvert.DeserializeObject<T1>(json);
            return m;
        }
    }
}