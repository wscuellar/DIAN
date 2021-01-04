using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Controllers;
using Gosocket.Dian.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Gosocket.Dian.TestProject2.Controllers
{
    [TestClass()]
    public class RadianContributorFileTypeControllerTest
    {
        private readonly RadianContributorFileTypeController _radianContributorFileTypeController;
        private readonly Mock<IRadianContributorFileTypeService> _radianContributorFileTypeService = new Mock<IRadianContributorFileTypeService>();
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();

        public RadianContributorFileTypeControllerTest() =>
            _radianContributorFileTypeController = new RadianContributorFileTypeController(_radianContributorFileTypeService.Object,
                                                                                           _radianContributorService.Object);

        [TestMethod()]
        public void List_Result_Test()
        {
            ViewResult result = _radianContributorFileTypeController.List() as ViewResult;

            Assert.AreEqual("List", result.ViewName);
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(RadianContributorFileTypeTableViewModel));

            RadianContributorFileTypeTableViewModel model = (RadianContributorFileTypeTableViewModel)result.ViewData.Model;

            Assert.IsTrue(model.RadianContributorFileTypes.Count() > 0
                          && model.RadianContributorTypes.Count() > 0);
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

            ViewResult result = _radianContributorFileTypeController.List(new RadianContributorFileTypeTableViewModel()) as ViewResult;

            Assert.AreEqual("List", result.ViewName);
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(RadianContributorFileTypeTableViewModel));

            RadianContributorFileTypeTableViewModel model = (RadianContributorFileTypeTableViewModel)result.ViewData.Model;

            Assert.IsTrue(model.RadianContributorFileTypes.Count() > 0
                          && model.RadianContributorFileTypeViewModel.RadianContributorTypes.Count() > 0);
        }

        [TestMethod()]
        public void Add_ValidState_Result_Test()
        {
            _radianContributorFileTypeService.Setup(rcf => rcf.Update(It.IsAny<RadianContributorFileType>())).Returns(1);

            ViewResult result = _radianContributorFileTypeController.Add(new RadianContributorFileTypeViewModel()
            {
                Mandatory = true,
                Name = string.Empty,
                SelectedRadianContributorTypeId = string.Empty
            }) as ViewResult;

            Assert.AreEqual("List", result.ViewName);
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(RadianContributorFileTypeTableViewModel));

            RadianContributorFileTypeTableViewModel model = (RadianContributorFileTypeTableViewModel)result.ViewData.Model;

            Assert.IsTrue(model.RadianContributorFileTypes.Count() > 0
                          && model.RadianContributorTypes.Count() > 0);
        }

        [TestMethod()]
        public void Add_InvalidState_Result_Test()
        {
            RadianContributorFileTypeController radianContributorFileTypeController = _radianContributorFileTypeController;
            radianContributorFileTypeController.ModelState.AddModelError("ErrorTest", "ErrorTest");

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

            Assert.IsTrue(false, "Error encontrado en las pruebas unitarias, la sintaxis del código es incorrecta: línea 104");
        }

        [TestMethod()]
        public void GetEditRadianContributorFileTypePartialView_Result_Test()
        {
            _radianContributorFileTypeService.Setup(rcf => rcf.Get(It.IsAny<int>()))
                .Returns(new RadianContributorFileType()
                {
                    Id = 1,
                    Name = string.Empty,
                    Mandatory = false,
                    RadianContributorType = new RadianContributorType() { Id = 1 }
                });

            PartialViewResult result = _radianContributorFileTypeController.GetEditRadianContributorFileTypePartialView(1);

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

            Assert.AreEqual("Edit", result.ViewName);
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(RadianContributorFileTypeViewModel));

            RadianContributorFileTypeViewModel model = (RadianContributorFileTypeViewModel)result.ViewData.Model;

            Assert.IsTrue(model.RadianContributorTypes == null);
        }

        [TestMethod()]
        public void Edit_WithModel_ValidState_Result_Test()
        {
            _radianContributorFileTypeService.Setup(rcf => rcf.Update(It.IsAny<RadianContributorFileType>())).Returns(1);

            ViewResult result = _radianContributorFileTypeController.Add(new RadianContributorFileTypeViewModel()
            {
                Id = 1,
                Mandatory = true,
                Name = string.Empty,
                SelectedRadianContributorTypeId = "0",
                RadianContributorType = new RadianContributorType()
            }) as ViewResult;

            Assert.AreEqual("List", result.ViewName);
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(RadianContributorFileTypeTableViewModel));

            RadianContributorFileTypeTableViewModel model = (RadianContributorFileTypeTableViewModel)result.ViewData.Model;

            Assert.IsTrue(model.RadianContributorFileTypes.Count() > 0
                          && model.RadianContributorTypes.Count() > 0);
        }

        [TestMethod()]
        public void Edit_WithModel_InvalidState_Result_Test()
        {
            RadianContributorFileTypeController radianContributorFileTypeController = _radianContributorFileTypeController;
            radianContributorFileTypeController.ModelState.AddModelError("ErrorTest", "ErrorTest");

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

            Assert.IsTrue(false, "Error encontrado en las pruebas unitarias, la sintaxis del código es incorrecta: línea 104");
        }
    }
}
