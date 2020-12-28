using System.Threading.Tasks;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianPdfCreationService
    {
        Task<byte[]> GetElectronicInvoicePdf(string eventItemIdentifier, string webPath);
    }
}