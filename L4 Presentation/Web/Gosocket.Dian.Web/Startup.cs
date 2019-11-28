using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Gosocket.Dian.Web.Startup))]
namespace Gosocket.Dian.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
