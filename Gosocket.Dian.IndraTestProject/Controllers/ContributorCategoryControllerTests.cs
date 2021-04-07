using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class ContributorCategoryControllerTests
    {
        private ContributorCategoryController _currentController;

        [TestInitialize]
        public void TestInitialize()
        {
            _currentController = new ContributorCategoryController();
        }

        [TestMethod()]
        public void IndexTest()
        {
            //arrange
            //act
            var viewResult = _currentController.Index() as ActionResult;
            //assert
            Assert.IsNotNull(viewResult);
        }
    }
}