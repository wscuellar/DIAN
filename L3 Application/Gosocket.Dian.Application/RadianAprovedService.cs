using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class RadianAprovedService : IRadianAprovedService
    {
        private readonly IRadianContributorRepository _radianContributorRepository;


        public RadianAprovedService(IRadianContributorRepository radianContributorRepository)
        {
            _radianContributorRepository = radianContributorRepository;
        }

        // Manquip
        public Tuple<string, string> FindNamesContributorAndSoftware(int radianContributorId, string softwareId)
        {
            RadianContributor radianContributor = _radianContributorRepository
                .Get(rc => rc.ContributorId == radianContributorId);

            string radianContributorName = radianContributor.Contributor.Name;
            string softwareName = radianContributor
                .Contributor
                .Softwares
                .FirstOrDefault(s => s.Id.ToString() == softwareId).Name;

            Tuple<string, string> data = Tuple.Create(radianContributorName, softwareName);

            return data;
        }

        public List<Contributor> ListContributorByType(int radianContributorTypeId)
        {
            throw new NotImplementedException();
        }

        // Manquip
        public List<Software> ListSoftwareByContributor(int radianContributorId)
        {
            return _radianContributorRepository
                .Get(rc => rc.ContributorId == radianContributorId)
                .Contributor
                .Softwares.ToList();
        }

        public List<RadianOperationMode> ListSoftwareModeOperation()
        {
            throw new NotImplementedException();
        }
    }
}
