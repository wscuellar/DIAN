using System.Threading.Tasks;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianSupportDocument
    {
        Task<byte[]> GetGraphicRepresentation(string cude, string webPath);
    }
}
