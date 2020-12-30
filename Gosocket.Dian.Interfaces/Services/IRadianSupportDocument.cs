using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianSupportDocument
    {
        Task<byte[]> GetGraphicRepresentation(string cufe);
    }
}
