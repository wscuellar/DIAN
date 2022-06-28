using System.IO;
using Gosocket.Dian.Services.ServicesGroup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Gosocket.Dian.TestProject.WebServices
{
    [TestClass]
    public class AllTest
    {
        private readonly DianPAServices service = new DianPAServices();
        LogicalNMService logicalService = new LogicalNMService();

        [TestMethod]
        public void GetExchangeEmails_Success_Test()
        {
            var authCode = "11111111";
            var response = service.GetExchangeEmails(authCode).Result;
            Assert.IsTrue(response.Success);
            Assert.AreEqual(response.StatusCode, "0");
            Assert.IsNotNull(response.CsvBase64Bytes);
        }

        [TestMethod]
        public void GetExchangeEmails_Fail_Test()
        {
            var authCode = "0001000010001";
            var response = service.GetExchangeEmails(authCode).Result;
            Assert.IsFalse(response.Success);
            Assert.AreEqual(response.StatusCode, "89");
            Assert.IsNull(response.CsvBase64Bytes);
        }

        [TestMethod]
        public void GetStatus_Success_Test()
        {
            var trackId = "424e7dc7df2719f6923d402365d467fc1abf7507995e5c0a2a036cbb86db530cc3ebae05a963e4dff1335605e787ac91";
            var response = service.GetStatus(trackId).Result;
            Assert.IsTrue(response.IsValid);
            Assert.AreEqual("00", response.StatusCode);
            Assert.AreEqual("Procesado Correctamente.", response.StatusDescription);
            Assert.IsNotNull(response.XmlBase64Bytes);
        }

        [TestMethod]
        public void GetStatus_Fail_Test()
        {
            var trackId = "424e7dc7df2719f6923d402365d467fc1abf7507995e5c0a2a036cbb86db530cc3ebae05a963e4dff1335605e787ac911";
            var response = service.GetStatus(trackId).Result;
            Assert.IsFalse(response.IsValid);
            Assert.AreEqual("66", response.StatusCode);
            Assert.AreEqual("TrackId no existe en los registros de la DIAN.", response.StatusDescription);
            Assert.IsNull(response.XmlBase64Bytes);
        }

        [TestMethod]
        public void GetStatusEvent_Success_Test()
        {
            var trackId = "424e7dc7df2719f6923d402365d467fc1abf7507995e5c0a2a036cbb86db530cc3ebae05a963e4dff1335605e787ac91";
            var response = service.GetStatusEvent(trackId);
            Assert.IsTrue(response.IsValid);
            Assert.AreEqual("00", response.StatusCode);
            Assert.AreEqual("Procesado Correctamente.", response.StatusDescription);
            Assert.IsNotNull(response.XmlBase64Bytes);
        }

        [TestMethod]
        public void GetStatusEvent_Fail_Test()
        {
            var trackId = "424e7dc7df2719f6923d402365d467fc1abf7507995e5c0a2a036cbb86db530cc3ebae05a963e4dff1335605e787ac911";
            var response = service.GetStatusEvent(trackId);
            Assert.IsFalse(response.IsValid);
            Assert.AreEqual("66", response.StatusCode);
            Assert.AreEqual("TrackId no existe en los registros de la DIAN.", response.StatusDescription);
            Assert.IsNull(response.XmlBase64Bytes);
        }

        //[TestMethod]
        public void SendEventUpdateStatus_Acuse_Success_Test()
        {
            StreamReader file = new StreamReader(@".\WebServices\Files\1.Acuse-030.zip");
            byte[] bytes;
            using (var memstream = new MemoryStream())
            {
                file.BaseStream.CopyTo(memstream);
                bytes = memstream.ToArray();
            }

            file.Close();
            var response = service.SendEventUpdateStatus(bytes, "11111111").Result;
            Assert.IsTrue(response.IsValid);
            Assert.AreEqual("00", response.StatusCode);
            Assert.AreEqual("Procesado Correctamente.", response.StatusDescription);
            Assert.IsNotNull(response.XmlBase64Bytes);
        }

        [TestMethod]
        public void SendEventUpdateStatus_Acuse_Fail_Test()
        {
            StreamReader file = new StreamReader(@".\WebServices\Files\1.Acuse-030.zip");
            byte[] bytes;
            using (var memstream = new MemoryStream())
            {
                file.BaseStream.CopyTo(memstream);
                bytes = memstream.ToArray();
            }

            file.Close();
            var response = service.SendEventUpdateStatus(bytes, "11111111").Result;
            Assert.IsFalse(response.IsValid);
            Assert.AreNotEqual("00", response.StatusCode);
            Assert.IsNotNull(response.ErrorMessage);
        }

        //[TestMethod]
        //public void SendBillSync_Success_Test()
        //{
        //    var fileName = "";
        //    StreamReader file = new StreamReader(@"C:\Users\oabetancourt\Desktop\Files\2.Acuse.zip");
        //    byte[] bytes;
        //    using (var memstream = new MemoryStream())
        //    {
        //        file.BaseStream.CopyTo(memstream);
        //        bytes = memstream.ToArray();
        //    }

        //    file.Close();
        //    var response = service.UploadDocumentSync(fileName, bytes, "11111111");
        //    Assert.IsTrue(response.IsValid);
        //    Assert.AreEqual("00", response.StatusCode);
        //    Assert.AreEqual("Procesado Correctamente.", response.StatusDescription);
        //    Assert.IsNotNull(response.XmlBase64Bytes);
        //}

        [TestMethod]
        public void SendBillSync_Fail_Test()
        {
            var fileName = "";
            StreamReader file = new StreamReader(@".\WebServices\Files\1.DocImportacion.zip");
            byte[] bytes;
            using (var memstream = new MemoryStream())
            {
                file.BaseStream.CopyTo(memstream);
                bytes = memstream.ToArray();
            }

            file.Close();
            var response = service.UploadDocumentSync(fileName, bytes, "11111111").Result;
            Assert.IsFalse(response.IsValid);
            Assert.AreNotEqual("00", response.StatusCode);
            Assert.IsNotNull(response.ErrorMessage);
        }

        //public void SendNominaSync_Success_Test()
        //{
        //    StreamReader file = new StreamReader(@"C:\Users\oabetancourt\Desktop\Files\2.Acuse.zip");
        //    byte[] bytes;
        //    using (var memstream = new MemoryStream())
        //    {
        //        file.BaseStream.CopyTo(memstream);
        //        bytes = memstream.ToArray();
        //    }

        //    file.Close();
        //    var response = logicalService.SendNominaUpdateStatusAsync(bytes, "11111111");
        //    Assert.IsTrue(response.IsValid);
        //    Assert.AreEqual("00", response.StatusCode);
        //    Assert.AreEqual("Procesado Correctamente.", response.StatusDescription);
        //    Assert.IsNotNull(response.XmlBase64Bytes);
        //}

        //public void SendNominaSync_Fail_Test()
        //{
        //    StreamReader file = new StreamReader(@"C:\Users\oabetancourt\Desktop\Files\2.Acuse.zip");
        //    byte[] bytes;
        //    using (var memstream = new MemoryStream())
        //    {
        //        file.BaseStream.CopyTo(memstream);
        //        bytes = memstream.ToArray();
        //    }

        //    file.Close();
        //    var response = logicalService.SendNominaUpdateStatusAsync(bytes, "11111111");
        //    Assert.IsFalse(response.IsValid);
        //    Assert.AreNotEqual("00", response.StatusCode);
        //    Assert.IsNotNull(response.ErrorMessage);
        //}




        //[TestMethod]
        //public void TestSuccessGetStatusZip()
        //{
        //    var trackId = "1b64ba89-5dde-4a64-8b49-558d267fc6a9";
        //    var responses = service.GetBatchStatus(trackId);
        //    var response = responses.FirstOrDefault();
        //    Assert.IsTrue(response.IsValid);
        //    Assert.AreEqual(response.StatusCode, "00");
        //    Assert.AreEqual(response.StatusDescription, "Procesado Correctamente.");
        //    Assert.IsNotNull(response.XmlBase64Bytes);
        //    //if (response.XmlBase64Bytes == null)
        //    //    Assert.IsTrue(response.XmlBase64Bytes != null);
        //    //if (response.ZipBase64Bytes == null)
        //    //    Assert.IsTrue(response.XmlBase64Bytes != null);
        //}

        //[TestMethod]
        //public void TestSuccessTestSetGetStatusZip()
        //{
        //    var trackId = "eb4e16cd-928f-43ac-b91a-d145fc1f878f";
        //    var responses = service.GetBatchStatus(trackId);
        //    var response = responses.FirstOrDefault();
        //    Assert.IsTrue(response.IsValid);
        //    Assert.AreEqual(response.StatusCode, "00");
        //    Assert.AreEqual(response.StatusDescription, "Procesado Correctamente.");
        //    Assert.IsTrue(response.XmlBase64Bytes != null);
        //}

        //[TestMethod]
        //public void TestSuccessGetXmlByDocumentKey()
        //{
        //    var authCode = "900508908";
        //    var trackId = "e761520babc21a65d073b71ef0bde46ca1d149eb243eb6d32abb8f8e8495de58552e52ae8ccf398fad52fee673a5c077";
        //    var response = service.GetXmlByDocumentKey(trackId, authCode);
        //    Assert.AreEqual(response.Code, "100");
        //    Assert.AreEqual(response.Message, "Accion completada OK");
        //    Assert.IsNotNull(response.XmlBytesBase64);
        //}

        //[TestMethod]
        //public void TestNotFoundXmlGetXmlByDocumentKey()
        //{
        //    var authCode = "900508908";
        //    var trackId = "e761520babc21a65d073b71ef0bde46ca1d149eb243eb6d32abb8f8e8495de58552e52ae8ccf398fad52fee673a5c07";
        //    var response = service.GetXmlByDocumentKey(trackId, authCode);
        //    Assert.AreEqual(response.Code, "404");
        //    Assert.IsNull(response.XmlBytesBase64);
        //}

        //[TestMethod]
        //public void TestNotAuthorizedGetXmlByDocumentKey()
        //{
        //    var authCode = "900508908";
        //    var trackId = "3c2d78f97d024f5e803e6ad3eb491bb6ac7c79922b819304028f44d9cf3de98a3bc68aa357eb8e82b8b733c874cf5bfc";
        //    var response = service.GetXmlByDocumentKey(trackId, authCode);
        //    Assert.AreEqual(response.Code, "401");
        //    Assert.IsNull(response.XmlBytesBase64);
        //}
    }
}