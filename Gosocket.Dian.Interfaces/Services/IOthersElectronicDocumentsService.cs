﻿using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IOthersElectronicDocumentsService
    {

        ResponseMessage Validation(string userCode, string Accion, int IdElectronicDocument, string complementeTexto, int ParticipanteId);

        ResponseMessage AddOtherDocElecContributorOperation(OtherDocElecContributorOperations ContributorOperation, OtherDocElecSoftware software,  bool isInsert, bool validateOperation);

        bool ChangeParticipantStatus(int contributorId, string newState, int ContributorTypeId, string actualState, string description);

        bool ChangeContributorStep(int ContributorId, int step);
    }
}
