using Gosocket.Dian.Application;
using Gosocket.Dian.Services.ServicesGroup;
using Gosocket.Dian.Services.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gosocket.Dian.TestProject
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestOseWCF()
        {
            //OseWCFLocal.WcfOseCustomerServicesClient client = new OseWCFLocal.WcfOseCustomerServicesClient();
            //client.ClientCredentials.UserName.UserName = "11111111111OseUser";
            //client.ClientCredentials.UserName.Password = "wkDYgBK2gio";

            //var allZipBytes = File.ReadAllBytes(@"D:/ValidarOseXmls/20556695548-01-F001-3611.zip");
            //var base64 = Convert.ToBase64String(allZipBytes);

            //var response = client.sendBill("20556695548-01-F001-3611.zip", allZipBytes);
            //Assert.AreEqual("DoWork Ok!", response);
        }
        [TestMethod]
        public void TestSendBillMethod()
        {
            try
            {
                DianPAServices service = new DianPAServices();
                var fileName = "SETP990001151";
                var allZipBytes = File.ReadAllBytes($@"D:\ValidarDianXmls\Junio 2019\{fileName}.zip");

                //var response = service.UploadMultipleDocumentAsync(fileName, allZipBytes, "900508908", "56796ae4-573b-4b13-b21b-4aeb35613073");
                var response = service.UploadDocumentSync(fileName, allZipBytes, "11111111");

                //var responses = service.UploadDocumentSync($"{fileName}.zip", allZipBytes);
                //var responses = service.UploadDocumentSync("XmlZip", allZipBytes, "11111111");

                //var xmlString = StringUtil.TransformToIso88591(Properties.Resources._800197268_01_DS32371.ToString());
                //var xmlBytes = StringUtil.ISO_8859_1.GetBytes(xmlString);
                //var multipleZipBytes = xmlBytes.CreateMultipleZip("800197268_01_DS32371.xml", xmlBytes, "20204844381-01-F787-00059096.pdf");

                //var xmlString = StringUtil.TransformToIso88591(Properties.Resources._800197268_01_DS32371.ToString());
                //var xmlBytes = StringUtil.ISO_8859_1.GetBytes(xmlString);

                //var xmlString1 = StringUtil.TransformToIso88591(Properties.Resources._20204844381_01_F787_00059096.ToString());
                //var xmlBytes1 = StringUtil.ISO_8859_1.GetBytes(xmlString1);

                //var multipleZipBytes = xmlBytes.CreateMultipleZip("20347100316-01-F114-934.xml", xmlBytes1, "20204844381-01-F787-00059096.xml");
                //var zipBytes = xmlBytes.CreateZip("_800197268_01_DS32371", "xml");

                // Test directly in service

                //var cdrBytes = service.SendBillAsync2("800197268_01_DS32371", zipBytes);
                //File.WriteAllBytes(@"D:\OSE\CDR\test_cdr.xml", cdrBytes);

                //PECustomerService customerService = new PECustomerService
                //{
                //    AuthUserValue = new AuthUser { UserName = userTest, Password = passTest }
                //};

                //var cdrBytes = customerService.SendBill("filenametest", zipBytes);

                //Assert.IsNotNull(cdrBytes);
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void TestSendTestSetAsyncMethod()
        {
            try
            {
                DianPAServices service = new DianPAServices();
                var filename = "XML _Dian";
                var allZipBytes = File.ReadAllBytes($@"D:\ValidarDianXmls\{filename}.zip");

                //WcfDianCustomerServices wcfDianCustomerServices = new WcfDianCustomerServices();
                //var response2 = wcfDianCustomerServices.SendTestSetAsync(filename, allZipBytes, filename);

                var response = service.UploadMultipleDocumentAsync(filename, allZipBytes, "985548530", "197ac707-a93f-42e2-b798-b565ee816ac3");

            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void TestGetStatusMethod()
        {
            try
            {
                var trackId = "8541bcc4beb56d4599a8c79cd5ac37f2b37ee3695da4b0769312f7570259ea04b43c969d82261c6dd4a1825bba67e923";

                DianPAServices service = new DianPAServices();
                var oseResponse = service.GetStatus(trackId);
                //var oseResponse = service.GetStatusZip(trackId);

                Assert.IsNotNull(oseResponse);
                //Assert.AreEqual(oseResponse.StatusCode, "98");
                //Assert.AreEqual(oseResponse.StatusDescription, "En Proceso");
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void TestUploadDocumentAttachmentMethod()
        {
            try
            {
                DianPAServices service = new DianPAServices();
                var allZipBytes = File.ReadAllBytes(@"D:\ValidarDianXmls\attacheddocument001.zip");
                var responses = service.UploadDocumentAttachmentAsync("DianXmlsALot50", allZipBytes);
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void TestSendEventUpdateStatusMethod()
        {
            try
            {
                DianPAServices service = new DianPAServices();
                var allZipBytes = File.ReadAllBytes(@"D:\ValidarDianXmls\ApplicationResponseTest.zip");
                var responses = service.SendEventUpdateStatus(allZipBytes);
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void TestGetNumberingRangeMethod()
        {
            try
            {

                //DianPAServices service = new DianPAServices();
                //var oseResponse = service.GetNumberingRange(accCode, docType);
                ////var oseResponse = service.GetStatusZip(trackId);

                //Assert.IsNotNull(oseResponse);
                //Assert.AreEqual(oseResponse.StatusCode, "98");
                //Assert.AreEqual(oseResponse.StatusDescription, "En Proceso");
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void TestGetXmlByDocumentKeyMethod()
        {
            try
            {
                DianPAServices service = new DianPAServices();
                var trackId = "f3be1a2f832c10564a18e5044e16891739f77631";
                var responses = service.GetXmlByDocumentKey(trackId, "f3be1a2f832c10564a18e5044e168ewdwkjednkw77653");
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void TestConvertBinaryToStringMethod()
        {
            try
            {
                var testString = File.ReadAllText(@"D:\ValidarDianXmls\ApplicationResponseTest.xml");

                var stringConverted = StringUtil.StringToBinary(testString);

                var stringInitial = StringUtil.BinaryToString(stringConverted);
            }
            catch (Exception)
            {

            }
        }

        [TestMethod]
        public void TestAddMandatoryFilesToProvider()
        {
            ContributorService contributorService = new ContributorService();

            var contributor = contributorService.Get(5230445);
            var mandatoryFiles = contributorService.GetMandatoryContributorFileTypes();
            var existFileIds = contributor.ContributorFiles.Select(f => f.FileType).ToList();
            var missingFiles = mandatoryFiles.Where(f => !f.Deleted && !existFileIds.Contains(f.Id));
            foreach (var item in missingFiles)
            {
                var contributorFile = new Domain.ContributorFile
                {
                    Id = Guid.NewGuid(),
                    FileName = item.Name,
                    Deleted = false,
                    Timestamp = DateTime.UtcNow,
                    CreatedBy = "",
                    FileType = item.Id,
                    Updated = DateTime.UtcNow,
                    Comments = null,
                    Status = 0,
                    ContributorId = contributor.Id
                };
                contributorService.AddOrUpdateContributorFile(contributorFile);
            }
        }

        [TestMethod]
        public void TestValidateNitDigit()
        {
            string nit = "900508908";
            int _dv = 9;
            string _numeroDocumentoString = nit.ToString();
            int[] _primos = new int[] { 0, 3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71 };
            int _digitoVerificacion, _primoActual, _totalOperacion = 0, _residuo, _cantidadDigitos = _numeroDocumentoString.Length;

            for (int i = 0; i < _cantidadDigitos; i++)
            {
                _primoActual = int.Parse(_numeroDocumentoString.Substring(i, 1));
                _totalOperacion += _primoActual * _primos[_cantidadDigitos - i];
            }
            _residuo = _totalOperacion % 11;
            if (_residuo > 1)
                _digitoVerificacion = 11 - _residuo;
            else
                _digitoVerificacion = _residuo;

            Assert.AreEqual(_dv, _digitoVerificacion);
        }

        private Dictionary<string, string> GetSubjectInfo(string subject)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            try
            {
                string[] subjectSplited = subject.Split(',');
                foreach (var item in subjectSplited)
                {
                    string[] itemSplit = item.Split('=');
                    result.Add(itemSplit[0].Trim(), itemSplit[1].Trim());
                }
            }
            catch { return result; }
            return result;
        }
    }
}
