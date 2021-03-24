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



        public List<EventDocument> GetEventsByTrackId(string trackId)
        {
            //Traemos las asociaciones de la factura = Eventos
            List<GlobalDocAssociate> associateDocumentList = TableManagerGlobalDocAssociate.FindpartitionKey<GlobalDocAssociate>(trackId.ToLower()).ToList();
            if (!associateDocumentList.Any())
                return new List<EventDocument>();

            //Organiza grupos por factura
            var groups = associateDocumentList.Where(t => t.Active && !string.IsNullOrEmpty(t.Identifier)).GroupBy(t => t.PartitionKey);
            List<EventDocument> responses = groups.Aggregate(new List<EventDocument>(), (list, source) =>
            {
                List<Task> arrayTasks = new List<Task>();

                //obtenemos el cufe
                string cufe = source.Key;
                List<GlobalDocAssociate> events = source.ToList();

                //calcula items del proceso
                List<GlobalDocValidatorDocumentMeta> meta = new List<GlobalDocValidatorDocumentMeta>();
                List<GlobalDocValidatorDocument> documents = new List<GlobalDocValidatorDocument>();
                List<GlobalDocValidatorTracking> notifications = new List<GlobalDocValidatorTracking>();
                OperationProcess(arrayTasks, events, meta, documents, notifications);

                //Unifica la data
                list.AddRange(from associate in events
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
                              });


                return list;
            });

            return responses.OrderByDescending(x => x.Event.SigningTimeStamp).ToList();
        }


        private void OperationProcess(List<Task> arrayTasks, List<GlobalDocAssociate> events, List<GlobalDocValidatorDocumentMeta> meta, List<GlobalDocValidatorDocument> documents, List<GlobalDocValidatorTracking> notifications)
        {
            //Consulta documentos en la meta
            Task operation1 = Task.Run(() =>
            {
                for (int i = 0; i < events.Count; i++)
                {
                    meta.Add(TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(events[i].RowKey, events[i].RowKey));
                }
            });

            //Trae las notificaciones por los eventos
            Task operation2 = Task.Run(() =>
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
            Task.WhenAll(arrayTasks).Wait();
        }

    }
}
