//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Gosocket.Dian.Interfaces;
//using System.Runtime.InteropServices;
//using System.Web.Mvc;
//using System.Collections.Generic;
//using System.Net.Http.Headers;

//namespace Gosocket.Dian.Web.Controllers.Tests
//{
//    [TestClass()]
//    public class RadianControllerTests
//    {

//        private RadianController _current;
//        private readonly Mock<IRadianContributorService> _RadianContributorService = new Mock<IRadianContributorService>();
//        private readonly Mock<IContributorService> _ContributorService = new Mock<IContributorService>();

//        [TestInitialize]
//        public void TestInitialize()
//        {
//            _current = new RadianController(_ContributorService.Object, _RadianContributorService.Object);
//        }

        
//        [TestMethod()]
//        public void Index_WithoutUser_Test()
//        {
//            //preparacion

//            //ejecucion
//            var result = _current.Index() as ViewResult;

//            //Validacion
//            var r = result.ViewBag.ContributorId;
//            Assert.IsNull(r);
//        }


//        [TestMethod()]
//        public void Index_WithUser_Test()
//        {
//            //preparacion
//            _ContributorService.Setup(t => t.GetByCode(It.IsAny<string>())).Returns(new Domain.Contributor());

//            //ejecucion
//            var result = _current.Index() as ViewResult;

//            //Validacion
//            var r = result.ViewBag.ContributorId;
//            Assert.IsNotNull(r);
//        }

//        [TestMethod()]
//        public void ElectronicInvoiceView_Test()
//        {
//            //preparacion
//            _ContributorService.Setup(t => t.GetByCode(It.IsAny<string>())).Returns(new Domain.Contributor());

//            //ejecucion
//            var result = _current.Index() as ViewResult;

//            //Validacion
//            var r = result.ViewBag.ContributorId;
//            Assert.IsNotNull(r);
//        }


//        [TestMethod()]
//        public void Index_WithUserInRadian_Test()
//        {
//            //preparacion
//            //Domain.Contributor contributor = new Domain.Contributor();
//            //List<Domain.Contributor> contributors = new List<Domain.Contributor>();
//            //ICollection<Domain.Software> softwares = new List<Domain.Software>() { new Domain.Software() };
//            //contributor.Softwares = softwares;
//            //contributor.Softwares.Add(new Domain.Software());
//            //List<Domain.RadianContributor> radianContributors = new List<Domain.RadianContributor>()
//            //{
//            //    new Domain.RadianContributor(){ RadianContributorTypeId=(int)Domain.Common.RadianContributorType.ElectronicInvoice},
//            //    new Domain.RadianContributor(){ RadianContributorTypeId=(int)Domain.Common.RadianContributorType.Factor}
//            //};
//            //_ContributorService.Setup(t => t.GetByCode(It.IsAny<string>())).Returns(contributor);
//            //_RadianContributorService.Setup(t => t.List(x => x.ContributorId == contributor.Id   && x.RadianState != "Cancelado", 0, 0)).Returns(radianContributors);

//            ////ejecucion
//            //var result = _current.Index() as ViewResult;

//            ////Validacion
//            //var r = result.ViewBag.ContributorId;
//            //Assert.IsNotNull(r);
//        }




//    }
//}