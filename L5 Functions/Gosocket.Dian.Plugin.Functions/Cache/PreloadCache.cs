using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
//using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Gosocket.Dian.Plugin.Functions.Cache
{

    public static class StartUp
    {
        private static bool executed = false;
        private static DateTime lastRunExecuted = DateTime.UtcNow.AddHours(-2);
        private static readonly Application.ContributorService contributorService = new Application.ContributorService();
        private static readonly Application.SoftwareService softwareService = new Application.SoftwareService();

        static readonly TableManager contributorTableManager = new TableManager("GlobalContributor");
        static readonly TableManager softwareTableManager = new TableManager("GlobalSoftware");
        static readonly TableManager typeListTableManager = new TableManager("GlobalTypeList");

        public static void Initialize()
        {

            //var softwares = softwareService.GetAll();
            //Parallel.ForEach(softwares.Where(s => !s.Deleted), new ParallelOptions { MaxDegreeOfParallelism = 100 }, item =>
            //  {
            //      var id = item.Id.ToString();
            //      var globalSoftware = new GlobalSoftware(id, id) { Id = item.Id, Deleted = item.Deleted, Pin = item.Pin, StatusId = item.AcceptanceStatusSoftwareId };
            //      softwareTableManager.InsertOrUpdate(globalSoftware);
            //  });

            //var statuses = new int[] { 3, 4 };
            //var contributors = contributorService.GetContributorsByAcceptanceStatusesId(statuses);
            //Parallel.ForEach(contributors, new ParallelOptions { MaxDegreeOfParallelism = 100 }, item =>
            //{
            //    var globalContributor = new GlobalContributor(item.Code, item.Code) { StatusId = item.AcceptanceStatusId, Code = item.Code, TypeId = item.ContributorTypeId };
            //    contributorTableManager.InsertOrUpdate(globalContributor);
            //});


            if (executed && DateTime.UtcNow.Subtract(lastRunExecuted).TotalHours < 24) return;
            try
            {
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(25)
                };

                var contributors = contributorTableManager.FindAll<GlobalContributor>();
                Parallel.ForEach(contributors, new ParallelOptions { MaxDegreeOfParallelism = 100 }, item =>
                {
                    var itemKey = $"contributor-{item.Code}";
                    InstanceCache.ContributorInstanceCache.Set(new CacheItem(itemKey, item), policy);
                });

                var softwares = softwareTableManager.FindAll<GlobalSoftware>();
                Parallel.ForEach(softwares, new ParallelOptions { MaxDegreeOfParallelism = 100 }, item =>
                {
                    InstanceCache.SoftwareInstanceCache.Set(new CacheItem(item.Id.ToString(), item), policy);
                });

                //var typesList = typeListTableManager.FindByPartition<GlobalTypeList>("new-dian-ubl21");
                //InstanceCache.TypesListInstanceCache.Set(new CacheItem("TypesList", typesList), policy);
            }
            catch (Exception)
            {
                //log.Error($"Error al cargar contribuyentes en caché. Message: {ex.Message}, StackTrace: {ex.StackTrace}");
            }
            executed = true;
            lastRunExecuted = DateTime.UtcNow;
        }
    }
}
