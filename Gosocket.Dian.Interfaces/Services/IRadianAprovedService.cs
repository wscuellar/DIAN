using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using System;
using System.Collections.Generic;
using System.IO;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IRadianApprovedService
    {
        List<Domain.RadianOperationMode> ListSoftwareModeOperation();

        List<RadianContributor> ListContributorByType(int radianContributorTypeId);

        List<Software> ListSoftwareByContributor(int radianContributorId);

       Tuple<string,string> FindNamesContributorAndSoftware(int radianContributorId, string softwareId);

        RadianContributor GetRadianContributor(int radianContributorId);

        List<RadianContributorFile> ListContributorFiles(int radianContributorId);

        RadianAdmin ContributorSummary(int contributorId);

        List<RadianContributorFileType> ContributorFileTypeList(int typeId);

        ResponseMessage Update(int radianContributorOperationId);

        ResponseMessage UploadFile(Stream fileStream, string code, RadianContributorFile radianContributorFile);

        ResponseMessage AddFileHistory(RadianContributorFileHistory radianContributorFileHistory);

        ResponseMessage UpdateRadianContributorStep(int radianContributorId, int radianContributorStep);

        int RadianContributorId(int contributorId);

    }
}
