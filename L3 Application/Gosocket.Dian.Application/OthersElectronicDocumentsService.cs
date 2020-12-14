using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class OthersElectronicDocumentsService : IOthersElectronicDocumentsService
    {
        SqlDBContext sqlDBContext;
        private readonly IContributorService _contributorService;

        public OthersElectronicDocumentsService(IContributorService contributorService )
        {
            _contributorService = contributorService;
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public ResponseMessage Validation(string userCode, string Accion, int IdElectronicDocument, string complementeTexto, int ContributorIdType)
        {
            Contributor contributor = _contributorService.GetByCode(userCode);
            if (contributor == null || contributor.AcceptanceStatusId != 4)
                return new ResponseMessage(TextResources.NonExistentParticipant, TextResources.alertType);
             

            if (Accion == "SeleccionElectronicDocument")
                return new ResponseMessage(  TextResources.OthersElectronicDocumentsSelect_Confirm .Replace("@docume", complementeTexto), TextResources.confirmType);

            if (Accion == "SeleccionParticipante")
                return new ResponseMessage(TextResources.OthersElectronicDocumentsSelectParticipante_Confirm.Replace("@Participante", complementeTexto), TextResources.confirmType);

            if (Accion == "SeleccionOperationMode")
                return new ResponseMessage(TextResources.OthersElectronicDocumentsSelectOperationMode_Confirm.Replace("@Participante", complementeTexto), TextResources.confirmType);

            if (Accion == "CancelRegister")
                return new ResponseMessage(TextResources.OthersElectronicDocumentsSelectOperationMode_Confirm.Replace("@Participante", complementeTexto), TextResources.confirmType);


            return new ResponseMessage(TextResources.FailedValidation, TextResources.alertType);
        }

        public List<Gosocket.Dian.Domain.OperationMode> GetOperationModes()
        {
            using (var context = new SqlDBContext())
            {
                return context.OperationModes.ToList();
            }
        }

    }
}
