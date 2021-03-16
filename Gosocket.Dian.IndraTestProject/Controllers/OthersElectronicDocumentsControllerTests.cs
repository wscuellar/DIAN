using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gosocket.Dian.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Application;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Web.Utils;
using System.Web.Mvc;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;

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

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new OthersElectronicDocumentsController(
                    _othersElectronicDocumentsService.Object,
                    _othersDocsElecContributorService.Object,
                    _contributorService.Object,
                    _electronicDocumentService.Object,
                    _othersDocsElecSoftwareService.Object);
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
        //[DataRow(3, DisplayName = "Create Exitoso")]
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
                    _othersDocsElecContributorService.Setup(x => x.GetDocElecContributorsByContributorId(It.IsAny<int>())).Returns(GetOtherDocElecContributor(1));
                    _othersElectronicDocumentsService.Setup(x => x.GetOtherDocElecContributorOperationByDocEleContributorId(It.IsAny<int>())).Returns(new OtherDocElecContributorOperations() { Id = 7 });
                    resultRedirect = _current.AddOrUpdate(dataentity);

                    Assert.IsNotNull(resultRedirect);
                    Assert.AreEqual("Index", ((RedirectToRouteResult)resultRedirect).RouteValues["Action"]);
                    Assert.AreEqual("OthersElectronicDocAssociated", ((RedirectToRouteResult)resultRedirect).RouteValues["Controller"]);
                    Assert.AreEqual("7", ((RedirectToRouteResult)resultRedirect).RouteValues["Id"].ToString());

                    break;
                case 2:
                    _ = _othersDocsElecContributorService.Setup(x => x.GetDocElecContributorsByContributorId(It.IsAny<int>())).Returns(GetOtherDocElecContributor(2));

                    resultRedirect = _current.AddOrUpdate(dataentity);
                    Assert.IsNotNull(resultRedirect);
                    Assert.AreEqual("AddParticipants", ((RedirectToRouteResult)resultRedirect).RouteValues["Action"]);
                    Assert.AreEqual(dataentity.ElectronicDocumentId.ToString(), ((RedirectToRouteResult)resultRedirect).RouteValues["electronicDocumentId"].ToString());

                    break;
                case 3:
                    _ = _othersDocsElecContributorService.Setup(x => x.GetDocElecContributorsByContributorId(It.IsAny<int>())).Returns(GetOtherDocElecContributor(3));
                    //_ = _othersDocsElecContributorService.Setup(x => x.List(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(GetPagedResult());
                    resultRedirect = _current.AddOrUpdate(dataentity);
                    Assert.IsNotNull(resultRedirect);
                    Assert.AreEqual("AddParticipants", ((RedirectToRouteResult)resultRedirect).RouteValues["Action"]);
                    Assert.AreEqual(dataentity.ElectronicDocumentId.ToString(), ((RedirectToRouteResult)resultRedirect).RouteValues["electronicDocumentId"].ToString());
                    break;
                case 4:
                    break;
            }


            /* ViewBag.OperationModes
             * 
             _contributorService.GetContributorById(User.ContributorId(), model.ContributorIdType);
             _othersDocsElecContributorService.GetTechnologicalProviders(User.ContributorId(), model.ElectronicDocumentId, (int)Domain.Common.OtherDocElecContributorType.TechnologyProvider, OtherDocElecState.Habilitado.GetDescription());
            */

        }



        [TestMethod()]
        public void AddOrUpdateContributorTest()
        {
            var data = new OthersElectronicDocumentsViewModel();
            var result = _current.AddOrUpdateContributor(data);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void AddParticipantsTest()
        {
            var result = _current.AddParticipants(1, "");
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void AddTest()
        {
            var data = new ValidacionOtherDocsElecViewModel();
            var result = _current.Add(data);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void ValidationTest()
        {
            var data = new ValidacionOtherDocsElecViewModel();
            var result = _current.Validation(data);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void GetSoftwaresByContributorIdTest()
        {
            var result = _current.GetSoftwaresByContributorId(1, 1);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void GetDataBySoftwareIdTest()
        {
            var result = _current.GetDataBySoftwareId(Guid.NewGuid());
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void CancelRegisterTest()
        {
            var result = _current.CancelRegister(1, "");
            Assert.IsNotNull(result);

        }


        #region Private
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
            return Lista;
        }
      /*  private PagedResult<OtherDocsElectData> GetPagedResult()
        {
            PagedResult<OtherDocsElectData> List;
            /*List.Results = new List<OtherDocsElectData>
            {
new OtherDocsElectData() { StateSoftware = OtherDocElecState.Registrado }
            };


            return List;
        }*/
        #endregion
    }
}