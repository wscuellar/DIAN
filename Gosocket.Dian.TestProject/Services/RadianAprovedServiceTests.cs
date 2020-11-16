using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianAprovedServiceTests
    {
        private readonly Mock<IRadianApprovedService> _radianAprovedService = new Mock<IRadianApprovedService>();


        

        [TestMethod()]
        public void FindContributorAndSoftwareTest()
        {
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void ListContributorByTypeTest()
        {
            // Arrange
            _radianAprovedService.Setup(t => true);
            int radianContributorTypeId = 1;

            //ACT
            //var actual = _radianAprovedService.Setup(t => t.ListContributorByType(radianContributorTypeId))
            //                                    .Returns(new List<Domain.Contributor>());

            // ASsert
            //Assert.IsNotNull(actual);

            Assert.Fail();
        }

        [TestMethod()]
        public void ListSoftwareByContributorTest()
        {
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void ListSoftwareModeOperationTest()
        {
            // Arrange 
            _radianAprovedService.Setup(t => true);

            // Act
            //var actual = _radianAprovedService.Setup(t => t.ListSoftwareModeOperation())
            //                                .Returns(new List<RadianOperationMode>);

            // Assert
            //Assert.IsNotNull(actual);
            Assert.Fail();
        }
    }
}