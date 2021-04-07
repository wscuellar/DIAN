using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Managers;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianContributorServiceTests
    {
        private readonly Mock<IContributorService> _contributorService = new Mock<IContributorService>();
        private readonly Mock<IRadianContributorRepository> _radianContributorRepository = new Mock<IRadianContributorRepository>();
        private readonly Mock<IRadianContributorTypeRepository> _radianContributorTypeRepository = new Mock<IRadianContributorTypeRepository>();
        private readonly Mock<IRadianContributorFileRepository> _radianContributorFileRepository = new Mock<IRadianContributorFileRepository>();
        private readonly Mock<IRadianContributorFileTypeRepository> _radianContributorFileTypeRepository = new Mock<IRadianContributorFileTypeRepository>();
        private readonly Mock<IRadianContributorOperationRepository> _radianContributorOperationRepository = new Mock<IRadianContributorOperationRepository>();
        private readonly Mock<IRadianCallSoftwareService> _radianCallSoftwareService = new Mock<IRadianCallSoftwareService>();
        private readonly Mock<IRadianTestSetResultManager> _radianTestSetResultManager = new Mock<IRadianTestSetResultManager>();
        private readonly Mock<IRadianOperationModeRepository> _radianOperationModeRepository = new Mock<IRadianOperationModeRepository>();
        private readonly Mock<IRadianContributorFileHistoryRepository> _radianContributorFileHistoryRepository = new Mock<IRadianContributorFileHistoryRepository>();
        private readonly Mock<IGlobalRadianOperationService> _globalRadianOperationService = new Mock<IGlobalRadianOperationService>();
        RadianContributorService _current;

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new RadianContributorService(_contributorService.Object,
                _radianContributorRepository.Object,
                _radianContributorTypeRepository.Object,
                _radianContributorFileRepository.Object,
                _radianContributorOperationRepository.Object,
                _radianTestSetResultManager.Object,
                _radianOperationModeRepository.Object,
                _radianContributorFileHistoryRepository.Object,
                _globalRadianOperationService.Object,
                _radianContributorFileTypeRepository.Object,
                _radianCallSoftwareService.Object);

        }

        [TestMethod()]
        [DataRow(1)]
        [DataRow(2)]
        public void SummaryTest(int input)
        {
            //arrange
            int contributorId = 0;
            switch (input)
            {
                case 1:
                    _contributorService.Setup(t => t.Get(contributorId)).Returns(new Domain.Contributor());
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new Domain.Entity.PagedResult<RadianContributor>() { Results = new List<RadianContributor>() { new RadianContributor() { RadianContributorTypeId = 1 } } });
                    break;
                case 2:
                    _contributorService.Setup(t => t.Get(contributorId)).Returns((Domain.Contributor)null);
                    break;
            }


            //act
            NameValueCollection result = _current.Summary(contributorId);

            //assert
            Assert.IsNotNull(result);

        }

        [TestMethod]
        [DataRow(1, 1, Domain.Common.RadianContributorType.ElectronicInvoice, DisplayName = "without Software Own ")]
        [DataRow(2, 1, Domain.Common.RadianContributorType.ElectronicInvoice, DisplayName = "without Software Own ")]
        [DataRow(3, 1, Domain.Common.RadianContributorType.ElectronicInvoice, DisplayName = "Operation Other Process")]
        [DataRow(4, 1, Domain.Common.RadianContributorType.ElectronicInvoice, DisplayName = "Participant Recorded")]
        [DataRow(5, 1, Domain.Common.RadianContributorType.TechnologyProvider, DisplayName = "should be PT")]
        [DataRow(6, 2, Domain.Common.RadianContributorType.ElectronicInvoice, DisplayName = "Electronic Invoicer")]
        [DataRow(7, 2, Domain.Common.RadianContributorType.TechnologyProvider, DisplayName = "Electronic Invoicer")]
        [DataRow(8, 2, Domain.Common.RadianContributorType.TradingSystem, DisplayName = "Electronic Invoicer")]
        [DataRow(9, 2, Domain.Common.RadianContributorType.Factor, DisplayName = "Electronic Invoicer")]
        [DataRow(10, 2, Domain.Common.RadianContributorType.Zero, DisplayName = "Electronic Invoicer")]
        public void RegistrationValidationTest(int input, int ContributorTypeId, Domain.Common.RadianContributorType radianContributorType)
        {
            //arrange
            int contributorId = 1;
            Domain.Common.RadianOperationMode radianOperationMode = Domain.Common.RadianOperationMode.Direct;
            _contributorService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Contributor() { Status = true, ContributorTypeId = ContributorTypeId, ContributorOperations = new List<ContributorOperations>() { new ContributorOperations() { ContributorTypeId = 1, SoftwareId = new Guid("fcd45b68-d957-4467-abde-8184bfe1239f") } } });

            switch (input)
            {
                case 1:
                    _contributorService.Setup(t => t.GetBaseSoftwareForRadian(It.IsAny<int>())).Returns(new List<Software>());
                    break;
                case 2:
                    _contributorService.Setup(t => t.GetBaseSoftwareForRadian(It.IsAny<int>())).Returns(new List<Software>() { new Software() });
                    _radianTestSetResultManager.Setup(t => t.GetTestSetResulByCatalog(It.IsAny<string>())).Returns(new List<GlobalTestSetResult>() { new GlobalTestSetResult() { SoftwareId = Guid.NewGuid().ToString() } });
                    break;
                case 3:
                    _contributorService.Setup(t => t.GetBaseSoftwareForRadian(It.IsAny<int>())).Returns(new List<Software>() { new Software() { Id = new Guid("fcd45b68-d957-4467-abde-8184bfe1239f") } });
                    _radianTestSetResultManager.Setup(t => t.GetTestSetResulByCatalog(It.IsAny<string>())).Returns(new List<GlobalTestSetResult>() { new GlobalTestSetResult() { SoftwareId = "fcd45b68-d957-4467-abde-8184bfe1239f", RowKey = "1|fcd45b68-d957-4467-abde-8184bfe1239f", Status = 1 } });
                    _radianContributorRepository.Setup(t => t.GetParticipantWithActiveProcess(It.IsAny<int>())).Returns(true);
                    break;
                case 4:
                    _contributorService.Setup(t => t.GetBaseSoftwareForRadian(It.IsAny<int>())).Returns(new List<Software>() { new Software() { Id = new Guid("fcd45b68-d957-4467-abde-8184bfe1239f") } });
                    _radianTestSetResultManager.Setup(t => t.GetTestSetResulByCatalog(It.IsAny<string>())).Returns(new List<GlobalTestSetResult>() { new GlobalTestSetResult() { SoftwareId = "fcd45b68-d957-4467-abde-8184bfe1239f", RowKey = "1|fcd45b68-d957-4467-abde-8184bfe1239f", Status = 1 } });
                    _radianContributorRepository.Setup(t => t.GetParticipantWithActiveProcess(It.IsAny<int>())).Returns(false);
                    _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor() { RadianState = "Habilitado" });
                    break;
                case 5:
                    _contributorService.Setup(t => t.GetBaseSoftwareForRadian(It.IsAny<int>())).Returns(new List<Software>() { new Software() { Id = new Guid("fcd45b68-d957-4467-abde-8184bfe1239f") } });
                    _radianTestSetResultManager.Setup(t => t.GetTestSetResulByCatalog(It.IsAny<string>())).Returns(new List<GlobalTestSetResult>() { new GlobalTestSetResult() { SoftwareId = "fcd45b68-d957-4467-abde-8184bfe1239f", RowKey = "1|fcd45b68-d957-4467-abde-8184bfe1239f", Status = 1 } });
                    _radianContributorRepository.Setup(t => t.GetParticipantWithActiveProcess(It.IsAny<int>())).Returns(false);
                    _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns((RadianContributor)null);
                    break;
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    _contributorService.Setup(t => t.GetBaseSoftwareForRadian(It.IsAny<int>())).Returns(new List<Software>() { new Software() { Id = new Guid("fcd45b68-d957-4467-abde-8184bfe1239f") } });
                    _radianTestSetResultManager.Setup(t => t.GetTestSetResulByCatalog(It.IsAny<string>())).Returns(new List<GlobalTestSetResult>() { new GlobalTestSetResult() { SoftwareId = "fcd45b68-d957-4467-abde-8184bfe1239f", RowKey = "1|fcd45b68-d957-4467-abde-8184bfe1239f", Status = 1 } });
                    _radianContributorRepository.Setup(t => t.GetParticipantWithActiveProcess(It.IsAny<int>())).Returns(false);
                    _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns((RadianContributor)null);
                    break;
            }

            //act
            ResponseMessage responseMessage = _current.RegistrationValidation(contributorId, radianContributorType, radianOperationMode);

            //assert
            switch (input)
            {
                case 1:
                case 2:
                    Assert.AreEqual(TextResources.ParticipantWithoutSoftware, responseMessage.Message);
                    break;
                case 3:
                    Assert.AreEqual(TextResources.OperationFailOtherInProcess, responseMessage.Message);
                    break;
                case 4:
                    Assert.AreEqual(TextResources.RegisteredParticipant, responseMessage.Message);
                    break;
                case 5:
                    Assert.AreEqual(TextResources.TechnologProviderDisabled, responseMessage.Message);
                    break;
                case 6:
                    Assert.AreEqual(TextResources.ElectronicInvoice_Confirm, responseMessage.Message);
                    break;
                case 7:
                    Assert.AreEqual(TextResources.TechnologyProvider_Confirm, responseMessage.Message);
                    break;
                case 8:
                    Assert.AreEqual(TextResources.TradingSystem_Confirm, responseMessage.Message);
                    break;
                case 9:
                    Assert.AreEqual(TextResources.Factor_Confirm, responseMessage.Message);
                    break;
                case 10:
                    Assert.AreEqual(TextResources.FailedValidation, responseMessage.Message);
                    break;
            }

        }

        [TestMethod]
        public void ListParticipants()
        {
            //arrange
            int page = 1;
            int size = 1;
            _radianContributorRepository.Setup(t => t.ListByDateDesc(It.IsAny<Expression<Func<RadianContributor, bool>>>(), page, size)).Returns(new PagedResult<RadianContributor>() { Results = new List<RadianContributor>() { new RadianContributor() { Contributor = new Contributor() { AcceptanceStatus = new AcceptanceStatus() } } } });
            _radianContributorTypeRepository.Setup(t => t.List(It.IsAny<Expression<Func<Domain.RadianContributorType, bool>>>())).Returns(new List<Domain.RadianContributorType>() { new Domain.RadianContributorType() });

            //act
            RadianAdmin radianAdmin = _current.ListParticipants(page, size);

            //assert
            Assert.IsNotNull(radianAdmin);

        }

        [TestMethod]
        [DataRow(1, DisplayName = "Without Filters")]
        [DataRow(2, DisplayName = "With Filters")]
        public void ListParticipantsFilterTest(int input)
        {
            //arrange
            AdminRadianFilter filter = new AdminRadianFilter();
            switch (input)
            {
                case 2:
                    filter.RadianState = "Registrado";
                    filter.StartDate = System.DateTime.Now.ToString();
                    filter.EndDate = System.DateTime.Now.ToString();
                    break;
            }

            int page = 1;
            int size = 1;
            _radianContributorRepository.Setup(t => t.ListByDateDesc(It.IsAny<Expression<Func<RadianContributor, bool>>>(), page, size)).Returns(new PagedResult<RadianContributor>() { Results = new List<RadianContributor>() { new RadianContributor() { Contributor = new Contributor() { AcceptanceStatus = new AcceptanceStatus() } } } });
            _radianContributorTypeRepository.Setup(t => t.List(k => true)).Returns(new List<Domain.RadianContributorType>() { new Domain.RadianContributorType() });

            //act
            RadianAdmin radianAdmin = _current.ListParticipantsFilter(filter, page, size);

            //assert
            Assert.IsNotNull(radianAdmin);

        }


        [TestMethod]
        [DataRow(0, DisplayName = "Without Type")]
        [DataRow(1, DisplayName = "With Type")]
        public void ContributorSummaryTest(int radianContributorType)
        {
            //arrange
            int contributorId = 1;
            _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new PagedResult<RadianContributor>() { Results = new List<RadianContributor>() { new RadianContributor() { Contributor = new Contributor() { AcceptanceStatus = new AcceptanceStatus() }, RadianContributorFile = new List<RadianContributorFile>() { new RadianContributorFile() } } } });
            _radianTestSetResultManager.Setup(t => t.GetAllTestSetResultByContributor(It.IsAny<int>())).Returns(new List<RadianTestSetResult>() { new RadianTestSetResult() });
            _radianContributorFileTypeRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributorFileType, bool>>>(), 0, 0)).Returns(new List<RadianContributorFileType>() { new RadianContributorFileType() { } });
            _contributorService.Setup(t => t.GetUserContributors(It.IsAny<int>())).Returns(new List<UserContributors>() { new UserContributors() });

            //act
            RadianAdmin radianAdmin = _current.ContributorSummary(contributorId, radianContributorType);

            //assert
            Assert.IsNotNull(radianAdmin);
        }


        [TestMethod]
        [DataRow(1, 1, "", 1, "", "")]
        [DataRow(2, 1, "En pruebas", 1, "Registrado", "")]
        [DataRow(3, 1, "En pruebas", 1, "Registrado", "")]
        [DataRow(4, 1, "Registrado", 1, "Registrado", "")]
        [DataRow(5, 1, "Habilitado", 1, "Registrado", "")]
        [DataRow(6, 1, "Cancelado", 1, "Registrado", "")]
        public void ChangeParticipantStatusTest(int input, int contributorId, string newState, int radianContributorTypeId, string actualState, string description)
        {
            //arrange
            switch (input)
            {
                case 1:
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(new PagedResult<RadianContributor>());
                    break;
                case 2:
                    ICollection<Gosocket.Dian.Domain.RadianContributorOperation> operations = new List<Gosocket.Dian.Domain.RadianContributorOperation>()
                    {
                        new RadianContributorOperation()
                    };
                    PagedResult<RadianContributor> pageresult = new PagedResult<RadianContributor>()
                    {
                        Results = new List<RadianContributor>()
                        {
                            new RadianContributor()
                            {
                                RadianContributorOperations = operations
                            }
                        }
                    };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(pageresult);
                    _radianContributorRepository.Setup(t => t.AddOrUpdate(It.IsAny<RadianContributor>())).Returns(1);
                    break;
                case 3:
                    ICollection<Gosocket.Dian.Domain.RadianContributorOperation> operations3 = new List<Gosocket.Dian.Domain.RadianContributorOperation>()
                    {
                        new RadianContributorOperation(){ OperationStatusId = 3 }
                    };
                    PagedResult<RadianContributor> pageresult3 = new PagedResult<RadianContributor>()
                    {
                        Results = new List<RadianContributor>()
                        {
                            new RadianContributor()
                            {
                                RadianContributorOperations = operations3,
                                Contributor = new Contributor()
                            }
                        }
                    };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(pageresult3);
                    _radianContributorRepository.Setup(t => t.AddOrUpdate(It.IsAny<RadianContributor>())).Returns(1);
                    _globalRadianOperationService.Setup(t => t.GetOperation(It.IsAny<string>(), It.IsAny<Guid>())).Returns(new GlobalRadianOperations());
                    _globalRadianOperationService.Setup(t => t.Update(It.IsAny<GlobalRadianOperations>())).Returns(true);
                    break;
                case 4:
                    ICollection<Gosocket.Dian.Domain.RadianContributorOperation> operations4 = new List<Gosocket.Dian.Domain.RadianContributorOperation>()
                    {
                        new RadianContributorOperation(){ OperationStatusId = 3 }
                    };
                    PagedResult<RadianContributor> pageresult4 = new PagedResult<RadianContributor>()
                    {
                        Results = new List<RadianContributor>()
                        {
                            new RadianContributor()
                            {
                                RadianContributorOperations = operations4,
                                Contributor = new Contributor()
                            }
                        }
                    };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(pageresult4);
                    _radianContributorRepository.Setup(t => t.AddOrUpdate(It.IsAny<RadianContributor>())).Returns(1);
                    _globalRadianOperationService.Setup(t => t.GetOperation(It.IsAny<string>(), It.IsAny<Guid>())).Returns(new GlobalRadianOperations());
                    _globalRadianOperationService.Setup(t => t.Update(It.IsAny<GlobalRadianOperations>())).Returns(true);
                    break;
                case 5:
                    ICollection<Gosocket.Dian.Domain.RadianContributorOperation> operations5 = new List<Gosocket.Dian.Domain.RadianContributorOperation>();
                    PagedResult<RadianContributor> pageresult5 = new PagedResult<RadianContributor>()
                    {
                        Results = new List<RadianContributor>()
                        {
                            new RadianContributor()
                            {
                                RadianContributorOperations = operations5,
                                Contributor = new Contributor()
                            }
                        }
                    };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(pageresult5);
                    _radianContributorRepository.Setup(t => t.AddOrUpdate(It.IsAny<RadianContributor>())).Returns(1);
                    _globalRadianOperationService.Setup(t => t.GetOperation(It.IsAny<string>(), It.IsAny<Guid>())).Returns(new GlobalRadianOperations());
                    _globalRadianOperationService.Setup(t => t.Update(It.IsAny<GlobalRadianOperations>())).Returns(true);
                    break;
                case 6:
                    ICollection<Gosocket.Dian.Domain.RadianContributorOperation> operations6 = new List<Gosocket.Dian.Domain.RadianContributorOperation>();
                    PagedResult<RadianContributor> pageresult6 = new PagedResult<RadianContributor>()
                    {
                        Results = new List<RadianContributor>()
                        {
                            new RadianContributor()
                            {
                                RadianContributorOperations = operations6,
                                Contributor = new Contributor()
                            }
                        }
                    };
                    _radianContributorRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributor, bool>>>(), 0, 0)).Returns(pageresult6);
                    _radianContributorRepository.Setup(t => t.AddOrUpdate(It.IsAny<RadianContributor>())).Returns(1);
                    _radianContributorFileRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributorFile, bool>>>())).Returns(new List<RadianContributorFile>() { new RadianContributorFile() });
                    _radianContributorFileRepository.Setup(t => t.AddOrUpdate(It.IsAny<RadianContributorFile>())).Returns("test");
                    _radianContributorFileHistoryRepository.Setup(t => t.AddRegisterHistory(It.IsAny<RadianContributorFileHistory>())).Returns(Guid.NewGuid());
                    _radianCallSoftwareService.Setup(t => t.List(It.IsAny<int>())).Returns(new List<RadianSoftware>() { new RadianSoftware() });
                    _radianCallSoftwareService.Setup(t => t.DeleteSoftware(It.IsAny<Guid>())).Returns(Guid.NewGuid());
                    _radianContributorOperationRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributorOperation, bool>>>())).Returns(new List<RadianContributorOperation>() { new RadianContributorOperation() });
                    _radianContributorOperationRepository.Setup(t => t.Update(It.IsAny<RadianContributorOperation>())).Returns(true);
                    break;
            }

            //act
            bool result = _current.ChangeParticipantStatus(contributorId, newState, radianContributorTypeId, actualState, description);

            //assert
            switch (input)
            {
                case 1:
                    Assert.IsFalse(result);
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    Assert.IsTrue(result);
                    break;
            }
        }


        [TestMethod]
        public void UpdateRadianOperationTest()
        {
            //arrange
            int radiancontributorId = 1, softwareType = 1;
            _radianContributorOperationRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributorOperation, bool>>>())).Returns(new List<RadianContributorOperation>() { new RadianContributorOperation() });
            _radianContributorOperationRepository.Setup(t => t.Update(It.IsAny<RadianContributorOperation>())).Returns(true);

            //act
            _current.UpdateRadianOperation(radiancontributorId, softwareType);

            //assert
            Assert.IsTrue(true);

        }

        [TestMethod]
        [DataRow(1, 1)]
        [DataRow(2, 2)]
        public void ChangeContributorStepTest(int input, int step)
        {
            //arrange
            if (input == 1)
                _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor());
            else
                _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns((RadianContributor)null);

            //act
            bool result = _current.ChangeContributorStep(1, step);

            //assert
            if (input == 1)
                Assert.IsTrue(result);
            else
                Assert.IsFalse(result);
        }


        [TestMethod]
        public void UpdateRadianContributorFileTest()
        {
            //arrange
            RadianContributorFile radianContributorFile = new RadianContributorFile();
            _radianContributorFileRepository.Setup(t => t.Update(radianContributorFile)).Returns(Guid.NewGuid());

            //act
            Guid result = _current.UpdateRadianContributorFile(radianContributorFile);

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateContributorTest()
        {
            //arrange
            int contributorId = 1;
            RadianState radianState = RadianState.Registrado;
            Domain.Common.RadianContributorType radianContributorType = Domain.Common.RadianContributorType.ElectronicInvoice;
            Domain.Common.RadianOperationMode radianOperationMode = Domain.Common.RadianOperationMode.Direct;
            string createdBy = "test";
            _radianContributorRepository.Setup(t => t.Get(It.IsAny<Expression<Func<RadianContributor, bool>>>())).Returns(new RadianContributor());
            _radianContributorRepository.Setup(t => t.AddOrUpdate(It.IsAny<RadianContributor>())).Returns(1);
            _contributorService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Contributor() { Status = true, ContributorTypeId = 1, ContributorOperations = new List<ContributorOperations>() { new ContributorOperations() { ContributorTypeId = 1, SoftwareId = new Guid("fcd45b68-d957-4467-abde-8184bfe1239f") } } });
            _contributorService.Setup(t => t.GetBaseSoftwareForRadian(It.IsAny<int>())).Returns(new List<Software>() { new Software() { Id = new Guid("fcd45b68-d957-4467-abde-8184bfe1239f") } });
            _radianTestSetResultManager.Setup(t => t.GetTestSetResulByCatalog(It.IsAny<string>())).Returns(new List<GlobalTestSetResult>() { new GlobalTestSetResult() { SoftwareId = "fcd45b68-d957-4467-abde-8184bfe1239f", RowKey = "1|fcd45b68-d957-4467-abde-8184bfe1239f", Status = 1 } });

            //act
            RadianContributor radianContributor = _current.CreateContributor(contributorId, radianState, radianContributorType, radianOperationMode, createdBy);

            //assert
            Assert.IsNotNull(radianContributor);
        }

        [TestMethod]
        public void RadianContributorFileListTest()
        {
            //arrange
            string id = "1";
            _radianContributorFileRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributorFile, bool>>>())).Returns(new List<RadianContributorFile>());

            //act
            var list = _current.RadianContributorFileList(id);

            //assert
            Assert.IsNotNull(list);
        }

        [TestMethod]
        public void GetOperationModeTest()
        {
            //arrange
            _radianOperationModeRepository.Setup(t => t.List(k => true)).Returns(new List<Domain.RadianOperationMode>());

            //act
            var list = _current.OperationModeList();

            //assert
            Assert.IsNotNull(list);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public void AddFileHistoryTest(int input)
        {
            //arrange
            RadianContributorFileHistory radianFileHistory = new RadianContributorFileHistory();
            if (input == 1)
                _radianContributorFileHistoryRepository.Setup(t => t.AddRegisterHistory(radianFileHistory)).Returns(new Guid("d41b08a2-4995-414c-9ead-4173dd069f92"));
            else
                _radianContributorFileHistoryRepository.Setup(t => t.AddRegisterHistory(radianFileHistory)).Returns(null);

            //act
            ResponseMessage result = _current.AddFileHistory(radianFileHistory);

            //assert
            if (input == 1)
                Assert.AreEqual("Información registrada id: d41b08a2-4995-414c-9ead-4173dd069f92", result.Message);
            else
                Assert.AreEqual("El registro no pudo ser guardado", result.Message);

        }

        [TestMethod]
        public void GetAssociatedClientsTest()
        {
            //arrange
            int radianContributorId = 1;
            _radianContributorRepository.Setup(t => t.CustomerList(radianContributorId, string.Empty, string.Empty,0,0)).Returns(new PagedResult<RadianCustomerList>() { Results = new List<RadianCustomerList>() { new RadianCustomerList() } });

            //act
            int count = _current.GetAssociatedClients(radianContributorId);

            //assert
            Assert.AreEqual(1, count);

        }

        [TestMethod]
        public void GetSetTestResultTest()
        {
            //arrange
            string code = "1";
            string softwareId = "soft";
            int type = 1;
            _radianTestSetResultManager.Setup(t => t.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>())).Returns(new RadianTestSetResult());

            //act
            RadianTestSetResult result = _current.GetSetTestResult(code, softwareId, type);

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DownloadContributorFileTest()
        {
            //arrange
            string code=string.Empty, fileName = string.Empty, contentType;

            //act
            byte[] result = _current.DownloadContributorFile(code, fileName,out  contentType);

            //assert
            Assert.IsTrue(true);
        }

    }
}