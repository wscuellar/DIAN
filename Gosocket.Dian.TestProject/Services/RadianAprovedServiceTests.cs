using Gosocket.Dian.Domain;
using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianAprovedServiceTests
    {
        private readonly Mock<IRadianApprovedService> _current = new Mock<IRadianApprovedService>();

        [TestMethod()]
        public void FindContributorAndSoftwareTest()
        {
            //Arrange
            int radianContributorId = 1;
            string softwareId = "";

            _current.Setup(t => t.FindNamesContributorAndSoftware(radianContributorId, softwareId))
                .Returns(It.IsAny<Tuple<string, string>>());

            Tuple<string, string> expected = null;

            //ACT
            var actual = _current.Object.FindNamesContributorAndSoftware(radianContributorId, softwareId);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ListContributorByTypeTest()
        {
            // Arrange
            int radianContributorTypeId = 0;
            _current.Setup(t => t.ListContributorByType(radianContributorTypeId))
                .Returns(It.IsAny<List<RadianContributor>>());

            List<RadianContributor> expected = null;

            //ACT
            var actual = _current.Object.ListContributorByType(radianContributorTypeId);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ListSoftwareByContributorTest()
        {
            // Arrange
            int radianContributorId = 0;

            _current.Setup(t => t.ListSoftwareByContributor(radianContributorId))
                .Returns(It.IsAny<List<Software>>());

            List<Software> expected = null;

            //ACT
            var actual = _current.Object.ListSoftwareByContributor(radianContributorId);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ListSoftwareModeOperationTest()
        {
            // Arrange
            _current.Setup(t => t.ListSoftwareModeOperation())
                .Returns(It.IsAny<List<RadianOperationMode>>());

            List<RadianOperationMode> expected = null;

            //ACT
            var actual = _current.Object.ListSoftwareModeOperation();

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}