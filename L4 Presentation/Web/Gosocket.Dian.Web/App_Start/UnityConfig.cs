using Gosocket.Dian.Application;
using Gosocket.Dian.Interfaces;
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
            container.RegisterType<IContributorService, ContributorService>();
            container.RegisterType<IRadianContributorService, RadianContributorService>();
            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}