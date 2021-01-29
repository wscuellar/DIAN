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

       

        //[TestMethod()]
        //public void GetRadianContributorTest()
        //{
        //    // Arrange
        //    int radianContributorId = 0;

        //    _current.Setup(t => t.GetRadianContributor(radianContributorId))
        //        .Returns(It.IsAny<RadianContributor>());

        //    RadianContributor expected = null;

        //    //ACT
        //    var actual = _current.Object.GetRadianContributor(radianContributorId);

        //    // Assert
        //    Assert.AreEqual(expected, actual);
        //}

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
            int radianContributorType = 0;

            _current.Setup(t => t.ContributorSummary(contributorId, radianContributorType))
                .Returns(It.IsAny<RadianAdmin>());

            RadianAdmin expected = null;

            //ACT
            var actual = _current.Object.ContributorSummary(contributorId, radianContributorType);

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

            _current.Setup(t => t.OperationDelete(radianContributorOperatorId))
                .Returns(It.IsAny<ResponseMessage>());

            ResponseMessage expected = null;

            //ACT
            var actual = _current.Object.OperationDelete(radianContributorOperatorId);

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

        [TestMethod()]
        public void UpdateRadianContributorStepTest()
        {
            // Arrange
            int radianContributorId = 0, radianContributorStep = 0;

            _current.Setup(t => t.UpdateRadianContributorStep(radianContributorId, radianContributorStep))
                .Returns(It.IsAny<ResponseMessage>());

            ResponseMessage expected = null;

            //ACT
            var actual = _current.Object.UpdateRadianContributorStep(radianContributorId, radianContributorStep);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RadianContributorIdTest()
        {
            // Arrange
            int contributorId = 1;
            int contributorTypeId = 1;
            string state = "";

            _current.Setup(t => t.RadianContributorId(contributorId, contributorTypeId, state))
                .Returns(It.IsAny<int>());

            int expected = 0;

            //ACT
            var actual = _current.Object.RadianContributorId(contributorId, contributorTypeId, state);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        //[TestMethod()]
        //public void AddRadianContributorOperationTest()
        //{
        //    // Arrange
        //    //RadianContributorOperation radianContributorOperation = null;

        //    //_current.Setup(t => t.AddRadianContributorOperation(radianContributorOperation))
        //    //    .Returns(It.IsAny<int>());

        //    //int expected = 0;

        //    ////ACT
        //    //var actual = _current.Object.AddRadianContributorOperation(radianContributorOperation);

        //    //// Assert
        //    //Assert.AreEqual(expected, actual);
        //}

        [TestMethod()]
        public void ListRadianContributorOperationsTest()
        {
            // Arrange
            int radianContributorId = 1;

            _current.Setup(t => t.ListRadianContributorOperations(radianContributorId))
                .Returns(It.IsAny<RadianContributorOperationWithSoftware>());

           RadianContributorOperationWithSoftware expected = null;

            //ACT
            var actual = _current.Object.ListRadianContributorOperations(radianContributorId);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        
        [TestMethod()]
        public void SoftwareByContributorTest()
        {
            // Arrange
            int contributorId = 0;

            _current.Setup(t => t.SoftwareByContributor(contributorId))
                .Returns(It.IsAny<Software>());

            Software expected = null;

            //ACT
            var actual = _current.Object.SoftwareByContributor(contributorId);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}