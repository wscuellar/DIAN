namespace Gosocket.Dian.Plugin.Functions.Models
{
    public class DocValidatorTrackingModel
    {
        public int Priority { get; set; }
        public string ErrorMessage { get; set; }
        public bool Mandatory { get; set; }
        public bool IsValid { get; set; }
        public bool IsNotification { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
    }
}
