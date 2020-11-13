using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianApprovedService
    {
        List<Domain.RadianOperationMode> ListSoftwareModeOperation();

        List<Contributor> ListContributorByType(int radianContributorTypeId);
        List<RadianContributor> ListContributorByType(int RadianContributorTypeId);

        List<Software> ListSoftwareByContributor(int radianContributorId);

       Tuple<string,string> FindNamesContributorAndSoftware(int radianContributorId, string softwareId);
    }
}
