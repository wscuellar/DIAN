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
        private readonly Mock<IRadianApprovedService> _radianApprovedService = new Mock<IRadianApprovedService>();
        private readonly Mock<IRadianTestSetResultService> _radianTestSetResultService = new Mock<IRadianTestSetResultService>();

        public RadianApprovedControllerTests()
        {
            _current = new RadianApprovedController(_radianContributorService.Object, _radianTestSetService.Object, _radianApprovedService.Object, _radianTestSetResultService.Object);
        }


        [TestMethod()]
        public void AddTest()
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
             _current.Add(registrationData);

            //assert
            Assert.IsTrue(true);

        }

       
    }
}