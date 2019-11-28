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
        public void TestNotificationGetStatus()
        {
            var trackId = "1d168afc3b628bfdbdfee5e3c5013df1475b3f666f90f5c40dcbccbb82d004b86be1000b7e8741f2d78c0333fc34a235";
            var response = service.GetStatus(trackId);
            Assert.IsTrue(response.IsValid);
            Assert.IsTrue(response.ErrorMessage.Any());
            Assert.AreEqual(response.StatusCode, "00");
            Assert.AreEqual(response.StatusDescription, "Procesado Correctamente.");
            Assert.IsTrue(response.XmlBase64Bytes != null);
        }

        [TestMethod]
        public void TestSuccessGetStatus()
        {
            var trackId = "50bd9d92538a5fc71900d82a51fa49682475b9d1a6fd1ba5bb80fcb7066db97b1ff2bd98594fbd82c90808eb0cc15d76";
            var response = service.GetStatus(trackId);
            Assert.IsTrue(response.IsValid);
            Assert.AreEqual(response.StatusCode, "00");
            Assert.AreEqual(response.StatusDescription, "Procesado Correctamente.");
            Assert.IsTrue(response.XmlBase64Bytes != null);
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
            Assert.IsTrue(response.XmlBase64Bytes != null);
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
            Assert.IsTrue(response.XmlBase64Bytes == null);
        }
    }
}
