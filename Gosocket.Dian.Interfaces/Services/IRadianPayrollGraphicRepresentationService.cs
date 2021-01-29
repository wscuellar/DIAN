using System.Threading.Tasks;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianPayrollGraphicRepresentationService
    {
        byte[] GetPdfReport(string id);
    }
}
