using Gosocket.Dian.Application;
using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.RadianApproved;
using Microsoft.ApplicationInsights;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianApprovedControllerTests
    {

        private RadianApprovedController _current;
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();
        private readonly Mock<IRadianTestSetService> _radianTestSetService = new Mock<IRadianTestSetService>();
        private readonly Mock<IRadianApprovedService> _radianAprovedService = new Mock<IRadianApprovedService>();
        private readonly Mock<IRadianTestSetResultService> _radianTestSetResultService = new Mock<IRadianTestSetResultService>();
        private readonly Mock<IRadianTestSetAppliedService> _radianTestSetAppliedService = new Mock<IRadianTestSetAppliedService>();
        private readonly Mock<IGlobalRadianOperationService> _globalRadianOperationService = new Mock<IGlobalRadianOperationService>();
        private readonly TelemetryClient telemetry;

        [TestInitialize]
        public void TestInitialize()
        {
           _current = new RadianApprovedController(
               _radianContributorService.Object, 
               _radianTestSetService.Object, 
               _radianAprovedService.Object, 
               _radianTestSetResultService.Object, 
               _radianTestSetAppliedService.Object,
               _globalRadianOperationService.Object,
               telemetry);
        }

        [TestMethod()]
        [DataRow(1, DisplayName = "Redirect Index is Cancel")]
        [DataRow(2, DisplayName = "Radian Operation Mode Direct")]
        [DataRow(3, DisplayName = "Radian participant is Enabled")]
        [DataRow(4, DisplayName = "Radian participant redirect GetFactorOperationMode")]
        public void IndexTest(int input)
        {
            //arrange
            RedirectToRouteResult result = null;
            ViewResult viewResult = null;
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel();

            switch (input)
            {
                case 1:
                    _radianAprovedService.Setup(t => t.ContributorSummary(registrationData.ContributorId, (int)registrationData.RadianContributorType)).Returns(new Domain.Entity.RadianAdmin() { Contributor = new Domain.Entity.RedianContributorWithTypes() { RadianState = "Cancelado" } });
                    break;
                case 2:
                    registrationData.RadianOperationMode = Domain.Common.RadianOperationMode.Direct;
                    RadianAdmin radianAdmin = new RadianAdmin()
                    {
                        Contributor = new RedianContributorWithTypes(),
                        LegalRepresentativeIds = new List<string>(),
                    };
                    _radianAprovedService.Setup(t => t.ContributorSummary(registrationData.ContributorId, (int)registrationData.RadianContributorType)).Returns(radianAdmin);
                    PagedResult<RadianCustomerList> customers = new PagedResult<RadianCustomerList>()
                    {
                        Results = new List<RadianCustomerList>() { new RadianCustomerList() },
                        RowCount = 1
                    };
                    _radianAprovedService.Setup(t => t.CustormerList(radianAdmin.Contributor.RadianContributorId, string.Empty, RadianState.none, 1, 10)).Returns(customers);
                    PagedResult<RadianContributorFileHistory> data = new PagedResult<RadianContributorFileHistory>()
                    {
                        RowCount = 1,
                        Results = new List<RadianContributorFileHistory>() { new RadianContributorFileHistory() }
                    };
                    _radianAprovedService.Setup(t => t.FileHistoryFilter(radianAdmin.Contributor.RadianContributorId, string.Empty, string.Empty, string.Empty, 1, 10)).Returns(data);
                    break;
                case 3:
                    registrationData.RadianOperationMode = Domain.Common.RadianOperationMode.Indirect;
                    RadianAdmin radianAdmin3 = new RadianAdmin()
                    {
                        Contributor = new RedianContributorWithTypes()
                        {
                            RadianState = "Habilitado"
                        },
                        LegalRepresentativeIds = new List<string>(),
                    };
                    _radianAprovedService.Setup(t => t.ContributorSummary(registrationData.ContributorId, (int)registrationData.RadianContributorType)).Returns(radianAdmin3);
                    PagedResult<RadianCustomerList> customers3 = new PagedResult<RadianCustomerList>()
                    {
                        Results = new List<RadianCustomerList>() { new RadianCustomerList() },
                        RowCount = 1
                    };
                    _radianAprovedService.Setup(t => t.CustormerList(radianAdmin3.Contributor.RadianContributorId, string.Empty, RadianState.none, 1, 10)).Returns(customers3);
                    PagedResult<RadianContributorFileHistory> data3 = new PagedResult<RadianContributorFileHistory>()
                    {
                        RowCount = 1,
                        Results = new List<RadianContributorFileHistory>() { new RadianContributorFileHistory() }
                    };
                    _radianAprovedService.Setup(t => t.FileHistoryFilter(radianAdmin3.Contributor.RadianContributorId, string.Empty, string.Empty, string.Empty, 1, 10)).Returns(data3);
                    break;
                case 4:
                    registrationData.RadianOperationMode = Domain.Common.RadianOperationMode.Indirect;
                    RadianAdmin radianAdmin4 = new RadianAdmin()
                    {
                        Contributor = new RedianContributorWithTypes(),
                        LegalRepresentativeIds = new List<string>(),
                    };
                    _radianAprovedService.Setup(t => t.ContributorSummary(registrationData.ContributorId, (int)registrationData.RadianContributorType)).Returns(radianAdmin4);
                    PagedResult<RadianCustomerList> customers4 = new PagedResult<RadianCustomerList>()
                    {
                        Results = new List<RadianCustomerList>() { new RadianCustomerList() },
                        RowCount = 1
                    };
                    _radianAprovedService.Setup(t => t.CustormerList(radianAdmin4.Contributor.RadianContributorId, string.Empty, RadianState.none, 1, 10)).Returns(customers4);
                    PagedResult<RadianContributorFileHistory> data4 = new PagedResult<RadianContributorFileHistory>()
                    {
                        RowCount = 1,
                        Results = new List<RadianContributorFileHistory>() { new RadianContributorFileHistory() }
                    };
                    _radianAprovedService.Setup(t => t.FileHistoryFilter(radianAdmin4.Contributor.RadianContributorId, string.Empty, string.Empty, string.Empty, 1, 10)).Returns(data4);
                    _radianAprovedService.Setup(t => t.SoftwareByContributor(registrationData.ContributorId)).Returns(new Software());
                    _radianTestSetService.Setup(t => t.OperationModeList(registrationData.RadianOperationMode)).Returns(new List<Domain.RadianOperationMode>());
                    _radianAprovedService.Setup(t => t.ListRadianContributorOperations(radianAdmin4.Contributor.RadianContributorId)).Returns(new RadianContributorOperationWithSoftware());
                    break;
            }


            //act
            switch (input)
            {
                case 1:
                    result = _current.Index(registrationData) as RedirectToRouteResult;
                    break;
                default:
                    viewResult = _current.Index(registrationData) as ViewResult;
                    break;
            }

            //assert
            switch (input)
            {
                case 1:
                    Assert.AreEqual("Index", result.RouteValues["Action"]);
                    Assert.AreEqual("Radian", result.RouteValues["Controller"]);
                    break;
                case 2:
                case 3:
                    Assert.AreEqual(string.Empty, viewResult.ViewName);
                    break;
                case 4:
                    Assert.AreEqual("GetFactorOperationMode", viewResult.ViewName);
                    break;
            }

        }

        [TestMethod]
        [DataRow(1, DisplayName = "WithOut Test Set")]
        [DataRow(2, DisplayName = "Create user With operation mode indirect")]
        [DataRow(3, DisplayName = "Create user Without software")]
        [DataRow(4, DisplayName = "Create user With operation")]
        public async void AddTest(int input)
        {
            //arrange
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel();
            switch (input)
            {
                case 1:
                    registrationData.RadianOperationMode = Domain.Common.RadianOperationMode.Direct;
                    _radianAprovedService.Setup(t => t.GetTestSet(((int)RadianOperationModeTestSet.OwnSoftware).ToString())).Returns((RadianTestSet)null);
                    break;
                case 2:
                    registrationData.RadianOperationMode = Domain.Common.RadianOperationMode.Indirect;
                    _radianContributorService.Setup(t => t.CreateContributor(registrationData.ContributorId,
                                                RadianState.Registrado,
                                                registrationData.RadianContributorType,
                                                registrationData.RadianOperationMode,
                                                It.IsAny<string>())).Returns(new RadianContributor());
                    break;
                case 3:
                    registrationData.RadianOperationMode = Domain.Common.RadianOperationMode.Direct;
                    _radianAprovedService.Setup(t => t.GetTestSet(((int)RadianOperationModeTestSet.OwnSoftware).ToString())).Returns(new RadianTestSet());
                    _radianContributorService.Setup(t => t.CreateContributor(registrationData.ContributorId,
                                                RadianState.Registrado,
                                                registrationData.RadianContributorType,
                                                registrationData.RadianOperationMode,
                                                It.IsAny<string>())).Returns(new RadianContributor());
                    break;
                case 4:
                    registrationData.RadianOperationMode = Domain.Common.RadianOperationMode.Direct;
                    RadianContributor radianContributor = new RadianContributor()
                    {
                        RadianSoftwares = new List<RadianSoftware>() { new RadianSoftware() }
                    };
                    _radianAprovedService.Setup(t => t.GetTestSet(((int)RadianOperationModeTestSet.OwnSoftware).ToString())).Returns(new RadianTestSet());
                    _radianContributorService.Setup(t => t.CreateContributor(registrationData.ContributorId,
                                                RadianState.Registrado,
                                                registrationData.RadianContributorType,
                                                registrationData.RadianOperationMode,
                                                It.IsAny<string>())).Returns(radianContributor);
                    _radianAprovedService.Setup(t => t.AddRadianContributorOperation(It.IsAny<RadianContributorOperation>(), It.IsAny<RadianSoftware>(), It.IsAny<RadianTestSet>(), true, false)).Returns(new Task<ResponseMessage>(() => new ResponseMessage(TextResources.SuccessSoftware, TextResources.alertType)));
                    break;
            }

            //act
            var result = await _current.Add(registrationData);
            ResponseMessage message = result.Data as ResponseMessage;

            //assert
            switch (input)
            {
                case 1:
                    Assert.AreEqual(TextResources.ModeWithoutTestSet, message.Message);
                    break;
                case 2:
                case 4:
                    Assert.AreEqual(TextResources.SuccessSoftware, message.Message);
                    break;
                case 3:
                    Assert.AreEqual(TextResources.ParticipantWithoutSoftware, message.Message);
                    break;
            }
        }

        [TestMethod]
        public void UploadFilesTest()
        {
            //arrange
            NameValueCollection form = new NameValueCollection
            {
                ["nit"] = "1111111",
                ["email"] = "test@gmail.com",
                ["contributorId"] = "1",
                ["radianContributorType"] = "1",
                ["radianOperationMode"] = "1",
                ["filesNumber"] = "1",
                ["step"] = "3",
                ["radianState"] = "En pruebas",
                ["radianContributorTypeiD"] = "1"
            };

            Mock<HttpContextBase> context = new Mock<HttpContextBase>();
            Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();
            Mock<HttpFileCollectionBase> files = new Mock<HttpFileCollectionBase>();
            Mock<HttpPostedFileBase> file = new Mock<HttpPostedFileBase>();

            //Simular contexto
            context.Setup(x => x.Request).Returns(request.Object);

            //simular listado de archivos
            files.Setup(x => x.Count).Returns(1);
            file.Setup(x => x.ContentLength).Returns(1);
            file.Setup(x => x.FileName).Returns("SimulateTestFile");
            files.Setup(x => x.Get(0).InputStream).Returns(file.Object.InputStream);

            //Simular llamados del request para los archivos
            request.Setup(x => x.Files).Returns(files.Object);
            request.Setup(x => x.Files[0]).Returns(file.Object);

            //Asigno la simulacion al contexto del controlador
            Mock<ControllerContext> controllercontext = new Mock<ControllerContext>();
            controllercontext.Setup(frm => frm.HttpContext.Request.Form).Returns(form);
            controllercontext.Setup(frm => frm.HttpContext.Request.Files).Returns(files.Object);
            _current.ControllerContext = controllercontext.Object;

            ResponseMessage responseUpload = new ResponseMessage(Guid.NewGuid().ToString(), TextResources.alertType);
            _radianAprovedService.Setup(t => t.UploadFile(file.Object.InputStream, form["nit"], It.IsAny<RadianContributorFile>())).Returns(responseUpload);
            _radianAprovedService.Setup(t => t.AddFileHistory(It.IsAny<RadianContributorFileHistory>())).Returns(new ResponseMessage());
            _radianAprovedService.Setup(t => t.UpdateRadianContributorStep(It.IsAny<int>(), It.IsAny<int>())).Returns(new ResponseMessage());

            //act
            JsonResult result = _current.UploadFiles();
            string message = result.Data.GetType().GetProperty("message").GetValue(result.Data).ToString();

            //assert
            Assert.AreEqual("Datos actualizados correctamente.", message);
        }

        [TestMethod]
        public void GetSetTestResultPostTest()
        {
            //arrange
            NameValueCollection form = new NameValueCollection
            {
                ["ContributorId"] = "1",
                ["Contributor.RadianContributorId"] = "1"
            };
            Mock<ControllerContext> controllercontext = new Mock<ControllerContext>();
            controllercontext.Setup(frm => frm.HttpContext.Request.Params).Returns(form);
            _current.ControllerContext = controllercontext.Object;
            RadianApprovedViewModel model = new RadianApprovedViewModel()
            {
                Contributor = new RedianContributorWithTypes() { RadianContributorId = 1 }
            };
            _radianAprovedService.Setup(t => t.GetSoftware(model.Contributor.RadianContributorId, 1)).Returns(new RadianSoftware());
            _radianTestSetResultService.Setup(t => t.GetTestSetResult(model.Nit, It.IsAny<string>())).Returns(new RadianTestSetResult());

            //act
            ViewResult result = _current.GetSetTestResult(model) as ViewResult;

            //assert
            Assert.IsNotNull(result);

        }

        [TestMethod]
        public void GetSetTestResultTest()
        {
            //arrange
            RadianGetSetTestViewModel viewModel = new RadianGetSetTestViewModel() { SoftwareId = Guid.NewGuid().ToString(), Nit = "11111", RadianContributorTypeId = 1, OperationMode = 1, SoftwareType = 1 };
            _radianAprovedService.Setup(t => t.GetSoftware(It.IsAny<Guid>())).Returns(new RadianSoftware());
            _radianTestSetResultService.Setup(t => t.GetTestSetResult(viewModel.Nit, It.IsAny<string>())).Returns(new RadianTestSetResult());

            //act 
            ViewResult result = _current.GetSetTestResult(viewModel) as ViewResult;

            //assert 
            Assert.IsNotNull(result);

        }

        [TestMethod]
        [DataRow(1, DisplayName = "Reset successfull")]
        [DataRow(2, DisplayName = "Reset Failed")]
        public void RestartSetTestResultTest(int input)
        {
            //arrange
            RadianTestSetResult result = new RadianTestSetResult();
            string sofwareId = Guid.NewGuid().ToString();
            int operationId = 1;
            if (input == 1)
            {
                _radianTestSetAppliedService.Setup(t => t.InsertOrUpdateTestSet(It.IsAny<RadianTestSetResult>())).Returns(true);
                _radianTestSetAppliedService.Setup(t => t.ResetPreviousCounts(It.IsAny<string>())).Returns(true);
                _radianAprovedService.Setup(t => t.ResetRadianOperation(It.IsAny<int>())).Returns(true);
            }
            else
            {
                _radianTestSetAppliedService.Setup(t => t.InsertOrUpdateTestSet(It.IsAny<RadianTestSetResult>())).Returns(true);
                _radianTestSetAppliedService.Setup(t => t.ResetPreviousCounts(It.IsAny<string>())).Returns(true);
                _radianAprovedService.Setup(t => t.ResetRadianOperation(It.IsAny<int>())).Returns(false);
            }


            //act
            JsonResult responseData = _current.RestartSetTestResult(result, sofwareId, operationId);
            ResponseMessage message = responseData.Data as ResponseMessage;

            //assert
            if (input == 1)
                Assert.AreEqual("Contadores reiniciados correctamente", message.Message);
            else
                Assert.AreEqual("¡Error en la actualización!", message.Message);

        }


        [TestMethod]
        public void GetFactorOperationMode()
        {
            //arrange
            RadianApprovedViewModel radianApprovedViewModel = new RadianApprovedViewModel();
            _radianAprovedService.Setup(t => t.ContributorSummary(radianApprovedViewModel.ContributorId, radianApprovedViewModel.RadianContributorTypeId)).Returns(new RadianAdmin() { Contributor = new RedianContributorWithTypes() });
            _radianAprovedService.Setup(t => t.SoftwareByContributor(radianApprovedViewModel.ContributorId)).Returns(new Software());
            _radianTestSetService.Setup(t => t.OperationModeList(It.IsAny<Domain.Common.RadianOperationMode>())).Returns(new List<Domain.RadianOperationMode>());
            _radianAprovedService.Setup(t => t.ListRadianContributorOperations(It.IsAny<int>())).Returns(new RadianContributorOperationWithSoftware());

            //act
            ViewResult viewResult = _current.GetFactorOperationMode(radianApprovedViewModel) as ViewResult;

            //assert 
            Assert.IsNotNull(viewResult);

        }


        [TestMethod]
        public async void UpdateFactorOperationModeTest()
        {
            //arrange
            SetOperationViewModel data = new SetOperationViewModel();
            _radianAprovedService.Setup(t => t.GetRadianContributor(It.IsAny<int>())).Returns(new RadianContributor());
            _radianAprovedService.Setup(t => t.GetTestSet(data.SoftwareType.ToString())).Returns(new RadianTestSet());
            _radianAprovedService.Setup(t => t.AddRadianContributorOperation(It.IsAny<RadianContributorOperation>(), It.IsAny<RadianSoftware>(), It.IsAny<RadianTestSet>(), false, true)).Returns(new Task<ResponseMessage>(() => new ResponseMessage("Test", "test")));
            _radianContributorService.Setup(t => t.ChangeParticipantStatus(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            //act
            var jsonResult = await _current.UpdateFactorOperationMode(data);
            ResponseMessage message = jsonResult.Data as ResponseMessage;

            //assert
            Assert.AreEqual("Test", message.Message);
        }


        [TestMethod]
        public void SetTestDetailsTest()
        {
            //arrange
            NameValueCollection form = new NameValueCollection
            {
                ["ContributorId"] = "1",
                ["Contributor.RadianContributorId"] = "1",
                ["SoftwareT"] = "1"
            };
            Mock<ControllerContext> controllercontext = new Mock<ControllerContext>();
            controllercontext.Setup(frm => frm.HttpContext.Request.Params).Returns(form);
            _current.ControllerContext = controllercontext.Object;
            RadianApprovedViewModel radianApprovedViewModel = new RadianApprovedViewModel()
            {
                Contributor = new RedianContributorWithTypes(),
                RadianTestSetResult = new RadianTestSetResult()
            };
            _radianAprovedService.Setup(t => t.RadianTestSetResultByNit(radianApprovedViewModel.Nit, radianApprovedViewModel.RadianTestSetResult.Id));

            //act
            ViewResult viewResult = _current.SetTestDetails(radianApprovedViewModel) as ViewResult;

            //assert
            Assert.IsNotNull(viewResult);

        }

        [TestMethod]
        public void RadianTestResultByNitTest()
        {
            //arrange
            string nit = "1111";
            string idTestSet = "1";
            _radianAprovedService.Setup(t => t.RadianTestSetResultByNit(nit, idTestSet)).Returns(new RadianTestSetResult());

            //act
            JsonResult jsonResult = _current.RadianTestResultByNit(nit, idTestSet);
            RadianTestSetResult radianTestSetResult = jsonResult.Data.GetType().GetProperty("data").GetValue(jsonResult.Data) as RadianTestSetResult;

            //assert
            Assert.IsNotNull(radianTestSetResult);
        }

        [TestMethod]
        public void DeleteUserTest()
        {
            //arrange
            int id = 1;
            int radianContributorTypeId = 1;
            string radianState = "Habilitado";
            string description = "test";
            _radianContributorService.Setup(t => t.ChangeParticipantStatus(id, It.IsAny<string>(), radianContributorTypeId, radianState, description)).Returns(true);

            //act
            JsonResult viewResult = _current.DeleteUser(id, radianContributorTypeId, radianState, description) as JsonResult;

            //assert
            Assert.IsNotNull(viewResult.Data);
        }


        [TestMethod]
        public void DeleteOperationModeTest()
        {
            //arrange
            string id = "1";
            _radianAprovedService.Setup(t => t.OperationDelete(Convert.ToInt32(id))).Returns(new ResponseMessage());

            //act
            JsonResult jsonResult = _current.DeleteOperationMode(id);

            //assert
            Assert.IsNotNull(jsonResult.Data);
        }


        [TestMethod]
        public void ViewTestSetTest()
        {
            //arrange
            int id = 1, radianTypeId = 1, softwareType = 1, operationId = 1;
            string softwareId = Guid.NewGuid().ToString();
            _radianAprovedService.Setup(t => t.ContributorSummary(id, radianTypeId)).Returns(new RadianAdmin() { Contributor = new RedianContributorWithTypes() });
            _radianTestSetResultService.Setup(t => t.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>())).Returns(new RadianTestSetResult());
            _radianTestSetService.Setup(t => t.GetTestSet(softwareType.ToString(), softwareType.ToString())).Returns(new RadianTestSet());
            _radianAprovedService.Setup(t => t.GetSoftware(It.IsAny<Guid>())).Returns(new RadianSoftware());

            //act
            ViewResult viewResult = _current.ViewTestSet(id, radianTypeId, softwareId, softwareType, operationId) as ViewResult;

            //assert
            Assert.AreEqual("GetSetTestResult", viewResult.ViewName);
        }

        [TestMethod]
        public void AutoCompleteProviderTest()
        {
            //arrange
            int contributorId = 1, contributorTypeId = 1;
            RadianOperationModeTestSet softwareType = RadianOperationModeTestSet.OwnSoftware;
            string term = string.Empty;
            _radianAprovedService.Setup(t => t.AutoCompleteProvider(contributorId, contributorTypeId, softwareType, term)).Returns(new List<RadianContributor>());

            //act
            JsonResult viewResult = _current.AutoCompleteProvider(contributorId, contributorTypeId, softwareType, term) as JsonResult;

            //assert
            Assert.IsNotNull(viewResult);
        }

        [TestMethod]
        public void SoftwareListTest()
        {
            //arrange
            int radianContributorId = 1;
            _radianAprovedService.Setup(t => t.SoftwareList(radianContributorId)).Returns(new List<RadianSoftware>());

            //act
            JsonResult jsonResult = _current.SoftwareList(radianContributorId) as JsonResult;

            //assert
            Assert.IsNotNull(jsonResult);
        }

        [TestMethod]
        public void CustomersListTest()
        {
            //arrange
            int radianContributorId = 1, page = 1, pagesize = 1;
            string code = "1";
            RadianState radianState = RadianState.Registrado;
            _radianAprovedService.Setup(t => t.CustormerList(radianContributorId, code, radianState, page, pagesize)).Returns(new PagedResult<RadianCustomerList>() { Results = new List<RadianCustomerList>() });

            //act
            JsonResult viewResult = _current.CustomersList(radianContributorId, code, radianState, page, pagesize) as JsonResult;

            //assert
            Assert.IsNotNull(viewResult.Data);
        }

        [TestMethod]
        public void FileHistoyListTest()
        {
            //arrange
            FileHistoryFilterViewModel filter = new FileHistoryFilterViewModel();
            _radianAprovedService.Setup(t => t.FileHistoryFilter(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(new PagedResult<RadianContributorFileHistory>() { Results = new List<RadianContributorFileHistory>() { new RadianContributorFileHistory() } });

            //act
            JsonResult jsonResult = _current.FileHistoyList(filter) as JsonResult;

            //assert
            Assert.IsNotNull(jsonResult.Data);
        }


    }
}