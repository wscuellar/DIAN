using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class RadianAprovedService : IRadianApprovedService
    {
        private readonly IRadianContributorRepository _radianContributorRepository;
        private readonly IRadianTestSetService _radianTestSetService;
        private readonly IRadianContributorService _radianContributorService;
        private readonly IRadianContributorFileTypeService _radianContributorFileTypeService;
        private readonly IRadianContributorOperationRepository _radianContributorOperationRepository;
        private ResponseMessage responseMessage;

        public RadianAprovedService(IRadianContributorRepository radianContributorRepository, IRadianTestSetService radianTestSetService, IRadianContributorService radianContributorService, IRadianContributorFileTypeService radianContributorFileTypeService, IRadianContributorOperationRepository radianContributorOperationRepository)
        {
            _radianContributorRepository = radianContributorRepository;
            _radianTestSetService = radianTestSetService;
            _radianContributorService = radianContributorService;
            _radianContributorFileTypeService = radianContributorFileTypeService;
            _radianContributorOperationRepository = radianContributorOperationRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radianContributorId"></param>
        /// <param name="softwareId"></param>
        /// <returns></returns>
        public Tuple<string, string> FindNamesContributorAndSoftware(int radianContributorId, string softwareId)
        {
            string radianContributorName = "No se encontró el contribuyente";
            string softwareName = "No hay software asociado al contribuyente";

            RadianContributor radianContributor = _radianContributorRepository
                .Get(rc => rc.ContributorId == radianContributorId);

            if (radianContributor != null)
            {
                radianContributorName = radianContributor.Contributor.Name;
                Software software = radianContributor
                    .Contributor
                    .Softwares
                    .FirstOrDefault(s => s.Id.ToString() == softwareId);

                if (software != null)
                {
                    softwareName = software.Name;
                }
            }

            Tuple<string, string> data = Tuple.Create(radianContributorName, softwareName);

            return data;
        }

        public List<RadianContributor> ListContributorByType(int radianContributorTypeId)
        {
            return _radianContributorRepository.List(t => t.RadianContributorTypeId == radianContributorTypeId);
        }

        // Manquip
        public List<Software> ListSoftwareByContributor(int radianContributorId)
        {
            List<Software> softwares = _radianContributorRepository
                .Get(rc => rc.ContributorId == radianContributorId)
                .Contributor
                .Softwares.ToList();

            return softwares;
        }

        public List<RadianOperationMode> ListSoftwareModeOperation()
        {
            List<RadianOperationMode> list = _radianTestSetService.OperationModeList();
            return list;
        }

        public RadianContributor GetRadianContributor(int radianContributorId)
        {
            RadianContributor radianContributor = _radianContributorRepository
                .Get(rc => rc.ContributorId == radianContributorId);

            return radianContributor;
        }

        public List<RadianContributorFile> ListContributorFiles(int radianContributorId)
        {
            RadianContributor radianContributor = _radianContributorRepository
                .Get(rc => rc.ContributorId == radianContributorId);

            return radianContributor.RadianContributorFile.ToList();
        }

        public RadianAdmin ContributorSummary(int contributorId)
        {
            return _radianContributorService.ContributorSummary(contributorId);
        }

        public List<RadianContributorFileType> ContributorFileTypeList(int typeId)
        {
            List<RadianContributorFileType> contributorTypeList = _radianContributorFileTypeService.FileTypeList()
                .Where(ft => ft.Id == typeId && !ft.Deleted).ToList();


            return contributorTypeList;
        }

        public ResponseMessage Update(int radianContributorOperationId)
        {
            bool updateResponse = _radianContributorOperationRepository.Update(radianContributorOperationId);

            if (updateResponse)
            {
                responseMessage = new ResponseMessage("Datos actuzalizados corresctamente", "Actualizado");
            }
            else
            {
                responseMessage = new ResponseMessage("Registro no encontrado en la base de datos", "Nulo");
            }

            return responseMessage;
        }

        public ResponseMessage UploadFile()
        {


            //ViewBag.CurrentPage = Navigation.NavigationEnum.Provider;
            //var model = new ContributorUploadFileViewModel
            //{
            //    Id = id,
            //    Code = code,
            //    FileId = fileId,
            //    FileTypeId = fileTypeId,
            //    FileTypeName = fileTypeName
            //};

            return responseMessage = new ResponseMessage("Archivo guardado correctamente", "Nulo");
        }
    }
}
