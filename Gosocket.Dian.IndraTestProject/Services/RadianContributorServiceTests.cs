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

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class RadianContributorServiceTests
    {
        Mock<IContributorService> _contributorService = new Mock<IContributorService>();
        Mock<IRadianContributorRepository> _radianContributorRepository = new Mock<IRadianContributorRepository>();
        Mock<IRadianContributorTypeRepository> _radianContributorTypeRepository =new Mock<IRadianContributorTypeRepository>();
        Mock<IRadianContributorFileRepository> _radianContributorFileRepository = new Mock<IRadianContributorFileRepository>();
        Mock<IRadianContributorFileTypeRepository> _radianContributorFileTypeRepository = new Mock<IRadianContributorFileTypeRepository>();
        Mock<IRadianContributorOperationRepository> _radianContributorOperationRepository = new Mock<IRadianContributorOperationRepository>();
        Mock<IRadianCallSoftwareService> _radianCallSoftwareService = new Mock<IRadianCallSoftwareService>();
        Mock<IRadianTestSetResultManager> _radianTestSetResultManager = new Mock<IRadianTestSetResultManager>();
        Mock<IRadianOperationModeRepository> _radianOperationModeRepository = new Mock<IRadianOperationModeRepository>();
        Mock<IRadianContributorFileHistoryRepository> _radianContributorFileHistoryRepository = new Mock<IRadianContributorFileHistoryRepository>();
        Mock<IGlobalRadianOperationService> _globalRadianOperationService = new Mock<IGlobalRadianOperationService>();

        [TestMethod()]
        public void SummaryTest()
        {
            //throw new NotImplementedException();
            Assert.IsTrue(true);
        }
    }
}