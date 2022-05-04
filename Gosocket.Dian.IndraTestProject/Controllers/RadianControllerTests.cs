using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianControllerTests
    {
        private RadianController _current;
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();        
        private readonly Mock<System.Web.Mvc.UrlHelper> mockUrlHelper = new Mock<System.Web.Mvc.UrlHelper>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianController(_radianContributorService.Object);
        }

        [TestMethod()]
        public void IndexTest()
        {
            //arrange
            NameValueCollection result = new NameValueCollection
            {
                { "ContributorId", "1" }
            };
            _radianContributorService.Setup(t => t.Summary(It.IsAny<int>())).Returns(result);

            //act
            ViewResult viewResult =  _current.Index() as ViewResult;

            //assert
            Assert.AreEqual(viewResult.ViewData["ContributorId"], "1");
        }

        [TestMethod]
        public  void ElectronicInvoiceView()
        {
            //arrange
            NameValueCollection result = new NameValueCollection
            {
                { "ContributorId", "1" }
            };
            _radianContributorService.Setup(t => t.Summary(It.IsAny<int>())).Returns(result);

            //act
            ViewResult viewResult = _current.ElectronicInvoiceView() as ViewResult;

            //assert
            Assert.AreEqual(viewResult.ViewData["ContributorId"], "1"); 

        }

        [TestMethod]
        public  void RegistrationValidation()
        {
            //arrange
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel();
            ResponseMessage validation = new ResponseMessage() { MessageType = "redirect", Code = 200, data = "Exitoso" };
            _radianContributorService.Setup(t => t.RegistrationValidation(It.IsAny<int>(), registrationData.RadianContributorType, registrationData.RadianOperationMode)).Returns(validation);

            var httpcontext = Mock.Of<HttpContextBase>();
            var httpcontextSetup = Mock.Get(httpcontext);
            var request = Mock.Of<HttpRequestBase>();
            httpcontextSetup.Setup(m => m.Request).Returns(request);
            string actionName = "Index";
            string controller = "RadianApproved";
            string expectedUrl = "http://myfakeactionurl.com";
            mockUrlHelper
                .Setup(m => m.Action(actionName, controller, It.IsAny<object>()))
                .Returns(expectedUrl)
                .Verifiable();
            _current.Url = mockUrlHelper.Object;
            _current.ControllerContext = new ControllerContext
            {
                Controller = _current,
                HttpContext = httpcontext,
            };

            //act
            JsonResult result = _current.RegistrationValidation(registrationData);
            ResponseMessage message = (ResponseMessage)result.Data;

            //assert
            Assert.AreEqual(message.RedirectTo, expectedUrl);
        }


        [TestMethod]
        public void AdminRadianViewTest()
        {
            //arrange
            int page = 1, size = 10;
            RadianAdmin radianAdmin = new RadianAdmin()
            { 
                RowCount=1,
                CurrentPage= 1,
                Contributors = new List<RedianContributorWithTypes>()
                {
                    new RedianContributorWithTypes(){ Id=1, Code = "1", TradeName = "test", BusinessName ="Test", AcceptanceStatusName = "AcceptTest", RadianState="Test"}
                },
                Types = new List<Domain.RadianContributorType>()
                {
                    new Domain.RadianContributorType(){ Id= 1, Name = "test"}
                }

            };
            _radianContributorService.Setup(t => t.ListParticipants(page, size)).Returns(radianAdmin);

            //act
            ViewResult result =  _current.AdminRadianView() as ViewResult;

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AdminRadianViewPost()
        {
            RadianAdmin radianAdmin = new RadianAdmin()
            {
                RowCount = 1,
                CurrentPage = 1,
                Contributors = new List<RedianContributorWithTypes>()
                {
                    new RedianContributorWithTypes(){ Id=1, Code = "1", TradeName = "test", BusinessName ="Test", AcceptanceStatusName = "AcceptTest", RadianState="Test"}
                },
                Types = new List<Domain.RadianContributorType>()
                {
                    new Domain.RadianContributorType(){ Id= 1, Name = "test"}
                }

            };
            AdminRadianViewModel model = new AdminRadianViewModel()
            { 
                Id = 1, Code = "2", StartDate =DateTime.Now.ToString(), EndDate=DateTime.Now.ToString(), Type=2, RadianState= Domain.Common.RadianState.Habilitado, Page=1,Length=1 
            };
            _radianContributorService.Setup(t => t.ListParticipantsFilter(It.IsAny<AdminRadianFilter>(),model.Page, model.Length)).Returns(radianAdmin);

            //act
            ViewResult result = _current.AdminRadianView(model) as ViewResult;

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public  void ViewDetailsTest()
        {
            //arrange
            int id = 1;
            RadianAdmin radianAdmin = new RadianAdmin()
            {
                RowCount = 1,
                CurrentPage = 1,
                Contributor= new RedianContributorWithTypes()
                {
                    RadianContributorId=1, Code="1", TradeName="Test", 
                    BusinessName="test", Email= "test", AcceptanceStatusId=1, AcceptanceStatusName="test", 
                    CreatedDate = DateTime.Now, Update = DateTime.Now, RadianState = "Habilitado"
                },
                FileTypes = new List<Domain.RadianContributorFileType>()
                {
                    new Domain.RadianContributorFileType(){ Id=1, Name= "test", CreatedBy= "test", Deleted=false, RadianContributorType = new Domain.RadianContributorType(), RadianContributorTypeId=1 }
                },
                Files = new List<Domain.RadianContributorFile>()
                {
                    new Domain.RadianContributorFile(){ Id= Guid.NewGuid(), Comments="Test", RadianContributorFileStatus = new Domain.RadianContributorFileStatus(){ Id = 1, Name = "Test"}, RadianContributorFileType = new Domain.RadianContributorFileType(){ Id = 1 , Name = "Test" } }
                },
                Contributors = new List<RedianContributorWithTypes>()
                {
                    new RedianContributorWithTypes(){ Id=1, Code = "1", TradeName = "test", BusinessName ="Test", AcceptanceStatusName = "AcceptTest", RadianState="Test"}
                },
                Types = new List<Domain.RadianContributorType>()
                {
                    new Domain.RadianContributorType(){ Id= 1, Name = "test"}
                },
                LegalRepresentativeIds = new List<string>() { "1"},
                Tests = new List<RadianTestSetResult>() { new RadianTestSetResult()}

            };
            _radianContributorService.Setup(t => t.ContributorSummary(id,0)).Returns(radianAdmin);

            //act
            ViewResult result = _current.ViewDetails(id) as ViewResult;

            //assert
            Assert.IsNotNull(result);

        }


        [TestMethod]
        public  void DownloadContributorFile()
        {
            //arrange
            string code = "1";
            string fileName = "Test";
            string contentType = "/pdf";
            byte[] data = new byte[100];
            _radianContributorService.Setup(t => t.DownloadContributorFile(code, fileName, out contentType)).Returns(data);

            //act
            var result = _current.DownloadContributorFile("1", "Test");

            //assert 
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void DownloadContributorFileException()
        {
            //arrange
            string code = "1";
            string fileName = "Test";
            string contentType = "/pdf";
            byte[] data = new byte[100];
            _radianContributorService.Setup(t => t.DownloadContributorFile(code, fileName, out contentType)).Throws(new Exception());

            //act
            var result = _current.DownloadContributorFile("1", "Test");

            //assert 
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [DataRow(1,DisplayName = "TestNotRemove")]
        [DataRow(2, DisplayName = "AllSoftware")]
        [DataRow(3, DisplayName = "AllSoftware")]
        [DataRow(4, DisplayName = "WithCustomerList")]
        [DataRow(5, DisplayName = "WithCustomerList")]
        public void ViewDetailsPost(int input)
        {
            //arrange
            int id = 0;
            string approveState = input == 4 ? "1" : input.ToString();
            string radianState = input == 4 ?  "Habilitado" : string.Empty;
            string description = string.Empty;

            List<FilesChangeStateViewModel> data = new List<FilesChangeStateViewModel>() { 
                new FilesChangeStateViewModel(){ Id="1", comment = "Comment test", NewState = 2}
            };
            _radianContributorService.Setup(t => t.RadianContributorFileList("1")).Returns(new List<Domain.RadianContributorFile>() { new Domain.RadianContributorFile() });
            _radianContributorService.Setup(t => t.UpdateRadianContributorFile(It.IsAny<RadianContributorFile>())).Returns(Guid.NewGuid());
            _radianContributorService.Setup(t => t.AddFileHistory(It.IsAny<RadianContributorFileHistory>())).Returns(new ResponseMessage());

            RadianAdmin radianAdmin = new RadianAdmin()
            {
                RowCount = 1,
                CurrentPage = 1,
                Contributor = new RedianContributorWithTypes()
                {
                    RadianContributorId = 1,
                    Code = "1",
                    TradeName = "Test",
                    BusinessName = "test",
                    Email = "test",
                    AcceptanceStatusId = 1,
                    AcceptanceStatusName = "test",
                    CreatedDate = DateTime.Now,
                    Update = DateTime.Now,
                    RadianOperationModeId = 1,
                    RadianState = input == 1 ? "En pruebas" : "Habilitado"
                },
                FileTypes = new List<Domain.RadianContributorFileType>()
                {
                    new Domain.RadianContributorFileType(){ Id=1, Name= "test", CreatedBy= "test", Deleted=false, RadianContributorType = new Domain.RadianContributorType(), RadianContributorTypeId=1 , Mandatory=true }
                },
                Files = new List<Domain.RadianContributorFile>()
                {
                    new Domain.RadianContributorFile(){ Id= Guid.NewGuid(), Comments="Test", RadianContributorFileStatus = new Domain.RadianContributorFileStatus(){ Id = 1, Name = "Test"}, RadianContributorFileType = new Domain.RadianContributorFileType(){ Id = 1 , Name = "Test", Mandatory= input>4 ? false : true } }
                },
                Contributors = new List<RedianContributorWithTypes>()
                {
                    new RedianContributorWithTypes(){ Id=1, Code = "1", TradeName = "test", BusinessName ="Test", AcceptanceStatusName = "AcceptTest", RadianState="Test"}
                },
                Types = new List<Domain.RadianContributorType>()
                {
                    new Domain.RadianContributorType(){ Id= 1, Name = "test"}
                },
                LegalRepresentativeIds = new List<string>() { "1" },
                Tests = new List<RadianTestSetResult>() { new RadianTestSetResult() }

            };
            _radianContributorService.Setup(t => t.ContributorSummary(id, 0)).Returns(radianAdmin);

            if(input == 4)
                _radianContributorService.Setup(t => t.GetAssociatedClients(It.IsAny<int>())).Returns(1);

            if(input > 4)
            {
                _radianContributorService.Setup(t => t.ChangeParticipantStatus(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            }

            //act
            JsonResult result = _current.ViewDetails(data, id, approveState, radianState, description);
            string message = result.Data.GetType().GetProperty("message").GetValue(result.Data).ToString();

            //assert 
            switch(input)
            {
                case 1:
                    Assert.AreEqual(message, TextResources.TestNotRemove);
                    break;
                case 2:
                    Assert.AreEqual(message, TextResources.AllSoftware);
                    break;
                case 4:
                    string expectMessage = string.Format(TextResources.WithCustomerList, "1");
                    Assert.AreEqual(message, expectMessage);
                                    break;
                case 5:
                    Assert.AreEqual(message, TextResources.SuccessSoftware);
                    break;
            }

        }

        [TestMethod]
        public  void GetSetTestByContributor()
        {
            //arrange
            string code = "1";
            string softwareId = Guid.NewGuid().ToString();
            int type = (int)Gosocket.Dian.Domain.Common.RadianContributorType.ElectronicInvoice;
            _radianContributorService.Setup(t => t.GetSetTestResult(code, softwareId, type)).Returns(new RadianTestSetResult());

            //act
            JsonResult result =  _current.GetSetTestByContributor(code, softwareId, type);
            List<EventCountersViewModel> events = (List<EventCountersViewModel>)result.Data;
            //assert
            Assert.AreEqual(events.Count, 17);
        }

    }
}