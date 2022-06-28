using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Gosocket.Dian.Domain.Common
{
    public static class EnumHelper
    {
        public static string GetEnumDescription<T>(T value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi == null) return "";
            DescriptionAttribute[] attributes =
              (DescriptionAttribute[])fi.GetCustomAttributes
              (typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

                        if (descriptionAttribute != null)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return null; // could also return string.Empty
        }

        public static T GetValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            return default(T);
        }

    }

    public enum AuthType
    {
        [Description("Company")]
        Company = 1,
        [Description("Person")]
        Person = 2,
        [Description("Certificate")]
        Certificate = 3,
        [Description("ProfileUser")]
        ProfileUser = 4,
    }

    public enum BatchFileStatus
    {
        [Description("En proceso de validación")]
        InProcess = 0,
        [Description("Aprobado")]
        Accepted = 1,
        [Description("Aprobado con notificación")]
        Notification = 10,
        [Description("Rechazado")]
        Rejected = 2,
    }

    public enum BigContributorAuthorizationStatus
    {
        [Description("Sin registrar")]
        Unregistered = 1,
        [Description("Pendiente autorizar")]
        Pending = 2,
        [Description("Autorizado")]
        Authorized = 3,
        [Description("Rechazado")]
        Rejected = 4
    }

    public enum BillerType
    {
        [Description("Voluntario")]
        Voluntary = 0,
        [Description("Obligado")]
        Obliged = 1,
        [Description("Indefinido")]
        None = 3
    }

    public enum ContributorStatus
    {
        [Description("Pendiente de registro")]
        Pending = 1,
        [Description("Registrado")]
        Registered = 3,
        [Description("Habilitado")]
        Enabled = 4,
        [Description("Cancelado")]
        Cancelled = 5,
        [Description("Set de prueba rechazado")]
        Rejected = 6,
    }

    public enum ContributorFileStatus
    {
        [Description("Pendiente")]
        Pending = 0,
        [Description("Cargado y en revisión")]
        Loaded = 1,
        [Description("Aprobado")]
        Approved = 2,
        [Description("Rechazado")]
        Rejected = 3,
        [Description("Obeservaciones")]
        Observation = 4,
    }

    public enum ContributorType
    {
        [Description("Cero")]
        Zero = 0,
        [Description("Facturador")]
        Biller = 1,
        [Description("Proveedor")]
        Provider = 2,
        [Description("Proveedor autorizado")]
        AuthorizedProvider = 3,
        [Description("No oligado a facturar")]
        BillerNoObliged = 4,
    }

    public enum DocumentType
    {
        [Description("Factura electrónica")]
        Invoice = 1,
        [Description("Factura electrónica de exportación")]
        ExportationInvoice = 2,
        [Description("Factura electrónica de contingencia")]
        ContingencyInvoice = 3,
        [Description("Factura electrónica de contingencia DIAN")]
        ContingencyDianInvoice = 4,
        [Description("Factura electrónica de AIU")]
        AIUInvoice = 9,
        [Description("Factura electrónica de Mandato")]
        MandateInvoice = 11,
        [Description("Documento de Importacion")]
        ImportDocumentInvoice = 101,
        [Description("Nomina Individual")]
        IndividualPayroll = 102,
        [Description("Nomina Individual de Ajustes")]
        IndividualPayrollAdjustments = 103,
        [Description("Nota de crédito electrónica")]
        CreditNote = 91,
        [Description("Nota de débito electrónica")]
        DebitNote = 92,
        [Description("Application response")]
        ApplicationResponse = 96,

        /*Documento Soporte*/
        [Description("Documento Soporte")]
        DocumentSupportInvoice = 5,
        [Description("Nota de Ajuste del Documento Soporte")]
        AdjustmentNoteDocumentSupport = 95,

        /*Documentos Equivalentes*/
        [Description("Documento equivalente POS")]
        EquivalentDocumentPOS = 20,
        [Description("Documento equivalente - Boleta ingreso a cine")]
        EquivalentDocumentCine = 25,
        [Description("Documento equivalente - Espectáculos públicos")]
        EquivalentDocumentEspectaculosPublicos = 27,
        [Description("Documento equivalente - Juegos localizados")]
        EquivalentDocumentJuegosLocalizados = 32,
        [Description("Documento equivalente - Transporte pasajeros terrestre")]
        EquivalentDocumentTransporteTerrestre = 35,
        [Description("Documento equivalente - Cobro de peajes")]
        EquivalentDocumentCobroPeaje = 40,
        [Description("Documento equivalente - Extractos")]
        EquivalentDocumentExtracto = 45,
        [Description("Documento equivalente - Transporte aéreo de pasajeros")]
        EquivalentDocumentTransporteAereo = 50,
        [Description("Documento equivalente - Bolsa de valores")]
        EquivalentDocumentBolsaValores = 55,
        [Description("Documento equivalente - Servicios públicos domiciliarios")]
        EquivalentDocumentServiciosPublicos = 60,
        [Description("Nota de ajuste del documento equivalente")]
        AdjustmentNoteEquivalentDocument = 94,
    }

    public static class OtherDocumentsDocumentType
    {
        public static bool IsSupportDocument(string documentTypeId)
        {
            documentTypeId = string.IsNullOrWhiteSpace(documentTypeId) ? "0" : documentTypeId;
            int documentType = Convert.ToInt32(documentTypeId);
            return new int[] {(int)DocumentType.DocumentSupportInvoice, (int)DocumentType.AdjustmentNoteDocumentSupport, }
                .Contains(documentType);
        }
        public static int[] GetAll()
        {
            return GetDocumentType().Concat(GetAdjustmentDocumentType()).ToArray();
        }
        public static int[] GetDocumentType()
        {
            return new int[] {
                (int)DocumentType.IndividualPayroll,

                (int)DocumentType.DocumentSupportInvoice,

                (int)DocumentType.EquivalentDocumentPOS,
                (int)DocumentType.EquivalentDocumentCine,
                (int)DocumentType.EquivalentDocumentEspectaculosPublicos,
                (int)DocumentType.EquivalentDocumentJuegosLocalizados,
                (int)DocumentType.EquivalentDocumentTransporteTerrestre,
                (int)DocumentType.EquivalentDocumentCobroPeaje,
                (int)DocumentType.EquivalentDocumentExtracto,
                (int)DocumentType.EquivalentDocumentTransporteAereo,
                (int)DocumentType.EquivalentDocumentBolsaValores,
                (int)DocumentType.EquivalentDocumentServiciosPublicos,
            };
        }
        public static int[] GetAdjustmentDocumentType()
        {
            return new int[] {
                (int)DocumentType.IndividualPayrollAdjustments,
                (int)DocumentType.AdjustmentNoteDocumentSupport,
                (int)DocumentType.AdjustmentNoteEquivalentDocument,
            };
        }
    }

    public enum DocumentStatus
    {
        [Description("Aprobado")]
        Approved = 1,
        [Description("Aprobado con notificación")]
        Notification = 10,
        [Description("Rechazado")]
        Rejected = 2,
    }

    public enum EventValidationMessage
    {
        [Description("Accion completada OK")]
        Success = 100,
        [Description("Pasados 8 días después de la recepción no es posible registrar eventos")]
        OutOffDate = 200,
        [Description("Evento registrado previamente")]
        PreviouslyRegistered = 201,
        [Description("No se puede rechazar un documento que ha sido aceptado previamente")]
        PreviouslyAccepted = 202,
        [Description("No se puede aceptar un documento que ha sido rechazado previamente")]
        PreviouslyRejected = 203,
        [Description("No se puede dar recepción de bienes a un documento que ha sido rechazado previamente")]
        ReceiptPreviouslyRejected = 204,
        [Description("No se puede ofrecer documento para negociación como título valor que ha sido rechazado previamente")]
        InvoiceOfferedForNegotiationPreviouslyRejected = 205,
        [Description("No se puede negociar documento como título valor que ha sido rechazado previamente")]
        NegotiatedInvoicePreviouslyRejected = 206,
        [Description("Evento '{0} {1}' no implementado")]
        NotImplemented = 222,
        [Description("Documento no econtrado en los regristros de la DIAN")]
        NotFound = 223,
        [Description("Código del evento es inválido")]
        InvalidResponseCode = 298,
        [Description("Error al registrar evento")]
        Error = 299,
        [Description("Error al registrar nomina")]
        ErrorNomina = 99,
    }

    public enum EventStatus
    {
        [Description("None")]
        None = 000,
        [Description("Acuse de recibo de la Factura Electrónica de Venta")]
        Received = 030,
        [Description("Reclamo de la Factura Electrónica de Venta")]
        Rejected = 031,
        [Description("Recibo del bien o prestación del servicio")]
        Receipt = 032,
        [Description("Aceptación expresa de la Factura Electrónica de Venta")]
        Accepted = 033,
        [Description("Aceptación tácita de la Factura Electrónica de Venta")]
        AceptacionTacita = 034,
        [Description("Aval")]
        Avales = 035,
        [Description("Inscripción en el RADIAN de la Factura Electrónica de Venta como Título Valor")]
        SolicitudDisponibilizacion = 036,
        [Description("Endoso electrónico en propiedad ")]
        EndosoPropiedad = 037,
        [Description("Endoso electrónico en garantía")]
        EndosoGarantia = 038,
        [Description("Endoso electrónico en procuración")]
        EndosoProcuracion = 039,
        [Description("Cancelación del endoso electrónico")]
        InvoiceOfferedForNegotiation = 040,
        [Description("Limitación para circulación de la Factura Electrónica de Venta como Título Valor")]
        NegotiatedInvoice = 041,
        [Description("Terminación de la limitación para circulación de la Factura Electrónica de Venta como Título Valor")]
        AnulacionLimitacionCirculacion = 042,
        [Description("Mandato")]
        Mandato = 043,
        [Description("Terminación del mandato")]
        TerminacionMandato = 044,
        [Description("Pago de la Factura Electrónica de Venta como Título Valor")]
        NotificacionPagoTotalParcial = 045,
        [Description("Informe para el pago de la Factura Electrónica de Venta como Título Valor")]
        ValInfoPago = 046,
        [Description("Endoso con efectos de cesión ordinaria")]
        EndorsementWithEffectOrdinaryAssignment = 047,
        [Description("Protesto")]
        Objection = 048,
        [Description("Transferencia de los derechos económicos")]
        TransferEconomicRights = 049,
        [Description("Notificación al deudor sobre la transferencia de los derechos económicos")]
        NotificationDebtorOfTransferEconomicRights = 050,
        [Description("Pago de la transferencia de los derechos económicos")]
        PaymentOfTransferEconomicRights = 051
    }

    public enum SubEventStatus
    {
        [Description("Endoso electrónico en propiedad con responsabilidad")]
        ConResponsabilidad = 371,
        [Description("Endoso electrónico en propiedad sin responsabilidad")]
        SinResponsabilidad = 372,

        [Description("Mandato general por documento general por tiempo limitado")]
        MandatoGenerarlLimitado = 431,
        [Description("Mandato general por documento general por tiempo ilimitado")]
        MandatoGenerarlIlimitado = 432,
        [Description("Mandato general por documento limitado por tiempo limitado")]
        MandatoGenerarlTiempo = 433,
        [Description("Mandato general por documento limitado por tiempo ilimitado")]
        MandatoGenerarlTiempoIlimitado = 434,


        [Description("Mandato especial por documento general por tiempo limitado")]
        MandatoGenerarlLimitadoEspecial = 4131,
        [Description("Mandato especial por documento general por tiempo ilimitado")]
        MandatoGenerarlIlimitadoEspecial = 4321,
        [Description("Mandato especial por documento limitado por tiempo limitado")]
        MandatoGenerarlTiempoEspecial = 4331,
        [Description("Mandato especial por documento limitado por tiempo ilimitado")]
        MandatoGenerarlTiempoIlimitadoEspecial = 4341,


        [Description("Primera inscripción de la Factura Electrónica de Venta como Título Valor en el RADIAN para negociación general")]
        PrimeraSolicitudDisponibilizacion = 361,
        [Description("Primera inscripción de la Factura Electrónica de Venta como Título Valor en el RADIAN para negociación directa previa")]
        SolicitudDisponibilizacionDirecta = 362,
        [Description("Inscripción posterior de la Factura Electrónica de Venta como Título Valor en el RADIAN para negociación general")]
        SolicitudDisponibilizacionPosterior = 363,
        [Description("Inscripción posterior de la Factura Electrónica de Venta como Título Valor en el RADIAN para negociación directa previa")]
        SolicitudDisponibilizacionPosteriorDirecta = 364,

        [Description("Cancelación del endoso electrónico en garantía")]
        CancelacionEndoso = 401,
        [Description("Cancelación del endoso electrónico en procuración")]
        CancelacionEndosoProcuracion = 402,

        [Description("Tacha de endosos por endoso en retorno")]
        TachaEndosoRetorno = 403,

        [Description("Limitación para circulación de la Factura Electrónica de Venta como Título Valor por auto que decreta medida cautelar por embargo")]
        MedidaCautelarEmbargo = 411,
        [Description("Limitación para circulación de la Factura Electrónica de Venta como Título Valor por auto que decreta medida cautelar por mandamiento de pago")]
        MedidaCautelarMandamiento = 412,

        [Description("Terminación de la limitación para circulación de la Factura Electrónica de Venta como Título Valor por sentencia")]
        TerminacionSentencia = 421,
        [Description("Terminación de la limitación para circulación de la Factura Electrónica de Venta como Título Valor por terminación anticipada")]
        TerminacionAnticipada = 422,

        [Description("Terminación del mandato por revocatoria")]
        TerminacionRevocatoria = 441,
        [Description("Terminación del mandato por renuncia")]
        TerminacionRenuncia = 442,
        [Description("Pago parcial de la Factura Electrónica de Venta como Título Valor")]
        PagoParcial = 451,
        [Description("Pago total de la Factura Electrónica de Venta como Título Valor")]
        PagoTotal = 452,

        [Description("Protesto por falta de aceptación")]
        ObjectionNonAcceptance = 481,
        [Description("Protesto por falta de pago")]
        ObjectionNonPayment = 482,
        [Description("Transferencia parcial de los derechos económicos con responsabilidad")]
        PartialTransferOfEconomicRightsWithLiability = 491,
        [Description("Transferencia total de los derechos económicos con responsabilidad")]
        FullTransferOfEconomicRightsWithLiability = 492,
        [Description("Transferencia parcial de los derechos económicos sin responsabilidad")]
        PartialTransferOfEconomicRightsWithoutLiability = 493,
        [Description("Transferencia total de los derechos económicos sin responsabilidad")]
        FullTransferOfEconomicRightsWithoutLiability = 494,
        [Description("Pago parcial de la transferencia de los derechos económicos")]
        PartialPaymentTransferEconomicRights = 511,
        [Description("Pago total de la transferencia de los derechos económicos")]
        TotalPaymentTransferEconomicRights = 512
    }

    public enum ExportStatus
    {
        [Description("Procesando")]
        InProcess = 0,
        [Description("Listo")]
        OK = 1,
        [Description("Cancelado")]
        Cancelled = 2,
        [Description("Error")]
        Error = 3,
    }

    public enum ExportType
    {
        [Description("Excel")]
        Excel = 0,
        [Description("PDF")]
        PDF = 1,
        [Description("XML")]
        XML = 2,
    }

    public enum HttpStatus
    {
        [Description("Responsabilidad 52-Facturador Electrónico fue ejecutada satisfactoriamente")]
        Success = 200,
        [Description("No se puede asignar responsabilidad en el ambiente actual")]
        InvalidEnvironment = 999,
    }

    public enum IdentificationType
    {
        [Description("Cédula de ciudadanía")]
        CC = 10910094,
        [Description("Cédula de extranjería")]
        CE = 10910096,
        [Description("Registro civil")]
        RC = 10910097,
        [Description("Tarjeta de identidad")]
        TI = 10910098,
        [Description("Tarjeta de extranjería")]
        TE = 10910099,
        [Description("Nit")]
        Nit = 10910100,
        [Description("Pasaporte")]
        Pasaporte = 10910101,
        [Description("Documento de identificación de extranjero")]
        DIE = 10910102,
        [Description("PEP")]
        PEP = 10910103,
        [Description("Nit de otro país")]
        NitOP = 10910104,
        [Description("NIUP")]
        NIUP = 10910105
    }

    public enum FiscalDocumentType
    {
        [Description("Tarjeta de extranjería ")]
        TE = 21,
        [Description("Cédula de extranjería ")]
        CE = 22,
        [Description("Nit")]
        Nit = 31,
        [Description("Pasaporte")]
        Pasaporte = 41,
        [Description("Documento de identificación extranjero")]
        DIE = 42,
        [Description("PEP")]
        PEP = 47,
        [Description("Nit de otro país")]
        NitOP = 50
    }

    public enum OperationMode
    {
        [Description("Facturador gratuito")]
        Free = 1,
        [Description("Propios medios")]
        Own = 2,
        [Description("Proveedor")]
        Provider = 3,
    }

    public enum LoginType
    {
        [Description("Administrador")]
        Administrator = 0,
        [Description("Certificado")]
        Certificate = 1,
        [Description("Empresa")]
        Company = 2,
        [Description("Persona")]
        Person = 3,
        [Description("Usuario Registrado")]
        ExternalUser = 4,
        //[Description("No obligados a Facturar")]
        [Description("No Facturador")]
        NotObligedInvoice = 5,

    }

    public enum NumberRangeState
    {
        [Description("Autorizado")]
        Authorized = 900002,
        [Description("Inhabilitado")]
        Disabled = 900003,
        [Description("Vencido")]
        Defeated = 900004,
    }
    public enum PersonType
    {
        [Description("Jurídica")]
        Juridic = 1,
        [Description("Natural")]
        Natural = 2,
    }

    public enum Role
    {
        [Description("Administrador")]
        Administrator = 0,
        [Description("Facturador")]
        Biller = 1,
        [Description("Proveedor")]
        Provider = 2,
    }

    public enum TestSetStatus
    {
        [Description("En proceso")]
        InProcess = 0,
        [Description("Aceptado")]
        Accepted = 1,
        [Description("Rechazado")]
        Rejected = 2
    }

    public enum SoftwareStatus
    {
        [Description("Pruebas")]
        Test = 1,
        [Description("Productivo")]
        Production = 2,
        [Description("Inactivo")]
        Inactive = 3,
    }

    public enum StatusRut
    {
        [Description("Registro cancelado")]
        Cancelled = 12310324,
        [Description("Registro inactivo")]
        Inactive = 12310343,
        [Description("Registro suspension")]
        Suspension = 12310396,
    }




    public enum RadianOperationMode
    {
        None = 0,
        [Description("Operación Directa")]
        Direct = 1,
        [Description("Operación Indirecta")]
        Indirect = 2,
    }


    public enum RadianSoftwareStatus
    {
        None = 0,
        [Description("En proceso")]
        InProcess = 1,
        [Description("Aceptado")]
        Accepted = 2,
        [Description("Rechazado")]
        Rejected = 3,
    }

    public enum RadianState
    {
        none = 0,
        [Display(Name = "Registrado")]
        [Description("Registrado")]
        Registrado = 1,
        [Display(Name = "En pruebas")]
        [Description("En pruebas")]
        Test = 3,
        [Display(Name = "Habilitado")]
        [Description("Habilitado")]
        Habilitado = 4,
        [Display(Name = "Cancelado")]
        [Description("Cancelado")]
        Cancelado = 5
    }


    public enum OtherDocumentStatus
    {
        none = 0,
        [Display(Name = "Registrado")]
        [Description("Registrado")]
        Registrado = 1,
        [Display(Name = "En pruebas")]
        [Description("En pruebas")]
        Test = 3,
        [Display(Name = "Habilitado")]
        [Description("Habilitado")]
        Habilitado = 4,
        [Display(Name = "Cancelado")]
        [Description("Cancelado")]
        Cancelado = 5
    }


    public enum RadianContributorType
    {
        [Description("Cero")]
        Zero = 0,
        [Description("Facturador Electrónico")]
        ElectronicInvoice = 1,
        [Description("Proveedor Tecnológico")]
        TechnologyProvider = 2,
        [Description("Sistema de Negociación")]
        TradingSystem = 3,
        [Description("Factor")]
        Factor = 4
    }

    public enum RadianOperationModeTestSet
    {
        [Display(Name = "Software Propio")]
        [Description("Software Propio")]
        OwnSoftware = 1,
        [Display(Name = "Software de un proveedor tecnológico")]
        [Description("Software de un proveedor tecnológico")]
        SoftwareTechnologyProvider = 2,
        [Display(Name = "Software de un sistema de negociación")]
        [Description("Software de un sistema de negociación")]
        SoftwareTradingSystem = 3,
        [Display(Name = "Software de un factor")]
        [Description("Software de un factor")]
        SoftwareFactor = 4
    }

    public enum RadianDocumentStatus
    {
        [Description("No Aplica")]
        DontApply = 0,
        [Description("Limitada")]
        Limited = 1,
        [Description("Pagada")]
        Paid = 2,
        [Description("Endosada")]
        Endorsed = 3,
        [Description("Disponibilizada")]
        Readiness = 4,
        [Description("Título Valor")]
        SecurityTitle = 5,
        [Description("Factura Electrónica")]
        ElectronicInvoice = 6,
        [Description("Transferida")]
        TransferOfEconomicRights = 7,
        [Description("Protestada")]
        Objection = 8
    }


    /// <summary>
    /// Documentos Electronicos. Utilizados en la configuración de Set de Pruebas - Otros Documentos
    /// </summary>
    public enum ElectronicsDocuments
    {
        [Description("Nomina Electrónica y Nomina de Ajuste")]
        ElectronicPayroll = 1,

        [Description("Documento de Importación")]
        ImportDocument = 2,

        [Description("Documento de Soporte")]
        SupportDocument = 3,

        [Description("Documento equivalente electrónico")]
        ElectronicEquivalent = 14,

        [Description("POS electrónico")]
        ElectronicPOS = 5,

        [Description("Nómina electrónica No OFE")]
        ElectronicPayrollNoOFE = 13
    }

    public enum OtherDocElecSoftwaresStatus
    {
        None = 0,
        [Description("En proceso")]
        InProcess = 1,
        [Description("Aceptado")]
        Accepted = 2,
        [Description("Rechazado")]
        Rejected = 3,
    }

    public enum OtherDocElecContributorType
    {
        [Description("Emisor")]
        Transmitter = 1,
        [Description("Proveedor Tecnologico")]
        TechnologyProvider = 2,
    }

    public enum OtherDocElecState
    {
        none = 0,
        [Display(Name = "Registrado")]
        [Description("Registrado")]
        Registrado = 1,
        [Display(Name = "En pruebas")]
        [Description("En pruebas")]
        Test = 2,
        [Display(Name = "Habilitado")]
        [Description("Habilitado")]
        Habilitado = 3,
        [Display(Name = "Cancelado")]
        [Description("Cancelado")]
        Cancelado = 4
    }

    public enum OtherDocElecOperationMode
    {
        [Display(Name = "Software Propio")]
        [Description("Software Propio")]
        OwnSoftware = 1,
        [Display(Name = "Software de un proveedor tecnológico")]
        [Description("Software de un proveedor tecnológico")]
        SoftwareTechnologyProvider = 2,
        [Display(Name = "Software solución gratuita")]
        [Description("Software solución gratuita")]
        FreeBiller = 3
    }

    public enum EventCustomization
    {
        [Description("Primera inscripción de la factura electrónica de venta como título valor para Negociación General")]
        FirstGeneralRegistration = 361,
        [Description("Primera inscripción de la factura electrónica de venta como título valor para Negociación Directa Previa")]
        FirstPriorDirectRegistration = 362,
        [Description("Inscripción posterior de la factura electrónica de venta como título valor para Negociación General")]
        GeneralSubsequentRegistration = 363,
        [Description("Inscripción posterior de la factura electrónica de venta como título valor para Negociación Directa Previa")]
        PriorDirectSubsequentEnrollment = 364,
        [Description("Cancelación del Endoso en Garantia")]
        CancellationGuaranteeEndorsement = 401,
        [Description("Cancelación del Endoso en Procuracion")]
        CancellationEndorsementProcurement = 402,
        [Description("Tacha de Endosos por Endoso en Retorno")]
        EndorsementsReturn = 403,
        [Description("Notificación de pago parcial")]
        PartialPaymentNotification = 451,
        [Description("Pago de la factura electrónica de venta como título valor")]
        PaymentBillFTV = 452,
        [Description("Protesto por falta de aceptación")]
        ObjectionNonAcceptance = 481,
        [Description("Protesto por falta de pago")]
        ObjectionNonPayment = 482,
        [Description("Transferencia parcial de los derechos económicos con responsabilidad")]
        PartialTransferOfEconomicRightsWithLiability = 491,
        [Description("Transferencia total de los derechos económicos con responsabilidad")]
        FullTransferOfEconomicRightsWithLiability = 492,
        [Description("Transferencia parcial de los derechos económicos sin responsabilidad")]
        PartialTransferOfEconomicRightsWithoutLiability = 493,
        [Description("Transferencia total de los derechos económicos sin responsabilidad")]
        FullTransferOfEconomicRightsWithoutLiability = 494,
        [Description("Pago parcial de la transferencia de los derechos económicos")]
        PartialPaymentTransferEconomicRights = 511,
        [Description("Pago total de la transferencia de los derechos económicos")]
        TotalPaymentTransferEconomicRights = 512

    }

    public enum PaymentMethods
    {
        [Description("Efectivo")]
        Efectivo = 10,
        [Description("Tarjeta Débito")]
        TarjetaDebito = 49,
        [Description("Consignación bancaria")]
        ConsignacionBancaria = 42,
        [Description("Débito ACH")]
        DebitoACH = 3,
        [Description("Cheque certificado")]
        ChequeCertificado = 25,
        [Description("Cheque Local")]
        ChequeLocal = 26,
        [Description("Tarjeta Crédito")]
        TarjetaCredito = 48,
        [Description("Transferencia Débito Bancaria")]
        TransferenciaDebitoBancaria = 47,
        [Description("Nota cambiaria")]
        NotaCambiaria = 44,
        [Description("Cheque")]
        Cheque = 20,
        [Description("Cheque bancario de gerencia")]
        ChequeBancarioDeGerencia = 23,
        [Description("Bonos")]
        Bonos = 71,
        [Description("Nota cambiaria esperando aceptación")]
        NotaCambiariaEsperandoAceptacion = 24,
        [Description("Nota promisoria firmada por el banco")]
        NotaPromisoriaFirmadaPorElBanco = 64,
        [Description("Nota promisoria firmada por un banco avalada por otro banco")]
        NotaPromisoriaFirmadaPorUnBancoAvaladaPorOtroBanco = 65,
        [Description("Nota promisoria firmada")]
        NotaPromisoriaFirmada = 66,
        [Description("Nota promisoria firmada por un tercero avalada por un banco")]
        NotaPromisoriaFirmadaPorUnTerceroAvaladaPorUnBanco = 67,
        [Description("Crédito ACH")]
        CreditoACH = 2,
        [Description("Giro formato abierto")]
        GiroFormatoAbierto = 95,
        [Description("Crédito Ahorro")]
        CreditoAhorro = 13,
        [Description("Débito Ahorro")]
        DebitoAhorro = 14,
        [Description("Crédito Intercambio Corporativo")]
        CreditoIntercambioCorporativoCTX = 39,
        [Description("Reversión débito de demanda ACH")]
        ReversionDebitoDeDemandaACH = 4,
        [Description("Reversión crédito de demanda ACH")]
        ReversionCreditoDeDemandaACH = 5,
        [Description("Crédito de demanda ACH")]
        CreditoDeDemandaACH = 6,
        [Description("Débito de demanda ACH")]
        DebitoDeDemandaACH = 7,
        [Description("Clearing Nacional o Regional")]
        ClearingNacionalORegional = 9,
        [Description("Reversión Crédito Ahorro")]
        ReversionCreditoAhorro = 11,
        [Description("Reversión Débito Ahorro")]
        ReversionDebitoAhorro = 12,
        [Description("Desembolso (CCD) débito")]
        DesembolsoCCDDebito = 18,
        [Description("Crédito Pago negocio corporativo")]
        CreditoPagoNegocioCorporativoCTP = 19,
        [Description("Poyecto bancario")]
        PoyectoPancario = 21,
        [Description("Proyecto bancario certificado")]
        ProyectoBancarioCertificado = 22,
        [Description("Débito Pago Negocio Corporativo (CTP)")]
        DebitoPagoNegocioCorporativoCTP = 27,
        [Description("Crédito Negocio Intercambio Corporativo (CTX)")]
        CreditoNegocioIntercambioCorporativoCTX = 28,
        [Description("Débito Negocio Intercambio Corporativo (CTX)")]
        DebitoNegocioIntercambioCorporativoCTX = 29,
        [Description("Transferencia Crédito")]
        TransferenciaCredito = 30,
        [Description("Transferencia Débito")]
        TransferenciaDebito = 31,
        [Description("Desembolso Crédito plus (CCD+)")]
        DesembolsoCreditoPlusCCD = 32,
        [Description("Vales")]
        Vales = 72,
        [Description("Nota promisoria firmada por el acreedor")]
        NotaPromisoriaFirmadaPorElAcreedor = 61,
        [Description("Nota promisoria firmada por el acreedor, avalada por el banco")]
        NotaPromisoriaFirmadaPorElAcreedorAvaladaPorElBanco = 62,
        [Description("Nota promisoria firmada por el acreedor, avalada por un tercero")]
        NotaPromisoriaFirmadaPorElAcreedorAvaladaPorUnTercero = 63,
        [Description("Nota promisoria")]
        NotaPromisoria = 60,
        [Description("Método de pago solicitado no usado")]
        MetodoDePagoSolicitadoNoUsado = 96,
        [Description("Nota bancaria transferible")]
        NotaBancariaTransferible = 91,
        [Description("Cheque local transferible")]
        ChequeLocalTransferible = 92,
        [Description("Giro referenciado")]
        GiroReferenciado = 93,
        [Description("Giro urgente")]
        GiroUrgente = 94,
        [Description("Débito Intercambio Corporativo (CTX)")]
        DebitoIntercambioCorporativoCTX = 40,
        [Description("Desembolso Crédito plus (CCD+)")]
        DesembolsoCreditoPlusCCDD = 41,
        [Description("Desembolso Débito plus (CCD+)")]
        DesembolsoDebitoPlusCCD = 43,
        [Description("Transferencia Crédito Bancario")]
        TransferenciaCrEditoBancario = 45,
        [Description("Transferencia Débito Interbancario")]
        TransferenciaDebitoInterbancario = 46,
        [Description("Postgiro")]
        Postgiro = 50,
        [Description("Telex estándar bancario")]
        TelexEstandarBancario = 51,
        [Description("Pago comercial urgente")]
        PagoComercialUrgente = 52,
        [Description("Pago Tesorería Urgente")]
        PagoTesoreriaUrgente = 53,
        [Description("Bookentry Crédito")]
        BookentryCredito = 15,
        [Description("Bookentry Débito")]
        BookentryDebito = 16,
        [Description("Desembolso Crédito (CCD)")]
        DesembolsoCreditoCCD = 17,
        [Description("Retiro de nota por el por el acreedor")]
        RetiroDeNotaPorElPorElAcreedor = 70,
        [Description("Retiro de nota por el por el acreedor sobre un banco")]
        RetiroDeNotaPorElPorElAcreedorSobreUnBanco = 74,
        [Description("Retiro de nota por el acreedor, avalada por otro banco")]
        RetiroDeNotaPorElAcreedorAvaladaPorOtroBanco = 75,
        [Description("Retiro de nota por el acreedor, sobre un banco avalada por un tercero")]
        RetiroDeNotaPorElAcreedorSobreUnBancoAvaladaPorUnTercero = 76,
        [Description("Retiro de una nota por el acreedor sobre un tercero")]
        RetiroDeUnaNotaPorElAcreedorSobreunTercero = 77,
        [Description("Retiro de una nota por el acreedor sobre un tercero avalada por un banco")]
        RetiroDeUnaNotaPorElAcreedorSobreUnTerceroAvaladaPorUnBanco = 78,
        [Description("Desembolso Débito plus (CCD+)")]
        DesembolsoDebitoPlusCCDD = 33,
        [Description("Pago y depósito pre acordado (PPD)")]
        PagoYDepositoPreAcordadoPPD = 34,
        [Description("Desembolso Crédito (CCD)")]
        DesembolsoCreditoCCDD = 35,
        [Description("Desembolso Débito (CCD)")]
        DesembolsoDebitoCCD = 36,
        [Description("Instrumento no definido")]
        InstrumentoNoDefinido = 1,
        [Description("Pago Negocio Corporativo Ahorros Crédito (CTP)")]
        PagoNegocioCorporativoAhorrosCreditoCTP = 37,
        [Description("Pago Negocio Corporativo Ahorros Débito (CTP)")]
        PagoNegocioCorporativoAhorrosDebitoCTP = 38,
        [Description("Clearing entre partners")]
        ClearingEntreartners = 97

    }
       

    public enum EndodoSubEventStatus
    {
        [Description("Completo")]
        Completo = 1,
        [Description("En blanco")]
        EnBlanco = 2
    }

    public enum IndividualPayrollAdjustmentNoteType
    {
        [Description("Reemplazar")]
        Replace = 1,
        [Description("Eliminar")]
        Remove = 2
    }
}