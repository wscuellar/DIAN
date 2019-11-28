using Gosocket.Dian.Application;
using Gosocket.Dian.Web.Filters;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    [ExcludeFilter(typeof(Authorization))]
    public class PublicInfoController : Controller
    {
        //ProviderService _providerService = new ProviderService();
        //ClientService _clientService = new ClientService();
        GlobalDocumentService _globalDocumentService = new GlobalDocumentService();

    }
}