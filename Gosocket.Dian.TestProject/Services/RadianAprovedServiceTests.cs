﻿using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;

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

        [TestMethod()]
        public void GetRadianContributorTest()
        {
            // Arrange
            int radianContributorId = 0;

            _current.Setup(t => t.GetRadianContributor(radianContributorId))
                .Returns(It.IsAny<RadianContributor>());

            RadianContributor expected = null;

            //ACT
            var actual = _current.Object.GetRadianContributor(radianContributorId);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ListContributorFilesTest()
        {
            // Arrange
            int radianContributorId = 0;

            _current.Setup(t => t.ListContributorFiles(radianContributorId))
                .Returns(It.IsAny<List<RadianContributorFile>>());

            List<RadianContributorFile> expected = null;

            //ACT
            var actual = _current.Object.ListContributorFiles(radianContributorId);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ContributorSummaryTest()
        {
            // Arrange
            int contributorId = 0;

            _current.Setup(t => t.ContributorSummary(contributorId))
                .Returns(It.IsAny<RadianAdmin>());

            RadianAdmin expected = null;

            //ACT
            var actual = _current.Object.ContributorSummary(contributorId);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ContributorFileTypeListTest()
        {
            // Arrange
            int typeId = 0;

            _current.Setup(t => t.ContributorFileTypeList(typeId))
                .Returns(It.IsAny<List<RadianContributorFileType>>());

            List<RadianContributorFileType> expected = null;

            //ACT
            var actual = _current.Object.ContributorFileTypeList(typeId);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            // Arrange
            int radianContributorOperatorId = 0;

            _current.Setup(t => t.Update(radianContributorOperatorId))
                .Returns(It.IsAny<ResponseMessage>());

            ResponseMessage expected = null;

            //ACT
            var actual = _current.Object.Update(radianContributorOperatorId);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void UploadFileTest()
        {
            // Arrange
            Stream fileStream = null;
            string code = "";
            RadianContributorFile radianContributorFile = null;

            _current.Setup(t => t.UploadFile(fileStream, code, radianContributorFile))
                .Returns(It.IsAny<ResponseMessage>());

            ResponseMessage expected = null;

            //ACT
            var actual = _current.Object.UploadFile(fileStream, code, radianContributorFile);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void AddFileHistoryTest()
        {
            // Arrange
            RadianContributorFileHistory radianContributorFileHistory = null;

            _current.Setup(t => t.AddFileHistory(radianContributorFileHistory))
                .Returns(It.IsAny<ResponseMessage>());

            ResponseMessage expected = null;

            //ACT
            var actual = _current.Object.AddFileHistory(radianContributorFileHistory);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}