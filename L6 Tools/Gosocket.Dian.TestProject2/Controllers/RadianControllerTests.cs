using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web.Mvc;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Domain.Entity;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianControllerTests
    {

        private RadianController _current;
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();
        private readonly Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianController(_radianContributorService.Object);
        }



       

        

      
     

    }
}
