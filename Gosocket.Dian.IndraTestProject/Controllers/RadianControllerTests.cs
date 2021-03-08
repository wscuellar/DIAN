using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Gosocket.Dian.Interfaces.Services;
using System.Web.Http.Routing;
using System.Web.Mvc;
using System.Collections.Specialized;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Domain.Entity;
using System.Web;

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
            NameValueCollection result = new NameValueCollection();
            result.Add("ContributorId", "1");
            _radianContributorService.Setup(t => t.Summary(It.IsAny<int>())).Returns(result);

            //act
           var viewResult=  _current.Index() as ViewResult;

            //assert
            Assert.AreEqual(viewResult.ViewData["ContributorId"], "1");
        }

        [TestMethod]
        public  void ElectronicInvoiceView()
        {
            //arrange
            NameValueCollection result = new NameValueCollection();
            result.Add("ContributorId", "1");
            _radianContributorService.Setup(t => t.Summary(It.IsAny<int>())).Returns(result);

            //act
            var viewResult = _current.ElectronicInvoiceView() as ViewResult;

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
            int page = 1, size = 10;
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

           

            _radianContributorService.Setup(t => t.ListParticipants(page, size)).Returns(radianAdmin);

            //act
            ViewResult result = _current.AdminRadianView() as ViewResult;

            //assert
            Assert.IsNotNull(result);

        }


    }
}