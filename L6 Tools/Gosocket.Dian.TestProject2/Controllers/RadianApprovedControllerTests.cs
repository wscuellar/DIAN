using Gosocket.Dian.Common.Resources;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.RadianApproved;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianApprovedControllerTests
    {
        private readonly RadianApprovedController _radianApprovedController;
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();
        private readonly Mock<IRadianTestSetService> _radianTestSetService = new Mock<IRadianTestSetService>();
        private readonly Mock<IRadianApprovedService> _radianApprovedService = new Mock<IRadianApprovedService>();
        private readonly Mock<IRadianTestSetResultService> _radianTestSetResultService = new Mock<IRadianTestSetResultService>();

        public RadianApprovedControllerTests() => _radianApprovedController = new RadianApprovedController(_radianContributorService.Object,
                                                                                                           _radianTestSetService.Object,
                                                                                                           _radianApprovedService.Object,
                                                                                                           _radianTestSetResultService.Object);

        [TestMethod()]
        public void Index_View_CanceledContributor_Test()
        {
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel
            {
                ContributorId = 1,
                RadianContributorType = Domain.Common.RadianContributorType.ElectronicInvoice,
                RadianOperationMode = 0
            };

            _radianApprovedService.Setup(ras => ras.ContributorSummary(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new RadianAdmin()
                {
                    Files = new List<RadianContributorFile>(),
                    Contributor = new RedianContributorWithTypes()
                    {
                        Id = 1,
                        TradeName = string.Empty,
                        Code = string.Empty,
                        BusinessName = string.Empty,
                        Email = string.Empty,
                        Step = 1,
                        RadianState = string.Empty,
                        RadianContributorTypeId = 1,
                    },
                    LegalRepresentativeIds = new List<string>() { "00003BD4-3F33-4EEE-9205-6C01DD8EAE95" }
                });

            _radianApprovedService.Setup(ras => ras.ContributorFileTypeList(It.IsAny<int>())).Returns(new List<RadianContributorFileType>());
            _radianApprovedService.Setup(ras => ras.CustormerList(It.IsAny<int>(),
                                                                  It.IsAny<string>(),
                                                                  It.IsAny<RadianState>(),
                                                                  It.IsAny<int>(),
                                                                  It.IsAny<int>()))
                .Returns(new PagedResult<RadianCustomerList>()
                {
                    Results = new List<RadianCustomerList>()
                    {
                        new RadianCustomerList()
                        {
                            BussinessName = string.Empty,
                            Nit = string.Empty,
                            RadianState = string.Empty,
                            Page = 1,
                            Length = 0
                        }
                    }
                });

            _radianApprovedService.Setup(ras => ras.FileHistoryFilter(It.IsAny<int>(),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<int>(),
                                                                      It.IsAny<int>()))
                .Returns(new PagedResult<RadianContributorFileHistory>()
                {
                    Results = new List<RadianContributorFileHistory>()
                    {
                        new RadianContributorFileHistory()
                        {
                            FileName = string.Empty,
                            Comments = string.Empty,
                            CreatedBy = string.Empty,
                            Status = 1,
                            Timestamp = DateTime.Now
                        }
                    }
                });


            ViewResult result = _radianApprovedController.Index(registrationData) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod()]
        public void GetSetTestResult_View_Test()
        {
            _radianApprovedService.Setup(ras => ras.GetSoftware(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new RadianSoftware() { Id = Guid.NewGuid() });

            _radianTestSetResultService.Setup(rtrs => rtrs.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new RadianTestSetResult());

            _radianTestSetService.Setup(rts => rts.GetTestSet(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new RadianTestSet());

            ViewResult result = (ViewResult)_radianApprovedController.GetSetTestResult(new RadianApprovedViewModel()
            {
                Contributor = new RedianContributorWithTypes()
                {
                    RadianContributorId = 1
                }
            });
            Assert.IsInstanceOfType(result.Model, typeof(RadianApprovedViewModel));
        }


        [TestMethod()]
        public void SetTestDetails_View_Test()
        {
            _radianTestSetResultService.Setup(rtrs => rtrs.GetTestSetResultByNit(It.IsAny<string>()))
                .Returns(new List<RadianTestSetResult>() { new RadianTestSetResult() });
            ViewResult result = (ViewResult)_radianApprovedController.SetTestDetails(new RadianApprovedViewModel());
            Assert.IsInstanceOfType(result.Model, typeof(RadianApprovedViewModel));
        }

        [TestMethod()]
        public void ViewTestSet_View_Test()
        {
            _radianApprovedService.Setup(ras => ras.ContributorSummary(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new RadianAdmin()
                {
                    Contributor = new RedianContributorWithTypes()
                    {
                        Id = 1,
                        TradeName = string.Empty,
                        Code = string.Empty,
                        BusinessName = string.Empty,
                        Email = string.Empty,
                        Step = 1,
                        RadianState = string.Empty,
                        RadianContributorTypeId = 1,
                    },
                });

            _radianApprovedService.Setup(ras => ras.GetSoftware(It.IsAny<Guid>())).Returns(new RadianSoftware());
            _radianTestSetResultService.Setup(rtrs => rtrs.GetTestSetResult(It.IsAny<string>(), It.IsAny<string>())).Returns(new RadianTestSetResult());
            _radianTestSetService.Setup(rtrs => rtrs.GetTestSet(It.IsAny<string>(), It.IsAny<string>())).Returns(new RadianTestSet());

            ViewResult result = _radianApprovedController.ViewTestSet(1, 1, "7e35c155-352f-411c-81ff-ce78c340f237", 1) as ViewResult;
            Assert.IsInstanceOfType(result.Model, typeof(RadianApprovedViewModel));
        }

        [TestMethod()]
        public void GetFactorOperationMode_View_Test()
        {
            RadianApprovedViewModel radianApprovedViewModel = new RadianApprovedViewModel();

            _radianApprovedService.Setup(ras => ras.ContributorSummary(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new RadianAdmin()
                {
                    Contributor = new RedianContributorWithTypes()
                    {
                        RadianOperationModeId = 1,
                        RadianContributorId = 1
                    }
                });

            _radianApprovedService.Setup(ras => ras.SoftwareByContributor(It.IsAny<int>())).Returns(new Software());
            _radianTestSetService.Setup(rts => rts.OperationModeList(It.IsAny<Domain.Common.RadianOperationMode>())).Returns(new List<Domain.RadianOperationMode>());
            _radianApprovedService.Setup(ras => ras.ListRadianContributorOperations(It.IsAny<int>())).Returns(new RadianContributorOperationWithSoftware());

            ViewResult result = (ViewResult)_radianApprovedController.GetFactorOperationMode(radianApprovedViewModel);
            Assert.IsInstanceOfType(result.Model, typeof(RadianApprovedOperationModeViewModel));
        }


        [TestMethod()]
        public void Add_Result_RadianOperationModeDirect_Test()
        {
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel() { RadianOperationMode = Domain.Common.RadianOperationMode.Direct };
            _radianApprovedService.Setup(ras => ras.GetTestResult(It.IsAny<string>())).Returns<RadianTestSet>(null);
            ResponseMessage result = _radianApprovedController.Add(registrationData).Data as ResponseMessage;
            Assert.IsTrue(result.Code == 500);
        }

        [TestMethod()]
        public void Add_Result_RadianOperationModeIndirect_Test()
        {
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel() { RadianOperationMode = Domain.Common.RadianOperationMode.Indirect };

            _radianContributorService.Setup(rcs => rcs.CreateContributor(registrationData.ContributorId,
                                                                     RadianState.Registrado,
                                                                     registrationData.RadianContributorType,
                                                                     registrationData.RadianOperationMode,
                                                                     It.IsAny<string>())).Returns<RadianContributor>(null);

            ResponseMessage result = _radianApprovedController.Add(registrationData).Data as ResponseMessage;
            Assert.IsTrue(result.Message.Equals(TextResources.SuccessSoftware));
        }

        [TestMethod()]
        public void Add_Result_WithoutSoftware_Test()
        {
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel() { RadianOperationMode = Domain.Common.RadianOperationMode.None };

            _radianContributorService.Setup(rcs => rcs.CreateContributor(registrationData.ContributorId,
                                                                     RadianState.Registrado,
                                                                     registrationData.RadianContributorType,
                                                                     registrationData.RadianOperationMode,
                                                                     It.IsAny<string>())).Returns(new RadianContributor());

            ResponseMessage result = _radianApprovedController.Add(registrationData).Data as ResponseMessage;
            Assert.IsTrue(result.Message.Equals(TextResources.ParticipantWithoutSoftware));
        }

        [TestMethod()]
        public void Add_Result_WithSoftware_ModeWithoutTestSet_Test()
        {
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel() { RadianOperationMode = Domain.Common.RadianOperationMode.Direct };

            _radianContributorService.Setup(rcs => rcs.CreateContributor(registrationData.ContributorId,
                                                                         RadianState.Registrado,
                                                                         registrationData.RadianContributorType,
                                                                         registrationData.RadianOperationMode,
                                                                         It.IsAny<string>())).Returns(new RadianContributor()
                                                                         {
                                                                             Id = 10,
                                                                             RadianSoftwares = new List<RadianSoftware>()
                                                                             {
                                                                                 new RadianSoftware() { Id = Guid.NewGuid() }
                                                                             }
                                                                         });

            ResponseMessage result = _radianApprovedController.Add(registrationData).Data as ResponseMessage;
            Assert.IsTrue(result.Message.Equals(TextResources.ModeWithoutTestSet));
        }

        [TestMethod()]
        public void Add_Result_WithSoftware_Test()
        {
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel() { RadianOperationMode = Domain.Common.RadianOperationMode.None };

            _radianContributorService.Setup(rcs => rcs.CreateContributor(registrationData.ContributorId,
                                                                         RadianState.Registrado,
                                                                         registrationData.RadianContributorType,
                                                                         registrationData.RadianOperationMode,
                                                                         It.IsAny<string>())).Returns(new RadianContributor()
                                                                         {
                                                                             Id = 10,
                                                                             RadianSoftwares = new List<RadianSoftware>()
                                                                             {
                                                                                 new RadianSoftware() { Id = Guid.NewGuid() }
                                                                             }
                                                                         });

            _radianApprovedService.Setup(ras => ras.AddRadianContributorOperation(It.IsAny<RadianContributorOperation>(),
                                                                                     It.IsAny<RadianSoftware>(),
                                                                                     It.IsAny<RadianTestSet>(),
                                                                                     true,
                                                                                     false)).Returns(new ResponseMessage(TextResources.SuccessSoftware,
                                                                                                                         TextResources.alertType));

            ResponseMessage result = _radianApprovedController.Add(registrationData).Data as ResponseMessage;
            Assert.IsTrue(result.Message.Equals(TextResources.SuccessSoftware));
        }

        [TestMethod()]
        public void UploadFiles_Result_Test()
        {
            Mock<HttpContextBase> moqContext = new Mock<HttpContextBase>();
            Mock<HttpRequestBase> moqRequest = new Mock<HttpRequestBase>();
            Mock<HttpFileCollectionBase> mockCollection = new Mock<HttpFileCollectionBase>();

            NameValueCollection formValues = new NameValueCollection() {
                { "contributorId", "1" },
                { "radianContributorType", "" },
                { "radianOperationMode", "" },
                { "filesNumber", "2" },
                { "radianContributorTypeiD", "1"}
            };

            moqContext.Setup(mc => mc.Request).Returns(moqRequest.Object);
            moqRequest.Setup(mr => mr.Form).Returns(formValues);

            mockCollection.Setup(x => x.Count).Returns(0);
            moqRequest.Setup(mr => mr.Files).Returns(mockCollection.Object);

            _radianApprovedService.Setup(ras => ras.RadianContributorId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(1);

            RadianApprovedController radianApprovedController = _radianApprovedController;
            radianApprovedController.ControllerContext = new ControllerContext(moqContext.Object, new RouteData(), radianApprovedController);

            JsonResult result = radianApprovedController.UploadFiles();
            Assert.IsInstanceOfType(result.Data.GetType().GetProperty("data").GetValue(result.Data, null), typeof(ParametersDataViewModel));
            Assert.IsTrue(((string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null)).Equals("Datos actualizados correctamente."));
        }

        [TestMethod()]
        public void UpdateFactorOperationMode_Result_Test()
        {
            _radianApprovedService.Setup(ras => ras.GetTestResult(It.IsAny<string>())).Returns(new RadianTestSet());

            _radianApprovedService.Setup(ras => ras.AddRadianContributorOperation(It.IsAny<RadianContributorOperation>(),
                                                                                  It.IsAny<RadianSoftware>(),
                                                                                  It.IsAny<RadianTestSet>(),
                                                                                  It.IsAny<bool>(),
                                                                                  It.IsAny<bool>())).Returns(new ResponseMessage()
                                                                                  {
                                                                                      Message = TextResources.SuccessSoftware,
                                                                                      MessageType = TextResources.alertType,
                                                                                      Code = 200
                                                                                  });

            _radianApprovedService.Setup(ras => ras.GetRadianContributor(It.IsAny<int>())).Returns(new RadianContributor()
            {
                RadianState = "Habilitado"
            });

            ResponseMessage result = (ResponseMessage)_radianApprovedController.UpdateFactorOperationMode(new SetOperationViewModel()
            {
                RadianContributorId = 1,
                SoftwareType = 1,
                SoftwareId = Guid.NewGuid().ToString(),
                Url = "http:",
                SoftwareName = "SoftName",
                Pin = "pin"
            }).Data;

            Assert.IsTrue(result.Message.Equals(TextResources.SuccessSoftware));
        }

        [TestMethod()]
        public void DeleteUser_Result_Test()
        {
            _radianContributorService.Setup(rcs => rcs.ChangeParticipantStatus(It.IsAny<int>(),
                                                                               It.IsAny<string>(),
                                                                               It.IsAny<int>(),
                                                                               It.IsAny<string>(),
                                                                               It.IsAny<string>())).Returns(true);

            JsonResult result = _radianApprovedController.DeleteUser(1, 1, string.Empty, string.Empty) as JsonResult;
            string message = (string)result.Data.GetType().GetProperty("message").GetValue(result.Data, null);

            Assert.AreEqual("Datos actualizados", message);
        }

        [TestMethod()]
        public void RadianTestResultByNit_Result_Test()
        {
            _radianApprovedService.Setup(ras => ras.RadianTestSetResultByNit(It.IsAny<string>())).Returns(new RadianTestSetResult());
            JsonResult result = _radianApprovedController.RadianTestResultByNit(string.Empty);
            Assert.IsInstanceOfType(result.Data.GetType().GetProperty("data").GetValue(result.Data, null), typeof(RadianTestSetResult));
        }

        [TestMethod()]
        public void DeleteOperationMode_Result_Test()
        {
            _radianApprovedService.Setup(ras => ras.OperationDelete(It.IsAny<int>())).Returns(new ResponseMessage());
            JsonResult result = _radianApprovedController.DeleteOperationMode("1");

            Type typeOfDynamic = result.Data.GetType();
            Assert.IsTrue(typeOfDynamic.GetProperties().Where(p => p.Name.Equals("message")).Any()
                          && typeOfDynamic.GetProperties().Where(p => p.Name.Equals("success")).Any());
        }

        [TestMethod()]
        public void AutoCompleteProvider_Result_Test()
        {
            _radianApprovedService.Setup(ras => ras.AutoCompleteProvider(It.IsAny<int>(),
                                                                         It.IsAny<int>(),
                                                                         It.IsAny<RadianOperationModeTestSet>(),
                                                                         It.IsAny<string>()))
                .Returns(new List<RadianContributor>()
                {
                    new RadianContributor()
                    {
                        Id = 1,
                        Contributor = new Contributor() { BusinessName = string.Empty}
                    }
                });


            JsonResult result = _radianApprovedController.AutoCompleteProvider(1, 1, RadianOperationModeTestSet.OwnSoftware, string.Empty) as JsonResult;
            Assert.IsInstanceOfType(result.Data, typeof(List<AutoListModel>));
        }

        [TestMethod()]
        public void SoftwareList_Result_Test()
        {
            _radianApprovedService.Setup(ras => ras.SoftwareList(It.IsAny<int>(), It.IsAny<RadianSoftwareStatus>()))
                .Returns(new List<RadianSoftware>()
                {
                    new RadianSoftware()
                    {
                        Id = Guid.NewGuid(),
                        Name = string.Empty
                    }
                });

            JsonResult result = (JsonResult)_radianApprovedController.SoftwareList(1);
            Assert.IsInstanceOfType(result.Data, typeof(List<AutoListModel>));
        }

        [TestMethod()]
        public void CustomersList_Result_Test()
        {
            _radianApprovedService.Setup(ras => ras.CustormerList(It.IsAny<int>(),
                                                                  It.IsAny<string>(),
                                                                  It.IsAny<RadianState>(),
                                                                  It.IsAny<int>(),
                                                                  It.IsAny<int>()))
                .Returns(new PagedResult<RadianCustomerList>()
                {
                    Results = new List<RadianCustomerList>()
                    {
                        new RadianCustomerList()
                        {
                            BussinessName = string.Empty,
                            Nit = string.Empty,
                            RadianState = string.Empty,
                            Page = 1,
                            Length = 1
                        }
                    }
                });

            JsonResult result = _radianApprovedController.CustomersList(1, string.Empty, RadianState.Habilitado, 1, 1) as JsonResult;
            Assert.IsInstanceOfType(result.Data, typeof(RadianApprovedViewModel));
        }

        [TestMethod()]
        public void FileHistoyList_Result_Test()
        {
            _radianApprovedService.Setup(ras => ras.FileHistoryFilter(It.IsAny<int>(),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<int>(),
                                                                      It.IsAny<int>()))
                .Returns(new PagedResult<RadianContributorFileHistory>()
                {
                    RowCount = 1,
                    Results = new List<RadianContributorFileHistory>()
                    {
                        new RadianContributorFileHistory()
                        {
                            FileName = string.Empty,
                            Comments = string.Empty,
                            CreatedBy = string.Empty,
                            RadianContributorFileStatus = new RadianContributorFileStatus() { Name = string.Empty },
                            Timestamp = DateTime.Now
                        }
                    }
                });

            JsonResult result = _radianApprovedController.FileHistoyList(new FileHistoryFilterViewModel() { Page = 1 }) as JsonResult;
            Assert.IsInstanceOfType(result.Data, typeof(FileHistoryListViewModel));
        }
    }
}