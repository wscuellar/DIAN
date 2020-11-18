using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly IRadianContributorFileRepository _radianContributorFileRepository;
        private readonly IRadianContributorFileHistoryRepository _radianContributorFileHistoryRepository;

        public RadianAprovedService(IRadianContributorRepository radianContributorRepository, IRadianTestSetService radianTestSetService, IRadianContributorService radianContributorService, IRadianContributorFileTypeService radianContributorFileTypeService, IRadianContributorOperationRepository radianContributorOperationRepository, IRadianContributorFileRepository radianContributorFileRepository, IRadianContributorFileHistoryRepository radianContributorFileHistoryRepository)
        {
            _radianContributorRepository = radianContributorRepository;
            _radianTestSetService = radianTestSetService;
            _radianContributorService = radianContributorService;
            _radianContributorFileTypeService = radianContributorFileTypeService;
            _radianContributorOperationRepository = radianContributorOperationRepository;
            _radianContributorFileRepository = radianContributorFileRepository;
            _radianContributorFileHistoryRepository = radianContributorFileHistoryRepository;
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

        public List<RadianContributorFileType> ContributorFileTypeList(int radianContributorTypeId)
        {
            List<RadianContributorFileType> contributorTypeList = _radianContributorFileTypeService.FileTypeList()
                .Where(ft => ft.RadianContributorTypeId == radianContributorTypeId && !ft.Deleted).ToList();

            return contributorTypeList;
        }

        public ResponseMessage Update(int radianContributorOperationId)
        {
            return _radianContributorOperationRepository.Update(radianContributorOperationId);
        }

        public ResponseMessage UploadFile(Stream fileStream, string code, RadianContributorFile radianContributorFile)
        {
            string fileName = StringTools.MakeValidFileName(radianContributorFile.FileName);
            var fileManager = new FileManager(ConfigurationManager.GetValue("GlobalStorage"));
            bool result = fileManager.Upload("radiancontributor-files", code.ToLower() + "/" + fileName, fileStream);

            if (result)
            {
                Guid idFile = _radianContributorFileRepository.Update(radianContributorFile);
                return new ResponseMessage($"Archivo {radianContributorFile.FileName} con el id {idFile} guardado", "Guardado");
            }            

           return new ResponseMessage($"No se guardó el archivo {radianContributorFile.FileName}", "Nulo");
        }

        public ResponseMessage AddFileHistory(RadianContributorFileHistory radianContributorFileHistory)
        {
            radianContributorFileHistory.Timestamp = DateTime.Now;
            string idHistoryRegister = string.Empty;

            idHistoryRegister = _radianContributorFileHistoryRepository.AddRegisterHistory(radianContributorFileHistory).ToString();

            if (!string.IsNullOrEmpty(idHistoryRegister))
            {
                return new ResponseMessage($"Información registrada id: {idHistoryRegister}", "Guardado");
            }

            return new ResponseMessage($"El registro no pudo ser guardado", "Nulo");
        }
    }
}
