using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianTradingSystemControllerTests
    {
        private readonly RadianTradingSystemController _radianTradingSystemController;
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();

        public RadianTradingSystemControllerTests() =>
            _radianTradingSystemController = new RadianTradingSystemController(_radianContributorService.Object);

        [TestMethod()]
        public void Index_Result_Test()
        {
            Mock<HttpContextBase> fakeHttpContext = new Mock<HttpContextBase>();
            GenericIdentity fakeIdentity = new GenericIdentity("User");
            GenericPrincipal principal = new GenericPrincipal(fakeIdentity, null);

            fakeHttpContext.Setup(t => t.User).Returns(principal);
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(t => t.HttpContext).Returns(fakeHttpContext.Object);

            _radianTradingSystemController.ControllerContext = controllerContext.Object;

            _radianContributorService.Setup(rcf => rcf.CreateContributor(It.IsAny<int>(),
                                                                         It.IsAny<Domain.Common.RadianState>(),
                                                                         It.IsAny<Domain.Common.RadianContributorType>(),
                                                                         It.IsAny<Domain.Common.RadianOperationMode>(),
                                                                         It.IsAny<string>()))
                .Returns(new RadianContributor());

            ActionResult result = _radianTradingSystemController.Index(1);
        }
    }
}
