using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Moq;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain.Sql;
using System.Linq.Expressions;
using Gosocket.Dian.Domain.Common;

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


        [TestInitialize]
        public void TestInitialize()
        {
            _current = new OthersElectronicDocumentsService(_contributorService.Object,
                        _othersDocsElecSoftwareService.Object,
                        _othersDocsElecContributorService.Object,
                        _othersDocsElecContributorOperationRepository.Object,
                        _othersDocsElecContributorRepository.Object,
                        _globalOtherDocElecOperationService.Object,
                        _testSetOthersDocumentsResultService.Object);
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

            _ = _othersDocsElecContributorRepository.Setup(x => x.Get(t => t.Id == ContributorOperation.OtherDocElecContributorId))
                .Returns(new OtherDocElecContributor() { OtherDocElecOperationModeId = 1, ElectronicDocumentId = 1 });

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
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.List(t => t.OtherDocElecContributorId == ContributorOperation.OtherDocElecContributorId
                                                                    && t.SoftwareType == ContributorOperation.SoftwareType
                                                                    && t.OperationStatusId == (int)OtherDocElecState.Test
                                                                    && !t.Deleted))
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
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.Get(t => t.OtherDocElecContributorId == ContributorOperation.OtherDocElecContributorId && t.SoftwareId == ContributorOperation.SoftwareId && !t.Deleted))
                    .Returns(new OtherDocElecContributorOperations());
                    //act
                    result = _current.AddOtherDocElecContributorOperation(ContributorOperation, software, true, false);

                    //Assert
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Message, TextResources.ExistingSoftware);
                    break;
                case 4:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns(new GlobalTestSetOthersDocuments());
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.Get(t => t.OtherDocElecContributorId == ContributorOperation.OtherDocElecContributorId && t.SoftwareId == ContributorOperation.SoftwareId && !t.Deleted))
                    .Returns((OtherDocElecContributorOperations)null);
                    _ = _othersDocsElecSoftwareService.Setup(x => x.CreateSoftware(new OtherDocElecSoftware())).Returns(new OtherDocElecSoftware());

                    _globalOtherDocElecOperationService.GetOperation(contributor.Code, software.SoftwareId);
                    _globalOtherDocElecOperationService.Insert(operation, existingOperation.Software)
                        _ = _testSetOthersDocumentsResultService.InsertTestSetResult(setResult);

                    //act
                    result = _current.AddOtherDocElecContributorOperation(ContributorOperation, software, true, false);
                    //Assert
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Message, TextResources.SuccessSoftware);
                    break;
            }
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Sin contributors")]
        [DataRow(2, DisplayName = "Con contributors")]
        public void ChangeParticipantStatusTest(int input)
        {
            //arrange 
            OtherDocElecContributorOperations ContributorOperation = new OtherDocElecContributorOperations();
            OtherDocElecSoftware software = new OtherDocElecSoftware();

            _ = _othersDocsElecContributorRepository.Setup(x => x.Get(t => t.Id == ContributorOperation.OtherDocElecContributorId))
                .Returns(new OtherDocElecContributor() { OtherDocElecOperationModeId = 1, ElectronicDocumentId = 1, State = OtherDocElecState.Cancelado.GetDescription() });

            bool resultbool; string actualState = string.Empty; int contributorId = 0;
            switch (input)
            {
                case 1:
                    //arrange
                    _ = _othersDocsElecContributorRepository.Setup(x => x.List(t => t.Id == contributorId && t.State == actualState, 0, 0).Results).Returns((List<OtherDocElecContributor>)null);
                    //act
                    resultbool = _current.ChangeParticipantStatus(1, string.Empty, 1, string.Empty, string.Empty);

                    //Assert
                    Assert.IsNotNull(resultbool);
                    Assert.IsTrue(!resultbool);
                    break;
                case 2:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns(new GlobalTestSetOthersDocuments());
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.List(t => t.OtherDocElecContributorId == ContributorOperation.OtherDocElecContributorId
                                                                    && t.SoftwareType == ContributorOperation.SoftwareType
                                                                    && t.OperationStatusId == (int)OtherDocElecState.Test
                                                                    && !t.Deleted))
                         .Returns(new List<OtherDocElecContributorOperations> { new OtherDocElecContributorOperations() { Id = 7 } });

                    _ = _othersDocsElecContributorRepository.Setup(x => x.AddOrUpdate(new OtherDocElecContributor())).Returns(1);
                    _ = _othersDocsElecSoftwareService.Setup(x => x.List(It.IsAny<int>())).Returns(new List<OtherDocElecSoftware> { new OtherDocElecSoftware() { Id = Guid.NewGuid() } });
                    _ = _othersDocsElecSoftwareService.Setup(x => x.DeleteSoftware(It.IsAny<Guid>())).Returns(Guid.NewGuid());
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.List(t => t.SoftwareId == software.Id && !t.Deleted))
                        .Returns(new List<OtherDocElecContributorOperations> { new OtherDocElecContributorOperations() { Id = 1 } });
                    _ = _othersDocsElecContributorOperationRepository.Setup(x => x.Update(new OtherDocElecContributorOperations())).Returns(true);
                    //act
                    resultbool = _current.ChangeParticipantStatus(1, string.Empty, 1, string.Empty, string.Empty);
                    //Assert
                    Assert.IsNotNull(resultbool);
                    Assert.IsTrue(resultbool);
                    break;
            }
        }

        [TestMethod()]
        public void ChangeContributorStepTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CustormerListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void OperationDeleteTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetOtherDocElecContributorOperationBySoftwareIdTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateOtherDocElecContributorOperationTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetOtherDocElecContributorOperationByIdTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetOtherDocElecContributorOperationByDocEleContributorIdTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetOtherDocElecContributorOperationsListByDocElecContributorIdTest()
        {
            Assert.Fail();
        }
    }
}