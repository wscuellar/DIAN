//using System;
//using System.Diagnostics;
//using System.IO;
//using Gosocket.Dian.Services.ServicesGroup;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Gosocket.Dian.TestProject.SendBill
//{
//    [TestClass]
//    public class SendBillTest
//    {
//        [TestMethod]
//        public void TestSendBillMethod()
//        {
//            try
//            {
//                DianPAServices service = new DianPAServices();
//                var fileName = "filename";
//                var allZipBytes = File.ReadAllBytes($@"D:\ValidarDianXmls\{fileName}.zip");
//                var response = service.UploadDocumentSync(fileName, allZipBytes, "9010660541");
//                Console.ReadKey();
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex.Message);
//                Console.ReadKey();
//            }
//        }

//        [TestMethod]
//        public void TestSendBillASyncMethod()
//        {
//            try
//            {
//                DianPAServices service = new DianPAServices();
//                var fileName = "lote6XMl";
//                var allZipBytes = File.ReadAllBytes($@"D:\ValidarDianXmls\{fileName}.zip");
//                var response = service.UploadMultipleDocumentAsync(fileName, allZipBytes, "9010660541");
//                Console.ReadKey();
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex.Message);
//                Console.ReadKey();
//            }
//        }
//    }
//}
