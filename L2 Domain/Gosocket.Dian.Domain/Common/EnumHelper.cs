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
        [Description("Nota de crédito electrónica")]
        CreditNote = 91,
        [Description("Nota de débito electrónica")]
        DebitNote = 92,
        [Description("Application response")]
        ApplicationResponse = 96,
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
    }

    public enum EventStatus
    {
        [Description("None")]
        None = 000,
        [Description("Acuse de recibo")]
        Received = 030,
        [Description("Rechazo de la Factura Electrónica")]
        Rejected = 031,
        [Description("Constancia de recibo del bien o aceptación de la prestación del servicio")]
        Receipt = 032,
        [Description("Aceptación Expresa")]
        Accepted = 033,
        [Description("Aceptación Tácita")]
        AceptacionTacita = 034,
        [Description("Avales")]
        Avales = 035,
        [Description("Solicitud de Disponibilizacion")]
        SolicitudDisponibilizacion = 036,
        [Description("Endoso en Propiedad")]
        EndosoPropiedad = 037,
        [Description("Endoso en Garantía")]
        EndosoGarantia = 038,
        [Description("Endoso en Procuración")]
        EndosoProcuracion = 039,
        [Description("Anulacion de endoso electrónico")]
        InvoiceOfferedForNegotiation = 040,
        [Description("Limitación de circulación")]
        NegotiatedInvoice = 041,
        [Description("Anulación de limitación de circulación")]
        AnulacionLimitacionCirculacion = 042,
        [Description("Mandato")]
        Mandato = 043,
        [Description("Terminación del mandato")]
        TerminacionMandato = 044,
        [Description("Notificación del pago total o parcial")]
        NotificacionPagoTotalParcial = 045,
        [Description("Valor Informe 3 dias Pago")]
        ValInfoPago = 046
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
        TI =10910098,
        [Description("Tarjeta de extranjería")]
        TE=10910099,
        [Description("Nit")]
        Nit=10910100,
        [Description("Pasaporte")]
        Pasaporte=10910101	,
        [Description("Documento de identificación de extranjero")]
        DIE=10910102,
        [Description("PEP")]
        PEP=10910103,
        [Description("Nit de otro país")]
        NitOP=10910104,
        [Description("NIUP")]
        NIUP=10910105
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

    public enum RadianContributorType
    {
        [Description("Cero")]
        Zero = 0,
        [Description("Facturador Electronico")]
        ElectronicInvoice = 1,
        [Description("Proveedor Tecnologico")]
        TechnologyProvider = 2,
        [Description("Sistema de Negociacion")]
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
        ElectronicInvoice = 6
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
        ElectronicEquivalent = 4,
        
        [Description("POS electrónico")]
        ElectronicPOS = 5
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

}