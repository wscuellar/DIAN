using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public class RadianAprovedService : IRadianApprovedService
    {
        private readonly IRadianContributorRepository _contributorRepository;

        public RadianAprovedService(IRadianContributorRepository contributorRepository)
        {
            _contributorRepository = contributorRepository;
        }


        // Manquip
        public Tuple<string, string> FindNamesContributorAndSoftware(int contributorId, int softwareId)
        {
            throw new NotImplementedException();
        }

        public List<RadianContributor> ListContributorByType(int radianContributorTypeId)
        {
          return   _contributorRepository.List(t => t.RadianContributorTypeId == radianContributorTypeId);
        }

        // Manquip
        public List<Software> ListSoftwareByContributor(int RadianContributorId)
        {
            throw new NotImplementedException();
        }

        public List<RadianOperationMode> ListSoftwareModeOperation()
        {
            throw new NotImplementedException();
        }
    }
}
