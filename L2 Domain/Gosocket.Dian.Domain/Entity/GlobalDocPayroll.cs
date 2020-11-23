using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocPayroll
    {

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste", IsNullable = false)]
        public partial class NominaIndividualDeAjuste
        {

            private NominaIndividualDeAjustePeriodo periodoField;

            private NominaIndividualDeAjusteNumeroSecuenciaXML numeroSecuenciaXMLField;

            private NominaIndividualDeAjusteLugarGeneracionXML lugarGeneracionXMLField;

            private NominaIndividualDeAjusteProveedorXML proveedorXMLField;

            private byte codigoQRField;

            private NominaIndividualDeAjusteInformacionGeneral informacionGeneralField;

            private string notasField;

            private NominaIndividualDeAjusteReemplazandoPredecesor reemplazandoPredecesorField;

            private NominaIndividualDeAjusteEmpleador empleadorField;

            private NominaIndividualDeAjusteTrabajador trabajadorField;

            private NominaIndividualDeAjustePago pagoField;

            private NominaIndividualDeAjusteDevengados devengadosField;

            private NominaIndividualDeAjusteDeducciones deduccionesField;

            private decimal devengadosTotalField;

            private decimal deduccionesTotalField;

            private decimal comprobanteTotalField;

            private string schemaLocationField;

            /// <remarks/>
            public NominaIndividualDeAjustePeriodo Periodo
            {
                get
                {
                    return this.periodoField;
                }
                set
                {
                    this.periodoField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteNumeroSecuenciaXML NumeroSecuenciaXML
            {
                get
                {
                    return this.numeroSecuenciaXMLField;
                }
                set
                {
                    this.numeroSecuenciaXMLField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteLugarGeneracionXML LugarGeneracionXML
            {
                get
                {
                    return this.lugarGeneracionXMLField;
                }
                set
                {
                    this.lugarGeneracionXMLField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteProveedorXML ProveedorXML
            {
                get
                {
                    return this.proveedorXMLField;
                }
                set
                {
                    this.proveedorXMLField = value;
                }
            }

            /// <remarks/>
            public byte CodigoQR
            {
                get
                {
                    return this.codigoQRField;
                }
                set
                {
                    this.codigoQRField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteInformacionGeneral InformacionGeneral
            {
                get
                {
                    return this.informacionGeneralField;
                }
                set
                {
                    this.informacionGeneralField = value;
                }
            }

            /// <remarks/>
            public string Notas
            {
                get
                {
                    return this.notasField;
                }
                set
                {
                    this.notasField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteReemplazandoPredecesor ReemplazandoPredecesor
            {
                get
                {
                    return this.reemplazandoPredecesorField;
                }
                set
                {
                    this.reemplazandoPredecesorField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteEmpleador Empleador
            {
                get
                {
                    return this.empleadorField;
                }
                set
                {
                    this.empleadorField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteTrabajador Trabajador
            {
                get
                {
                    return this.trabajadorField;
                }
                set
                {
                    this.trabajadorField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjustePago Pago
            {
                get
                {
                    return this.pagoField;
                }
                set
                {
                    this.pagoField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteDevengados Devengados
            {
                get
                {
                    return this.devengadosField;
                }
                set
                {
                    this.devengadosField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteDeducciones Deducciones
            {
                get
                {
                    return this.deduccionesField;
                }
                set
                {
                    this.deduccionesField = value;
                }
            }

            /// <remarks/>
            public decimal DevengadosTotal
            {
                get
                {
                    return this.devengadosTotalField;
                }
                set
                {
                    this.devengadosTotalField = value;
                }
            }

            /// <remarks/>
            public decimal DeduccionesTotal
            {
                get
                {
                    return this.deduccionesTotalField;
                }
                set
                {
                    this.deduccionesTotalField = value;
                }
            }

            /// <remarks/>
            public decimal ComprobanteTotal
            {
                get
                {
                    return this.comprobanteTotalField;
                }
                set
                {
                    this.comprobanteTotalField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string SchemaLocation
            {
                get
                {
                    return this.schemaLocationField;
                }
                set
                {
                    this.schemaLocationField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjustePeriodo
        {

            private System.DateTime fechaIngresoField;

            private System.DateTime fechaPagoInicioField;

            private System.DateTime fechaPagoFinField;

            private string tiempoLaboradoField;

            private System.DateTime fechaLiquidacionField;

            private System.DateTime fechaGenField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime FechaIngreso
            {
                get
                {
                    return this.fechaIngresoField;
                }
                set
                {
                    this.fechaIngresoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime FechaPagoInicio
            {
                get
                {
                    return this.fechaPagoInicioField;
                }
                set
                {
                    this.fechaPagoInicioField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime FechaPagoFin
            {
                get
                {
                    return this.fechaPagoFinField;
                }
                set
                {
                    this.fechaPagoFinField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string TiempoLaborado
            {
                get
                {
                    return this.tiempoLaboradoField;
                }
                set
                {
                    this.tiempoLaboradoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime FechaLiquidacion
            {
                get
                {
                    return this.fechaLiquidacionField;
                }
                set
                {
                    this.fechaLiquidacionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime FechaGen
            {
                get
                {
                    return this.fechaGenField;
                }
                set
                {
                    this.fechaGenField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteNumeroSecuenciaXML
        {

            private ulong codigoTrabajadorField;

            private string prefijoField;

            private byte consecutivoField;

            private string numeroField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public ulong CodigoTrabajador
            {
                get
                {
                    return this.codigoTrabajadorField;
                }
                set
                {
                    this.codigoTrabajadorField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Prefijo
            {
                get
                {
                    return this.prefijoField;
                }
                set
                {
                    this.prefijoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte Consecutivo
            {
                get
                {
                    return this.consecutivoField;
                }
                set
                {
                    this.consecutivoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Numero
            {
                get
                {
                    return this.numeroField;
                }
                set
                {
                    this.numeroField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteLugarGeneracionXML
        {

            private string paisField;

            private byte departamentoEstadoField;

            private ushort municipioCiudadField;

            private string idiomaField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Pais
            {
                get
                {
                    return this.paisField;
                }
                set
                {
                    this.paisField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte DepartamentoEstado
            {
                get
                {
                    return this.departamentoEstadoField;
                }
                set
                {
                    this.departamentoEstadoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public ushort MunicipioCiudad
            {
                get
                {
                    return this.municipioCiudadField;
                }
                set
                {
                    this.municipioCiudadField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Idioma
            {
                get
                {
                    return this.idiomaField;
                }
                set
                {
                    this.idiomaField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteProveedorXML
        {

            private uint nITField;

            private byte dvField;

            private string softwareIDField;

            private string softwareSCField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public uint NIT
            {
                get
                {
                    return this.nITField;
                }
                set
                {
                    this.nITField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte DV
            {
                get
                {
                    return this.dvField;
                }
                set
                {
                    this.dvField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string SoftwareID
            {
                get
                {
                    return this.softwareIDField;
                }
                set
                {
                    this.softwareIDField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string SoftwareSC
            {
                get
                {
                    return this.softwareSCField;
                }
                set
                {
                    this.softwareSCField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteInformacionGeneral
        {

            private string versionField;

            private byte ambienteField;

            private string cUNEField;

            private string encripCUNEField;

            private System.DateTime fechaGenField;

            private System.DateTime horaGenField;

            private byte tipoNominaField;

            private byte periodoNominaField;

            private string tipoMonedaField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte Ambiente
            {
                get
                {
                    return this.ambienteField;
                }
                set
                {
                    this.ambienteField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string CUNE
            {
                get
                {
                    return this.cUNEField;
                }
                set
                {
                    this.cUNEField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string EncripCUNE
            {
                get
                {
                    return this.encripCUNEField;
                }
                set
                {
                    this.encripCUNEField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime FechaGen
            {
                get
                {
                    return this.fechaGenField;
                }
                set
                {
                    this.fechaGenField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "time")]
            public System.DateTime HoraGen
            {
                get
                {
                    return this.horaGenField;
                }
                set
                {
                    this.horaGenField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte TipoNomina
            {
                get
                {
                    return this.tipoNominaField;
                }
                set
                {
                    this.tipoNominaField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte PeriodoNomina
            {
                get
                {
                    return this.periodoNominaField;
                }
                set
                {
                    this.periodoNominaField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string TipoMoneda
            {
                get
                {
                    return this.tipoMonedaField;
                }
                set
                {
                    this.tipoMonedaField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteReemplazandoPredecesor
        {

            private string numeroPredField;

            private string cUNEPredField;

            private System.DateTime fechaGenPredField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string NumeroPred
            {
                get
                {
                    return this.numeroPredField;
                }
                set
                {
                    this.numeroPredField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string CUNEPred
            {
                get
                {
                    return this.cUNEPredField;
                }
                set
                {
                    this.cUNEPredField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime FechaGenPred
            {
                get
                {
                    return this.fechaGenPredField;
                }
                set
                {
                    this.fechaGenPredField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteEmpleador
        {

            private string razonSocialField;

            private uint nITField;

            private byte dvField;

            private string paisField;

            private byte departamentoEstadoField;

            private ushort municipioCiudadField;

            private string direccionField;

            private uint celularField;

            private string correoField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string RazonSocial
            {
                get
                {
                    return this.razonSocialField;
                }
                set
                {
                    this.razonSocialField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public uint NIT
            {
                get
                {
                    return this.nITField;
                }
                set
                {
                    this.nITField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte DV
            {
                get
                {
                    return this.dvField;
                }
                set
                {
                    this.dvField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Pais
            {
                get
                {
                    return this.paisField;
                }
                set
                {
                    this.paisField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte DepartamentoEstado
            {
                get
                {
                    return this.departamentoEstadoField;
                }
                set
                {
                    this.departamentoEstadoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public ushort MunicipioCiudad
            {
                get
                {
                    return this.municipioCiudadField;
                }
                set
                {
                    this.municipioCiudadField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Direccion
            {
                get
                {
                    return this.direccionField;
                }
                set
                {
                    this.direccionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public uint Celular
            {
                get
                {
                    return this.celularField;
                }
                set
                {
                    this.celularField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Correo
            {
                get
                {
                    return this.correoField;
                }
                set
                {
                    this.correoField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteTrabajador
        {

            private byte tipoTrabajadorField;

            private byte subTipoTrabajadorField;

            private bool altoRiesgoPensionField;

            private byte tipoDocumentoField;

            private uint numeroDocumentoField;

            private string primerApellidoField;

            private string segundoApellidoField;

            private string primerNombreField;

            private string otrosNombresField;

            private string lugarTrabajoPaisField;

            private byte lugarTrabajoDepartamentoEstadoField;

            private ushort lugarTrabajoMunicipioCiudadField;

            private string lugarTrabajoDireccionField;

            private uint celularField;

            private string correoField;

            private bool salarioIntegralField;

            private byte codigoAreaField;

            private string nombreAreaField;

            private byte codigoCargoField;

            private string nombreCargoField;

            private byte tipoContratoField;

            private decimal salarioField;

            private ulong codigoTrabajadorField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte TipoTrabajador
            {
                get
                {
                    return this.tipoTrabajadorField;
                }
                set
                {
                    this.tipoTrabajadorField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte SubTipoTrabajador
            {
                get
                {
                    return this.subTipoTrabajadorField;
                }
                set
                {
                    this.subTipoTrabajadorField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public bool AltoRiesgoPension
            {
                get
                {
                    return this.altoRiesgoPensionField;
                }
                set
                {
                    this.altoRiesgoPensionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte TipoDocumento
            {
                get
                {
                    return this.tipoDocumentoField;
                }
                set
                {
                    this.tipoDocumentoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public uint NumeroDocumento
            {
                get
                {
                    return this.numeroDocumentoField;
                }
                set
                {
                    this.numeroDocumentoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string PrimerApellido
            {
                get
                {
                    return this.primerApellidoField;
                }
                set
                {
                    this.primerApellidoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string SegundoApellido
            {
                get
                {
                    return this.segundoApellidoField;
                }
                set
                {
                    this.segundoApellidoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string PrimerNombre
            {
                get
                {
                    return this.primerNombreField;
                }
                set
                {
                    this.primerNombreField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string OtrosNombres
            {
                get
                {
                    return this.otrosNombresField;
                }
                set
                {
                    this.otrosNombresField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string LugarTrabajoPais
            {
                get
                {
                    return this.lugarTrabajoPaisField;
                }
                set
                {
                    this.lugarTrabajoPaisField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte LugarTrabajoDepartamentoEstado
            {
                get
                {
                    return this.lugarTrabajoDepartamentoEstadoField;
                }
                set
                {
                    this.lugarTrabajoDepartamentoEstadoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public ushort LugarTrabajoMunicipioCiudad
            {
                get
                {
                    return this.lugarTrabajoMunicipioCiudadField;
                }
                set
                {
                    this.lugarTrabajoMunicipioCiudadField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string LugarTrabajoDireccion
            {
                get
                {
                    return this.lugarTrabajoDireccionField;
                }
                set
                {
                    this.lugarTrabajoDireccionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public uint Celular
            {
                get
                {
                    return this.celularField;
                }
                set
                {
                    this.celularField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Correo
            {
                get
                {
                    return this.correoField;
                }
                set
                {
                    this.correoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public bool SalarioIntegral
            {
                get
                {
                    return this.salarioIntegralField;
                }
                set
                {
                    this.salarioIntegralField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte CodigoArea
            {
                get
                {
                    return this.codigoAreaField;
                }
                set
                {
                    this.codigoAreaField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string NombreArea
            {
                get
                {
                    return this.nombreAreaField;
                }
                set
                {
                    this.nombreAreaField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte CodigoCargo
            {
                get
                {
                    return this.codigoCargoField;
                }
                set
                {
                    this.codigoCargoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string NombreCargo
            {
                get
                {
                    return this.nombreCargoField;
                }
                set
                {
                    this.nombreCargoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte TipoContrato
            {
                get
                {
                    return this.tipoContratoField;
                }
                set
                {
                    this.tipoContratoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal Salario
            {
                get
                {
                    return this.salarioField;
                }
                set
                {
                    this.salarioField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public ulong CodigoTrabajador
            {
                get
                {
                    return this.codigoTrabajadorField;
                }
                set
                {
                    this.codigoTrabajadorField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjustePago
        {

            private byte formaField;

            private byte metodoField;

            private string bancoField;

            private string tipoCuentaField;

            private string numeroCuentaField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte Forma
            {
                get
                {
                    return this.formaField;
                }
                set
                {
                    this.formaField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte Metodo
            {
                get
                {
                    return this.metodoField;
                }
                set
                {
                    this.metodoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string Banco
            {
                get
                {
                    return this.bancoField;
                }
                set
                {
                    this.bancoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string TipoCuenta
            {
                get
                {
                    return this.tipoCuentaField;
                }
                set
                {
                    this.tipoCuentaField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string NumeroCuenta
            {
                get
                {
                    return this.numeroCuentaField;
                }
                set
                {
                    this.numeroCuentaField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteDevengados
        {

            private NominaIndividualDeAjusteDevengadosBasico basicoField;

            private NominaIndividualDeAjusteDevengadosVacaciones vacacionesField;

            private NominaIndividualDeAjusteDevengadosBonificaciones bonificacionesField;

            /// <remarks/>
            public NominaIndividualDeAjusteDevengadosBasico Basico
            {
                get
                {
                    return this.basicoField;
                }
                set
                {
                    this.basicoField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteDevengadosVacaciones Vacaciones
            {
                get
                {
                    return this.vacacionesField;
                }
                set
                {
                    this.vacacionesField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteDevengadosBonificaciones Bonificaciones
            {
                get
                {
                    return this.bonificacionesField;
                }
                set
                {
                    this.bonificacionesField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteDevengadosBasico
        {

            private byte diasTrabajadosField;

            private decimal salarioTrabajadoField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte DiasTrabajados
            {
                get
                {
                    return this.diasTrabajadosField;
                }
                set
                {
                    this.diasTrabajadosField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal SalarioTrabajado
            {
                get
                {
                    return this.salarioTrabajadoField;
                }
                set
                {
                    this.salarioTrabajadoField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteDevengadosVacaciones
        {

            private NominaIndividualDeAjusteDevengadosVacacionesVacacionesComunes vacacionesComunesField;

            /// <remarks/>
            public NominaIndividualDeAjusteDevengadosVacacionesVacacionesComunes VacacionesComunes
            {
                get
                {
                    return this.vacacionesComunesField;
                }
                set
                {
                    this.vacacionesComunesField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteDevengadosVacacionesVacacionesComunes
        {

            private System.DateTime fechaInicioField;

            private System.DateTime fechaFinField;

            private byte cantidadField;

            private decimal pagoField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime FechaInicio
            {
                get
                {
                    return this.fechaInicioField;
                }
                set
                {
                    this.fechaInicioField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime FechaFin
            {
                get
                {
                    return this.fechaFinField;
                }
                set
                {
                    this.fechaFinField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte Cantidad
            {
                get
                {
                    return this.cantidadField;
                }
                set
                {
                    this.cantidadField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal Pago
            {
                get
                {
                    return this.pagoField;
                }
                set
                {
                    this.pagoField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteDevengadosBonificaciones
        {

            private NominaIndividualDeAjusteDevengadosBonificacionesBonificacion bonificacionField;

            /// <remarks/>
            public NominaIndividualDeAjusteDevengadosBonificacionesBonificacion Bonificacion
            {
                get
                {
                    return this.bonificacionField;
                }
                set
                {
                    this.bonificacionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteDevengadosBonificacionesBonificacion
        {

            private decimal bonificacionNSField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal BonificacionNS
            {
                get
                {
                    return this.bonificacionNSField;
                }
                set
                {
                    this.bonificacionNSField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteDeducciones
        {

            private NominaIndividualDeAjusteDeduccionesSalud saludField;

            private NominaIndividualDeAjusteDeduccionesFondoPension fondoPensionField;

            private NominaIndividualDeAjusteDeduccionesFondoSP fondoSPField;

            private decimal retencionFuenteField;

            private decimal aFCField;

            private decimal deudaField;

            /// <remarks/>
            public NominaIndividualDeAjusteDeduccionesSalud Salud
            {
                get
                {
                    return this.saludField;
                }
                set
                {
                    this.saludField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteDeduccionesFondoPension FondoPension
            {
                get
                {
                    return this.fondoPensionField;
                }
                set
                {
                    this.fondoPensionField = value;
                }
            }

            /// <remarks/>
            public NominaIndividualDeAjusteDeduccionesFondoSP FondoSP
            {
                get
                {
                    return this.fondoSPField;
                }
                set
                {
                    this.fondoSPField = value;
                }
            }

            /// <remarks/>
            public decimal RetencionFuente
            {
                get
                {
                    return this.retencionFuenteField;
                }
                set
                {
                    this.retencionFuenteField = value;
                }
            }

            /// <remarks/>
            public decimal AFC
            {
                get
                {
                    return this.aFCField;
                }
                set
                {
                    this.aFCField = value;
                }
            }

            /// <remarks/>
            public decimal Deuda
            {
                get
                {
                    return this.deudaField;
                }
                set
                {
                    this.deudaField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteDeduccionesSalud
        {

            private decimal porcentajeField;

            private decimal valorBaseField;

            private decimal deduccionField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal Porcentaje
            {
                get
                {
                    return this.porcentajeField;
                }
                set
                {
                    this.porcentajeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal ValorBase
            {
                get
                {
                    return this.valorBaseField;
                }
                set
                {
                    this.valorBaseField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal Deduccion
            {
                get
                {
                    return this.deduccionField;
                }
                set
                {
                    this.deduccionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteDeduccionesFondoPension
        {

            private decimal porcentajeField;

            private decimal valorBaseField;

            private decimal deduccionField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal Porcentaje
            {
                get
                {
                    return this.porcentajeField;
                }
                set
                {
                    this.porcentajeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal ValorBase
            {
                get
                {
                    return this.valorBaseField;
                }
                set
                {
                    this.valorBaseField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal Deduccion
            {
                get
                {
                    return this.deduccionField;
                }
                set
                {
                    this.deduccionField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "dian:gov:co:facturaelectronica:NominaIndividualDeAjuste")]
        public partial class NominaIndividualDeAjusteDeduccionesFondoSP
        {

            private decimal porcentajeField;

            private decimal deduccionField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal Porcentaje
            {
                get
                {
                    return this.porcentajeField;
                }
                set
                {
                    this.porcentajeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal Deduccion
            {
                get
                {
                    return this.deduccionField;
                }
                set
                {
                    this.deduccionField = value;
                }
            }
        }


    }
}
