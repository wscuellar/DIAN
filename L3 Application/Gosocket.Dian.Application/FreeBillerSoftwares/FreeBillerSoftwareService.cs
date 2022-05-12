using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application.FreeBillerSoftwares
{
    public static class FreeBillerSoftwareService
    {
        public static string Get(int electronicDocumentId)
        {
            if (electronicDocumentId == (int)ElectronicsDocuments.SupportDocument)
            {
                return ConfigurationManager.GetValue("BillerSoftwareIdSupportDocument");
            }

            return ConfigurationManager.GetValue("BillerSoftwareId");
        }

        public static string Get(string documentTypeId)
        {
            if (OtherDocumentsDocumentType.IsSupportDocument(documentTypeId))
            {
                return ConfigurationManager.GetValue("BillerSoftwareIdSupportDocument");
            }

            return ConfigurationManager.GetValue("BillerSoftwareId");
        }
    }
}
