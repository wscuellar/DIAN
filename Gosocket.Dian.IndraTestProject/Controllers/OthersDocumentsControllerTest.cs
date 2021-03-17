using Gosocket.Dian.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;

namespace Gosocket.Dian.IndraTestProject.Controllers
{
    [TestClass]
    public class OthersDocumentsControllerTest
    {
        private OthersDocumentsController _currentController;

        [TestInitialize]
        public void TestInitialize()
        {
            _currentController = new OthersDocumentsController();
        }

        [TestMethod]
        public void Index_Result_test()
        {
            //arrange
            //act
            var viewResult = _currentController.Index() as ViewResult;
            //assert
            Assert.IsNotNull(viewResult);
        }

        [TestMethod]
        public void AddOrUpdate_Result_test()
        {
            //arrange
            //act
            var viewResult = _currentController.AddOrUpdate() as ViewResult;
            //assert
            Assert.IsNotNull(viewResult);
        }
    }
}
