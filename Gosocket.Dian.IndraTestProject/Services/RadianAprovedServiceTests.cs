using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Moq;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Interfaces;
using System.Linq.Expressions;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Common;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianAprovedServiceTests
    {

        private readonly Mock<IRadianContributorRepository> _radianContributorRepository = new Mock<IRadianContributorRepository>();
        private readonly Mock<IRadianTestSetService> _radianTestSetService = new Mock<IRadianTestSetService>();
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();
        private readonly Mock<IRadianContributorFileTypeService> _radianContributorFileTypeService = new Mock<IRadianContributorFileTypeService>();
        private readonly Mock<IRadianContributorOperationRepository> _radianContributorOperationRepository = new Mock<IRadianContributorOperationRepository>();
        private readonly Mock<IRadianContributorFileRepository> _radianContributorFileRepository = new Mock<IRadianContributorFileRepository>();
        private readonly Mock<IRadianContributorFileHistoryRepository> _radianContributorFileHistoryRepository = new Mock<IRadianContributorFileHistoryRepository>();
        private readonly Mock<IContributorOperationsService> _contributorOperationsService = new Mock<IContributorOperationsService>();
        private readonly Mock<IRadianTestSetResultService> _radianTestSetResultService = new Mock<IRadianTestSetResultService>();
        private readonly Mock<IRadianCallSoftwareService> _radianCallSoftwareService = new Mock<IRadianCallSoftwareService>();
        private readonly Mock<IGlobalRadianOperationService> _globalRadianOperationService = new Mock<IGlobalRadianOperationService>();
        RadianAprovedService _current;

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianAprovedService(
                                _radianContributorRepository.Object,
                                _radianTestSetService.Object,
                                _radianContributorService.Object,
                                _radianContributorFileTypeService.Object,
                                _radianContributorOperationRepository.Object,
                                _radianContributorFileRepository.Object,
                                _radianContributorFileHistoryRepository.Object,
                                _contributorOperationsService.Object,
                                _radianTestSetResultService.Object,
                                _radianCallSoftwareService.Object,
                                _globalRadianOperationService.Object
                            );

        }

        [TestMethod()]
        public void ListContributorByTypeTest()
        {
            //arrange
            int radianContributorTypeId = 1;
            _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new Domain.Entity.PagedResult<RadianContributor>() { Results = new List<RadianContributor>() { new RadianContributor() } });

            //act
            List<RadianContributor> result = _current.ListContributorByType(radianContributorTypeId);

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetRadianContributorTest()
        {
            //arrange
            int radianContributorId = 1;
            _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor());

            //act
            RadianContributor radianContributor = _current.GetRadianContributor(radianContributorId);

            //assert
            Assert.IsNotNull(radianContributor);

        }

        [TestMethod]
        public void ListContributorFilesTest()
        {
            //arrange
            int radianContributorId = 1;
            ICollection<RadianContributorFile> lst = new List<RadianContributorFile>() { new RadianContributorFile() };
            _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor() { RadianContributorFile = lst });

            //act
            List<RadianContributorFile> result = _current.ListContributorFiles(radianContributorId);

            //assert
            Assert.IsNotNull(result);

        }

        [TestMethod]
        public void ContributorSummaryTest()
        {
            //arrange
            int contributorId = 1;
            int radianContributorType = 1;
            _radianContributorService.Setup(t => t.ContributorSummary(contributorId, radianContributorType)).Returns(new Domain.Entity.RadianAdmin());

            //act
            Domain.Entity.RadianAdmin result = _current.ContributorSummary(contributorId, radianContributorType);

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        public void SoftwareByContributorTest(int input)
        {
            //arrange
            int contributorId = 1;
            switch (input)
            {
                case 1:
                    _contributorOperationsService.Setup(t => t.GetContributorOperations(contributorId)).Returns(new List<ContributorOperations>() { new ContributorOperations() { Deleted=false, OperationModeId=2 ,Software = new Software() { Id = Guid.NewGuid(), Status = true } } });
                    break;
                case 2:
                    _contributorOperationsService.Setup(t => t.GetContributorOperations(contributorId)).Returns((List<ContributorOperations>)null);
                    break;
                case 3:
                    _contributorOperationsService.Setup(t => t.GetContributorOperations(contributorId)).Returns(new List<ContributorOperations>() { new ContributorOperations() { Deleted = true } });
                    break;
            }

            //act
            Software software = _current.SoftwareByContributor(contributorId);

            //assert
            if (input == 1)
                Assert.IsTrue(Guid.Empty != software.Id);
            else
                Assert.AreEqual(Guid.Empty, software.Id);

        }


        [TestMethod]
        public void ContributorFileTypeListTest()
        {
            //arrange
            int typeId = 1;
            _radianContributorFileTypeService.Setup(t => t.FileTypeList()).Returns(new List<RadianContributorFileType>() { new RadianContributorFileType() { RadianContributorTypeId = typeId, Deleted = false } });

            //act
            List<RadianContributorFileType> radianContributorFileTypes = _current.ContributorFileTypeList(typeId);

            //assert
            Assert.IsNotNull(radianContributorFileTypes);
        }

        [TestMethod]
        public void OperationDeleteTest()
        {
            //arrange
            RadianContributorOperation operationToDelete = new RadianContributorOperation();
            _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor() { Contributor = new Contributor() });
            _globalRadianOperationService.Setup(t => t.Delete(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _radianContributorOperationRepository.Setup(t => t.Delete(It.IsAny<int>())).Returns(new ResponseMessage());

            //act
            ResponseMessage responseMessage = _current.OperationDelete(operationToDelete);

            //assert
            Assert.IsNotNull(responseMessage);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public void OperationDeleteTest2(int input)
        {
            //arrange
            int radianContributorOperationId = 1;
            _radianContributorOperationRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributorOperation, bool>>>())).Returns(new RadianContributorOperation() { SoftwareType = (int)RadianOperationModeTestSet.OwnSoftware });
            switch(input)
            {
                case 1:
                    _radianCallSoftwareService.Setup(t => t.Get(It.IsAny<Guid>())).Returns(new RadianSoftware() { RadianSoftwareStatusId = (int)RadianSoftwareStatus.Accepted });
                    break;
                case 2:
                    _radianCallSoftwareService.Setup(t => t.Get(It.IsAny<Guid>())).Returns(new RadianSoftware());
                    _radianCallSoftwareService.Setup(t => t.DeleteSoftware(It.IsAny<Guid>())).Returns(Guid.NewGuid());
                    _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor() { Contributor = new Contributor() });
                    _globalRadianOperationService.Setup(t => t.Delete(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
                    _radianTestSetResultService.Setup(t => t.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>())).Returns(new RadianTestSetResult());
                    _radianTestSetResultService.Setup(t => t.InsertTestSetResult(It.IsAny<RadianTestSetResult>())).Returns(true);
                    _radianContributorOperationRepository.Setup(t => t.Delete(It.IsAny<int>())).Returns(new ResponseMessage());
                    break;
            }
            

            //act
            ResponseMessage responseMessage = _current.OperationDelete(radianContributorOperationId);

            //assert
            Assert.IsNotNull(responseMessage);
        }
    }
}