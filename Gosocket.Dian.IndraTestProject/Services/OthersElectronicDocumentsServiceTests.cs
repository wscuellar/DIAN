using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class OthersElectronicDocumentsServiceTests
    {
        private OthersElectronicDocumentsService _current;

        private readonly Mock<IContributorService> _contributorService = new Mock<IContributorService>();
        private readonly Mock<IOthersDocsElecContributorService> _othersDocsElecContributorService = new Mock<IOthersDocsElecContributorService>();
        private readonly Mock<IOthersDocsElecSoftwareService> _othersDocsElecSoftwareService = new Mock<IOthersDocsElecSoftwareService>();
        private readonly Mock<IGlobalOtherDocElecOperationService> _globalOtherDocElecOperationService = new Mock<IGlobalOtherDocElecOperationService>();
        private readonly Mock<ITestSetOthersDocumentsResultService> _testSetOthersDocumentsResultService = new Mock<ITestSetOthersDocumentsResultService>();
        private readonly Mock<IOthersDocsElecContributorRepository> _othersDocsElecContributorRepository = new Mock<IOthersDocsElecContributorRepository>();
        private readonly Mock<IOthersDocsElecContributorOperationRepository> _othersDocsElecContributorOperationRepository = new Mock<IOthersDocsElecContributorOperationRepository>();
        private readonly Mock<IEquivalentElectronicDocumentRepository> _equivalentElectronicDocumentRepository = new Mock<IEquivalentElectronicDocumentRepository>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new OthersElectronicDocumentsService(_contributorService.Object,
                        _othersDocsElecSoftwareService.Object,
                        _othersDocsElecContributorService.Object,
                        _othersDocsElecContributorOperationRepository.Object,
                        _othersDocsElecContributorRepository.Object,
                        _globalOtherDocElecOperationService.Object,
                        _testSetOthersDocumentsResultService.Object,
                        _equivalentElectronicDocumentRepository.Object);
        }


        [TestMethod()]
        public void ValidationTest()
        {
            //arrange 
            //act
            var Result = _current.Validation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>());

            //assert
            Assert.IsNotNull(Result);
            Assert.AreEqual(Result.Message, TextResources.FailedValidation);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Sin Test de pruebas")]
        [DataRow(2, DisplayName = "Con software en proceso")]
        [DataRow(3, DisplayName = "Ya tienes asociado software")]
        [DataRow(4, DisplayName = "Datos actualizados")]
        public void AddOtherDocElecContributorOperationTest(int input)
        {
            //arrange
            ResponseMessage result;
            OtherDocElecContributorOperations ContributorOperation = new OtherDocElecContributorOperations();
            OtherDocElecSoftware software = new OtherDocElecSoftware();

            _ = _othersDocsElecContributorRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributor, bool>>>()))
                .Returns(new OtherDocElecContributor() { OtherDocElecOperationModeId = 1, ElectronicDocumentId = 1, Contributor = new Domain.Contributor() { Id = 77, Code = "77" } });

            switch (input)
            {
                case 1:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns((GlobalTestSetOthersDocuments)null);
                    //act
                    result = _current.AddOtherDocElecContributorOperation(It.IsAny<OtherDocElecContributorOperations>(), It.IsAny<OtherDocElecSoftware>(), true, true);
                    //Assert
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Message, TextResources.ModeElectroniDocWithoutTestSet);
                    break;
                case 2:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns(new GlobalTestSetOthersDocuments());
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.List(It.IsAny<Expression<Func<OtherDocElecContributorOperations, bool>>>()))
                         .Returns(new List<OtherDocElecContributorOperations> { new OtherDocElecContributorOperations() { Id = 7 } });
                    //act
                    result = _current.AddOtherDocElecContributorOperation(ContributorOperation, software, true, true);
                    //Assert
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Message, TextResources.OperationFailOtherInProcess);
                    break;
                case 3:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns(new GlobalTestSetOthersDocuments());
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributorOperations, bool>>>())).Returns(new OtherDocElecContributorOperations());
                    //act
                    result = _current.AddOtherDocElecContributorOperation(ContributorOperation, software, true, false);

                    //Assert
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Message, TextResources.ExistingSoftware);
                    break;
                /*case 4:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns(new GlobalTestSetOthersDocuments());
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributorOperations, bool>>>())).Returns((OtherDocElecContributorOperations)null);
                    _ = _othersDocsElecSoftwareService.Setup(x => x.CreateSoftware(It.IsAny<OtherDocElecSoftware>())).Returns(new OtherDocElecSoftware());

                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.Add(ContributorOperation)).Returns(1);
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributorOperations, bool>>>()))
                        .Returns(new OtherDocElecContributorOperations() { SoftwareId = Guid.NewGuid(), Software = new OtherDocElecSoftware(), SoftwareType = 7 });
                    // se asigna el nuevo set de pruebas...
                    //ApplyTestSet
                    _ = _globalOtherDocElecOperationService.Setup(x => x.GetOperation(It.IsAny<string>(), It.IsAny<Guid>())).Returns((GlobalOtherDocElecOperation)null);
                    _ = _globalOtherDocElecOperationService.Setup(x => x.Insert(It.IsAny<GlobalOtherDocElecOperation>(), It.IsAny<OtherDocElecSoftware>())).Returns(true);
                    _ = _testSetOthersDocumentsResultService.Setup(x => x.InsertTestSetResult(It.IsAny<GlobalTestSetOthersDocumentsResult>())).Returns(false);
                    //act
                    result = _current.AddOtherDocElecContributorOperation(ContributorOperation, software, true, false);
                    //Assert
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Message, TextResources.SuccessSoftware);
                    break;*/
            }
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Sin contributors")]
        [DataRow(2, DisplayName = "Con contributors")]
        public void ChangeParticipantStatusTest(int input)
        {
            bool resultbool;
            switch (input)
            {
                case 1:
                    //arrange
                    _ = _othersDocsElecContributorRepository.Setup(x => x.List(It.IsAny<Expression<Func<OtherDocElecContributor, bool>>>(), 0, 0))
                        .Returns(new PagedResult<OtherDocElecContributor>() { Results = new List<OtherDocElecContributor>() });
                    //act
                    resultbool = _current.ChangeParticipantStatus(1, string.Empty, 1, string.Empty, string.Empty);

                    //Assert
                    Assert.IsNotNull(resultbool);
                    Assert.IsTrue(!resultbool);
                    break;
                case 2:
                    //arrange
                    _ = _othersDocsElecContributorRepository.Setup(x => x.List(It.IsAny<Expression<Func<OtherDocElecContributor, bool>>>(), 0, 0))
                        .Returns(new PagedResult<OtherDocElecContributor>() { Results = new List<OtherDocElecContributor>() { new OtherDocElecContributor() { OtherDocElecContributorTypeId = 1 } } });
                    _ = _othersDocsElecContributorRepository.Setup(x => x.AddOrUpdate(It.IsAny<OtherDocElecContributor>())).Returns(7);
                    //CancelParticipant
                    _ = _othersDocsElecSoftwareService.Setup(x => x.List(It.IsAny<int>())).Returns(new List<OtherDocElecSoftware>() { new OtherDocElecSoftware() { Id = Guid.NewGuid() } });
                    _ = _othersDocsElecSoftwareService.Setup(x => x.DeleteSoftware(It.IsAny<Guid>())).Returns(Guid.NewGuid());
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.List(It.IsAny<Expression<Func<OtherDocElecContributorOperations, bool>>>()))
                        .Returns(new List<OtherDocElecContributorOperations> { new OtherDocElecContributorOperations() { Id = 7007 } });
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.Update(It.IsAny<OtherDocElecContributorOperations>())).Returns(true);

                    //act
                    resultbool = _current.ChangeParticipantStatus(1, OtherDocElecState.Test.GetDescription(), 1, string.Empty, string.Empty);
                    //Assert
                    Assert.IsNotNull(resultbool);
                    Assert.IsTrue(resultbool);
                    break;
            }
        }

        [TestMethod()]
        [DataRow(1, DisplayName = "Sin Contributors")]
        [DataRow(2, DisplayName = "Con Contributors")]
        public void ChangeContributorStepTest(int input)
        {
            bool Result;

            switch (input)
            {
                case 1:
                    //arrange
                    _ = _othersDocsElecContributorRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributor, bool>>>()))
                        .Returns((OtherDocElecContributor)null);
                    //act
                    Result = _current.ChangeContributorStep(1, 1);
                    //Assert
                    Assert.IsNotNull(Result);
                    Assert.IsTrue(!Result);
                    break;
                case 2:
                    //arrange
                    _ = _othersDocsElecContributorRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributor, bool>>>()))
                        .Returns(new OtherDocElecContributor());
                    _ = _othersDocsElecContributorRepository.Setup(x => x.AddOrUpdate(It.IsAny<OtherDocElecContributor>())).Returns(7);
                    //act
                    Result = _current.ChangeContributorStep(1, 1);
                    //Assert
                    Assert.IsNotNull(Result);
                    Assert.IsTrue(Result);
                    break;
            }
        }

        [TestMethod()]
        public void CustormerListTest()
        {
            //arrange
            _ = _othersDocsElecContributorRepository.Setup(x => x.CustomerList(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new PagedResult<OtherDocElecCustomerList>() { PageSize = 702 });
            //act
            var Result = _current.CustormerList(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<OtherDocElecState>(), It.IsAny<int>(), It.IsAny<int>());
            //Assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(PagedResult<OtherDocElecCustomerList>));
        }

        [TestMethod()]
        [DataRow(1, DisplayName = "Sin contributors")]
        [DataRow(2, DisplayName = "Con contributors")]
        public void OperationDeleteTest(int input)
        {
            ResponseMessage Result;
            //arrange
            _ = _othersDocsElecContributorOperationRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributorOperations, bool>>>()))
                .Returns(new OtherDocElecContributorOperations() { Id = 703, SoftwareId = Guid.NewGuid() });
            switch (input)
            {
                case 1:
                    //arrange
                    _ = _othersDocsElecSoftwareService.Setup(x => x.Get(It.IsAny<Guid>())).Returns(new OtherDocElecSoftware() { OtherDocElecSoftwareStatusId = 2 });
                    //act
                    Result = _current.OperationDelete(1);
                    //Assert
                    Assert.IsNotNull(Result);
                    Assert.IsTrue(Result.Message.Equals("El software encuentra en estado aceptado."));
                    break;
                case 2:
                    //arrange
                    _ = _othersDocsElecSoftwareService.Setup(x => x.Get(It.IsAny<Guid>())).Returns(new OtherDocElecSoftware() { OtherDocElecSoftwareStatusId = 1 });
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.Delete(It.IsAny<int>())).Returns(new ResponseMessage() { Message = "Ok OperationDeleteTest" });
                    _ = _othersDocsElecSoftwareService.Setup(x => x.DeleteSoftware(It.IsAny<Guid>())).Returns(Guid.NewGuid());
                    //act
                    Result = _current.OperationDelete(1);
                    //Assert
                    Assert.IsNotNull(Result);
                    Assert.IsTrue(Result.Message.Equals("Ok OperationDeleteTest"));
                    break;
            }
        }

        [TestMethod()]
        public void GetOtherDocElecContributorOperationBySoftwareIdTest()
        {
            //arrange 
            _othersDocsElecContributorOperationRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributorOperations, bool>>>()))
                .Returns(new OtherDocElecContributorOperations() { Id = 704 });
            //act
            var Result = _current.GetOtherDocElecContributorOperationBySoftwareId(It.IsAny<Guid>());

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(OtherDocElecContributorOperations));
        }

        [TestMethod()]
        public void UpdateOtherDocElecContributorOperationTest()
        {
            //arrange 
            _othersDocsElecContributorOperationRepository.Setup(x => x.Update(It.IsAny<OtherDocElecContributorOperations>()))
                .Returns(true);
            //act
            var Result = _current.UpdateOtherDocElecContributorOperation(It.IsAny<OtherDocElecContributorOperations>());

            //assert
            Assert.IsNotNull(Result);
            Assert.IsTrue(Result);
        }

        [TestMethod()]
        public void GetOtherDocElecContributorOperationByIdTest()
        {
            //arrange 
            _othersDocsElecContributorOperationRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributorOperations, bool>>>()))
                .Returns(new OtherDocElecContributorOperations() { Id = 705 });
            //act
            var Result = _current.GetOtherDocElecContributorOperationById(It.IsAny<int>());

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(OtherDocElecContributorOperations));
        }

        [TestMethod()]
        public void GetOtherDocElecContributorOperationByDocEleContributorIdTest()
        {
            //arrange 
            _othersDocsElecContributorOperationRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributorOperations, bool>>>()))
                .Returns(new OtherDocElecContributorOperations() { Id = 706 });
            //act
            var Result = _current.GetOtherDocElecContributorOperationByDocEleContributorId(It.IsAny<int>());

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(OtherDocElecContributorOperations));
        }

        [TestMethod()]
        public void GetOtherDocElecContributorOperationsListByDocElecContributorIdTest()
        {
            //arrange 
            _othersDocsElecContributorOperationRepository.Setup(x => x.List(It.IsAny<Expression<Func<OtherDocElecContributorOperations, bool>>>()))
                .Returns(new List<OtherDocElecContributorOperations> { new OtherDocElecContributorOperations() { Id = 707 } });
            //act
            var Result = _current.GetOtherDocElecContributorOperationsListByDocElecContributorId(It.IsAny<int>());

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(List<OtherDocElecContributorOperations>));
        }
    }
}