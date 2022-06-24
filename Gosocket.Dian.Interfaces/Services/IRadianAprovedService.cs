using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianApprovedService
    {

        List<RadianContributor> ListContributorByType(int radianContributorTypeId);

        RadianContributor GetRadianContributor(int radianContributorId);

        List<RadianContributorFile> ListContributorFiles(int radianContributorId);

        RadianAdmin ContributorSummary(int contributorId, int radianContributorType);

        List<RadianContributorFileType> ContributorFileTypeList(int typeId);

        ResponseMessage OperationDelete(RadianContributorOperation operationToDelete);
        ResponseMessage OperationDelete(int radianContributorOperationId);

        ResponseMessage UploadFile(Stream fileStream, string code, RadianContributorFile radianContributorFile);

        ResponseMessage AddFileHistory(RadianContributorFileHistory radianContributorFileHistory);

        ResponseMessage UpdateRadianContributorStep(int radianContributorId, int radianContributorStep);

        int RadianContributorId(int contributorId, int contributorTypeId, string state);

        Software SoftwareByContributor(int contributorId);
        
        RadianTestSet GetTestSet(string softwareType);

        Task<ResponseMessage> AddRadianContributorOperation(RadianContributorOperation radianContributorOperation, RadianSoftware software, RadianTestSet testSet, bool isInsert, bool validateOperation);

        RadianTestSetResult RadianTestSetResultByNit(string nit, string idTestSet);

        RadianContributorOperationWithSoftware ListRadianContributorOperations(int radianContributorId);

        List<RadianSoftware> SoftwareList(int radianContributorId);

        List<RadianContributor> AutoCompleteProvider(int contributorId, int contributorTypeId, RadianOperationModeTestSet softwareType, string term);

        PagedResult<RadianCustomerList> CustormerList(int radianContributorId, string code, RadianState radianState, int page, int pagesize);

        PagedResult<RadianContributorFileHistory> FileHistoryFilter(int radiancontributorId, string fileName, string initial, string end, int page, int pagesize);

        RadianSoftware GetSoftware(Guid id);

        RadianSoftware GetSoftware(int radianContributorId, int softwareType);
        void DeleteSoftware(Guid softwareId);
        List<RadianContributorOperation> OperationsBySoftwareId(Guid id);
        bool ResetRadianOperation(int radianOperationId);
    }
}
