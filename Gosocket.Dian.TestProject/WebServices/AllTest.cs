using Gosocket.Dian.Services.ServicesGroup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Gosocket.Dian.TestProject.WebServices
{
    [TestClass]
    public class AllTest
    {
        private readonly DianPAServices service = new DianPAServices();

        [TestMethod]
        public void TestSuccessGetExchangeEmails()
        {
            var authCode = "9005089089";
            var response = service.GetExchangeEmails(authCode);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.StatusCode, "0");
            Assert.IsNotNull(response.CsvBase64Bytes);
        }

        [TestMethod]
        public void TestFailGetExchangeEmails()
        {
            var authCode = "90050890111";
            var response = service.GetExchangeEmails(authCode);
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.StatusCode, "89");
            Assert.IsNull(response.CsvBase64Bytes);
        }

        [TestMethod]
        public void TestNotificationGetStatus()
        {
            var trackId = "1d168afc3b628bfdbdfee5e3c5013df1475b3f666f90f5c40dcbccbb82d004b86be1000b7e8741f2d78c0333fc34a235";
            var response = service.GetStatus(trackId);
            Assert.IsTrue(response.IsValid);
            Assert.IsTrue(response.ErrorMessage.Any());
            Assert.AreEqual(response.StatusCode, "00");
            Assert.AreEqual(response.StatusDescription, "Procesado Correctamente.");
            Assert.IsNotNull(response.XmlBase64Bytes);
        }

        [TestMethod]
        public void TestSuccessGetStatus()
        {
            var trackId = "50bd9d92538a5fc71900d82a51fa49682475b9d1a6fd1ba5bb80fcb7066db97b1ff2bd98594fbd82c90808eb0cc15d76";
            var response = service.GetStatus(trackId);
            Assert.IsTrue(response.IsValid);
            Assert.AreEqual(response.StatusCode, "00");
            Assert.AreEqual(response.StatusDescription, "Procesado Correctamente.");
            Assert.IsNotNull(response.XmlBase64Bytes);
        }

        [TestMethod]
        public void TestSuccessGetStatusZip()
        {
            var trackId = "1b64ba89-5dde-4a64-8b49-558d267fc6a9";
            var responses = service.GetBatchStatus(trackId);
            var response = responses.FirstOrDefault();
            Assert.IsTrue(response.IsValid);
            Assert.AreEqual(response.StatusCode, "00");
            Assert.AreEqual(response.StatusDescription, "Procesado Correctamente.");
            Assert.IsNotNull(response.XmlBase64Bytes);
            //if (response.XmlBase64Bytes == null)
            //    Assert.IsTrue(response.XmlBase64Bytes != null);
            //if (response.ZipBase64Bytes == null)
            //    Assert.IsTrue(response.XmlBase64Bytes != null);
        }

        [TestMethod]
        public void TestSuccessTestSetGetStatusZip()
        {
            var trackId = "eb4e16cd-928f-43ac-b91a-d145fc1f878f";
            var responses = service.GetBatchStatus(trackId);
            var response = responses.FirstOrDefault();
            Assert.IsTrue(response.IsValid);
            Assert.AreEqual(response.StatusCode, "00");
            Assert.AreEqual(response.StatusDescription, "Procesado Correctamente.");
            Assert.IsTrue(response.XmlBase64Bytes != null);
        }

        [TestMethod]
        public void TestNotFoundTrackIdGetStatus()
        {
            var trackId = "1";
            var response = service.GetStatus(trackId);
            Assert.IsFalse(response.IsValid);
            Assert.AreEqual(response.StatusCode, "66");
            Assert.AreEqual(response.StatusDescription, "TrackId no existe en los registros de la DIAN.");
            Assert.IsNull(response.XmlBase64Bytes);
        }

        [TestMethod]
        public void TestSuccessGetXmlByDocumentKey()
        {
            var authCode = "900508908";
            var trackId = "e761520babc21a65d073b71ef0bde46ca1d149eb243eb6d32abb8f8e8495de58552e52ae8ccf398fad52fee673a5c077";
            var response = service.GetXmlByDocumentKey(trackId, authCode);
            Assert.AreEqual(response.Code, "100");
            Assert.AreEqual(response.Message, "Accion completada OK");
            Assert.IsNotNull(response.XmlBytesBase64);
        }

        [TestMethod]
        public void TestNotFoundXmlGetXmlByDocumentKey()
        {
            var authCode = "900508908";
            var trackId = "e761520babc21a65d073b71ef0bde46ca1d149eb243eb6d32abb8f8e8495de58552e52ae8ccf398fad52fee673a5c07";
            var response = service.GetXmlByDocumentKey(trackId, authCode);
            Assert.AreEqual(response.Code, "404");
            Assert.IsNull(response.XmlBytesBase64);
        }

        [TestMethod]
        public void TestNotAuthorizedGetXmlByDocumentKey()
        {
            var authCode = "900508908";
            var trackId = "3c2d78f97d024f5e803e6ad3eb491bb6ac7c79922b819304028f44d9cf3de98a3bc68aa357eb8e82b8b733c874cf5bfc";
            var response = service.GetXmlByDocumentKey(trackId, authCode);
            Assert.AreEqual(response.Code, "401");
            Assert.IsNull(response.XmlBytesBase64);
        }
    }
}