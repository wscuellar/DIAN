using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class OthersDocsElecContributorServiceTests
    {
        private OthersDocsElecContributorService _current;

        private readonly Mock<IContributorService> _contributorService = new Mock<IContributorService>();
        private readonly Mock<IOthersDocsElecContributorRepository> _othersDocsElecContributorRepository = new Mock<IOthersDocsElecContributorRepository>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new OthersDocsElecContributorService(_contributorService.Object, _othersDocsElecContributorRepository.Object);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Sin Contributor")]
        [DataRow(2, DisplayName = "Con Contributor")]
        public void SummaryTest(int input)
        {
            string userCode = "userCode";
            NameValueCollection result;
            //arrange
            _ = _othersDocsElecContributorRepository.Setup(x => x.List(It.IsAny<Expression<Func<OtherDocElecContributor, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new PagedResult<OtherDocElecContributor>
                {
                    Results = new List<OtherDocElecContributor> {
                                new OtherDocElecContributor() {
                                    OtherDocElecContributorTypeId=1,
                                     OtherDocElecOperationModeId=2
                                }}
                });
            switch (input)
            {
                case 1:
                    //arrange
                    _ = _contributorService.Setup(x => x.GetByCode(It.IsAny<string>())).Returns((Contributor)null);
                    //act
                    result = _current.Summary(userCode);
                    //Assert  
                    Assert.IsNotNull(result);
                    Assert.IsInstanceOfType(result, typeof(NameValueCollection));
                    break;
                case 2:
                    //arrange
                    _ = _contributorService.Setup(x => x.GetByCode(It.IsAny<string>())).Returns(new Contributor() { Id = 707, ContributorTypeId = 1, Status = true });
                    //act
                    result = _current.Summary(userCode);
                    //Assert  
                    Assert.IsNotNull(result);
                    Assert.IsInstanceOfType(result, typeof(NameValueCollection));
                    Assert.IsTrue(result["ContributorId"].ToString().Equals("707"));
                    break;
            }
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Sin Contributor")]
        [DataRow(2, DisplayName = "Con Contributor")]
        public void CreateContributorTest(int input)
        {
            OtherDocElecContributor result;
            int contributorId = 7;
            OtherDocElecState State = OtherDocElecState.Habilitado;
            int ContributorType = 1;
            int OperationMode = 1;
            int ElectronicDocumentId = 1;
            string createdBy = "createdBy";

            switch (input)
            {
                case 1:
                    //arrange
                    _ = _othersDocsElecContributorRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributor, bool>>>()))
                        .Returns(new OtherDocElecContributor() { State = OtherDocElecState.Habilitado.GetDescription(), Id = 707 });
                    //act
                    result = _current.CreateContributor(contributorId, State, ContributorType, OperationMode, ElectronicDocumentId, createdBy);
                    //Assert  
                    Assert.IsNotNull(result);
                    Assert.IsInstanceOfType(result, typeof(OtherDocElecContributor));
                    Assert.IsTrue(result.Id.Equals(707));
                    break;
                case 2:
                    //arrange
                    _ = _othersDocsElecContributorRepository.Setup(x => x.Get(It.IsAny<Expression<Func<OtherDocElecContributor, bool>>>()))
                        .Returns((OtherDocElecContributor)null);
                    _ = _othersDocsElecContributorRepository.Setup(x => x.AddOrUpdate(It.IsAny<OtherDocElecContributor>())).Returns(2021);
                    //act
                    result = _current.CreateContributor(contributorId, State, ContributorType, OperationMode, ElectronicDocumentId, createdBy);
                    //Assert  
                    Assert.IsNotNull(result);
                    Assert.IsInstanceOfType(result, typeof(OtherDocElecContributor));
                    Assert.IsTrue(result.Id.Equals(2021));
                    break;
            }
        }

        [TestMethod()]
        public void ValidateExistenciaContribuitorTest()
        {
            int ContributorId = 7; int contributorTypeId = 7; int OperationModeId = 7; string state = "Habilitado";
            //arrange
            _ = _othersDocsElecContributorRepository.Setup(x => x.List(It.IsAny<Expression<Func<OtherDocElecContributor, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new PagedResult<OtherDocElecContributor> { Results = new List<OtherDocElecContributor> { new OtherDocElecContributor() { Id = 2121 } } });
            //act
            var result = _current.ValidateExistenciaContribuitor(ContributorId, contributorTypeId, OperationModeId, state);
            //Assert  
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<OtherDocElecContributor>));
            Assert.IsTrue(result.First().Id.Equals(2121));
        }

        [TestMethod()]
        public void ValidateSoftwareActiveTest()
        {
            int ContributorId = 7; int ContributorTypeId = 7; int OperationModeId = 7; int stateSofware = 7;
            //arrange
            _ = _othersDocsElecContributorRepository.Setup(x => x.GetParticipantWithActiveProcess(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);
            //act
            var result = _current.ValidateSoftwareActive(ContributorId, ContributorTypeId, OperationModeId, stateSofware);
            //Assert  
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public void GetDocElecContributorsByContributorIdTest()
        {
            int ContributorId = 7;
            //arrange
            _ = _othersDocsElecContributorRepository.Setup(x => x.List(It.IsAny<Expression<Func<OtherDocElecContributor, bool>>>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new PagedResult<OtherDocElecContributor> { Results = new List<OtherDocElecContributor> { new OtherDocElecContributor() { Id = 2022 } } });
            //act
            var result = _current.GetDocElecContributorsByContributorId(ContributorId);
            //Assert  
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<OtherDocElecContributor>));
            Assert.IsTrue(result.First().Id.Equals(2022));
        }

        [TestMethod()]
        public void GetTechnologicalProvidersTest()
        {
            int contributorId = 7; int electronicDocumentId = 7; int contributorTypeId = 7; string state = "state";
            //arrange
            _ = _othersDocsElecContributorRepository.Setup(x => x.GetTechnologicalProviders(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new List<Contributor> { new Contributor() { Id = 2022 } });
            //act
            var result = _current.GetTechnologicalProviders(contributorId, electronicDocumentId, contributorTypeId, state);
            //Assert  
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<Contributor>));
            Assert.IsTrue(result.First().Id.Equals(2022));
        }
    }
}