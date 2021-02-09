using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Plugin.Functions.Common;
using Gosocket.Dian.Plugin.Functions.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.TestProject.Fucntions.Plugins
{
    [TestClass]
    public class AllTest
    {
        private readonly string trackIdInvoiceHab = "9f889609fb388066a27414c963c611ed2925feac11731409632dfc651df240df708f440f5bf45cc93e3a2343254f2929";
        private readonly string trackIdCreditNoteHab = "4deff84b1e6cdb40adc5a2e4b7cc1bc46a95c98ba056b17353a4f2b8502e07f055bc66007f638bc940afb03b9e3fd9ea";
        private readonly string trackIdDebitNoteHab = "3f4c2d167a90faca85393639bf5db72270e3600f089bdbaa7a22ace9d74563035a492234773f077042a2d0bdca419d90";
        private const string DefaultStatusRutValidationErrorMessage = "El facturador tiene el RUT en estado cancelado, suspendido o inactivo.";
        private List<ValidateListResponse> responses = new List<ValidateListResponse>();
        private GlobalContributor sender = null;
        private GlobalContributor sender2 = null;

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
            //Assert.IsTrue(responses.Count(r => !r.IsValid) == 0);
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

        [TestMethod]
        public void TestSuccessValidateDigitCodes()
        {
            var validator = new Validator();
            var isValid = validator.ValidateDigitCode("1832", 6);
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void GetStatusRutValidationWithSendersNull()
        {
            // Arrange
            var validator = new Validator();

            // Act
            responses = validator.GetStatusRutValidation(new List<ValidateListResponse>(), sender, sender2);

            // Assert
            Assert.IsTrue(responses.Count() == 0);
        }

        [TestMethod]
        public void GetStatusRutValidationWithStatusRutCancelledAndStatusRutNull()
        {
            // Arrange
            var validator = new Validator();
            sender = new GlobalContributor();

            // Act
            sender.StatusRut = (int)StatusRut.Cancelled;
            responses = validator.GetStatusRutValidation(new List<ValidateListResponse>(), sender, sender2);

            // Assert
            Assert.AreEqual(responses.LastOrDefault().ErrorMessage, DefaultStatusRutValidationErrorMessage);
        }

        [TestMethod]
        public void GetStatusRutValidationWithStatusRutInactiveAndStatusRutNull()
        {
            // Arrange
            var validator = new Validator();
            sender = new GlobalContributor();

            // Act
            sender.StatusRut = (int)StatusRut.Inactive;
            responses = validator.GetStatusRutValidation(new List<ValidateListResponse>(), sender, sender2);

            // Assert
            Assert.AreEqual(responses.LastOrDefault().ErrorMessage, DefaultStatusRutValidationErrorMessage);
        }

        [TestMethod]
        public void GetStatusRutValidationWithStatusRutSuspensionAndStatusRutNull()
        {
            // Arrange
            var validator = new Validator();
            sender = new GlobalContributor();

            // Act
            sender.StatusRut = (int)StatusRut.Suspension;
            responses = validator.GetStatusRutValidation(new List<ValidateListResponse>(), sender, sender2);
            
            // Assert
            Assert.AreEqual(responses.LastOrDefault().ErrorMessage, DefaultStatusRutValidationErrorMessage);
        }


        [TestMethod]
        public void GetStatusRutValidationWithStatusRutCancelledAndStatusRutInactive()
        {
            // Arrange
            var validator = new Validator();
            sender = new GlobalContributor();
            sender2 = new GlobalContributor();

            // Act
            sender.StatusRut = (int)StatusRut.Cancelled;
            sender2.StatusRut = (int)StatusRut.Inactive;
            responses = validator.GetStatusRutValidation(new List<ValidateListResponse>(), sender, sender2);

            // Assert
            Assert.IsTrue(responses.Count() == 2);
        }

        [TestMethod]
        public void GetStatusRutValidationWithStatusRutCancelledAndStatusRutSuspension()
        {
            // Arrange
            var validator = new Validator();
            sender = new GlobalContributor();
            sender2 = new GlobalContributor();

            // Act
            sender.StatusRut = (int)StatusRut.Cancelled;
            sender2.StatusRut = (int)StatusRut.Suspension;
            responses = validator.GetStatusRutValidation(new List<ValidateListResponse>(), sender, sender2);

            // Assert
            Assert.IsTrue(responses.Count() == 2);
        }

        [TestMethod]
        public void GetStatusRutValidationWithStatusRutInactiveAndStatusRutSuspension()
        {
            // Arrange
            var validator = new Validator();
            sender = new GlobalContributor();
            sender2 = new GlobalContributor();

            // Act
            sender.StatusRut = (int)StatusRut.Inactive;
            sender2.StatusRut = (int)StatusRut.Suspension;
            responses = validator.GetStatusRutValidation(new List<ValidateListResponse>(), sender, sender2);

            // Assert
            Assert.IsTrue(responses.Count() == 2);
        }
    }
}
