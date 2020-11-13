using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianAprovedService
    {
        List<Domain.RadianOperationMode> ListSoftwareModeOperation();

        List<Contributor> ListContributorByType(int RadianContributorTypeId);

        List<Software> ListSoftwareByContributor(int RadianContributorId);

       Tuple<string,string> FindContributorAndSoftware(int contributorId, int softwareId);
    }
}
