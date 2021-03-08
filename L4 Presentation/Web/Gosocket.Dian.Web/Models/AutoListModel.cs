namespace Gosocket.Dian.Web.Models
{
    public class AutoListModel
    {
        private string text;
        private string value;
        public AutoListModel(string value, string text)
        {
            this.Value = value;
            this.Text = text;
        }

        public string Text { get => text; set => text = value; }
        public string Value { get => value; set => this.value = value; }
    }
}