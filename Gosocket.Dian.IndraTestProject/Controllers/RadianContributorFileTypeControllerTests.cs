using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianContributorFileTypeControllerTests
    {

        private RadianContributorFileTypeController _current;
        private readonly Mock<IRadianContributorFileTypeService> _radianContributorFileTypeService = new Mock<IRadianContributorFileTypeService>();
        

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianContributorFileTypeController(_radianContributorFileTypeService.Object);
        }

        [TestMethod()]
        public void ListTest()
        {
            //arrange
            _radianContributorFileTypeService.Setup(t => t.FileTypeList()).Returns(new List<Domain.RadianContributorFileType>() { new Domain.RadianContributorFileType() });
            _radianContributorFileTypeService.Setup(t => t.ContributorTypeList()).Returns(new List<Domain.RadianContributorType>() { new Domain.RadianContributorType() });

            //act
            ViewResult viewResult = _current.List() as ViewResult;

            //assert
            Assert.IsNotNull(viewResult);
        }


        [TestMethod()]
        public void ListPostTest()
        {
            //arrange
            RadianContributorFileTypeTableViewModel model = new RadianContributorFileTypeTableViewModel();
            _radianContributorFileTypeService.Setup(t => t.Filter(model.Name, model.SelectedRadianContributorTypeId)).Returns(new List<Domain.RadianContributorFileType>() { new Domain.RadianContributorFileType() });
            _radianContributorFileTypeService.Setup(t => t.ContributorTypeList()).Returns(new List<Domain.RadianContributorType>() { new Domain.RadianContributorType() });

            //act
            ViewResult viewResult = _current.List(model) as ViewResult;

            //assert
            Assert.IsNotNull(viewResult);
        }

        [TestMethod]
        [DataRow(1, DisplayName ="ModelState Invalid")]
        [DataRow(2, DisplayName = "Complete process")]
        public void AddTest(int input)
        {
            //arrange
            RadianContributorFileTypeViewModel model = new RadianContributorFileTypeViewModel() { SelectedRadianContributorTypeId= "1" };
            switch (input)
            {
                case 1:
                    _current.ModelState.AddModelError("test", "test");
                    break;
                default:
                    _current.ModelState.Clear();
                    _radianContributorFileTypeService.Setup(t => t.Update(It.IsAny<RadianContributorFileType>())).Returns(1);
                    break;
            }

            //act
            RedirectToRouteResult result =  _current.Add(model) as RedirectToRouteResult;

            //assert
            Assert.AreEqual("List", result.RouteValues["Action"]);
        }

        [TestMethod]
        public void GetEditRadianContributorFileTypePartialViewTest()
        {
            //arrange
            int id = 1;
            Mock<ControllerContext> controllercontext = new Mock<ControllerContext>();
            NameValueCollection headerCollection = new NameValueCollection
            {
                ["InjectingPartialView"] = "true"
            };
            controllercontext.Setup(frm => frm.HttpContext.Response.Headers).Returns(headerCollection);
            _current.ControllerContext = controllercontext.Object;
            _radianContributorFileTypeService.Setup(t => t.Get(id)).Returns(new RadianContributorFileType() { RadianContributorType = new RadianContributorType()});
            _radianContributorFileTypeService.Setup(t => t.ContributorTypeList()).Returns(new List<RadianContributorType>() { new RadianContributorType() });

            //act
            PartialViewResult partialViewResult = _current.GetEditRadianContributorFileTypePartialView(id);

            //assert
            Assert.AreEqual("~/Views/RadianContributorFileType/_Edit.cshtml", partialViewResult.ViewName);
        }

        [TestMethod]
        public void EditTest()
        {
            //arrange
            int id = 1;
            _radianContributorFileTypeService.Setup(t => t.Get(id)).Returns(new RadianContributorFileType());

            //act
            ViewResult viewResult = _current.Edit(id) as ViewResult;

            //assert
            Assert.IsNotNull(viewResult);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "ModelState Invalid")]
        [DataRow(2, DisplayName = "Complete process")]
        public void EditPostTest(int input)
        {
            //arrange
            RadianContributorFileTypeViewModel model = new RadianContributorFileTypeViewModel() { SelectedRadianContributorTypeId="1"};
            switch (input)
            {
                case 1:
                    _current.ModelState.AddModelError("test", "test");
                    break;
                default:
                    _current.ModelState.Clear();
                    break;
            }
            _radianContributorFileTypeService.Setup(t => t.Update(It.IsAny<RadianContributorFileType>())).Returns(1);

            //act
            RedirectToRouteResult viewResult = _current.Edit(model) as RedirectToRouteResult;

            //assert
            Assert.IsNotNull(viewResult);
        }


        [TestMethod]
        public void GetDeleteRadianContributorFileTypePartialViewTest()
        {
            //arrange
            int id = 1;
            Mock<ControllerContext> controllercontext = new Mock<ControllerContext>();
            NameValueCollection headerCollection = new NameValueCollection
            {
                ["InjectingPartialView"] = "true"
            };
            controllercontext.Setup(frm => frm.HttpContext.Response.Headers).Returns(headerCollection);
            _current.ControllerContext = controllercontext.Object;
            _radianContributorFileTypeService.Setup(t => t.Get(id)).Returns(new RadianContributorFileType() { RadianContributorType =new RadianContributorType() });

            //act
            PartialViewResult result = _current.GetDeleteRadianContributorFileTypePartialView(id);

            //assert
            Assert.AreEqual("~/Views/RadianContributorFileType/_Delete.cshtml", result.ViewName);

        }

        [TestMethod]
        public void DeleteTest()
        {
            //arrange
            RadianContributorFileTypeViewModel model = new RadianContributorFileTypeViewModel();
            _radianContributorFileTypeService.Setup(t => t.IsAbleForDelete(It.IsAny<RadianContributorFileType>())).Returns(true);
            _radianContributorFileTypeService.Setup(t => t.Delete(It.IsAny<RadianContributorFileType>())).Returns(1);

            //act
            RedirectToRouteResult viewResult = _current.Delete(model) as RedirectToRouteResult;

            //assert
            Assert.AreEqual("List", viewResult.RouteValues["action"]);


        }





    }
}