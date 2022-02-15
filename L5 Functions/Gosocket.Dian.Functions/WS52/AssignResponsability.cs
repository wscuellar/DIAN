using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gosocket.Dian.Functions.WS52
{
    public static class AssignResponsability
    {

        private const string queueName="global-assign-responsability-input%Slot%";

        [FunctionName("AssignResponsability")]
        public static async Task Run([QueueTrigger(queueName, Connection = "GlobalStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            try
            {
                //var nit = "800197268";
                var loginUrl = ConfigurationManager.GetValue("AssignResponsabilityLoginUrl");
                var obj = new { };
                var result = await ApiHelpers.ExecuteRequestAsync<ResponseAssignResponsabilityToken>(loginUrl, obj);

                var headers = new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "ClientId", result.ClientId },
                    { "Authorization", $"{result.TokenType} {result.TokenId}" }
                };

                var assignResponsabilityUrl = ConfigurationManager.GetValue("AssignResponsabilityUrl");
                var request = new { idTransaccion = Guid.NewGuid().ToString(), nroIdentificacion = "55230483", fechaCambio = "20181212" };
                var result2 = await ApiHelpers.ExecuteRequestWithHeaderAsync<ResponseAssignResponsability>(assignResponsabilityUrl, request, headers);

            }
            catch (Exception ex)
            {
                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                throw;
            }
        }
    }
}
