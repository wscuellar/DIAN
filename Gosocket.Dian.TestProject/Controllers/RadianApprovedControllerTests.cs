using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Moq;
using Gosocket.Dian.Interfaces.Services;
using System.Web.Mvc;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Domain.Common;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianApprovedControllerTests
    {
        private readonly RadianApprovedController _current;
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();
        private readonly Mock<IRadianTestSetService> _radianTestSetService = new Mock<IRadianTestSetService>();
        private readonly Mock<IRadianContributorFileTypeService> _radianContributorFileTypeService = new Mock<IRadianContributorFileTypeService>();

        public RadianApprovedControllerTests()
        {
            _current = new RadianApprovedController(_radianContributorService.Object, _radianTestSetService.Object, _radianContributorFileTypeService.Object );
        }


        [TestMethod()]
        public void IndexTest()
        {
            //arrange
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel();
            _radianContributorService.Setup(t=> t.CreateContributor(registrationData.ContributorId, 
                                                        RadianState.Registrado,
                                                        registrationData.RadianContributorType,
                                                        registrationData.RadianOperationMode,
                                                        It.IsAny<string>()));
            _radianTestSetService.Setup(t => t.OperationModeList()).Returns(new List<Domain.RadianOperationMode>());

            //add
            var r =  _current.Index(registrationData) as ViewResult;

            //assert
            Assert.IsNotNull(r.ViewBag.RadianSoftwareOperationMode);

        }

       
    }
}