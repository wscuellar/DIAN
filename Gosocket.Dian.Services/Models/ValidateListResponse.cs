namespace Gosocket.Dian.Services.Models
{
    public class ValidateListResponse
    {
        public bool IsValid { get; set; }
        public bool Mandatory { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public double? ExecutionTime { get; set; }
    }
}
