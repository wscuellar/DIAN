using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Cosmos;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class GlobalDocumentService
    {
        private static CosmosDbManager cosmosDbManager;
        public GlobalDocumentService()
        {
            
        }
        //public async Task<Tuple<bool, string, List<GlobalDataDocument>>> GetGlobalDocumentsAsync(string continuationToken, List<string> datesRange,
        //    int status, string documentTypeId, string senderCode, string receiverCode, string providerCode, int maxItemCount, string cufe, string referencesType)
        //{
        //    try
        //    {
        //        if (cufe != null)
        //        {
        //            try
        //            {
        //                var tableManager = new TableManager("GlobalDocValidatorDocument");
        //                var globalDocValidatorDocument = tableManager.Find<GlobalDocValidatorDocument>(cufe, cufe);
        //                string partitionKey = "";
        //                if (!string.IsNullOrEmpty(cufe))
        //                {
        //                    partitionKey = globalDocValidatorDocument.EmissionDateNumber;
        //                }

        //                return new Tuple<bool, string, List<GlobalDataDocument>>(false, null, new List<GlobalDataDocument>() { await cosmosDbManager.ReadOne(cufe, partitionKey, senderCode, providerCode) });

        //            }
        //            catch (Exception ex)
        //            {
        //                return new Tuple<bool, string, List<GlobalDataDocument>>(false, null, new List<GlobalDataDocument>());
        //            }
        //        }
        //        if (referencesType != "00")
        //        {
        //            return await cosmosDbManager.ReadAllAsync(continuationToken, datesRange, senderCode, receiverCode, providerCode, maxItemCount, referencesType);
        //        }
        //        return await cosmosDbManager.ReadAllAsync(continuationToken, datesRange, status, documentTypeId, senderCode, receiverCode, providerCode, maxItemCount);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Tuple<bool, string, List<GlobalDataDocument>>(false, null, new List<GlobalDataDocument>());
        //    }
        //}

        //public Task<GlobalDataDocument> GetGlobalDocumentAsync(string trackId, string partitionKey = null)
        //{
        //    if (partitionKey == null)
        //    {
        //        var tableManager = new TableManager("GlobalDocValidatorDocument");
        //        var globalDocValidatorDocument = tableManager.Find<GlobalDocValidatorDocument>(trackId, trackId);
        //        partitionKey = "";
        //        if (!string.IsNullOrEmpty(trackId))
        //        {
        //            partitionKey = globalDocValidatorDocument.EmissionDateNumber;
        //        }
        //    }
        //    return cosmosDbManager.ReadOne(trackId, partitionKey);
        //}

        public Task<bool> UpsertDocument(GlobalDataDocument document)
        {
            return cosmosDbManager.UpsertDocument(document);
        }
    }
}
