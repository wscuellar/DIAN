using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IOthersElectronicDocumentsService
    {

        ResponseMessage Validation(string userCode, string Accion, int IdElectronicDocument, string complementeTexto, int ParticipanteId);

        ResponseMessage AddOtherDocElecContributorOperation(OtherDocElecContributorOperations ContributorOperation, OtherDocElecSoftware software,  bool isInsert, bool validateOperation);

        bool ChangeParticipantStatus(int contributorId, string newState, int ContributorTypeId, string actualState, string description);

        bool ChangeContributorStep(int ContributorId, int step);
        PagedResult<OtherDocElecCustomerList> CustormerList(int ContributorId, string code, OtherDocElecState nState, int page, int pagesize);

        ResponseMessage OperationDelete(int ODEContributorId);
        ResponseMessage ValidaSoftwareDelete(int ODEContributorId);

        OtherDocElecContributorOperations GetOtherDocElecContributorOperationBySoftwareId(Guid softwareId);

        ResponseMessage AddOtherDocElecContributorOperationNew(OtherDocElecContributorOperations ContributorOperation, OtherDocElecSoftware software, bool isInsert, bool validateOperation, int ContributorId, int ContributorIdType, int OperationModeId);

        bool UpdateOtherDocElecContributorOperation(OtherDocElecContributorOperations model);

        OtherDocElecContributorOperations GetOtherDocElecContributorOperationById(int id);

        OtherDocElecContributorOperations GetOtherDocElecContributorOperationByDocEleContributorId(int id);

        List<OtherDocElecContributorOperations> GetOtherDocElecContributorOperationsListByDocElecContributorId(int id);
    }
}