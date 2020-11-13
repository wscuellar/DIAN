using Gosocket.Dian.Domain;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianAprovedService
    {
        List<Domain.RadianOperationMode> ListSoftwareModeOperation();

        List<Contributor> ListContributorByType(int radianContributorTypeId);

        List<Software> ListSoftwareByContributor(int radianContributorId);

       Tuple<string,string> FindNamesContributorAndSoftware(int radianContributorId, string softwareId);
    }
}
