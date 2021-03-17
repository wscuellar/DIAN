using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Interfaces.Managers;
using System.Collections.Specialized;
using System.Linq.Expressions;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Common.Resources;

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
            _contributorService.Setup(t => t.Get(It.IsAny<int>())).Returns(new Contributor() {Status=true, ContributorTypeId = ContributorTypeId,  ContributorOperations = new List<ContributorOperations>() { new ContributorOperations() { ContributorTypeId= 1 ,SoftwareId= new Guid("fcd45b68-d957-4467-abde-8184bfe1239f") } } });

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
                    _contributorService.Setup(t => t.GetBaseSoftwareForRadian(It.IsAny<int>())).Returns(new List<Software>() { new Software() { Id= new Guid("fcd45b68-d957-4467-abde-8184bfe1239f") } });
                    _radianTestSetResultManager.Setup(t => t.GetTestSetResulByCatalog(It.IsAny<string>())).Returns(new List<GlobalTestSetResult>() { new GlobalTestSetResult() { SoftwareId = "fcd45b68-d957-4467-abde-8184bfe1239f", RowKey= "1|fcd45b68-d957-4467-abde-8184bfe1239f", Status = 1 } });
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
        public  void ListParticipants()
        {
            //arrange
            int page = 1;
            int size = 1;
            _radianContributorRepository.Setup(t => t.ListByDateDesc(It.IsAny<Expression<Func<RadianContributor, bool>>>(), page, size)).Returns(new PagedResult<RadianContributor>() { Results = new List<RadianContributor>() });
            _radianContributorTypeRepository.Setup(t => t.List(It.IsAny<Expression<Func<RadianContributorType, bool>>>())).Returns(new List<RadianContributorType>() { new RadianContributorType() });

            //act
            RadianAdmin radianAdmin = _current.ListParticipants(page, size);

            //assert
            Assert.IsNotNull(radianAdmin);
        }

    }
}