using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application
{
    public class RadianAprovedService : IRadianAprovedService
    {

        public RadianAprovedService()
        {
                
        }
        // Manquip
        public Tuple<string, string> FindNamesContributorAndSoftware(int contributorId, int softwareId)
        {
            throw new NotImplementedException();
        }

        public List<Contributor> ListContributorByType(int RadianContributorTypeId)
        {
            throw new NotImplementedException();
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
