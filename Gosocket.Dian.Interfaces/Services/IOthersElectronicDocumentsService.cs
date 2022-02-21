﻿using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        OtherDocElecContributorOperations GetOtherDocElecContributorOperationBySoftwareId(Guid softwareId);

        bool UpdateOtherDocElecContributorOperation(OtherDocElecContributorOperations model);

        OtherDocElecContributorOperations GetOtherDocElecContributorOperationById(int id);

        OtherDocElecContributorOperations GetOtherDocElecContributorOperationByDocEleContributorId(int id);

        List<OtherDocElecContributorOperations> GetOtherDocElecContributorOperationsListByDocElecContributorId(int id);

        Task<int> UpdateOtherDocElecContributorOperationStatusId(OtherDocElecContributorOperations contributorOperations, Domain.Common.OtherDocElecState operationStatus);

        bool QualifiedContributor(OtherDocElecContributorOperations filters, OtherDocElecContributor otherDocElecContributorPar, string sqlConnectionStringProd);
    }
}