using System.Collections.Generic;
using System.Linq;
using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Sql.FreeBiller;
using Gosocket.Dian.Interfaces.Services;

namespace Gosocket.Dian.Application.FreeBiller
{
    public class ProfileService 
    {
        SqlDBContext sqlDBContext;

        public ProfileService()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        public List<Profile> GetAll() 
        {
            return sqlDBContext.Profile.ToList();
        }

        public List<MenuOptions> GetMenuOptions() 
        {
            return sqlDBContext.MenuOptions.ToList();
        }

        public List<MenuOptionsByProfiles> GetMenuOptionsByProfile()
        {
            return sqlDBContext.MenuOptionsByProfiles.ToList();
        }

    }
}
