using Gosocket.Dian.Domain.Sql.FreeBiller;
using System.Collections.Generic;

namespace Gosocket.Dian.Interfaces.Services
{
    public interface IProfileService
    {
        Profile CreateNewProfile(Profile newPerfil);
        List<Profile> GetAll();
        List<MenuOptions> GetMenuOptions();
        List<MenuOptionsByProfiles> GetMenuOptionsByProfile();
        List<MenuOptionsByProfiles> GetMenuOptionsByProfile(int profileId);
        List<MenuOptions> GetOptionsByProfile(int profileId);
        bool SaveOptionsMenuByProfile(List<MenuOptionsByProfiles> menuOptionsByProfiles);
    }
}
