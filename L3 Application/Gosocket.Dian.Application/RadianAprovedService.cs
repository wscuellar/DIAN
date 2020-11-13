using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Application
{
    public class RadianAprovedService : IRadianAprovedService
    {

        public RadianAprovedService()
        {
                
        }


        public List<Contributor> ListContributorByType(int RadianContributorTypeId)
        {
            throw new NotImplementedException();
        }

        public List<RadianOperationMode> ListSoftwareModeOperation()
        {
            throw new NotImplementedException();
        }
    }
}
