namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianPayrollGraphicRepresentationService
    {
        byte[] GetPdfReport(string id, ref string documentName);
    }
}
