using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Gosocket.Dian.Interfaces.Services;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class RadianApprovedControllerTests
    {
        private readonly RadianApprovedController _current;
        private readonly Mock<IRadianContributorService> _radianContributorService = new Mock<IRadianContributorService>();
        private readonly Mock<IRadianTestSetService> _radianTestSetService = new Mock<IRadianTestSetService>();
        private readonly Mock<IRadianApprovedService> _radianApprovedService = new Mock<IRadianApprovedService>();
        private readonly Mock<IRadianTestSetResultService> _radianTestSetResultService = new Mock<IRadianTestSetResultService>();

        public RadianApprovedControllerTests()
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



            ViewResult result = _radianApprovedController.Index(registrationData) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }


        [TestMethod()]
        [DataRow(1, 1)]
        public void Index_View_Contributor(int contributorId, Domain.Common.RadianContributorType radianContributorType)
        {
            RegistrationDataViewModel registrationData = new RegistrationDataViewModel
            {
                ContributorId = contributorId,
                RadianContributorType = radianContributorType
            };

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

       
    }
}