using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianApprovedService
    {
        List<Domain.RadianOperationMode> ListSoftwareModeOperation();

        List<RadianContributor> ListContributorByType(int RadianContributorTypeId);

        List<Software> ListSoftwareByContributor(int RadianContributorId);

       Tuple<string,string> FindNamesContributorAndSoftware(int contributorId, int softwareId);
    }
}
