using System.Threading.Tasks;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianPDFCreationService
    {
        Task<byte[]> GetElectronicInvoicePdf(string eventItemIdentifier);
    }
}