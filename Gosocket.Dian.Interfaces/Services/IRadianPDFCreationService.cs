namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianPDFCreationService
    {
        byte[] GetElectronicInvoicePdf(string eventItemIdentifier);
    }
}