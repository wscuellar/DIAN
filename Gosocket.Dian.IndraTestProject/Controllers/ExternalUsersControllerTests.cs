using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;


namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class ExternalUsersControllerTests
    {
        private ExternalUsersController _currentController;
        private readonly Mock<IPermissionService> _permisionService = new Mock<IPermissionService>();
        private readonly Mock<IContributorService> _contributorService = new Mock<IContributorService>();

        [TestInitialize]
        public void TestInitialize()
        {
            _currentController = new ExternalUsersController(_permisionService.Object, _contributorService.Object);
        }

        [TestMethod()]
        public void LoginTest()
        {
            //arrange
            //act
            var viewResult = _currentController.Login() as ViewResult;
            //assert
            Assert.IsNotNull(viewResult);
        }

        [TestMethod]
        public void AddUserTest()
        { 
            //arrange
            _ = _permisionService.SetupSequence(t => t.GetAppMenu(It.IsAny<string>()))
                .Returns(new List<Menu> ());

            _ = _permisionService.SetupSequence(t => t.GetSubMenusByMenuId(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new List<SubMenu>());
            //act 
            _currentController.ControllerContext = GetControllerContext().Object;
            var viewResult1 = _currentController.AddUser(new ExternalUserViewModel(), new FormCollection()).Result as ViewResult;
            //assert
            Assert.IsNotNull(viewResult1);
            Assert.AreEqual(Navigation.NavigationEnum.ExternalUsersCreate.ToString(), viewResult1.ViewData["CurrentPage"].ToString());
            Assert.AreEqual("Crear usuario", viewResult1.ViewData["txtAccion"].ToString()); 
        }

        [TestMethod()]
        public void LoadViewBagsTest()
        {
            //arrange
            _ = _permisionService.SetupSequence(t => t.GetAppMenu(It.IsAny<string>()))
                .Returns(new List<Menu>());

            _ = _permisionService.SetupSequence(t => t.GetSubMenusByMenuId(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new List<SubMenu>());
            //act 
            _currentController.ControllerContext = GetControllerContext().Object;
            _currentController.LoadViewBags();
            //assert 
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void AddUserTest1()
        {
            string id = "";
            int Page = 0;
            //arrange
            _ = _permisionService.SetupSequence(t => t.GetAppMenu(It.IsAny<string>()))
                .Returns(new List<Menu>());

            _ = _permisionService.SetupSequence(t => t.GetSubMenusByMenuId(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new List<SubMenu>());
            //act 
            _currentController.ControllerContext = GetControllerContext().Object;
            var viewResult1 = _currentController.AddUser(id, Page) as ViewResult;
            //assert
            Assert.IsNotNull(viewResult1);
            Assert.AreEqual(Navigation.NavigationEnum.ExternalUsersCreate.ToString(), viewResult1.ViewData["CurrentPage"].ToString());
            Assert.AreEqual("Crear usuario", viewResult1.ViewData["txtAccion"].ToString());
        }
   
        #region -- Private --- 
        private Mock<ControllerContext> GetControllerContext()
        {
            Mock<HttpContextBase> fakeHttpContext = new Mock<HttpContextBase>();
            GenericIdentity fakeIdentity = new GenericIdentity("User");
            GenericPrincipal principal = new GenericPrincipal(fakeIdentity, null);

            fakeHttpContext.Setup(t => t.User).Returns(principal);
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(t => t.HttpContext).Returns(fakeHttpContext.Object);

            return controllerContext;
        }
        #endregion
    }
}