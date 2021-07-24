using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Application.Tests
{
    [TestClass()]
    public class QueryAssociatedEventsServiceTests
    {

        private QueryAssociatedEventsService _current;

        private readonly Mock<IGlobalDocValidationDocumentMetaService> _radianGlobalDocValidationDocumentMeta = new Mock<IGlobalDocValidationDocumentMetaService>();
        private readonly Mock<IGlobalDocValidatorDocumentService> _globalDocValidatorDocument = new Mock<IGlobalDocValidatorDocumentService>();
        private readonly Mock<IGlobalDocValidatorTrackingService> _globalDocValidatorTracking = new Mock<IGlobalDocValidatorTrackingService>();
        private readonly Mock<IGlobalDocPayrollService> _globalDocPayrollService = new Mock<IGlobalDocPayrollService>();
        private readonly Mock<IAssociateDocuments> _associateDocuments = new Mock<IAssociateDocuments>();


        [TestInitialize]
        public void QueryAssociatedEventsServiceTest()
        {

            _current = new QueryAssociatedEventsService(
                 _radianGlobalDocValidationDocumentMeta.Object,
             _globalDocValidatorDocument.Object,
             _globalDocValidatorTracking.Object,
             _globalDocPayrollService.Object,
             _associateDocuments.Object
                );
        }

        [TestMethod()]
        public void DocumentValidationTest()
        {
            //arrange
            _ = _radianGlobalDocValidationDocumentMeta.Setup(x => x.DocumentValidation(It.IsAny<string>()))
            .Returns(new GlobalDocValidatorDocumentMeta());
            //act
            var Result = _current.DocumentValidation(It.IsAny<string>());

            //assert
            Assert.IsNotNull(Result);
            Assert.IsInstanceOfType(Result, typeof(GlobalDocValidatorDocumentMeta));
        }

        [TestMethod()]
        public void EventVerificationTest()
        {
            //arrange
            _ = _globalDocValidatorDocument.Setup(t => t.EventVerification(It.IsAny<GlobalDocValidatorDocumentMeta>())).Returns(new GlobalDocValidatorDocument());
            //act
            var result = _current.EventVerification(It.IsAny<GlobalDocValidatorDocumentMeta>());

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocument));
        }

        [TestMethod()]
        public void ReferenceAttorneysTest()
        {
            //arrange
            _ = _radianGlobalDocValidationDocumentMeta.Setup(t => t.ReferenceAttorneys(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<GlobalDocReferenceAttorney>());
            //act
            var result = _current.ReferenceAttorneys(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<GlobalDocReferenceAttorney>));
        }

        [TestMethod()]
        public void OtherEventsTest()
        {
            //arrange
            _ = _radianGlobalDocValidationDocumentMeta.Setup(t => t.GetAssociatedDocuments(It.IsAny<string>(), It.IsAny<string>())).Returns(new List<GlobalDocValidatorDocumentMeta>());


            //act
            var result = _current.OtherEvents(It.IsAny<string>(), It.IsAny<EventStatus>());
            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<GlobalDocValidatorDocumentMeta>));
        }

        [TestMethod()]
        [DataRow("030", DisplayName = "Documento tipo 30")]
        [DataRow("032", DisplayName = "Documento tipo 32")]
        [DataRow("033", DisplayName = "Documento tipo 33")]
        [DataRow("036", DisplayName = "Documento tipo 36")]
        public void EventTitleTest(string type)
        {
            //arrange
            string CustomizationID = "1";
            string result;
            EventStatus statusData;
            switch (type)
            {
                case "030":
                    //act
                    statusData = EventStatus.Received;

                    result = _current.EventTitle(statusData, CustomizationID, type,null);
                    Assert.IsNotNull(result);
                    break;
                case "032":
                    //act
                    statusData = EventStatus.Receipt;
                    result = _current.EventTitle(statusData, CustomizationID, type, null);
                    //assert
                    Assert.IsNotNull(result);
                    break;
                case "033":
                    statusData = EventStatus.Accepted;
                    result = _current.EventTitle(statusData, CustomizationID, type, null);
                    Assert.IsNotNull(result);
                    break;
                case "036":

                    //act
                    CustomizationID = "361";
                    statusData = EventStatus.SolicitudDisponibilizacion;
                    result = _current.EventTitle(statusData, CustomizationID, type, null);
                    //assert
                    Assert.IsNotNull(result, $"Fallo en documento {type} con custom {CustomizationID}");

                    //act
                    CustomizationID = "362";
                    statusData = EventStatus.SolicitudDisponibilizacion;
                    result = _current.EventTitle(statusData, CustomizationID, type, null);
                    //assert
                    Assert.IsNotNull(result, $"Fallo en documento {type} con custom {CustomizationID}");

                    //act
                    CustomizationID = "364";
                    statusData = EventStatus.SolicitudDisponibilizacion;
                    result = _current.EventTitle(statusData, CustomizationID, type, null);
                    //assert
                    Assert.IsNotNull(result, $"Fallo en documento {type} con custom {CustomizationID}");

                    //act
                    CustomizationID = "363";
                    statusData = EventStatus.SolicitudDisponibilizacion;
                    result = _current.EventTitle(statusData, CustomizationID, type, null);
                    //assert
                    Assert.IsNotNull(result, $"Fallo en documento {type} con custom {CustomizationID}");
                    break;
                default:
                    //act
                    statusData = EventStatus.Received;
                    result = _current.EventTitle(statusData, CustomizationID, type, null);
                    //assert
                    Assert.IsNotNull(result);
                    break;
            }
            Assert.IsInstanceOfType(result, typeof(string));

        }

        [TestMethod()]
        public void IsVerificatedTest()
        {
            _ = _globalDocValidatorDocument.Setup(t => t.EventVerification(It.IsAny<GlobalDocValidatorDocumentMeta>())).Returns(new GlobalDocValidatorDocument());

            var result = _current.IsVerificated(new GlobalDocValidatorDocumentMeta() { EventCode = "" });
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
        }

        [TestMethod()]
        public void ListTrackingTest()
        {
            _ = _globalDocValidatorTracking.Setup(t => t.ListTracking(It.IsAny<string>())).Returns(new List<GlobalDocValidatorTracking>());


            var result = _current.ListTracking("");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<GlobalDocValidatorTracking>));
        }

        [TestMethod()]
        [DataRow("030", DisplayName = "Documento tipo endosos")]
        [DataRow("032", DisplayName = "Documento tipo mandatos")]
        [DataRow("033", DisplayName = "Documento tipo limitaciones")]
        [DataRow("000", DisplayName = "Documento tipo default")]
        public void IdentifyEventTest(string type)
        {
            EventStatus result;

            result = _current.IdentifyEvent(new GlobalDocValidatorDocumentMeta() { EventCode = type });

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(EventStatus));
        }

        [TestMethod()]
        public void IconTypeTest()
        {
            var lst = new List<GlobalDocValidatorDocumentMeta>
            {
                new GlobalDocValidatorDocumentMeta(){ EventCode="030",Timestamp = DateTimeOffset.Now},
                new GlobalDocValidatorDocumentMeta(){ EventCode="032",Timestamp = DateTimeOffset.Now},
                new GlobalDocValidatorDocumentMeta(){ EventCode="033",Timestamp = DateTimeOffset.Now},
                new GlobalDocValidatorDocumentMeta(){ EventCode="036",Timestamp = DateTimeOffset.Now},
                new GlobalDocValidatorDocumentMeta(){ EventCode="037",Timestamp = DateTimeOffset.Now}
            };
            var result = _current.IconType(lst);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Dictionary<int, string>));

        }

        [TestMethod()]
        public void CreditAndDebitNotesTest()
        {
            //arrage
            List<GlobalDocValidatorDocumentMeta> allReferencedDocuments = new List<GlobalDocValidatorDocumentMeta>();
            allReferencedDocuments.Add(new GlobalDocValidatorDocumentMeta() { DocumentTypeId = "91", EmissionDate = DateTime.Now });
            allReferencedDocuments.Add(new GlobalDocValidatorDocumentMeta() { DocumentTypeId = "11", EmissionDate = DateTime.Now });
            allReferencedDocuments.Add(new GlobalDocValidatorDocumentMeta() { DocumentTypeId = "92", EmissionDate = DateTime.Now });
            _ = _globalDocValidatorDocument.Setup(t => t.FindByGlobalDocumentId(It.IsAny<string>())).Returns(new GlobalDocValidatorDocument());
            //act
            var data = _current.CreditAndDebitNotes(allReferencedDocuments);
            //assert
            Assert.IsNotNull(data);
            Assert.IsInstanceOfType(data, typeof(List<GlobalDocValidatorDocumentMeta>));
        }

        [TestMethod()]
        public void FindAllNotesTest()
        {
            //arrage
            List<GlobalDocValidatorDocumentMeta> allReferencedDocuments = new List<GlobalDocValidatorDocumentMeta>();
            allReferencedDocuments.Add(new GlobalDocValidatorDocumentMeta() { DocumentTypeId = "91", EmissionDate = DateTime.Now });
            allReferencedDocuments.Add(new GlobalDocValidatorDocumentMeta() { DocumentTypeId = "11", EmissionDate = DateTime.Now });
            allReferencedDocuments.Add(new GlobalDocValidatorDocumentMeta() { DocumentTypeId = "92", EmissionDate = DateTime.Now });
            _ = _globalDocValidatorDocument.Setup(t => t.FindByGlobalDocumentId(It.IsAny<string>())).Returns(new GlobalDocValidatorDocument());
            //act
            var result = _current.FindAllNotes(allReferencedDocuments);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<GlobalDocValidatorDocumentMeta>));
        }

        [TestMethod()]
        public void GlobalDocValidatorDocumentByGlobalIdTest()
        {

            _ = _globalDocValidatorDocument.Setup(t => t.FindByGlobalDocumentId(It.IsAny<string>())).Returns(new GlobalDocValidatorDocument());

            var result = _current.GlobalDocValidatorDocumentByGlobalId(It.IsAny<string>());
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocValidatorDocument));
        }

        [TestMethod()]
        public void GetPayrollByIdTest()
        {
            _ = _globalDocPayrollService.Setup(t => t.Find(It.IsAny<string>())).Returns(new GlobalDocPayroll());
            var result = _current.GetPayrollById(It.IsAny<string>());
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(GlobalDocPayroll));
        }
    }
}