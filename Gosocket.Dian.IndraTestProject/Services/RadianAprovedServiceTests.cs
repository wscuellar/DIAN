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
using System.IO;
using Gosocket.Dian.Common.Resources;

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
        private readonly Mock<IGlobalAuthorizationService> _globalAuthorizationService = new Mock<IGlobalAuthorizationService>();
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
                                _globalRadianOperationService.Object,
                                _globalAuthorizationService.Object
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
                    _contributorOperationsService.Setup(t => t.GetContributorOperations(contributorId)).Returns(new List<ContributorOperations>() { new ContributorOperations() { Deleted = false, OperationModeId = 2, Software = new Software() { Id = Guid.NewGuid(), Status = true } } });
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
            switch (input)
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

        [TestMethod]
        public void UploadFileTest()
        {
            //arrange
            Stream fileStream = null;
            string code = "1";
            RadianContributorFile radianContributorFile = new RadianContributorFile() { FileName = "test" };

            //act
            ResponseMessage response = _current.UploadFile(fileStream, code, radianContributorFile);

            //assert
            Assert.IsNotNull(response);

        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public void AddFileHistoryTest(int input)
        {
            //arrange
            RadianContributorFileHistory radianContributorFileHistory = new RadianContributorFileHistory();
            if (input == 1)
                _radianContributorFileHistoryRepository.Setup(t => t.AddRegisterHistory(It.IsAny<RadianContributorFileHistory>())).Returns(new Guid("A511E53C-C0A0-4471-9D68-51968AFE1346"));
            else
                _radianContributorFileHistoryRepository.Setup(t => t.AddRegisterHistory(It.IsAny<RadianContributorFileHistory>())).Returns(Guid.Empty);

            //act
            ResponseMessage responseMessage = _current.AddFileHistory(radianContributorFileHistory);

            //assert
            if (input == 1)
                Assert.AreEqual("Información registrada id: a511e53c-c0a0-4471-9d68-51968afe1346", responseMessage.Message);
            else
                Assert.AreEqual("El registro no pudo ser guardado", responseMessage.Message);
        }

        [TestMethod]
        [DataRow(true, "Paso actualizado")]
        [DataRow(false, "El registro no pudo ser actualizado")]
        public void UpdateRadianContributorStepTest(bool action, string expected)
        {
            //arrange
            int radianContributorId = 1;
            int radianContributorStep = 3;
            _radianContributorService.Setup(t => t.ChangeContributorStep(radianContributorId, radianContributorStep)).Returns(action);

            //act
            ResponseMessage responseMessage = _current.UpdateRadianContributorStep(radianContributorId, radianContributorStep);

            //assert
            Assert.AreEqual(expected, responseMessage.Message);
        }

        [TestMethod]
        public void RadianContributorIdTest()
        {
            //arrange
            int contributorId = 1;
            int contributorTypeId = 1;
            string state = "Registrado";

            _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor() { Id = 1 });

            //act
            int id = _current.RadianContributorId(contributorId, contributorTypeId, state);

            //assert
            Assert.AreEqual(1, id);
        }

        [TestMethod]
        public void GetTestSetTest()
        {
            //arrange
            _radianTestSetService.Setup(t => t.GetTestSet(It.IsAny<string>(), It.IsAny<string>())).Returns(new RadianTestSet());

            //act
            RadianTestSet testSet = _current.GetTestSet("1");

            //assert
            Assert.IsNotNull(testSet);
        }


        [TestMethod]
        [DataRow(1, DisplayName = "ModeWithoutTestSet")]
        [DataRow(2, DisplayName = "OperationFailOtherInProcess")]
        [DataRow(3, DisplayName = "ExistingSoftware")]
        [DataRow(4, DisplayName = "ExistingSoftware")]
        public async void AddRadianContributorOperationTest(int input)
        {
            //arrange
            RadianContributorOperation radianContributorOperation = new RadianContributorOperation()
            {
                OperationStatusId = (int)RadianState.Test
            };
            RadianSoftware software = new RadianSoftware();
            RadianTestSet testSet = new RadianTestSet();
            bool isInsert = true;
            bool validateOperation = true;
            switch (input)
            {
                case 1:
                    testSet = null;
                    break;
                case 2:
                    _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor());
                    _radianContributorRepository.Setup(t => t.GetParticipantWithActiveProcess(It.IsAny<int>())).Returns(true);
                    break;
                case 3:
                    _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor());
                    _radianContributorRepository.Setup(t => t.GetParticipantWithActiveProcess(It.IsAny<int>())).Returns(false);
                    _radianContributorOperationRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributorOperation, bool>>>())).Returns(new RadianContributorOperation());
                    break;
                case 4:
                    _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor() { RadianState = "Registrado", Contributor = new Contributor() });
                    _radianContributorRepository.Setup(t => t.GetParticipantWithActiveProcess(It.IsAny<int>())).Returns(false);
                    _radianContributorOperationRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributorOperation, bool>>>())).Returns((RadianContributorOperation)null);
                    _radianCallSoftwareService.Setup(t => t.CreateSoftware(software)).Returns(new RadianSoftware());
                    _radianContributorOperationRepository.Setup(t => t.Add(It.IsAny<RadianContributorOperation>())).Returns(1);
                    _globalRadianOperationService.Setup(t => t.GetOperation(It.IsAny<string>(), It.IsAny<Guid>())).Returns((GlobalRadianOperations)null);
                    _radianContributorOperationRepository.Setup(t => t.Get(k => k.Id == 1)).Returns(new RadianContributorOperation());
                    _globalRadianOperationService.Setup(t => t.Insert(It.IsAny<GlobalRadianOperations>(), It.IsAny<RadianSoftware>())).Returns(true);

                    _radianTestSetResultService.Setup(t => t.InsertTestSetResult(It.IsAny<RadianTestSetResult>())).Returns(true);

                    break;
            }

            //act
            var responseMessage = await _current.AddRadianContributorOperation(radianContributorOperation, software, testSet, isInsert, validateOperation);

            //assert
            switch (input)
            {
                case 1:
                    Assert.AreEqual(TextResources.ModeWithoutTestSet, responseMessage.Message);
                    break;
                case 2:
                    Assert.AreEqual(TextResources.OperationFailOtherInProcess, responseMessage.Message);
                    break;
                case 3:
                    Assert.AreEqual(TextResources.ExistingSoftware, responseMessage.Message);
                    break;
                case 4:
                    Assert.AreEqual(TextResources.SuccessSoftware, responseMessage.Message);
                    break;
            }
        }

        [TestMethod]
        public void ListRadianContributorOperationsTest()
        {
            //arrange
            int radianContributorId = 1;
            _radianContributorOperationRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributorOperation, bool>>>())).Returns(new List<RadianContributorOperation>() { new RadianContributorOperation() });

            //act
            RadianContributorOperationWithSoftware result = _current.ListRadianContributorOperations(radianContributorId);

            //assert
            Assert.IsNotNull(result);

        }

        [TestMethod]
        public void RadianTestSetResultByNitTest()
        {
            //arrange
            string nit = "1";
            string idTestSet = "1";
            _radianTestSetResultService.Setup(t => t.GetTestSetResultByNit(nit)).Returns(new List<RadianTestSetResult>() { new RadianTestSetResult() { Id = idTestSet } });

            //act
            RadianTestSetResult testSetResult = _current.RadianTestSetResultByNit(nit, idTestSet);

            //assert
            Assert.IsNotNull(testSetResult);
        }

        [TestMethod]
        public void SoftwareListTest()
        {
            //arrange
            int radianContributorId = 1;
            _radianContributorRepository.Setup(t => t.RadianSoftwareByParticipante(radianContributorId)).Returns(new List<RadianSoftware>() { new RadianSoftware() });

            //act
            List<RadianSoftware> radianSoftwares = _current.SoftwareList(radianContributorId);

            //assert
            Assert.AreEqual(1, radianSoftwares.Count);
        }

        [TestMethod]
        public void GetSoftwareTest()
        {
            //arrange
            Guid id = Guid.NewGuid();
            _radianCallSoftwareService.Setup(t => t.Get(id)).Returns(new RadianSoftware());

            //act
            RadianSoftware radianSoftware = _current.GetSoftware(id);

            //assert
            Assert.IsNotNull(radianSoftware);

        }


        [TestMethod]
        public void GetSoftwareTest2()
        {
            //arrange
            int radianContributorId = 1;
            int softwareType = 1;
            _radianContributorOperationRepository.Setup(t => t.Get(k => k.RadianContributorId == radianContributorId && k.SoftwareType == softwareType)).Returns(new RadianContributorOperation());
            _radianCallSoftwareService.Setup(t => t.Get(It.IsAny<Guid>())).Returns(new RadianSoftware());

            //act
            RadianSoftware radianSoftware = _current.GetSoftware(radianContributorId, softwareType);

            //assert
            Assert.IsNotNull(radianSoftware);

        }

        [TestMethod]
        [DataRow(RadianOperationModeTestSet.OwnSoftware)]
        [DataRow(RadianOperationModeTestSet.SoftwareFactor)]
        public void AutoCompleteProviderTest(RadianOperationModeTestSet softwareType)
        {
            //arrange
            int contributorId = 1;
            int contributorTypeId = 1;
            string term = string.Empty;

            if (softwareType == RadianOperationModeTestSet.OwnSoftware)
                _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new PagedResult<RadianContributor>() { Results = new List<RadianContributor>() });
            else
                _radianContributorRepository.Setup(t => t.ActiveParticipantsWithSoftware((int)softwareType)).Returns(new List<RadianContributor>());

            //act
            List<RadianContributor> participants = _current.AutoCompleteProvider(contributorId, contributorTypeId, softwareType, term);

            //assert
            Assert.IsNotNull(participants);
        }





    }
}
