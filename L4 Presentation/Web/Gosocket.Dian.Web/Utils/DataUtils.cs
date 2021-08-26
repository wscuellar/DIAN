using Gosocket.Dian.Application;
using Gosocket.Dian.Application.Managers;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Web.Utils
{
    public static class DataUtils
    {
        //public static List<AcceptanceStatus> GetAcceptanceStatusesProvider()
        //{
        //    return new ProviderService().GetAcceptanceStatuses();
        //}

        //public static List<AcceptanceStatusSoftware> GetAcceptanceStatusesSoftware()
        //{
        //    return new ProviderService().GetAcceptanceStatusesSoftware();
        //}

        //public static List<AcceptanceStatusClient> GetAcceptanceStatusesClient()
        //{
        //    return new ProviderService().GetAcceptanceStatusesClient();
        //}
        //public static AcceptanceStatus GetAcceptanceStatusById(int statusId)
        //{
        //    return new ProviderService().GetAcceptanceStatusById(statusId);
        //}

        //public static int GetSoftwareCount()
        //{
        //    return new SoftwareService().GetCount();
        //}
        //public static List<ProviderFileStatus> GetProviderFileStatuses()
        //{
        //    return new ProviderService().GetProviderFilesStatuses();
        //}

        //Obsolete
        //public static List<ProviderFileType> GetMandatoryProviderFileTypes()
        //{
        //    return new ProviderService().GetMandatoryProviderFileTypes();
        //}

        public static List<ContributorFileType> GetMandatoryContributorFileTypes()
        {
            return new ContributorService().GetMandatoryContributorFileTypes();
        }

        //public static List<ProviderFileType> GetNotRequiredProviderFileTypes()
        //{
        //    return new ProviderService().GetNotRequiredProviderFileTypes();
        //}

        public static List<ContributorFileType> GetNotRequiredContributorFileTypes()
        {
            return new ContributorService().GetNotRequiredContributorFileTypes();
        }
        public static string GetProviderFileStatusStyle(int status)
        {
            switch (status)
            {
                case 1:
                    return "text-yellow";
                case 2:
                    return "text-gosocket";
                case 3:
                    return "text-red";
                default:
                    return "text-warning";
            }
        }
        public static string GetProviderStatusStyle(int status)
        {
            switch (status)
            {
                case 1:
                    return "text-warning";
                case 2:
                    return "text-yellow";
                case 3:
                    return "text-info";
                case 4:
                    return "text-gosocket";
                default:
                    return "text-red";
            }
        }

        public static List<GlobalDocValidatorCategory> GetValidatorCategories()
        {
            var categoryManager = new CategoryManager();
            return categoryManager.GetAll().ToList();
        }
    }
}