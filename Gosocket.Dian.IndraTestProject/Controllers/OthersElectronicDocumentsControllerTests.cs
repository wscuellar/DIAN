using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Repositories;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class OthersElectronicDocumentsControllerTests
    {
        private OthersElectronicDocumentsController _current;

        private readonly Mock<IOthersElectronicDocumentsService> _othersElectronicDocumentsService = new Mock<IOthersElectronicDocumentsService>();
        private readonly Mock<IOthersDocsElecContributorService> _othersDocsElecContributorService = new Mock<IOthersDocsElecContributorService>();
        private readonly Mock<IContributorService> _contributorService = new Mock<IContributorService>();
        private readonly Mock<IElectronicDocumentService> _electronicDocumentService = new Mock<IElectronicDocumentService>();
        private readonly Mock<IOthersDocsElecSoftwareService> _othersDocsElecSoftwareService = new Mock<IOthersDocsElecSoftwareService>();
        private readonly Mock<IContributorOperationsService> _contributorOperationsService = new Mock<IContributorOperationsService>();
        private readonly Mock<ITestSetOthersDocumentsResultService> _testSetOthersDocumentsResultService = new Mock<ITestSetOthersDocumentsResultService>();
        private readonly Mock<IEquivalentElectronicDocumentRepository> _equivalentElectronicDocumentRepository = new Mock<IEquivalentElectronicDocumentRepository>();
        
        [TestInitialize]
        public void TestInitialize()
        {
            _current = new OthersElectronicDocumentsController(
                    _othersElectronicDocumentsService.Object,
                    _othersDocsElecContributorService.Object,
                    _contributorService.Object,
                    _electronicDocumentService.Object,
                    _othersDocsElecSoftwareService.Object,
                    _contributorOperationsService.Object,
                    _testSetOthersDocumentsResultService.Object,
                    _equivalentElectronicDocumentRepository.Object);
        }

        [TestMethod()]
        public void IndexTest()
        {
            //arrange
            _ = _electronicDocumentService.Setup(x => x.GetElectronicDocuments()).Returns(GetDocumentos());
            //act
            var viewResult = _current.Index() as ViewResult;

            //assert
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(Navigation.NavigationEnum.OthersEletronicDocuments.ToString(), viewResult.ViewData["CurrentPage"].ToString());
            Assert.IsTrue(((List<AutoListModel>)viewResult.ViewData["ListElectronicDocuments"]).Count == 3);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Return Index OthersElectronicDocAssociated")]
        [DataRow(2, DisplayName = "Return AddParticipants")]
        [DataRow(3, DisplayName = "Finist model.OperationModeId == 1")]
        [DataRow(4, DisplayName = "Finist model.OperationModeId != 1")]
        public void AddOrUpdateTest(int input)
        {
            ActionResult resultRedirect;
            ValidacionOtherDocsElecViewModel dataentity = new ValidacionOtherDocsElecViewModel()
            {
                OperationModeId = Domain.Common.OtherDocElecOperationMode.OwnSoftware,
                ContributorIdType = Domain.Common.OtherDocElecContributorType.Transmitter,
                ElectronicDocumentId = 1,
            };
            //arrange
            _ = _electronicDocumentService.Setup(x => x.GetElectronicDocuments()).Returns(GetDocumentos());
            _ = _othersDocsElecContributorService.Setup(x => x.GetOperationModes()).Returns(GetOperationMode());
            _ = _electronicDocumentService.Setup(x => x.GetNameById(It.IsAny<int>())).Returns("ElectronicDocumentName");

            switch (input)
            {
                case 1:
                    //arrange
                    _othersDocsElecContributorService.Setup(x => x.GetDocElecContributorsByContributorId(It.IsAny<int>())).Returns(GetOtherDocElecContributor(1));
                    _othersElectronicDocumentsService.Setup(x => x.GetOtherDocElecContributorOperationByDocEleContributorId(It.IsAny<int>())).Returns(new OtherDocElecContributorOperations() { Id = 7 });

                    //act
                    resultRedirect = _current.AddOrUpdate(dataentity);

                    Assert.IsNotNull(resultRedirect);
                    Assert.AreEqual("Index", ((RedirectToRouteResult)resultRedirect).RouteValues["Action"]);
                    Assert.AreEqual("OthersElectronicDocAssociated", ((RedirectToRouteResult)resultRedirect).RouteValues["Controller"]);
                    Assert.AreEqual("7", ((RedirectToRouteResult)resultRedirect).RouteValues["Id"].ToString());

                    break;
                case 2:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetDocElecContributorsByContributorId(It.IsAny<int>())).Returns(GetOtherDocElecContributor(2));

                    //act 
                    resultRedirect = _current.AddOrUpdate(dataentity);

                    //assert
                    Assert.IsNotNull(resultRedirect);
                    Assert.AreEqual("AddParticipants", ((RedirectToRouteResult)resultRedirect).RouteValues["Action"]);
                    Assert.AreEqual(dataentity.ElectronicDocumentId.ToString(), ((RedirectToRouteResult)resultRedirect).RouteValues["electronicDocumentId"].ToString());

                    break;
                case 3:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetDocElecContributorsByContributorId(It.IsAny<int>())).Returns(GetOtherDocElecContributor(3));
                    _ = _othersDocsElecContributorService.Setup(x => x.List(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(GetPagedResult());
                    _ = _contributorService.Setup(x => x.GetContributorById(It.IsAny<int>(), It.IsAny<int>())).Returns(new Contributor() { Name = "NameContributor" });

                    //act
                    resultRedirect = _current.AddOrUpdate(dataentity);

                    //assert
                    Assert.IsNotNull(resultRedirect);
                    Assert.IsInstanceOfType(((ViewResult)resultRedirect).Model, typeof(OthersElectronicDocumentsViewModel));
                    Assert.IsTrue(condition: ((SelectList)((ViewResult)resultRedirect).ViewData["OperationModes"]).Any());
                    Assert.IsTrue(((OthersElectronicDocumentsViewModel)((ViewResult)resultRedirect).Model).ContributorName.Equals("NameContributor"));
                    break;
                case 4:
                    dataentity.OperationModeId = Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider;
                    _ = _othersDocsElecContributorService.Setup(x => x.GetDocElecContributorsByContributorId(It.IsAny<int>())).Returns(GetOtherDocElecContributor(4));
                    _ = _othersDocsElecContributorService.Setup(x => x.List(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(GetPagedResult());
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTechnologicalProviders(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(new List<Contributor> { new Contributor() { Id = 1001, Name = "NameContributor" } });

                    //act
                    resultRedirect = _current.AddOrUpdate(dataentity);
                    var viewResult = (ViewResult)resultRedirect;
                    //assert
                    Assert.IsNotNull(resultRedirect);
                    Assert.IsInstanceOfType(viewResult.Model, typeof(OthersElectronicDocumentsViewModel));
                    Assert.IsTrue(condition: ((SelectList)viewResult.ViewData["OperationModes"]).Any());
                    Assert.IsTrue(condition: ((SelectList)viewResult.ViewData["ListTechnoProviders"]).Any());
                    break;
            }
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Return Index OthersElectronicDocAssociated")]
        [DataRow(2, DisplayName = "response.Code == 500")]
        [DataRow(3, DisplayName = "Finist")]
        public async Task AddOrUpdateContributorTest(int input)
        {
            JsonResult jsonResult;
            ResponseMessage result;
            RedirectToRouteResult _redirectToRouteResult;
            OthersElectronicDocumentsViewModel model = new OthersElectronicDocumentsViewModel()
            {
                OperationModeId = (int)Domain.Common.OtherDocElecOperationMode.OwnSoftware,
                ContributorIdType = (int)Domain.Common.OtherDocElecContributorType.Transmitter,
                ElectronicDocumentId = 1,
                ProviderId = 1,
                SoftwareId = Guid.NewGuid().ToString()
            };

            switch (input)
            {
                case 1:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns((GlobalTestSetOthersDocuments)null);

                    //act
                    jsonResult = (JsonResult) await _current.AddOrUpdateContributor(model);
                    result = jsonResult.Data as ResponseMessage;

                    //Assert
                    Assert.AreEqual(TextResources.ModeElectroniDocWithoutTestSet, result.Message);
                    break;
                case 2:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns(new GlobalTestSetOthersDocuments());
                    _ = _othersElectronicDocumentsService.Setup(x => x.AddOtherDocElecContributorOperation(It.IsAny<OtherDocElecContributorOperations>(), It.IsAny<OtherDocElecSoftware>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new ResponseMessage() { Code = 500, Message = "PruebaCode500" });

                    //act
                    _redirectToRouteResult = await _current.AddOrUpdateContributor(model) as RedirectToRouteResult;

                    //assert
                    Assert.IsNotNull(_redirectToRouteResult);
                    Assert.AreEqual("AddParticipants", _redirectToRouteResult.RouteValues["Action"]);
                    Assert.AreEqual(model.ElectronicDocumentId.ToString(), _redirectToRouteResult.RouteValues["electronicDocumentId"].ToString());
                    Assert.AreEqual("PruebaCode500", _redirectToRouteResult.RouteValues["message"].ToString());

                    break;
                case 3:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns(new GlobalTestSetOthersDocuments());
                    _ = _othersElectronicDocumentsService.Setup(x => x.AddOtherDocElecContributorOperation(It.IsAny<OtherDocElecContributorOperations>(), It.IsAny<OtherDocElecSoftware>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new ResponseMessage() { Code = 200, Message = "" });
                    _ = _othersElectronicDocumentsService.Setup(x => x.ChangeParticipantStatus(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                    //act
                    _redirectToRouteResult = await _current.AddOrUpdateContributor(model) as RedirectToRouteResult;

                    //assert
                    Assert.IsNotNull(_redirectToRouteResult);
                    Assert.AreEqual("Index", _redirectToRouteResult.RouteValues["Action"].ToString());
                    Assert.AreEqual("OthersElectronicDocAssociated", _redirectToRouteResult.RouteValues["Controller"].ToString());
                    break;
            }
        }

        [TestMethod()]
        public void AddParticipantsTest()
        {
            //arrange
            _ = _othersDocsElecContributorService.Setup(x => x.GetOperationModes()).Returns(new List<Domain.Sql.OtherDocElecOperationMode> { new Domain.Sql.OtherDocElecOperationMode() { Id = 8, Name = "Name8" } });
            //act
            var viewResult = _current.AddParticipants(8, "MetodoMensaje") as ViewResult;

            //assert
            Assert.IsNotNull(viewResult);
            Assert.IsTrue(viewResult.ViewData["Message"].ToString().Equals("MetodoMensaje"));
            Assert.IsTrue(viewResult.ViewData["electronicDocumentId"].ToString().Equals("8"));
            Assert.IsTrue(((IEnumerable<SelectListItem>)viewResult.ViewData["ListOperationMode"]).Any());
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Sin test de pruebas")]
        [DataRow(2, DisplayName = "Finist")]
        public void AddTest(int input)
        {
            JsonResult jsonResult;
            ResponseMessage result;
            ValidacionOtherDocsElecViewModel registrationData = new ValidacionOtherDocsElecViewModel()
            {
                OperationModeId = Domain.Common.OtherDocElecOperationMode.OwnSoftware,
                ContributorIdType = Domain.Common.OtherDocElecContributorType.Transmitter,
                ElectronicDocumentId = 1,
            };

            switch (input)
            {
                case 1:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns((GlobalTestSetOthersDocuments)null);

                    //act
                    jsonResult = _current.Add(registrationData);
                    result = jsonResult.Data as ResponseMessage;

                    //Assert
                    Assert.AreEqual(TextResources.ModeElectroniDocWithoutTestSet, result.Message);
                    break;
                case 2:
                    //arrange
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns(new GlobalTestSetOthersDocuments());
                    _ = _othersDocsElecContributorService.Setup(t => t.CreateContributor(It.IsAny<int>(), OtherDocElecState.Registrado, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(new Domain.Sql.OtherDocElecContributor());
                    //act
                    jsonResult = _current.Add(registrationData);

                    //assert
                    Assert.IsNotNull(jsonResult);
                    Assert.AreEqual(TextResources.OtherSuccessSoftware, ((ResponseMessage)jsonResult.Data).Message);
                    break;
            }
        }

        [TestMethod()]
        public void GetSoftwaresByContributorIdTest()
        {
            //arrange
            _ = _othersDocsElecSoftwareService.Setup(x => x.GetSoftwaresByProviderTechnologicalServices(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new List<OtherDocElecSoftware> { new OtherDocElecSoftware() { Id = Guid.NewGuid(), Name = "Name8" } });
            //act
            var viewResult = _current.GetSoftwaresByContributorId(1, 2);

            //assert
            Assert.IsNotNull(viewResult);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Sin software")]
        [DataRow(2, DisplayName = "Con software")]
        public void GetDataBySoftwareIdTest(int input)
        {
            JsonResult viewResult;
            switch (input)
            {
                case 1:
                    //arrange
                    _ = _othersDocsElecSoftwareService.Setup(x => x.GetBySoftwareId(It.IsAny<Guid>()))
                        .Returns((OtherDocElecSoftware)null);
                    //act
                    viewResult = _current.GetDataBySoftwareId(Guid.NewGuid());

                    //assert
                    Assert.IsNotNull(viewResult);
                    Assert.IsTrue(viewResult.Data == null);
                    break;
                case 2:
                    //arrange
                    //arrange
                    _ = _othersDocsElecSoftwareService.Setup(x => x.GetBySoftwareId(It.IsAny<Guid>()))
                        .Returns(new OtherDocElecSoftware() { Id = Guid.NewGuid(), Name = "Name8" });
                    //act
                    viewResult = _current.GetDataBySoftwareId(Guid.NewGuid());

                    //assert
                    Assert.IsNotNull(viewResult);
                    Assert.IsTrue(viewResult.Data != null);
                    break;
            }


        }

        [TestMethod()]
        public void CancelRegisterTest()
        {
            //arrange
            _ = _othersDocsElecContributorService.Setup(x => x.CancelRegister(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(new ResponseMessage() { Message = "CancelRegisterTest" });
            //act
            JsonResult viewResult = _current.CancelRegister(1, "Descr");

            //assert
            Assert.IsNotNull(viewResult);
            Assert.IsTrue(((ResponseMessage)viewResult.Data).Message == "CancelRegisterTest");
        }

        [TestMethod]
        [DataRow(1, DisplayName = "Entrada Accion == SeleccionElectronicDocument")]
        [DataRow(2, DisplayName = "Entrada Accion == SeleccionParticipante TechnologyProvider")]
        [DataRow(3, DisplayName = "Entrada Accion == SeleccionParticipante No TechnologyProvider")]
        [DataRow(4, DisplayName = "Entrada Accion == SeleccionOperationMode --> Lista.Any()=True Sin test")]
        [DataRow(5, DisplayName = "Entrada Accion == SeleccionOperationMode --> Lista.Any()=False")]
        [DataRow(6, DisplayName = "Entrada Accion == CancelRegister")]
        [DataRow(7, DisplayName = "Entrada Accion == '' Fallido")]

        public void ValidationTest(int input)
        {
            JsonResult _jsonResult;
            ValidacionOtherDocsElecViewModel ValidacionOtherDocs = new ValidacionOtherDocsElecViewModel()
            {
                OperationModeId = Domain.Common.OtherDocElecOperationMode.OwnSoftware,
                ContributorIdType = Domain.Common.OtherDocElecContributorType.Transmitter,
                ElectronicDocumentId = 1,
            };

            switch (input)
            {
                case 1:
                    #region  case 1:
                    //arrange
                    ValidacionOtherDocs.Accion = "SeleccionElectronicDocument";
                    //act
                    _jsonResult = _current.Validation(ValidacionOtherDocs);

                    Assert.IsNotNull(_jsonResult);
                    Assert.IsTrue(((ResponseMessage)_jsonResult.Data).Message.Contains("¿Desea iniciar el proceso de habilitación para ?"));
                    #endregion
                    break;
                case 2:
                    #region  case 2:
                    //arrange 
                    ValidacionOtherDocs.Accion = "SeleccionParticipante";
                    ValidacionOtherDocs.ContributorIdType = Domain.Common.OtherDocElecContributorType.TechnologyProvider;
                    _ = _contributorService.Setup(x => x.Get(It.IsAny<int>())).Returns(new Contributor() { Name = "NameContributor", ContributorTypeId = (int)Domain.Common.ContributorType.Biller });

                    //act 
                    _jsonResult = _current.Validation(ValidacionOtherDocs);

                    //assert
                    Assert.IsNotNull(_jsonResult);
                    Assert.IsTrue(((ResponseMessage)_jsonResult.Data).Message.Equals(TextResources.TechnologProviderDisabled));

                    #endregion 
                    break;
                case 3:
                    #region  case 3:
                    //arrange 
                    ValidacionOtherDocs.Accion = "SeleccionParticipante";
                    ValidacionOtherDocs.ContributorIdType = Domain.Common.OtherDocElecContributorType.Transmitter;
                    _ = _contributorService.Setup(x => x.Get(It.IsAny<int>())).Returns(new Contributor() { Name = "NameContributor", ContributorTypeId = (int)Domain.Common.ContributorType.Biller });

                    //act 
                    _jsonResult = _current.Validation(ValidacionOtherDocs);

                    //assert
                    Assert.IsNotNull(_jsonResult);
                    Assert.IsTrue(((ResponseMessage)_jsonResult.Data).Message.Contains("¿Desea iniciar el proceso como para  de otros documentos electrónicos?"));

                    #endregion
                    break;
                case 4:
                    #region  case 4:

                    //arrange 
                    ValidacionOtherDocs.Accion = "SeleccionOperationMode";
                    _ = _othersDocsElecContributorService.Setup(x => x.ValidateExistenciaContribuitor(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                        .Returns(new List<OtherDocElecContributor> { new OtherDocElecContributor() { ElectronicDocumentId = 2 } });
                    _ = _othersDocsElecContributorService.Setup(x => x.GetTestResult(It.IsAny<int>(), It.IsAny<int>())).Returns((GlobalTestSetOthersDocuments)null);

                    //act 
                    _jsonResult = _current.Validation(ValidacionOtherDocs);

                    //assert
                    Assert.IsNotNull(_jsonResult);
                    Assert.IsTrue(((ResponseMessage)_jsonResult.Data).Message.Equals(TextResources.ModeElectroniDocWithoutTestSet));

                    #endregion

                    break;
                case 5:
                    #region  case 5:

                    //arrange 
                    ValidacionOtherDocs.Accion = "SeleccionOperationMode";
                    _ = _othersDocsElecContributorService.Setup(x => x.ValidateExistenciaContribuitor(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                        .Returns(new List<OtherDocElecContributor>());
     
                    //act 
                    _jsonResult = _current.Validation(ValidacionOtherDocs);

                    //assert
                    Assert.IsNotNull(_jsonResult);
                    Assert.IsTrue(((ResponseMessage)_jsonResult.Data).Message.Equals("¿Desea habilitar la trasmisión de \"Otros Documentos Electrónicos\" en el sistema como  de otros documentos electrónicos?"));

                    #endregion  

                    break;
                case 6:
                    #region  case 6:

                    //arrange 
                    ValidacionOtherDocs.Accion = "CancelRegister";
                    //act 
                    _jsonResult = _current.Validation(ValidacionOtherDocs);
                    //Assert
                    Assert.IsNotNull(_jsonResult);
                    Assert.IsTrue(((ResponseMessage)_jsonResult.Data).Message.Contains("¿Desea habilitar la trasmisión de \"Otros Documentos Electrónicos\" en el sistema como  de otros documentos electrónicos?"));
                    #endregion 

                    break;
                case 7:
                    #region  case 7:

                    //arrange 
                    ValidacionOtherDocs.Accion = "";
                    //act 
                    _jsonResult = _current.Validation(ValidacionOtherDocs);
                    //Assert
                    Assert.IsNotNull(_jsonResult);
                    Assert.IsTrue(((ResponseMessage)_jsonResult.Data).Message.Contains("No se logro realizar la validación del participante a Registrar!!!"));
                    #endregion 

                    break;
            }
        }

        #region ---Private---

        private List<ElectronicDocument> GetDocumentos()
        {
            List<ElectronicDocument> Lista = new List<ElectronicDocument>
            {
                new ElectronicDocument() { Id = 1, Name = "Doc1" },
                new ElectronicDocument() { Id = 2, Name = "Doc2" },
                new ElectronicDocument() { Id = 3, Name = "Doc3" }
            };
            return Lista;
        }
        private List<Domain.Sql.OtherDocElecOperationMode> GetOperationMode()
        {
            List<Domain.Sql.OtherDocElecOperationMode> Lista = new List<Domain.Sql.OtherDocElecOperationMode>
            {
                new Domain.Sql.OtherDocElecOperationMode() { Id = 1, Name = "OperationMode1" },
                new Domain.Sql.OtherDocElecOperationMode() { Id = 2, Name = "OperationMode2" },
                new Domain.Sql.OtherDocElecOperationMode() { Id = 3, Name = "OperationMode3" }
            };
            return Lista;
        }

        private List<OtherDocElecContributor> GetOtherDocElecContributor(int tipoReturn)
        {
            List<OtherDocElecContributor> Lista = null;
            if (tipoReturn.Equals(1))
            {
                Lista = new List<OtherDocElecContributor>
                {
                    new OtherDocElecContributor() { Id = 1, State = OtherDocElecState.Habilitado.GetDescription(),
                        OtherDocElecContributorTypeId= (int)Domain.Common.OtherDocElecContributorType.Transmitter,
                        OtherDocElecOperationModeId= (int)Domain.Common.OtherDocElecOperationMode.OwnSoftware }
                };
                return Lista;
            }
            if (tipoReturn.Equals(2))
            {
                Lista = new List<OtherDocElecContributor>
                {
                    new OtherDocElecContributor() { Id = 1, State = OtherDocElecState.Test.GetDescription(),
                        OtherDocElecContributorTypeId= (int)Domain.Common.OtherDocElecContributorType.Transmitter,
                        OtherDocElecOperationModeId= (int)Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider }
                };
                return Lista;
            }
            if (tipoReturn.Equals(3))
            {
                Lista = new List<OtherDocElecContributor>
                {
                    new OtherDocElecContributor() { Id = 1, State = OtherDocElecState.Test.GetDescription(),
                        OtherDocElecContributorTypeId= (int)Domain.Common.OtherDocElecContributorType.Transmitter,
                        OtherDocElecOperationModeId= (int)Domain.Common.OtherDocElecOperationMode.OwnSoftware }
                };
                return Lista;
            }
            if (tipoReturn.Equals(4))
            {
                Lista = new List<OtherDocElecContributor>
                {
                    new OtherDocElecContributor() { Id = 1, State = OtherDocElecState.Test.GetDescription(),
                        OtherDocElecContributorTypeId= (int)Domain.Common.OtherDocElecContributorType.Transmitter,
                        OtherDocElecOperationModeId= (int)Domain.Common.OtherDocElecOperationMode.SoftwareTechnologyProvider }
                };
                return Lista;
            }
            return Lista;
        }
        private PagedResult<OtherDocsElectData> GetPagedResult()
        {
            PagedResult<OtherDocsElectData> List = new PagedResult<OtherDocsElectData>
            {
                Results = new List<OtherDocsElectData>
                        {
                             new OtherDocsElectData() { StateSoftware = ((int)OtherDocElecState.Registrado).ToString() }
                        }
            };


            return List;
        }

        #endregion
    }
}