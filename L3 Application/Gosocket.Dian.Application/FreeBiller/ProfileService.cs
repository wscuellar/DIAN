using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Sql.FreeBiller;
using Gosocket.Dian.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;

namespace Gosocket.Dian.Application.FreeBiller
{
    public class ProfileService : IProfileService
    {
        readonly SqlDBContext sqlDBContext;

        public ProfileService()
        {
            if (sqlDBContext == null)
                sqlDBContext = new SqlDBContext();
        }

        /// <summary>
        /// Obtiene todos los perfiles para el Facturador Gratuito.
        /// </summary>
        /// <returns>List<Profile></returns>
        public List<Profile> GetAll()
        {
            return sqlDBContext.Profile.ToList();
        }

        /// <summary>
        /// Crea un nuevo perfil para el facturador gratuito.
        /// </summary>
        /// <param name="newPerfil">Object Profile que se va a guardar en DB.</param>
        /// <returns>Nuevo objeto de la DB. Incluyendo el nuevo ID.</returns>
        public Profile CreateNewProfile(Profile newPerfil)
        {
            sqlDBContext.Profile.Add(newPerfil);
            sqlDBContext.SaveChanges();
            return newPerfil;
        }

        /// <summary>
        /// Obtiene todas las opciones del menú para el facturador gratuito.
        /// </summary>
        /// <returns>List<MenuOptions></returns>
        public List<MenuOptions> GetMenuOptions()
        {
            return sqlDBContext.MenuOptions.ToList();
        }

        /// <summary>
        /// Guardas la lista de opciones de menú por el perfil. 
        /// </summary>
        /// <param name="menuOptionsByProfiles">Lista con los Id de MenuOption y el perfil.</param>
        /// <returns>bool. Indicando si el proceso de guardao fue exitoso o no.</returns>
        public bool SaveOptionsMenuByProfile(List<MenuOptionsByProfiles> menuOptionsByProfiles)
        {

            foreach (MenuOptionsByProfiles newMenu in menuOptionsByProfiles)
            {
                sqlDBContext.MenuOptionsByProfiles.Add(newMenu);
            }
            return sqlDBContext.SaveChanges() > 0;
        }

        public List<MenuOptionsByProfiles> GetMenuOptionsByProfile()
        {
            return sqlDBContext.MenuOptionsByProfiles.ToList();
        }


        public List<MenuOptionsByProfiles> GetMenuOptionsByProfile(int profileId)
        {
            return sqlDBContext.MenuOptionsByProfiles.Where(t => t.ProfileId == profileId).ToList();
        }


        public List<MenuOptions> GetOptionsByProfile(int profileId)
        {
            List<MenuOptions> menuOptions = GetMenuOptions().ToList();
            List<MenuOptionsByProfiles> optionsByProfile = profileId > 0 ? GetMenuOptionsByProfile(profileId) : new List<MenuOptionsByProfiles>();
            return GetChildren(menuOptions, null, optionsByProfile);
        }

        static List<MenuOptions> GetChildren(List<MenuOptions> menuOptions, int? parentId, List<MenuOptionsByProfiles> optionsByProfile)
        {
            return menuOptions
                    .Where(c => c.ParentId == parentId)
                    .Select(c => new MenuOptions
                    {
                        Id = c.Id,
                        Name = c.Name,
                        ParentId = c.ParentId,
                        IsActive = c.IsActive,
                        MenuLevel = c.MenuLevel,
                        SeccionOptions = c.SeccionOptions,
                        Children = GetChildren(menuOptions, c.Id, optionsByProfile),
                        Checked = optionsByProfile.Any(t => t.MenuOptionId == c.Id)
                    })
                    .ToList();
        }

    }
}
