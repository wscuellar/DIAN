using Gosocket.Dian.Plugin.Functions.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.TestProject.Fucntions.Plugins
{
    [TestClass]
    public class AllTest
    {
        private readonly string trackIdInvoiceHab = "9f889609fb388066a27414c963c611ed2925feac11731409632dfc651df240df708f440f5bf45cc93e3a2343254f2929";
        private readonly string trackIdCreditNoteHab = "4deff84b1e6cdb40adc5a2e4b7cc1bc46a95c98ba056b17353a4f2b8502e07f055bc66007f638bc940afb03b9e3fd9ea";
        private readonly string trackIdDebitNoteHab = "401f93398adc2367b3a96d8845d070d2d2a5c0a20d520d6fd9397ebaa4c6f082a78e3ade5698c35be936966fcb17a974";

        #region Invoices
        [TestMethod]
        public async Task TestSuccessInvoiceCufeValidations()
        {
            var responses = await ValidatorEngine.Instance.StartCufeValidationAsync(trackIdInvoiceHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessInvoiceNitValidations()
        {
            var responses = await ValidatorEngine.Instance.StartNitValidationAsync(trackIdInvoiceHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessInvoiceNumberingRangeValidations()
        {
            var responses = await ValidatorEngine.Instance.StartNumberingRangeValidationAsync(trackIdInvoiceHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessInvoiceSignatureValidations()
        {
            var responses = await ValidatorEngine.Instance.StartSignValidationAsync(trackIdInvoiceHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessInvoiceSoftwareValidations()
        {
            var responses = await ValidatorEngine.Instance.StartSoftwareValidationAsync(trackIdInvoiceHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessInvoiceTaxLevelCodesValidations()
        {
            var responses = await ValidatorEngine.Instance.StartTaxLevelCodesValidationAsync(trackIdInvoiceHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }
        #endregion

        #region Notes

        #region Credit notes
        [TestMethod]
        public void TestSuccessCreditNoteReferenceValidations()
        {
            var responses = ValidatorEngine.Instance.StartNoteReferenceValidation(trackIdCreditNoteHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessCreditNoteCudeValidations()
        {
            var responses = await ValidatorEngine.Instance.StartCufeValidationAsync(trackIdCreditNoteHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessCreditNoteSignatureValidations()
        {
            var responses = await ValidatorEngine.Instance.StartSignValidationAsync(trackIdCreditNoteHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessCreditNoteSoftwareValidations()
        {
            var responses = await ValidatorEngine.Instance.StartSoftwareValidationAsync(trackIdCreditNoteHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }
        #endregion


        #region Debit note
        [TestMethod]
        public void TestSuccessDebitNoteReferenceValidations()
        {
            var responses = ValidatorEngine.Instance.StartNoteReferenceValidation(trackIdDebitNoteHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessDebitNoteCudeValidations()
        {
            var responses = await ValidatorEngine.Instance.StartCufeValidationAsync(trackIdCreditNoteHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessDebitNoteSignatureValidations()
        {
            var responses = await ValidatorEngine.Instance.StartSignValidationAsync(trackIdDebitNoteHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }

        [TestMethod]
        public async Task TestSuccessDebitNoteSoftwareValidations()
        {
            var responses = await ValidatorEngine.Instance.StartSoftwareValidationAsync(trackIdDebitNoteHab);
            Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
        }
        #endregion

        #endregion
    }
}
