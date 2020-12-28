using System.Diagnostics.CodeAnalysis;

namespace Gosocket.Dian.Web.Models
{
    [ExcludeFromCodeCoverage]
    public class AutoListModel
    {
        public string text;
        public string value;
        public AutoListModel(string value, string text)
        {
            this.value = value;
            this.text = text;
        }
    }
}