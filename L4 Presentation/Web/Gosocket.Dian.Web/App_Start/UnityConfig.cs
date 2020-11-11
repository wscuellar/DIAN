using Gosocket.Dian.Application;
using Gosocket.Dian.Application.Managers;
using Gosocket.Dian.DataContext.Repositories;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace Gosocket.Dian.Web
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            #region Repositories

            container.RegisterType<IRadianContributorFileHistoryRepository, RadianContributorFileHistoryRepository>();
            container.RegisterType<IRadianContributorFileRepository, RadianContributorFileRepository>();
            container.RegisterType<IRadianContributorFileStatusRepository, RadianContributorFileStatusRepository>();
            container.RegisterType<IRadianContributorRepository, RadianContributorRepository>();
            container.RegisterType<IRadianContributorTypeRepository, RadianContributorTypeRepository>();

            #endregion

            #region Services

            container.RegisterType<IContributorService, ContributorService>();
            container.RegisterType<IRadianContributorService, RadianContributorService>();
            container.RegisterType<IRadianContributorFileTypeService, RadianContributorFileTypeService>();

            #endregion

            #region Managers

            container.RegisterType<IRadianTestSetResultManager, RadianTestSetResultManager>();

            #endregion

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}