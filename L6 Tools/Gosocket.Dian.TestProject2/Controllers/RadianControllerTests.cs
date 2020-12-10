using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web.Mvc;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Domain.Entity;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianControllerTests
    {

        private RadianController _current;
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();
        private readonly Mock<UrlHelper> mockUrlHelper = new Mock<UrlHelper>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianController(_radianContributorService.Object);
        }



        [TestMethod()]
        [DataRow(1, "1")]
        [DataRow(2, "")]
        public void Index_WithUser_Test(int input, string expected)
        {
            //preparacion
            System.Collections.Specialized.NameValueCollection nameValue = new System.Collections.Specialized.NameValueCollection();

            switch (input)
            {
                case 1:
                    nameValue.Add("ContributorId", expected);
                    break;
            }
            _radianContributorService.Setup(t => t.Summary(It.IsAny<string>())).Returns(nameValue);


            //ejecucion
            var result = _current.Index() as ViewResult;

            //Validacion
            var actual = result.ViewBag.ContributorId;
            Assert.AreEqual(input == 2 ? null : expected, actual);
        }


        [TestMethod()]
        [DataRow(1, "1")]
        [DataRow(2, "")]
        public void ElectronicInvoiceView_WithUser_Test(int input, string expected)
        {
            //preparacion
            System.Collections.Specialized.NameValueCollection nameValue = new System.Collections.Specialized.NameValueCollection();

            switch (input)
            {
                case 1:
                    nameValue.Add("ContributorId", expected);
                    break;
            }
            _radianContributorService.Setup(t => t.Summary(It.IsAny<string>())).Returns(nameValue);


            //ejecucion
            var result = _current.ElectronicInvoiceView() as ViewResult;

            //Validacion
            var actual = result.ViewBag.ContributorId;
            Assert.AreEqual(input == 2 ? null : expected, actual);

        }

        [TestMethod()]
        [DataRow(1, "messageText", "alert", "")]
        [DataRow(2, "messageText2", "redirect", "http://locationTest/")]
        public void RegistrationValidation_Test(int input, string expectedMessage, string messageType, string expectedLocation)
        {
            //arrange
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel();
            ResponseMessage validation = new ResponseMessage() { Message = expectedMessage, MessageType= messageType };
            if (input == 2)
            {
                mockUrlHelper.Setup(x => x.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>())).Returns(expectedLocation);
                _current.Url = mockUrlHelper.Object;
                validation.MessageType = "redirect" ;
            }
            _radianContributorService.Setup(t => t.RegistrationValidation(It.IsAny<string>(), registrationData.RadianContributorType, registrationData.RadianOperationMode)).Returns(validation);

            //add
            JsonResult result = _current.RegistrationValidation(registrationData);
            ResponseMessage data = (ResponseMessage)result.Data;

            //assert
            Assert.AreEqual(expectedMessage, data.Message);
            if (input == 1)
                Assert.IsNull(data.RedirectTo);
            else
                Assert.AreEqual(expectedLocation, data.RedirectTo);
        }

        [TestMethod]
        public void AdminRadianView_GET_Test()
        {
            //arrange
            RadianAdmin radianAdmin = new RadianAdmin()
            {
                RowCount = 1,
                CurrentPage = 1,
                Contributors = new System.Collections.Generic.List<RedianContributorWithTypes>() {
                    new RedianContributorWithTypes() {
                          Id=1,
                          Code= "123",
                          TradeName = "test",
                          BusinessName= "test",
                          AcceptanceStatusName = "test",
                          RadianState= "registrado"
                    }
                },
                Types = new System.Collections.Generic.List<Domain.RadianContributorType>()
                {
                    new Domain.RadianContributorType()
                    {
                        Id= 1,
                        Name = "tes"
                    }
                }
            };
            _radianContributorService.Setup(t => t.ListParticipants(1, 10)).Returns(radianAdmin);

            //act
            ViewResult result = _current.AdminRadianView() as ViewResult;
            AdminRadianViewModel model = result.Model as AdminRadianViewModel;

            //assert
            Assert.IsNotNull(model);
        }

    }
}
