using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IOthersElectronicDocumentsService
    {

        ResponseMessage Validation(string userCode, string Accion, int IdElectronicDocument, string complementeTexto, int ParticipanteId);

        /// <summary>
        /// Retornar la lista de Modos de Operación
        /// </summary>
        /// <returns></returns>
        List<OperationMode> GetOperationModes();
    }
}
