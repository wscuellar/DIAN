using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianContributorFileTypeControllerTests
    {
        private readonly RadianContributorFileTypeController _radianContributorFileTypeController;
        private readonly Mock<IRadianContributorFileTypeService> _radianContributorFileTypeService = new Mock<IRadianContributorFileTypeService>();
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();

        public RadianContributorFileTypeControllerTests() =>
            _radianContributorFileTypeController = new RadianContributorFileTypeController(_radianContributorFileTypeService.Object,
                                                                                           _radianContributorService.Object);

        [TestMethod()]
        public void List_Result_Test()
        {
            _radianContributorFileTypeService.Setup(rcf => rcf.FileTypeList())
                .Returns(new List<RadianContributorFileType>()
                {
                    new RadianContributorFileType()
                    {
                        Id = 1,
                        Name = string.Empty,
                        Mandatory = false,
                        Timestamp = DateTime.Now,
                        Updated = DateTime.Now,
                        RadianContributorType = new RadianContributorType(),
                        HideDelete = true
                    }
                });

            _radianContributorFileTypeService.Setup(rcf => rcf.ContributorTypeList())
                .Returns(new List<RadianContributorType>()
                {
                    new RadianContributorType() { Id = 1, Name = "1" },
                    new RadianContributorType() { Id = 2, Name = "2" }
                });

            ViewResult result = _radianContributorFileTypeController.List() as ViewResult;
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(RadianContributorFileTypeTableViewModel));

            RadianContributorFileTypeTableViewModel model = (RadianContributorFileTypeTableViewModel)result.ViewData.Model;
            Assert.IsTrue(model.RadianContributorFileTypes.Count() > 0
                          && model.RadianContributorTypes.Count() > 0);

            Assert.AreEqual(Navigation.NavigationEnum.RadianContributorFileType.ToString(), result.ViewData["CurrentPage"].ToString());
        }

        [TestMethod()]
        public void List_WithModelInput_Result_Test()
        {
            _radianContributorFileTypeService.Setup(rcf => rcf.Filter(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<RadianContributorFileType>()
                {
                    new RadianContributorFileType()
                    {
                        Id = 1,
                        Name = string.Empty,
                        Mandatory = false,
                        Timestamp = DateTime.Now,
                        Updated = DateTime.Now,
                        RadianContributorType = new RadianContributorType(),
                        HideDelete = false
                    }
                });

            _radianContributorFileTypeService.Setup(rcf => rcf.ContributorTypeList())
                .Returns(new List<RadianContributorType>()
                {
                    new RadianContributorType() { Id = 1, Name = "1" },
                    new RadianContributorType() { Id = 2, Name = "2" }
                });

            ViewResult result = _radianContributorFileTypeController.List(new RadianContributorFileTypeTableViewModel()) as ViewResult;
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(RadianContributorFileTypeTableViewModel));

            RadianContributorFileTypeTableViewModel model = (RadianContributorFileTypeTableViewModel)result.ViewData.Model;
            Assert.IsTrue(model.RadianContributorFileTypes.Count() > 0
                          && model.RadianContributorFileTypeViewModel.RadianContributorTypes.Count() > 0);

            Assert.AreEqual(Navigation.NavigationEnum.RadianContributorFileType.ToString(), result.ViewData["CurrentPage"].ToString());
        }

        [TestMethod()]
        public void Add_ValidState_Result_Test()
        {
            Mock<HttpContextBase> fakeHttpContext = new Mock<HttpContextBase>();
            GenericIdentity fakeIdentity = new GenericIdentity("User");
            GenericPrincipal principal = new GenericPrincipal(fakeIdentity, null);

            fakeHttpContext.Setup(t => t.User).Returns(principal);
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(t => t.HttpContext).Returns(fakeHttpContext.Object);

            _radianContributorFileTypeController.ControllerContext = controllerContext.Object;
            _radianContributorFileTypeService.Setup(rcf => rcf.Update(It.IsAny<RadianContributorFileType>())).Returns(1);

            ActionResult result = _radianContributorFileTypeController.Add(new RadianContributorFileTypeViewModel()
            {
                Mandatory = true,
                Name = string.Empty,
                SelectedRadianContributorTypeId = "1"
            });

            Assert.AreEqual("List", ((RedirectToRouteResult)result).RouteValues.Values.First().ToString());
        }

        [TestMethod()]
        public void Add_InvalidState_Result_Test()
        {
            RadianContributorFileTypeController radianContributorFileTypeController = _radianContributorFileTypeController;
            radianContributorFileTypeController.ModelState.AddModelError("ErrorTest", "ErrorTest");

            RedirectToRouteResult result = (RedirectToRouteResult)radianContributorFileTypeController.Add(new RadianContributorFileTypeViewModel()
            {
                Mandatory = true,
                Name = string.Empty,
                SelectedRadianContributorTypeId = "1"
            });

            Assert.AreEqual("List", result.RouteValues["action"].ToString());
            Assert.IsTrue(result.RouteValues.ContainsKey("Id"));
            Assert.IsTrue(result.RouteValues.ContainsKey("HideDelete"));
            Assert.IsTrue(result.RouteValues.ContainsKey("Name"));
            Assert.IsTrue(result.RouteValues.ContainsKey("Timestamp"));
            Assert.IsTrue(result.RouteValues.ContainsKey("Updated"));
            Assert.IsTrue(result.RouteValues.ContainsKey("Mandatory"));
            Assert.IsTrue(result.RouteValues.ContainsKey("RadianContributorType"));
            Assert.IsTrue(result.RouteValues.ContainsKey("SelectedRadianContributorTypeId"));
            Assert.IsTrue(result.RouteValues.ContainsKey("RadianContributorTypes"));
        }

        [TestMethod()]
        public void GetEditRadianContributorFileTypePartialView_Result_Test()
        {
            Mock<HttpContextBase> moqContext = new Mock<HttpContextBase>();
            Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
            moqResponse.Setup(r => r.Headers).Returns(new NameValueCollection());

            moqContext.Setup(mc => mc.Response).Returns(moqResponse.Object);

            RadianContributorFileTypeController radianContributorFileTypeController = _radianContributorFileTypeController;
            radianContributorFileTypeController.ControllerContext = new ControllerContext(moqContext.Object, new RouteData(), radianContributorFileTypeController);

            _radianContributorFileTypeService.Setup(rcf => rcf.Get(It.IsAny<int>()))
                .Returns(new RadianContributorFileType()
                {
                    Id = 1,
                    Name = string.Empty,
                    Mandatory = false,
                    RadianContributorType = new RadianContributorType() { Id = 1 }
                });

            _radianContributorFileTypeService.Setup(rcf => rcf.ContributorTypeList())
                .Returns(new List<RadianContributorType>()
                {
                    new RadianContributorType() { Id = 1, Name = "1" },
                    new RadianContributorType() { Id = 2, Name = "2" }
                });

            PartialViewResult result = radianContributorFileTypeController.GetEditRadianContributorFileTypePartialView(1);

            Assert.IsTrue(result.ViewName.Contains("RadianContributorFileType/_Edit"));
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(RadianContributorFileTypeViewModel));

            RadianContributorFileTypeViewModel model = (RadianContributorFileTypeViewModel)result.ViewData.Model;
            Assert.IsTrue(model.RadianContributorTypes.Count() > 0);
        }

        [TestMethod()]
        public void Edit_Result_Test()
        {
            _radianContributorFileTypeService.Setup(rcf => rcf.Get(It.IsAny<int>()))
                .Returns(new RadianContributorFileType()
                {
                    Id = 1,
                    Name = string.Empty,
                    Mandatory = false
                });

            ViewResult result = _radianContributorFileTypeController.Edit(1) as ViewResult;

            Assert.IsInstanceOfType(result.Model, typeof(RadianContributorFileTypeViewModel));
            Assert.AreEqual(Navigation.NavigationEnum.RadianContributorFileType.ToString(), result.ViewData["CurrentPage"].ToString());
        }

        [TestMethod()]
        public void Edit_WithModel_ValidState_Result_Test()
        {
            Mock<HttpContextBase> fakeHttpContext = new Mock<HttpContextBase>();
            GenericIdentity fakeIdentity = new GenericIdentity("User");
            GenericPrincipal principal = new GenericPrincipal(fakeIdentity, null);

            fakeHttpContext.Setup(t => t.User).Returns(principal);
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(t => t.HttpContext).Returns(fakeHttpContext.Object);

            _radianContributorFileTypeController.ControllerContext = controllerContext.Object;
            _radianContributorFileTypeService.Setup(rcf => rcf.Update(It.IsAny<RadianContributorFileType>())).Returns(1);

            ActionResult result = _radianContributorFileTypeController.Edit(new RadianContributorFileTypeViewModel()
            {
                Mandatory = true,
                Name = string.Empty,
                SelectedRadianContributorTypeId = "1"
            });

            Assert.AreEqual("List", ((RedirectToRouteResult)result).RouteValues.Values.First().ToString());
        }

        [TestMethod()]
        public void Edit_WithModel_InvalidState_Result_Test()
        {
            RadianContributorFileTypeController radianContributorFileTypeController = _radianContributorFileTypeController;
            radianContributorFileTypeController.ModelState.AddModelError("ErrorTest", "ErrorTest");

            RedirectToRouteResult result = (RedirectToRouteResult)radianContributorFileTypeController.Edit(new RadianContributorFileTypeViewModel()
            {
                Mandatory = true,
                Name = string.Empty,
                SelectedRadianContributorTypeId = "1"
            });

            Assert.AreEqual("List", result.RouteValues["action"].ToString());
            Assert.IsTrue(result.RouteValues.ContainsKey("Id"));
            Assert.IsTrue(result.RouteValues.ContainsKey("HideDelete"));
            Assert.IsTrue(result.RouteValues.ContainsKey("Name"));
            Assert.IsTrue(result.RouteValues.ContainsKey("Timestamp"));
            Assert.IsTrue(result.RouteValues.ContainsKey("Updated"));
            Assert.IsTrue(result.RouteValues.ContainsKey("Mandatory"));
            Assert.IsTrue(result.RouteValues.ContainsKey("RadianContributorType"));
            Assert.IsTrue(result.RouteValues.ContainsKey("SelectedRadianContributorTypeId"));
            Assert.IsTrue(result.RouteValues.ContainsKey("RadianContributorTypes"));
        }

        [TestMethod()]
        public void GetDeleteRadianContributorFileTypePartialView_Result_Test()
        {
            Mock<HttpContextBase> moqContext = new Mock<HttpContextBase>();
            Mock<HttpResponseBase> moqResponse = new Mock<HttpResponseBase>();
            moqResponse.Setup(r => r.Headers).Returns(new NameValueCollection());

            moqContext.Setup(mc => mc.Response).Returns(moqResponse.Object);

            RadianContributorFileTypeController radianContributorFileTypeController = _radianContributorFileTypeController;
            radianContributorFileTypeController.ControllerContext = new ControllerContext(moqContext.Object, new RouteData(), radianContributorFileTypeController);

            _radianContributorFileTypeService.Setup(rcf => rcf.Get(It.IsAny<int>()))
                .Returns(new RadianContributorFileType()
                {
                    Id = 1,
                    Name = string.Empty,
                    Mandatory = false,
                    RadianContributorType = new RadianContributorType() { Id = 1 }
                });

            PartialViewResult result = radianContributorFileTypeController.GetDeleteRadianContributorFileTypePartialView(1);
            Assert.IsTrue(result.ViewName.Contains("RadianContributorFileType/_Delete"));
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(RadianContributorFileTypeViewModel));
        }

        [TestMethod()]
        public void Delete_EnableForDelete_Result_Test()
        {
            Mock<HttpContextBase> fakeHttpContext = new Mock<HttpContextBase>();
            GenericIdentity fakeIdentity = new GenericIdentity("User");
            GenericPrincipal principal = new GenericPrincipal(fakeIdentity, null);

            fakeHttpContext.Setup(t => t.User).Returns(principal);
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(t => t.HttpContext).Returns(fakeHttpContext.Object);

            _radianContributorFileTypeController.ControllerContext = controllerContext.Object;

            _radianContributorFileTypeService.Setup(rcf => rcf.IsAbleForDelete(It.IsAny<RadianContributorFileType>())).Returns(true);

            RedirectToRouteResult result = (RedirectToRouteResult)_radianContributorFileTypeController.Delete(new RadianContributorFileTypeViewModel()
            {
                Id = 1,
                Mandatory = true,
                Name = string.Empty
            });

            Assert.AreEqual("List", result.RouteValues["action"].ToString());
        }

        [TestMethod()]
        public void Delete_DisableForDelete_Result_Test()
        {
            Mock<HttpContextBase> fakeHttpContext = new Mock<HttpContextBase>();
            GenericIdentity fakeIdentity = new GenericIdentity("User");
            GenericPrincipal principal = new GenericPrincipal(fakeIdentity, null);

            fakeHttpContext.Setup(t => t.User).Returns(principal);
            Mock<ControllerContext> controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(t => t.HttpContext).Returns(fakeHttpContext.Object);

            _radianContributorFileTypeController.ControllerContext = controllerContext.Object;

            _radianContributorFileTypeService.Setup(rcf => rcf.IsAbleForDelete(It.IsAny<RadianContributorFileType>())).Returns(false);

            RedirectToRouteResult result = (RedirectToRouteResult)_radianContributorFileTypeController.Delete(new RadianContributorFileTypeViewModel()
            {
                Id = 1,
                Mandatory = true,
                Name = string.Empty
            });

            Assert.AreEqual("List", result.RouteValues["action"].ToString());
        }
    }
}
