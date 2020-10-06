using Gosocket.Dian.Services.Utils.Common;
using System;

namespace Gosocket.Dian.Services.ServicesGroup
{
    /// <summary>
    /// Clase que contiene la logica de validaciones de los documentos para factura como documento valor
    /// </summary>
    public class LogicalARService : IDisposable
    {
        public LogicalARService()
        {
        }   
        
        public void Dispose()
        {
            // is empty 
        }

        public bool ValidateSoftwareId(ref DianResponse response, string softwareId)
        {


            return true;
        }
    }
}
