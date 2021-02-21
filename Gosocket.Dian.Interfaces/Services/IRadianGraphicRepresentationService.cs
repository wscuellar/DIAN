using System.Threading.Tasks;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianGraphicRepresentationService
    {
        Task<byte[]> GetPdfReport(string cude, string urlBase);
    }
}
