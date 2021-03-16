using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Controllers;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.IndraTestProject.Controllers
{
    /// <summary>
    /// Summary description for RadianTestSetControllerTest
    /// </summary>
    [TestClass]
    public class RadianTestSetControllerTest
    {
        private RadianTestSetController _currentController;
        private readonly Mock<IRadianTestSetService> _radianTestSetService = new Mock<IRadianTestSetService>();
        private readonly Mock<IRadianApprovedService> _radianAprovedService = new Mock<IRadianApprovedService>();

        [TestInitialize]
        public void TestInitialize()
        {
            _currentController = new RadianTestSetController(_radianTestSetService.Object, _radianAprovedService.Object);
        }

        [TestMethod]
        public void Index_Result_Test()
        {
            //arrange
            _ = _radianTestSetService.Setup(t => t.GetAllTestSet()).Returns(new List<RadianTestSet>());
            _ = _radianTestSetService.Setup(t => t.GetOperationMode(It.IsAny<int>())).Returns(new RadianOperationMode() { Name = "OperationModename" });

            //act
            var viewResult = _currentController.Index() as ViewResult;

            //assert
            Assert.IsInstanceOfType(viewResult.Model, typeof(RadianTestSetTableViewModel));
            Assert.AreEqual(Navigation.NavigationEnum.RadianSetPruebas.ToString(), viewResult.ViewData["CurrentPage"].ToString());
        }

        [TestMethod]
        public void Add_Result_Test()
        {
            //arrange
            _radianTestSetService.Setup(t => t.OperationModeList(Domain.Common.RadianOperationMode.None)).Returns(GetTestRadianOperationMode());

            //act
            var viewResult = _currentController.Add() as ViewResult;

            //assert
            Assert.IsInstanceOfType(viewResult.Model, typeof(RadianTestSetViewModel));
            Assert.AreEqual(Navigation.NavigationEnum.RadianSetPruebas.ToString(), viewResult.ViewData["CurrentPage"].ToString());
            Assert.IsTrue(((RadianTestSetViewModel)viewResult.Model).OperationModes.Count == 2);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "TestSetReplace=false Error Exists TestSet")]
        [DataRow(2, DisplayName = "Create TesStSet - No pudo Guardar")]
        [DataRow(3, DisplayName = "Create TesStSet - Si pudo Guardar, con TestSetReplace=false ")]
        [DataRow(4, DisplayName = "Create TesStSet - Si pudo Guardar, con TestSetReplace=true ")]
        public void AddPost_Result_Test(int input)
        {
            //arrange
            RedirectToRouteResult result = null;
            ViewResult viewResult = null;
            RadianTestSetViewModel model = new RadianTestSetViewModel();
            RadianTestSet ValidTest = null;

            switch (input)
            {
                case 1:
                    model.TestSetReplace = false;
                    _ = _radianTestSetService.SetupSequence(t => t.GetTestSet(It.IsAny<string>(), It.IsAny<string>())).Returns(new RadianTestSet());
                    _ = _radianTestSetService.SetupSequence(t => t.OperationModeList(Domain.Common.RadianOperationMode.None)).Returns(GetTestRadianOperationMode());
                    break;
                case 2:
                    model.TestSetReplace = true;
                    _ = _radianTestSetService.SetupSequence(t => t.InsertTestSet(It.IsAny<RadianTestSet>())).Returns(false);
                    break;
                case 3:
                    model.TestSetReplace = false;
                    _ = _radianTestSetService.SetupSequence(t => t.GetTestSet(It.IsAny<string>(), It.IsAny<string>())).Returns(ValidTest);
                    _ = _radianTestSetService.SetupSequence(t => t.InsertTestSet(It.IsAny<RadianTestSet>())).Returns(true);
                    break;
                case 4:
                    model.TestSetReplace = true;
                    _ = _radianTestSetService.SetupSequence(t => t.InsertTestSet(It.IsAny<RadianTestSet>())).Returns(true);
                    break;
            }

            _currentController.ControllerContext = GetControllerContext().Object;

            //act
            switch (input)
            {
                case 3:
                case 4:
                    result = _currentController.Add(model) as RedirectToRouteResult;
                    break;
                default:
                    viewResult = _currentController.Add(model) as ViewResult;
                    break;
            }

            //assert
            switch (input)
            {
                case 1:
                    Assert.IsTrue(Convert.ToBoolean(viewResult.ViewData["ErrorExistsTestSet"]));
                    Assert.IsInstanceOfType(viewResult.Model, typeof(RadianTestSetViewModel));
                    Assert.IsTrue(((RadianTestSetViewModel)viewResult.Model).OperationModes.Count == 2);
                    break;
                case 2:
                    Assert.IsTrue(!viewResult.ViewData["ErrorMessage"].ToString().Equals(""));
                    Assert.IsInstanceOfType(viewResult.Model, typeof(RadianTestSetViewModel));
                    break;
                case 3:
                case 4:
                    Assert.AreEqual("Index", result.RouteValues["Action"]);
                    break;
            }
        }


        [TestMethod]
        [DataRow(1, DisplayName = "Sin TestSet and return")]
        [DataRow(2, DisplayName = "Edit ")]
        public void Edit_Result_Test(int input)
        {
            //arrange
            RedirectToRouteResult result = null;
            ViewResult viewResult = null;
            RadianTestSet ValidTest = null;

            if (input.Equals(1))
            {
                _ = _radianTestSetService.SetupSequence(t => t.GetTestSet(It.IsAny<string>(), It.IsAny<string>())).Returns(ValidTest);
                result = _currentController.Edit(It.IsAny<int>()) as RedirectToRouteResult;
                Assert.AreEqual("Index", result.RouteValues["Action"]);
            }
            if (input.Equals(2))
            {
                _ = _radianTestSetService.SetupSequence(t => t.GetTestSet(It.IsAny<string>(), It.IsAny<string>())).Returns(new RadianTestSet() { TestSetId = "1", PartitionKey = "1" });
                _ = _radianTestSetService.SetupSequence(t => t.OperationModeList(Domain.Common.RadianOperationMode.None)).Returns(GetTestRadianOperationMode());

                _currentController.ControllerContext = GetControllerContext().Object;
                viewResult = _currentController.Edit(It.IsAny<int>()) as ViewResult;

                Assert.IsInstanceOfType(viewResult.Model, typeof(RadianTestSetViewModel));
                Assert.AreEqual(Navigation.NavigationEnum.RadianSetPruebas.ToString(), viewResult.ViewData["CurrentPage"].ToString());
                Assert.IsTrue(((RadianTestSetViewModel)viewResult.Model).OperationModes.Count == 2);
            }
        }


        [TestMethod]
        [DataRow(1, DisplayName = "Edit TesStSet - No pudo Guardar")]
        [DataRow(2, DisplayName = "Edit TesStSet - Si pudo Guardar")]
        public void EditPost_Result_Test(int input)
        {
            //arrange
            RadianTestSetViewModel model = new RadianTestSetViewModel() { TestSetId = Guid.NewGuid().ToString() };

            _ = _radianTestSetService.SetupSequence(t => t.InsertTestSet(It.IsAny<RadianTestSet>())).Returns(input != 1);

            _currentController.ControllerContext = GetControllerContext().Object;
            //act
            if (input.Equals(1))
            {
                var viewResult = _currentController.Edit(model) as ViewResult;

                Assert.IsTrue(!viewResult.ViewData["ErrorMessage"].ToString().Equals(""));
                Assert.IsInstanceOfType(viewResult.Model, typeof(RadianTestSetViewModel));
            }
            if (input.Equals(2))
            {
                var result = _currentController.Edit(model) as RedirectToRouteResult;

                Assert.AreEqual("Index", result.RouteValues["Action"]);
            }
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Sin TesStSet")]
        [DataRow(2, DisplayName = "Con TesStSet")]
        public void GetTestSetSummary_Result_Test(int input)
        {
            //arrange
            RadianTestSet ValidTest = null;

            if (input.Equals(1))
            {
                _ = _radianAprovedService.SetupSequence(t => t.GetTestSet(It.IsAny<string>())).Returns(ValidTest);
                var viewResult = _currentController.GetTestSetSummary(It.IsAny<string>());

                Assert.IsNotNull(viewResult);
                Assert.IsTrue(((List<EventCountersViewModel>)viewResult.Data).Count == 0);
            }
            if (input.Equals(2))
            {
                _ = _radianAprovedService.SetupSequence(t => t.GetTestSet(It.IsAny<string>())).Returns(new RadianTestSet() { TestSetId = "1", PartitionKey = "1" });
                var viewResult = _currentController.GetTestSetSummary(It.IsAny<string>());
                Assert.IsNotNull(viewResult);
                Assert.IsTrue(((List<EventCountersViewModel>)viewResult.Data).Count > 0);
            } 
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

        private List<RadianOperationMode> GetTestRadianOperationMode()
        {
            return new List<RadianOperationMode>()
            {
                GetTestRadianOperationMode(1,"Uno"),
                GetTestRadianOperationMode(2,"Dos"),
            };
        }

        private RadianOperationMode GetTestRadianOperationMode(int Id, string Name)
        {
            return new RadianOperationMode
            {
                Id = Id,
                Name = Name,
            };
        }

        #endregion
    }
}