using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web.Mvc;
using Gosocket.Dian.Interfaces.Services;
using System;
using Gosocket.Dian.Web.Models;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianControllerTests
    {

        private RadianController _current;
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianController(_radianContributorService.Object);
        }


        
        [TestMethod()]
        [DataRow(1,"1")]
        [DataRow(2, "")]
        public void Index_WithUser_Test(int input, string expected)
        {
            //preparacion
            System.Collections.Specialized.NameValueCollection nameValue = new System.Collections.Specialized.NameValueCollection();

            switch(input)
            {
                case 1:
                    nameValue.Add("ContributorId", expected);                  
                    break;
            }
            _radianContributorService.Setup(t => t.Summary(It.IsAny<string>())).Returns(nameValue);


            //ejecucion
            var  result= _current.Index() as ViewResult;

            //Validacion
            var actual = result.ViewBag.ContributorId;
            Assert.AreEqual(input == 2 ? null : expected,   actual);
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
            var result = _current.Index() as ViewResult;

            //Validacion
            var actual = result.ViewBag.ContributorId;
            Assert.AreEqual(input == 2 ? null : expected, actual);

        }

        [TestMethod()]
        public void RegistrationValidation_Test()
        {
            //arrange
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel();
            _radianContributorService.Setup(t => t.RegistrationValidation(It.IsAny<string>(), registrationData.RadianContributorType, registrationData.RadianOperationMode)).Returns(new Domain.Entity.RadianRegistrationValidation());

            //add
            var result = _current.RegistrationValidation(registrationData);

            //assert
            Assert.IsNotNull(result);
        }

    }
}