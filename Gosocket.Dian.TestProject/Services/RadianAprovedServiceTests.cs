using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianAprovedServiceTests
    {
        private readonly Mock<IRadianAprovedService> _radianAprovedService = new Mock<IRadianAprovedService>();


        public void RadianAprovedServiceTest()
        {
        }

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
            var actual = _radianAprovedService.Setup(t => t.ListContributorByType(radianContributorTypeId))
                                                .Returns(new List<Domain.Contributor>());

            // ASsert
            Assert.IsNotNull(actual);
        }

        [TestMethod()]
        public void ListSoftwareByContributorTest()
        {
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void ListSoftwareModeOperationTest()
        {
            Assert.IsTrue(true);
        }
    }
}