using Gosocket.Dian.Services.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Gosocket.Dian.Test
{
    [TestClass]
    public class ServicesTest
    {

        [TestMethod]
        public void TestGetStatus()
        {
            //try
            //{
            //    var trackId = "519aca1b-8ce2-4c26-8e6d-463f1a3ccf7a";

            //    OseInputValidator service = new OseInputValidator();
            //    var oseResponse = service.GetStatus(trackId);

            //    Assert.IsNotNull(oseResponse);
            //    Assert.AreEqual(oseResponse.StatusCode, "98");
            //    Assert.AreEqual(oseResponse.StatusDescription, "En Proceso");
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    Assert.IsTrue(false);
            //}
        }

        [TestMethod]
        public void TestSendBill()
        {
            try
            {
                var xmlString = StringUtil.TransformToIso88591(Properties.Resources._20347100316_01_F114_00000934.ToString());
                //var xmlBytes = StringUtil.ISO_8859_1.GetBytes(xmlString);

                //var zipBytes = xmlBytes.CreateZip("20100035121-01-F023-33170", "zip");

                // Test directly in service
                //DianPAServices service = new DianPAServices();
                //var cdrBytes = service.SendBill("20100035121-01-F023-33170", zipBytes);
                //File.WriteAllBytes(@"D:\OSE\CDR\test_cdr.xml", cdrBytes);

                //PECustomerService customerService = new PECustomerService
                //{
                //    AuthUserValue = new AuthUser { UserName = userTest, Password = passTest }
                //};

                //var cdrBytes = customerService.SendBill("filenametest", zipBytes);

                //Assert.IsNotNull(cdrBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.IsTrue(false);
            }
        }
    }
}
