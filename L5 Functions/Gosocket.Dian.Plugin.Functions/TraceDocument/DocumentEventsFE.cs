using Gosocket.Dian.Application.Cosmos;
using Gosocket.Dian.Plugin.Functions.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.Plugin.Functions.TraceDocument
{
    /// <summary>
    /// Clase que validara los diferentes eventos de las facturas electronicas de la DIAN como
    /// documento valor
    /// </summary>
    public class DocumentEventsFE
    {
        /// <summary>
        /// Metodo que va y busca los documentos y eventos de la factura 
        /// </summary>
        /// <param name="documentKey">CUFE de la factura electronica</param>
        /// <param name="partitionKey">EL PartitionKey de CosmosDB</param>
        /// <returns>Una lista generica de EventViewModel para dicho documento de FE de los eventos registrados para esa FE</returns>
        public async Task<List<EventViewModel>> TraceEventsFE(string documentKey, string partitionKey)
        {
            var date = DateTime.ParseExact(DateTime.Now.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture); 
            var globalDataDocument = await CosmosDBService.Instance(date).ReadDocumentAsync(documentKey, partitionKey, date);
            var model = new DocValidatorModel();

            model.Document.Events = globalDataDocument.Events.Select(e => new EventViewModel
            {
                Code = e.Code,
                Date = e.Date,
                DateNumber = e.DateNumber,
                Description = e.Description,
                ReceiverCode = e.ReceiverCode,
                ReceiverName = e.ReceiverName,
                SenderCode = e.SenderCode,
                SenderName = e.SenderName,
                TimeStamp = e.TimeStamp
            }).ToList();

            return model.Document.Events;
        }

        /// <summary>
        /// Metodo que valida si el codigo del evento enviado en los parametros ya esta en los eventos de la FE
        /// </summary>
        /// <param name="codeEvent">Codigo del evento a validar</param>
        /// <param name="documentKey">CUFE de la factura electronica</param>
        /// <param name="partitionKey">PartitionKey CosmosDB</param>
        /// <returns></returns>
        public async Task<bool> CheckEventDuplicate(string codeEvent, string documentKey, string partitionKey)
        {
            List<EventViewModel> eventsFE = await TraceEventsFE(documentKey, partitionKey);
            bool found = false;
            foreach (var item in eventsFE)
            {
                if(item.Code==codeEvent)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Metodo que valida si un evento puede ser incluido, siempre y cuando ya exista un evento anterior.
        /// Por ejemplo no puede incluir un evento 032, si no hay un evento 030
        /// </summary>
        /// <param name="codeEvent">Codigo del evento a validar si se puede insertar</param>
        /// <param name="documentKey">CUFE de la factura</param>
        /// <param name="partitionKey">Partition Key de CosmosDB</param>
        /// <returns>Devuelve true si puede incluir el evento codeEvent, false si no puede</returns>
        public async Task<bool> ValidateEventDependentRule(string codeEvent, string documentKey, string partitionKey)
        {
            List<EventViewModel> eventsFE = await TraceEventsFE(documentKey, partitionKey);
            bool found = false;
            string codeToValidate = "";
            if (codeEvent == "032")
                codeToValidate = "030";

            foreach (var item in eventsFE)
            {
                if (item.Code == codeToValidate)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }
    }
}
