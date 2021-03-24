using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Domain.Entity;
using System.Collections.Generic;
using Gosocket.Dian.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class AssociateDocumentService : IAssociateDocuments
    {

        static TableManager TableManagerGlobalDocAssociate;
        static TableManager TableManagerGlobalDocValidatorDocumentMeta;
        static TableManager TableManagerGlobalDocValidatorDocument;
        static TableManager globalDocValidatorTrackingTableManager;
        static string s = Initializate();

        static string Initializate()
        {
            TableManagerGlobalDocAssociate = new TableManager("GlobalDocAssociate");
            TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
            TableManagerGlobalDocValidatorDocument = new TableManager("GlobalDocValidatorDocument");
            globalDocValidatorTrackingTableManager = new TableManager("GlobalDocValidatorTracking");
            return "Ok";
        }

        public List<InvoiceWrapper> GetEventsByTrackId(string trackId)
        {
            //Traemos las asociaciones de la factura = Eventos
            List<GlobalDocAssociate> associateDocumentList = TableManagerGlobalDocAssociate.FindpartitionKey<GlobalDocAssociate>(trackId.ToLower()).ToList();
            if (!associateDocumentList.Any())
                return new List<InvoiceWrapper>();

            //Organiza grupos por factura
            var groups = associateDocumentList.Where(t => t.Active && !string.IsNullOrEmpty(t.Identifier)).GroupBy(t => t.PartitionKey);
            List<InvoiceWrapper> responses = groups.Aggregate(new List<InvoiceWrapper>(), (list, source) =>
            {              
                //obtenemos el cufe
                string cufe = source.Key;
                List<GlobalDocAssociate> events = source.ToList();

                //calcula items del proceso
                List<GlobalDocValidatorDocumentMeta> meta = new List<GlobalDocValidatorDocumentMeta>();
                List<GlobalDocValidatorDocument> documents = new List<GlobalDocValidatorDocument>();
                List<GlobalDocValidatorTracking> notifications = new List<GlobalDocValidatorTracking>();

                GlobalDocValidatorDocumentMeta invoice = OperationProcess(events, meta, documents, notifications, cufe);

                //Unifica la data
                var eventDoc = from associate in events
                              join docMeta in meta on associate.RowKey equals docMeta.PartitionKey
                              join document in documents on docMeta.Identifier equals document.PartitionKey
                              select new EventDocument()
                              {
                                  Cufe = cufe,
                                  Associate = associate,
                                  Event = docMeta,
                                  Document = document,
                                  IsNotifications = notifications.Any(t => t.PartitionKey == document.DocumentKey),
                                  Notifications = notifications.Where(t => t.PartitionKey == document.DocumentKey).ToList()
                              };

                InvoiceWrapper invoiceWrapper = new InvoiceWrapper()
                {
                    Cufe = cufe,
                    Invoice = invoice,
                    Events = eventDoc.OrderByDescending(t => t.Event.SigningTimeStamp).ToList()
                };

                list.Add(invoiceWrapper);
                
                return list;
            });

            return responses;
        }


        private GlobalDocValidatorDocumentMeta OperationProcess(List<GlobalDocAssociate> events, List<GlobalDocValidatorDocumentMeta> meta, List<GlobalDocValidatorDocument> documents, List<GlobalDocValidatorTracking> notifications, string cufe)
        {
            List<Task> arrayTasks = new List<Task>();
            GlobalDocValidatorDocumentMeta invoice = new GlobalDocValidatorDocumentMeta();

            //Consulta documentos en la meta Factura
            Task operation1 = Task.Run(() =>
            {               
                 invoice = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(cufe, cufe);               
            });

            //Consulta documentos en la meta
            Task operation2 = Task.Run(() =>
            {
                for (int i = 0; i < events.Count; i++)
                {
                    meta.Add(TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(events[i].RowKey, events[i].RowKey));
                }
            });
            
            //Trae las notificaciones por los eventos
            Task operation3 = Task.Run(() =>
            {
                //Documentos
                for (int i = 0; i < events.Count; i++)
                {
                    GlobalDocValidatorDocument itemDocument = TableManagerGlobalDocValidatorDocument.FindByDocumentKey<GlobalDocValidatorDocument>(events[i].Identifier, events[i].Identifier, events[i].RowKey);
                    if (itemDocument != null && (itemDocument.ValidationStatus == 0 || itemDocument.ValidationStatus == 1 || itemDocument.ValidationStatus == 10))
                        documents.Add(itemDocument);
                }

                //Validaciones para la notificacion.
                List<GlobalDocValidatorDocument> documentsByNotification = documents.Where(t => t.ValidationStatus == 10).ToList();
                for (int i = 0; i < documentsByNotification.Count; i++)
                {
                    notifications.AddRange(globalDocValidatorTrackingTableManager.FindByPartition<GlobalDocValidatorTracking>(documentsByNotification[i].DocumentKey));
                }

            });

            arrayTasks.Add(operation1);
            arrayTasks.Add(operation2);
            arrayTasks.Add(operation3);
            Task.WhenAll(arrayTasks).Wait();

            return invoice;
        }

    }
}
