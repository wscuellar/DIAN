//using System;
//using System.Diagnostics;
//using System.IO;
//using Gosocket.Dian.Services.ServicesGroup;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Gosocket.Dian.TestProject.Event
//{
//    [TestClass]
//    public class SendEventUpdateStatusTest
//    {
//        [TestMethod]
//        public void TestSendEventUpdateStatusMethod()
//        {
//            try
//            {
//                DianPAServices service = new DianPAServices();
//                var fileName = "AR_TEST_Wilmer-firmado-SHA384";
//                var allZipBytes = File.ReadAllBytes($@"D:\ValidarDianXmls\{fileName}.zip");
//                var response = service.SendEventUpdateStatus(allZipBytes);
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
