using System;

namespace Gosocket.Dian.Web.Common
{
    public static class CustomClaimTypes
    {
        public const string ContributorAcceptanceStatusId = "http://dian/claim/ContributorAcceptanceStatusId";
        public const string ContributorId = "http://dian/claim/ContributorId";
        public const string ContributorBusinesName = "http://dian/claim/ContributorBusinesName";
        public const string ContributorCode = "http://dian/claim/ContributorCode";
        public const string ContributorName = "http://dian/claim/ContributorName";
        public const string ContributorOperationModeId = "http://dian/claim/ContributorOperationModeId";
        public const string ContributorTypeId = "http://dian/claim/ContributorTypeId";
        public const string GoToInvoicer = "http://dian/claim/goToInvoicer";
        public const string UserCode = "http://dian/claim/userCode";
        public const string UserEmail = "http://dian/claim/userEmail";
        public const string UserFullName = "http://dian/claim/userFullName";
        public const string ShowTestSet = "http://dian/claim/showTestSet";
        public const string IdentificationTypeId = "http://dian/claim/identificationTypeId";
        public const string ElectronicDocumentId = "http://dian/claim/ElectronicDocumentId";

        [Obsolete]
        public const string ProviderId = "http://dian/claim/showTestSet";
    }
}