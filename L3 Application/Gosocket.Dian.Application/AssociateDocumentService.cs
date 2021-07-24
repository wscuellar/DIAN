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
        static TableManager GlobalDocReferenceAttorneyTableManager;
        static string s = Initializate();

        static string Initializate()
        {
            TableManagerGlobalDocAssociate = new TableManager("GlobalDocAssociate");
            TableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
            TableManagerGlobalDocValidatorDocument = new TableManager("GlobalDocValidatorDocument");
            globalDocValidatorTrackingTableManager = new TableManager("GlobalDocValidatorTracking");
            GlobalDocReferenceAttorneyTableManager = new TableManager("GlobalDocReferenceAttorney");
            return "Ok";
        }

        public List<InvoiceWrapper> GetEventsByTrackId(string trackId)
        {
            //Traemos las asociaciones de la factura = Eventos
            List<GlobalDocAssociate> associateDocumentList = TableManagerGlobalDocAssociate.FindpartitionKey<GlobalDocAssociate>(trackId.ToLower()).ToList();
            if (!associateDocumentList.Any())
                return new List<InvoiceWrapper>();

            return InvoiceProcess(associateDocumentList);
        }

        private List<InvoiceWrapper> InvoiceProcess(List<GlobalDocAssociate> associateDocumentList)
        {
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
                List<GlobalDocReferenceAttorney> attorneys = new List<GlobalDocReferenceAttorney>();

                GlobalDocValidatorDocumentMeta invoice = OperationProcess(events, meta, documents, notifications, attorneys, cufe);

                //Unifica la data
                var eventDoc = from associate in events
                               join docMeta in meta on associate.RowKey equals docMeta.PartitionKey
                               join document in documents on docMeta.Identifier equals document.PartitionKey
                               join attorney in attorneys on docMeta.PartitionKey equals attorney.PartitionKey into left
                               from item in left.DefaultIfEmpty()
                               select new EventDocument()
                               {
                                   Cufe = cufe,
                                   Associate = associate,
                                   DocumentMeta = docMeta,
                                   Attorney = item,
                                   ValidateDocument = document,
                                   IsNotifications = notifications.Any(t => t.PartitionKey == document.DocumentKey),
                                   Notifications = notifications.Where(t => t.PartitionKey == document.DocumentKey).ToList()
                               };

                InvoiceWrapper invoiceWrapper = new InvoiceWrapper()
                {
                    Cufe = cufe,
                    Invoice = invoice,
                    Documents = eventDoc.OrderByDescending(t => t.DocumentMeta.SigningTimeStamp).ToList()
                };

                list.Add(invoiceWrapper);

                return list;
            });

            return responses;
        }

        private GlobalDocValidatorDocumentMeta OperationProcess(List<GlobalDocAssociate> associations, List<GlobalDocValidatorDocumentMeta> meta, List<GlobalDocValidatorDocument> documents, List<GlobalDocValidatorTracking> notifications, List<GlobalDocReferenceAttorney> attorneys, string cufe)
        {
            List<Task> arrayTasks = new List<Task>();
            GlobalDocValidatorDocumentMeta invoice = new GlobalDocValidatorDocumentMeta();

            //Consulta documentos en la meta Factura
            Task operation1 = Task.Run(() =>
            {
                invoice = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(cufe, cufe);
            });

            //Consulta documentos en la meta para los eventos
            Task operation2 = Task.Run(() =>
            {
                for (int i = 0; i < associations.Count; i++)
                {
                    GlobalDocValidatorDocumentMeta current = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(associations[i].RowKey, associations[i].RowKey);
                    //if (!string.IsNullOrEmpty(current.EventCode))
                    meta.Add(current);
                }
            });

            //Trae las notificaciones por los eventos
            Task operation3 = Task.Run(() =>
            {
                //Documentos
                for (int i = 0; i < associations.Count; i++)
                {
                    GlobalDocValidatorDocument itemDocument = TableManagerGlobalDocValidatorDocument.FindByDocumentKey<GlobalDocValidatorDocument>(associations[i].Identifier, associations[i].Identifier, associations[i].RowKey);
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

            //Mandatos (DocumentReferencesAttorney)
            Task operation4 = Task.Run(() =>
            {
                //Lista la referencias de mandato
                for (int i = 0; i < associations.Count; i++)
                {
                    attorneys.AddRange(GlobalDocReferenceAttorneyTableManager.FindByPartition<GlobalDocReferenceAttorney>(associations[i].RowKey));
                }

                //Organiza los cufes para la busqueda
                var attorneyCudes = attorneys.Where(x => x.DocReferencedEndAthorney != null).Select(t => new
                {
                    OriginalCude = t.PartitionKey,
                    CancelAttorneyCude = t.DocReferencedEndAthorney ?? string.Empty
                }).ToList();

                //Obtiene los eventos de la documentMeta
                for (int i = 0; i < attorneyCudes.Count; i++)
                {
                    if (!string.IsNullOrEmpty(attorneyCudes[i].CancelAttorneyCude))
                        meta.Add(TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(attorneyCudes[i].CancelAttorneyCude, attorneyCudes[i].CancelAttorneyCude));
                }

            });

            arrayTasks.Add(operation1);
            arrayTasks.Add(operation2);
            arrayTasks.Add(operation3);
            arrayTasks.Add(operation4);

            Task.WhenAll(arrayTasks).Wait();

            return invoice;
        }

        //public InvoiceWrapper GetEventFromInvoice(string trackId)
        //{
        //    InvoiceWrapper response = new InvoiceWrapper();

        //    var globalDocValidatorDocumentMeta = TableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);

        //    var identifier = globalDocValidatorDocumentMeta.Identifier;

        //    GlobalDocValidatorDocument globalDocValidatorDocument = TableManagerGlobalDocValidatorDocument.FindByDocumentKey<GlobalDocValidatorDocument>(identifier, identifier, trackId);




        //    return response;
        //}

    }
}
