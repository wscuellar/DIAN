using System.ComponentModel;
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
        [Description("Acuse de recibo")]
        Received = 030,
        [Description("Rechazo de documento")]
        Rejected = 031,
        [Description("Recibimiento de los bienes")]
        Receipt = 032,
        [Description("Aceptación de documento")]
        Accepted = 033,
        [Description("Factura ofrecida para negociación como título valor")]
        InvoiceOfferedForNegotiation = 040,
        [Description("Factura negociada como título valor")]
        NegotiatedInvoice = 041,
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
        CE = 10910096
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
}
