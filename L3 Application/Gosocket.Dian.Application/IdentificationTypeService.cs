using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application
{
    public class IdentificationTypeService
    {
        SqlDBContext sqlDBContext;
        public IdentificationTypeService()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public IEnumerable<IdentificationType> List()
        {
            return sqlDBContext.IdentificationTypes.ToList();

        }
    }
}
