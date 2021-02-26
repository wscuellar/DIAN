using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Functions.Utils;
using Gosocket.Dian.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosocket.Dian.TestProject.Functions.Commons
{
    [TestClass]
    public class AllTest
    {
        private static readonly TableManager tableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");
        private static readonly TableManager tableManagerGlobalDocValidatorDocument = new TableManager("GlobalDocValidatorDocument");
        private static readonly TableManager tableManagerDocumentTracking = new TableManager("GlobalDocValidatorTracking");

        //[TestMethod]
        public void TestSuccessGetApplicationResponse()
        {
            var trackId = "eb24a315e134bbec7f863c8a7b5756c089a71f740e378cf50452c86696d58cff34efcd9ffa17d640cc85a6e6ddf93fc5";
            var documentMeta = tableManagerGlobalDocValidatorDocumentMeta.Find<GlobalDocValidatorDocumentMeta>(trackId, trackId);
            var validations = tableManagerDocumentTracking.FindByPartition<GlobalDocValidatorTracking>(trackId);

            var xmlBytes = XmlUtil.GenerateApplicationResponseBytes(trackId, documentMeta, validations, null);
            Assert.IsTrue(xmlBytes != null);
        }
    }
}
