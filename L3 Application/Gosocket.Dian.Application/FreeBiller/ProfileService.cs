﻿using System.Collections.Generic;
using System.Linq;
using Gosocket.Dian.DataContext;
using Gosocket.Dian.Domain.Sql.FreeBiller;

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

    }
}