using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.Activation
{
    public static class CheckContributorActivation
    {
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly TableManager globalContributorActivationTableManager = new TableManager("GlobalContributorActivation");
        private static readonly TableManager globalContributorPendingActivationTableManager = new TableManager("GlobalContributorPendingActivation");

        private static readonly string sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");
        private static readonly string sendToActivateContributorUrl = ConfigurationManager.GetValue("SendToActivateContributorUrl");

        // Set queue name
        private const string queueName = "global-check-contributor-activation-input%Slot%";

        [FunctionName("CheckContributorActivation")]
        public static async Task Run([QueueTrigger(queueName, Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            if (ConfigurationManager.GetValue("Environment") == "Hab")
            {
                try
                {
                    var pendingLastCheck = globalContributorPendingActivationTableManager.Find<ContributorPendingActivation>("LastHabilitationDateChecked", "Prod");

                    var utcNow = DateTime.UtcNow;
                    var lastHabilitationDateChecked = new ContributorPendingActivation("LastHabilitationDateChecked", "Prod") { LastHabilitationDateChecked = utcNow };

                    var pendings = globalContributorPendingActivationTableManager.FindAll<ContributorPendingActivation>();
                    var codes = pendings.Where(p => p.CheckOnlyProd && p.PartitionKey != "LastHabilitationDateChecked").Select(p => p.Code).ToList();

                    var statuses = new int[] { (int)Domain.Common.ContributorStatus.Enabled };
                    var contributors = contributorService.GetContributorsByAcceptanceStatusesId(statuses);

                    contributors = contributors.Where(c => c.ContributorTypeId == (int)Domain.Common.ContributorType.Biller).ToList();

                    contributors = contributors.Where(c => c.HabilitationDate > pendingLastCheck.LastHabilitationDateChecked).ToList();

                    contributors = contributors.Where(c => !codes.Contains(c.Code)).ToList();

                    log.Info($"{contributors.Count} news contributors for check.");

                    foreach (var c in contributors)
                    {
                        var id = c.Id.ToString();
                        var contributorProd = contributorService.GetByCode(c?.Code, sqlConnectionStringProd);
                        if (contributorProd != null && contributorProd.AcceptanceStatusId != (int)Domain.Common.ContributorStatus.Enabled)
                        {
                            var contributorPendingActivation = new ContributorPendingActivation(id, id) { Code = contributorProd.Code, Success = false, CheckOnlyProd = true };
                            globalContributorPendingActivationTableManager.InsertOrUpdate(contributorPendingActivation);
                            log.Info($"Id: {id} need verification only in prod.");
                        }
                    };

                    pendings = globalContributorPendingActivationTableManager.FindAll<ContributorPendingActivation>();

                    pendings = pendings.Where(p => !p.Success && p.CheckOnlyProd && p.PartitionKey != "LastHabilitationDateChecked");

                    foreach (var p in pendings)
                    {
                        try
                        {
                            var id = int.Parse(p.PartitionKey);

                            var requestObject = new { contributorId = id };
                            var activation = await ApiHelpers.ExecuteRequestAsync<GlobalContributorActivation>(sendToActivateContributorUrl, requestObject);

                            var guid = Guid.NewGuid().ToString();
                            var contributorActivation = new GlobalContributorActivation(p.Code, guid)
                            {
                                Success = activation.Success,
                                ContributorCode = p.Code,
                                ContributorTypeId = 1,
                                OperationModeId = 0,
                                OperationModeName = "",
                                SentToActivateBy = "Function",
                                SoftwareId = "",
                                SendDate = DateTime.UtcNow,
                                TestSetId = "",
                                Trace = activation.Trace,
                                Message = activation.Message,
                                Detail = activation.Detail,
                                Request = JsonConvert.SerializeObject(requestObject)
                            };
                            globalContributorActivationTableManager.InsertOrUpdate(contributorActivation);

                            var check = p.CheckOnlyProd ? "(Checking only in prod)" : "";

                            if (activation.Success)
                            {
                                p.Success = true;
                                p.Message = $"Forced activation OK {check}.";
                                log.Info($"Forced activation OK, id: {p.PartitionKey} {check}");
                            }
                            else
                            {
                                p.Success = false;
                                p.Message = $"Failed activation {activation.Message} {check}.";
                                log.Info($"Failed activation, id: {p.PartitionKey} {check}");
                            }
                            globalContributorPendingActivationTableManager.InsertOrUpdate(p);
                        }
                        catch (Exception ex)
                        {
                            log.Info($"{ex.Message}");
                        }
                    }

                    //
                    globalContributorPendingActivationTableManager.InsertOrUpdate(lastHabilitationDateChecked);

                    Thread.Sleep(120000);

                    var queueManager = new QueueManager(queueName);
                    queueManager.Put(JsonConvert.SerializeObject(new { date = DateTime.UtcNow }));
                    log.Info("Verificación finalizada en produción.");
                }
                catch (Exception ex)
                {
                    Thread.Sleep(120000);

                    var queueManager = new QueueManager(queueName);
                    queueManager.Put(JsonConvert.SerializeObject(new { date = DateTime.UtcNow }));

                    log.Error($"Error al verificar activación de contribuyentes en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);
                    throw;
                }
            }
        }

        public class ContributorPendingActivation : TableEntity
        {
            public ContributorPendingActivation() { }
            public ContributorPendingActivation(string pk, string rk) : base(pk, rk)
            { }

            public bool CheckOnlyProd { get; set; }
            public string Code { get; set; }
            public DateTime? LastHabilitationDateChecked { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }
    }
}
