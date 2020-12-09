//using Gosocket.Dian.Domain.KeyVault;
//using Gosocket.Dian.Infrastructure;
//using Gosocket.Dian.Services.Utils.Helpers;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.IO;

//namespace Gosocket.Dian.TestProject.KeyVault
//{
//    [TestClass]
//    public class KeyVaultTest
//    {
//        [TestMethod]
//        public void ExportCertificateTest()
//        {
//            var request = new { Name = "PersonaJuridica" };
//            var response = ApiHelpers.ExecuteRequest<ExportCertificatResult>(ConfigurationManager.GetValue("ExportCertificateUrl"), request);
//            var bytes  = Convert.FromBase64String(response.Base64Data);
//            File.WriteAllBytes($@"D:\ValidarDianXmls\PersonaJuridica.pfx", bytes);
//            Assert.IsTrue(response.Success);
//        }
//    }
//}
