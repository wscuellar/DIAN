using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Web.Controllers;
using System;
using System.Collections.Generic;
using Moq;
using Gosocket.Dian.Interfaces;
using Moq;
using Gosocket.Dian.Interfaces.Services;
using System.Web.Mvc;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using System;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Common.Resources;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class OthersElectronicDocAssociatedControllerTests
    {
        OthersElectronicDocAssociatedController _current;
        private readonly Mock<IContributorService> _contributorService = new Mock<IContributorService>();
        private readonly Mock<IOthersDocsElecContributorService> _othersDocsElecContributorService = new Mock<IOthersDocsElecContributorService>();
        private readonly Mock<IOthersElectronicDocumentsService> _othersElectronicDocumentsService = new Mock<IOthersElectronicDocumentsService>();
        private readonly Mock<IOthersDocsElecSoftwareService> _othersDocsElecSoftwareService = new Mock<IOthersDocsElecSoftwareService>();
        private readonly Mock<ITestSetOthersDocumentsResultService> _testSetOthersDocumentsResultService = new Mock<ITestSetOthersDocumentsResultService>();
        private readonly Mock<IGlobalOtherDocElecOperationService> _globalOtherDocElecOperationService = new Mock<IGlobalOtherDocElecOperationService>();

        private readonly Mock<IContributorService> contributorService = new Mock<IContributorService>();
        private readonly Mock<IOthersDocsElecContributorService> othersDocsElecContributorService = new Mock<IOthersDocsElecContributorService>();
        private readonly Mock<IOthersElectronicDocumentsService> othersElectronicDocumentsService = new Mock<IOthersElectronicDocumentsService>();
        private readonly Mock<ITestSetOthersDocumentsResultService> testSetOthersDocumentsResultService = new Mock<ITestSetOthersDocumentsResultService>();
        private readonly Mock<IOthersDocsElecSoftwareService> othersDocsElecSoftwareService = new Mock<IOthersDocsElecSoftwareService>();
        private readonly Mock<IGlobalOtherDocElecOperationService> globalOtherDocElecOperationService = new Mock<IGlobalOtherDocElecOperationService>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new OthersElectronicDocAssociatedController(
                _contributorService.Object,
                _othersDocsElecContributorService.Object,
                _othersElectronicDocumentsService.Object,
                _testSetOthersDocumentsResultService.Object,
                _othersDocsElecSoftwareService.Object,
                _globalOtherDocElecOperationService.Object);
        }



        [TestMethod()]
        [DataRow(1, DisplayName = "Without Contributor ODE")]
        [DataRow(2, DisplayName = "Without Contributor")]
        [DataRow(3, DisplayName = "TechnologyProvider")]
        [DataRow(4, DisplayName = "Without TechnologyProvider")]
        public void IndexTest(int input)
        {
            //arrange
            int id = 0;
            _othersElectronicDocumentsService.Setup(t => t.GetOtherDocElecContributorOperationById(id)).Returns(new Domain.Sql.OtherDocElecContributorOperations());
            switch (input)
            {
                case 1:
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns((Domain.Entity.OtherDocsElectData)null);
                    break;
                case 2:
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns(new Domain.Entity.OtherDocsElectData());
                    _contributorService.Setup(t => t.GetContributorById(It.IsAny<int>(), It.IsAny<int>())).Returns((Domain.Contributor)null);
                    break;
                case 3:
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns(new Domain.Entity.OtherDocsElectData() { Step = 3, LegalRepresentativeIds = new List<string>(), ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _contributorService.Setup(t => t.GetContributorById(It.IsAny<int>(), It.IsAny<int>())).Returns(new Domain.Contributor() { ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    Domain.Entity.PagedResult<Domain.Entity.OtherDocElecCustomerList> pagedResult = new Domain.Entity.PagedResult<Domain.Entity.OtherDocElecCustomerList>();
                    pagedResult.Results = new List<Domain.Entity.OtherDocElecCustomerList>() { new Domain.Entity.OtherDocElecCustomerList() };
                    _othersElectronicDocumentsService.Setup(t => t.CustormerList(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<OtherDocElecState>(), It.IsAny<int>(), It.IsAny<int>())).Returns(pagedResult);
                    break;
                case 4:
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns(new Domain.Entity.OtherDocsElectData() { Step = 2, LegalRepresentativeIds = new List<string>(), ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _contributorService.Setup(t => t.GetContributorById(It.IsAny<int>(), It.IsAny<int>())).Returns(new Domain.Contributor() { ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    break;
            }

            //act
            RedirectToRouteResult redirectResult = null;
            ViewResult viewResult = null;
            switch (input)
            {
                case 1:
                    redirectResult = _current.Index(id) as RedirectToRouteResult;
                    break;
                case 2:
                case 3:
                case 4:
                    viewResult = _current.Index(id) as ViewResult;
                    break;
            }

            //assert
            switch (input)
            {
                case 1:
                    Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
                    break;
                case 2:
                    Assert.AreEqual(false, (bool)viewResult.ViewData["ValidateRequest"]);
                    break;
                case 3:
                case 4:
                    Assert.AreEqual(id, (int)viewResult.ViewData["Id"]);
                    break;
            }
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Operation Status Habilitado")]
        [DataRow(2, DisplayName = "Operation Status Test")]
        public void CancelRegisterTest(int input)
        {
            //arrange
            int id = 1;
            string description = string.Empty;
            switch (input)
            {
                case 1:
                    _othersElectronicDocumentsService.Setup(t => t.GetOtherDocElecContributorOperationById(id)).Returns(new Domain.Sql.OtherDocElecContributorOperations() { OperationStatusId = (int)OtherDocElecState.Habilitado });
                    break;
                case 2:
                    _othersElectronicDocumentsService.Setup(t => t.GetOtherDocElecContributorOperationById(id)).Returns(new Domain.Sql.OtherDocElecContributorOperations() { OperationStatusId = (int)OtherDocElecState.Test });
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns(new Domain.Entity.OtherDocsElectData() { Step = 2, LegalRepresentativeIds = new List<string>(), ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _contributorService.Setup(t => t.GetContributorById(It.IsAny<int>(), It.IsAny<int>())).Returns(new Domain.Contributor() { ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _othersDocsElecSoftwareService.Setup(t => t.Get(It.IsAny<Guid>())).Returns(new Domain.Sql.OtherDocElecSoftware());
                    _testSetOthersDocumentsResultService.Setup(t => t.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>())).Returns(new GlobalTestSetOthersDocumentsResult());
                    _testSetOthersDocumentsResultService.Setup(t => t.InsertTestSetResult(It.IsAny<GlobalTestSetOthersDocumentsResult>())).Returns(true);
                    _globalOtherDocElecOperationService.Setup(t => t.GetOperation(It.IsAny<string>(), It.IsAny<Guid>())).Returns(new GlobalOtherDocElecOperation());
                    _globalOtherDocElecOperationService.Setup(t => t.Update(It.IsAny<GlobalOtherDocElecOperation>())).Returns(true);
                    _othersDocsElecContributorService.Setup(t => t.CancelRegister(id, description)).Returns(new ResponseMessage());
                    break;
            }


            //act
            JsonResult jsonResult = _current.CancelRegister(id, description);
            string message = string.Empty;
            ResponseMessage responseMessage = null;
            switch (input)
            {
                case 1:
                    message = jsonResult.Data.GetType().GetProperty("message").GetValue(jsonResult.Data).ToString();
                    break;
                case 2:
                    responseMessage = jsonResult.Data as ResponseMessage;
                    break;
            }


            //assert
            switch (input)
            {
                case 1:
                    Assert.AreEqual("Modo de operación se encuentra en estado 'Habilitado', no se permite eliminar.", message);
                    break;
                case 2:
                    Assert.IsNull(responseMessage.Message);
                    break;
            }

        }

        [TestMethod]
        [DataRow(1, DisplayName = "Successfull")]
        [DataRow(2, DisplayName = "Failed")]
        public void EnviarContributorTest(int input)
        {
            //arrange
            OthersElectronicDocAssociatedViewModel entity = new OthersElectronicDocAssociatedViewModel();
            _othersElectronicDocumentsService.Setup(t => t.ChangeContributorStep(It.IsAny<int>(), It.IsAny<int>())).Returns((input == 1));

            //act
            JsonResult jsonResult = _current.EnviarContributor(entity);

            //assert
            if (input == 1)
            {
                string message = jsonResult.Data.GetType().GetProperty("message").GetValue(jsonResult.Data).ToString();
                Assert.AreEqual("Datos enviados correctamente.", message);
            }

            if (input == 2)
            {
                ResponseMessage responseMessage = jsonResult.Data as ResponseMessage;
                Assert.AreEqual("El registro no pudo ser actualizado", responseMessage.Message);
            }

        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void GetSetTestResultTest(int input)
        {
            //arrange
            int id = 0;
            switch (input)
            {
                case 1:
                    _othersElectronicDocumentsService.Setup(t => t.GetOtherDocElecContributorOperationById(id)).Returns(new Domain.Sql.OtherDocElecContributorOperations() { OperationStatusId = (int)OtherDocElecState.Test });
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns((OtherDocsElectData)null);
                    break;
                case 2:
                    _othersElectronicDocumentsService.Setup(t => t.GetOtherDocElecContributorOperationById(id)).Returns(new Domain.Sql.OtherDocElecContributorOperations() { OperationStatusId = (int)OtherDocElecState.Test });
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns(new Domain.Entity.OtherDocsElectData() { Step = 2, LegalRepresentativeIds = new List<string>(), ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _contributorService.Setup(t => t.GetContributorById(It.IsAny<int>(), It.IsAny<int>())).Returns((Domain.Contributor)null);
                    break;
                case 3:
                    _othersElectronicDocumentsService.Setup(t => t.GetOtherDocElecContributorOperationById(id)).Returns(new Domain.Sql.OtherDocElecContributorOperations() { OperationStatusId = (int)OtherDocElecState.Test });
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns(new Domain.Entity.OtherDocsElectData() { Step = 2, LegalRepresentativeIds = new List<string>(), ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _contributorService.Setup(t => t.GetContributorById(It.IsAny<int>(), It.IsAny<int>())).Returns(new Domain.Contributor() { ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _othersDocsElecSoftwareService.Setup(t => t.Get(It.IsAny<Guid>())).Returns(new Domain.Sql.OtherDocElecSoftware());
                    _othersDocsElecContributorService.Setup(t => t.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns((GlobalTestSetOthersDocuments)null);
                    break;
                case 4:
                    _othersElectronicDocumentsService.Setup(t => t.GetOtherDocElecContributorOperationById(id)).Returns(new Domain.Sql.OtherDocElecContributorOperations() { OperationStatusId = (int)OtherDocElecState.Test });
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns(new Domain.Entity.OtherDocsElectData() { Step = 2, LegalRepresentativeIds = new List<string>(), ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider, OperationModeId = 1, ElectronicDocId = 1 });
                    _contributorService.Setup(t => t.GetContributorById(It.IsAny<int>(), It.IsAny<int>())).Returns(new Domain.Contributor() { ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _othersDocsElecSoftwareService.Setup(t => t.Get(It.IsAny<Guid>())).Returns(new Domain.Sql.OtherDocElecSoftware());
                    _othersDocsElecContributorService.Setup(t => t.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns(new GlobalTestSetOthersDocuments());
                    _othersDocsElecSoftwareService.Setup(t => t.Get(It.IsAny<Guid>())).Returns(new Domain.Sql.OtherDocElecSoftware());
                    _testSetOthersDocumentsResultService.Setup(t => t.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>())).Returns(new GlobalTestSetOthersDocumentsResult());
                    break;
            }


            //act
            RedirectToRouteResult redirectToAction = null;
            ViewResult viewResult = null;
            ResponseMessage responseMessage = null;
            switch (input)
            {
                case 1:
                    redirectToAction = _current.GetSetTestResult(id) as RedirectToRouteResult;
                    break;
                case 2:
                case 4:
                    viewResult = _current.GetSetTestResult(id) as ViewResult;
                    break;
                case 3:
                    JsonResult jsonResult = _current.GetSetTestResult(id) as JsonResult;
                    responseMessage = jsonResult.Data as ResponseMessage;
                    break;

            }


            //assert
            switch (input)
            {
                case 1:
                    Assert.AreEqual("Index", redirectToAction.RouteValues["action"]);
                    break;
                case 2:
                    Assert.AreEqual(false, (bool)viewResult.ViewData["ValidateRequest"]);
                    break;
                case 3:
                    Assert.AreEqual(TextResources.ModeElectroniDocWithoutTestSet, responseMessage.Message);
                    break;
                case 4:
                    Assert.AreEqual(id, (int)viewResult.ViewData["Id"]);
                    break;
            }

        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        public void SetTestDetailsTest(int input)
        {
            //arrange
            int id = 0;
            switch (input)
            {
                case 1:
                    _othersElectronicDocumentsService.Setup(t => t.GetOtherDocElecContributorOperationById(id)).Returns(new Domain.Sql.OtherDocElecContributorOperations() { OperationStatusId = (int)OtherDocElecState.Test });
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns((OtherDocsElectData)null);
                    break;
                case 2:
                    _othersElectronicDocumentsService.Setup(t => t.GetOtherDocElecContributorOperationById(id)).Returns(new Domain.Sql.OtherDocElecContributorOperations() { OperationStatusId = (int)OtherDocElecState.Test });
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns(new Domain.Entity.OtherDocsElectData() { Step = 2, LegalRepresentativeIds = new List<string>(), ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _contributorService.Setup(t => t.GetContributorById(It.IsAny<int>(), It.IsAny<int>())).Returns((Domain.Contributor)null);
                    break;
                case 3:
                    _othersElectronicDocumentsService.Setup(t => t.GetOtherDocElecContributorOperationById(id)).Returns(new Domain.Sql.OtherDocElecContributorOperations() { OperationStatusId = (int)OtherDocElecState.Test });
                    _othersDocsElecContributorService.Setup(t => t.GetCOntrinutorODE(It.IsAny<int>())).Returns(new Domain.Entity.OtherDocsElectData() { Step = 2, LegalRepresentativeIds = new List<string>(), ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _contributorService.Setup(t => t.GetContributorById(It.IsAny<int>(), It.IsAny<int>())).Returns(new Domain.Contributor() { ContributorTypeId = (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider });
                    _othersDocsElecSoftwareService.Setup(t => t.Get(It.IsAny<Guid>())).Returns(new Domain.Sql.OtherDocElecSoftware());
                    _testSetOthersDocumentsResultService.Setup(t => t.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>())).Returns(new GlobalTestSetOthersDocumentsResult());
                    _othersDocsElecContributorService.Setup(t => t.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns(new GlobalTestSetOthersDocuments());
                    break;
            }

            //act
            RedirectToRouteResult redirectToAction = null;
            ViewResult viewResult = null;
            switch (input)
            {
                case 1:
                    redirectToAction = _current.SetTestDetails(id) as RedirectToRouteResult;
                    break;
                case 2:
                case 3:
                    viewResult = _current.SetTestDetails(id) as ViewResult;
                    break;
            }

            //assert
            switch (input)
            {
                case 1:
                    Assert.AreEqual("Index", redirectToAction.RouteValues["action"]);
                    break;
                case 2:
                    Assert.AreEqual(false, (bool)viewResult.ViewData["ValidateRequest"]);
                    break;
                case 3:
                    Assert.AreEqual(id, (int)viewResult.ViewData["Id"]);
                    break;
            }
        }

        [TestMethod]
        public void CustomersListTest()
        {
            //arrange
            int ContributorId = 1, page = 1, pagesize = 1;
            string code = string.Empty;
            OtherDocElecState State = OtherDocElecState.Registrado;
            _othersElectronicDocumentsService.Setup(t => t.CustormerList(It.IsAny<int>(), code, State, page, pagesize)).Returns(new PagedResult<OtherDocElecCustomerList>() { Results = new List<OtherDocElecCustomerList>() { new OtherDocElecCustomerList() } });

            //act
            JsonResult jsonResult = _current.CustomersList(ContributorId, code, State, page, pagesize) as JsonResult;

            //assert
            Assert.IsNotNull(jsonResult.Data);

        }

        [TestMethod]
        public void SetupOperationModeTest()
        {
            //arrange

            //act

            //assert

        }

    }
}