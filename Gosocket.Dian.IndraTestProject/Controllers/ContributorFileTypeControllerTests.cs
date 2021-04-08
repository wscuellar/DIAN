using Gosocket.Dian.Web.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class ContributorFileTypeControllerTests
    {
        private ContributorFileTypeController _currentController;

        [TestInitialize]
        public void TestInitialize()
        {
            _currentController = new ContributorFileTypeController();
        }

        [TestMethod()]
        public void AddTest()
        {
            //arrange
            //act
            var viewResult = _currentController.Add() as ViewResult;
            //assert
            Assert.IsNotNull(viewResult); 
            Assert.AreEqual(Navigation.NavigationEnum.ContributorFileType.ToString(), viewResult.ViewData["CurrentPage"].ToString());
        }
    }
}