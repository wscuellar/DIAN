using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Specialized;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers.Tests
{
    [TestClass()]
    public class QueryAssociatedEventsControllerTests
    {

        QueryAssociatedEventsController _current;
        private readonly Mock<IQueryAssociatedEventsService> _queryAssociatedEventsService = new Mock<IQueryAssociatedEventsService>();
        private readonly Mock<IContributorService> _contributorService = new Mock<IContributorService>(); 
        private readonly Mock<IAssociateDocuments> _associateDocuments = new Mock<IAssociateDocuments>();

        [TestInitialize]
        public void TestInitialize()
        {
            _current = new QueryAssociatedEventsController(_queryAssociatedEventsService.Object, _contributorService.Object, _associateDocuments.Object);
        }
        

        [TestMethod()]
        public void IndexTest()
        {
            //arrange

            //act
            var result = _current.Index();

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [DataRow(EventStatus.Received, 1)]
        [DataRow(EventStatus.Mandato, 10)]
        [DataRow(EventStatus.EndosoGarantia, 0)]
        public void EventsViewTest(EventStatus eventStatus, int validationStatus)
        {
            //arrange
            string id = "1";
            string cufe = "";
            string eventCode = "0" + ((int)eventStatus).ToString();
            Mock<ControllerContext> controllercontext = new Mock<ControllerContext>();
            NameValueCollection headerCollection = new NameValueCollection
            {
                ["InjectingPartialView"] = "true"
            };
            controllercontext.Setup(frm => frm.HttpContext.Response.Headers).Returns(headerCollection);
            _current.ControllerContext = controllercontext.Object;
            _queryAssociatedEventsService.Setup(t => t.DocumentValidation(id)).Returns(new GlobalDocValidatorDocumentMeta() { EventCode = eventCode, DocumentReferencedKey = "invoice", SenderCode= "111" });
            _queryAssociatedEventsService.Setup(t => t.EventTitle(It.IsAny<EventStatus>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("test");
            _queryAssociatedEventsService.Setup(t => t.DocumentValidation(cufe)).Returns(new GlobalDocValidatorDocumentMeta());
            _queryAssociatedEventsService.Setup(t => t.EventVerification(It.IsAny<GlobalDocValidatorDocumentMeta>())).Returns(new Domain.Entity.GlobalDocValidatorDocument() { ValidationStatus = validationStatus });
            _queryAssociatedEventsService.Setup(t => t.DocumentValidation("invoice")).Returns(new GlobalDocValidatorDocumentMeta());
            _queryAssociatedEventsService.Setup(t => t.IdentifyEvent(It.IsAny<GlobalDocValidatorDocumentMeta>())).Returns(eventStatus);
            _queryAssociatedEventsService.Setup(t => t.OtherEvents(It.IsAny<string>(), It.IsAny<EventStatus>())).Returns(new System.Collections.Generic.List<GlobalDocValidatorDocumentMeta>() { new GlobalDocValidatorDocumentMeta() { EventCode = eventCode } });
            _queryAssociatedEventsService.Setup(t => t.IsVerificated(It.IsAny<GlobalDocValidatorDocumentMeta>())).Returns(true);

            if(validationStatus == 10)
            {
                _queryAssociatedEventsService.Setup(t => t.ListTracking(It.IsAny<string>())).Returns(new System.Collections.Generic.List<Domain.Entity.GlobalDocValidatorTracking>() { new Domain.Entity.GlobalDocValidatorTracking() });
            }

            if(eventStatus == EventStatus.Mandato)
            {
                _contributorService.Setup(t => t.GetByCode(It.IsAny<string>())).Returns(new Domain.Contributor() {  BusinessName= "test"});
                _queryAssociatedEventsService.Setup(t => t.ReferenceAttorneys(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new System.Collections.Generic.List<Domain.Entity.GlobalDocReferenceAttorney>() { new Domain.Entity.GlobalDocReferenceAttorney() { EffectiveDate = System.DateTime.Now.ToString()} });
            }

            //act
            PartialViewResult partialViewResult = _current.EventsView(id, cufe);

            //assert
            Assert.IsNotNull(partialViewResult.Model);
        }

    }
}