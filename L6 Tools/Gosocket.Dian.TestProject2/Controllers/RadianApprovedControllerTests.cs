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
using System.Web;
using System.Web.Mvc;

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
                        RadianState = "Cancelado",
                        RadianContributorTypeId = 1,
                    },
                    LegalRepresentativeIds = new List<string>() { "00003BD4-3F33-4EEE-9205-6C01DD8EAE95" }
                });

            _radianApprovedService.Setup(ras => ras.ContributorFileTypeList(It.IsAny<int>())).Returns(new List<RadianContributorFileType>());

            ViewResult result = _radianApprovedController.Index(registrationData) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }


        [TestMethod()]
        public void Index_View_Contributor()
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
        [DataRow(1, "1")]
        public void GetSetTestResult_View_Test(int radianContributorId, string nit)
        {
            RadianApprovedViewModel radianApprovedViewModel = new RadianApprovedViewModel()
            {
                Contributor = new RedianContributorWithTypes() { RadianContributorId = radianContributorId },
                Nit = nit,
            };

            ViewResult result = _radianApprovedController.GetSetTestResult(radianApprovedViewModel) as ViewResult;
            Assert.AreEqual("GetSetTestResult", result.ViewName);
        }


        [TestMethod()]
        [DataRow(1)]
        public void SetTestDetails_View_Test(string nit)
        {
            RadianApprovedViewModel radianApprovedViewModel = new RadianApprovedViewModel() { Nit = nit, };
            ViewResult result = _radianApprovedController.SetTestDetails(radianApprovedViewModel) as ViewResult;
            Assert.AreEqual("SetTestDetails", result.ViewName);
        }

        [TestMethod()]
        [DataRow(1, 1, "1", 1)]
        public void ViewTestSet_View_Test(int id, int radianTypeId, string softwareId, int softwareType)
        {
            ViewResult result = _radianApprovedController.ViewTestSet(id, radianTypeId, softwareId, softwareType) as ViewResult;
            Assert.AreEqual("GetSetTestResult", result.ViewName);
        }

        [TestMethod()]
        public void GetFactorOperationMode_View_Test()
        {
            RadianApprovedViewModel radianApprovedViewModel = new RadianApprovedViewModel();

            _radianApprovedService.Setup(ras => ras.ContributorSummary(It.IsAny<int>(), It.IsAny<int>())).Returns(new RadianAdmin());
            _radianApprovedService.Setup(ras => ras.SoftwareByContributor(It.IsAny<int>())).Returns(new Software());
            _radianTestSetService.Setup(rts => rts.OperationModeList(It.IsAny<Domain.Common.RadianOperationMode>())).Returns(new List<Domain.RadianOperationMode>());
            _radianApprovedService.Setup(ras => ras.ListRadianContributorOperations(It.IsAny<int>())).Returns(new RadianContributorOperationWithSoftware());

            ViewResult result = _radianApprovedController.SetTestDetails(radianApprovedViewModel) as ViewResult;
            Assert.AreEqual("GetFactorOperationMode", result.ViewName);
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(RadianApprovedOperationModeViewModel));
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
                { "contributorId", "" },
                { "radianContributorType", "" },
                { "radianOperationMode", "" },
                { "filesNumber", "2" }
            };

            moqContext.Setup(mc => mc.Request).Returns(moqRequest.Object);
            moqRequest.Setup(mr => mr.Form).Returns(formValues);

            _radianApprovedService.Setup(ras => ras.RadianContributorId(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(1);

            mockCollection.Setup(x => x.Count).Returns(0);

            ResponseMessage result = _radianApprovedController.UploadFiles().Data as ResponseMessage;
            Assert.IsTrue(result.Message.Equals("Datos actualizados correctamente."));
        }

        [TestMethod()]
        public void UpdateFactorOperationMode_Result_Test()
        {
            Mock<SetOperationViewModel> setOperationViewModel = new Mock<SetOperationViewModel>();
            setOperationViewModel.Setup(x => x.RadianContributorId).Returns(1);
            setOperationViewModel.Setup(x => x.SoftwareType).Returns(1);
            setOperationViewModel.Setup(x => x.SoftwareId).Returns(Guid.NewGuid().ToString());
            setOperationViewModel.Setup(x => x.Url).Returns("http:");
            setOperationViewModel.Setup(x => x.SoftwareName).Returns("SoftName");
            setOperationViewModel.Setup(x => x.Pin).Returns("pin");

            _radianApprovedService.Setup(ras => ras.GetTestResult(It.IsAny<string>())).Returns(new RadianTestSet());

            _radianApprovedService.Setup(ras => ras.AddRadianContributorOperation(It.IsAny<RadianContributorOperation>(),
                                                                                  It.IsAny<RadianSoftware>(),
                                                                                  It.IsAny<RadianTestSet>(),
                                                                                  true,
                                                                                  false)).Returns(new ResponseMessage(TextResources.SuccessSoftware,
                                                                                                                      TextResources.alertType));

            ResponseMessage result = _radianApprovedController.UpdateFactorOperationMode(setOperationViewModel.Object).Data as ResponseMessage;
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

            ViewResult result = _radianApprovedController.DeleteUser(1, 1, string.Empty, string.Empty) as ViewResult;
            string message = (string)result.ViewData.Model.GetType().GetProperty("message").GetValue(result.ViewData.Model, null);

            Assert.AreEqual("Datos actualizados", message);
        }
    }
}