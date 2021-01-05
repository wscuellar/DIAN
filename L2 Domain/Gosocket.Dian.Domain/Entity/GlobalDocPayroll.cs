using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
    public class GlobalDocPayroll : TableEntity
    {

        public Dictionary<string, object> Fields { get; set; }

        public GlobalDocPayroll() { }

        public GlobalDocPayroll(string pk, string rk) : base(pk, rk)
        {

        }

        //Periodo

        public System.DateTime FechaIngreso { get; set; }

        public System.DateTime FechaPagoInicio { get; set; }

        public System.DateTime FechaPagoFin { get; set; }

        public string TiempoLaborado { get; set; }


        public System.DateTime FechaLiquidacion { get; set; }


        public System.DateTime FechaGen { get; set; }

        //NumeroSecuenciaXML
        public ulong CodigoTrabajador { get; set; }

        public string Prefijo { get; set; }

        public byte Consecutivo { get; set; }

        public string Numero { get; set; }

        //LugarGeneracionXML
        public string Pais { get; set; }

        public byte DepartamentoEstado { get; set; }

        public ushort MunicipioCiudad { get; set; }

        public string Idioma { get; set; }

        //ProveedorXml
        public uint NIT { get; set; }

        public byte DV { get; set; }

        public string SoftwareID { get; set; }

        public string SoftwareSC { get; set; }

        //CodigoQR

        public byte CodigoQR { get; set; }


        //InformacionGeneral
        public string Version { get; set; }

        public byte Ambiente { get; set; }

        public string CUNE { get; set; }

        public string EncripCUNE { get; set; }

        public System.DateTime Info_FechaGen { get; set; }

        public string HoraGen { get; set; }

        public byte TipoNomina { get; set; }

        public byte PeriodoNomina { get; set; }


        public string TipoMoneda { get; set; }

        //Notas
        public string Notas { get; set; }

        //ReemplazandoPrececesor

        public string NumeroPred { get; set; }

        public string CUNEPred { get; set; }

        public System.DateTime FechaGenPred { get; set; }

        //Empleador
        public string Emp_RazonSocial { get; set; }
        public uint Emp_NIT { get; set; }
        public byte Emp_DV { get; set; }
        public string Emp_Pais { get; set; }
        public byte Emp_DepartamentoEstado { get; set; }
        public ushort Emp_MunicipioCiudad { get; set; }
        public string Emp_Direccion { get; set; }
        public string Emp_Celular { get; set; }
        public string Emp_Correo { get; set; }

        //Trabajador
        public byte TipoTrabajador { get; set; }

        public byte SubTipoTrabajador { get; set; }

        public bool AltoRiesgoPension { get; set; }


        public byte TipoDocumento { get; set; }


        public uint NumeroDocumento { get; set; }


        public string PrimerApellido { get; set; }


        public string SegundoApellido { get; set; }


        public string PrimerNombre { get; set; }

        public string OtrosNombres { get; set; }

        public string LugarTrabajoPais { get; set; }

        public byte LugarTrabajoDepartamentoEstado { get; set; }

        public ushort LugarTrabajoMunicipioCiudad { get; set; }

        public string LugarTrabajoDireccion { get; set; }

        public uint Celular { get; set; }

        public string Correo { get; set; }

        public bool SalarioIntegral { get; set; }

        public byte CodigoArea { get; set; }

        public string NombreArea { get; set; }

        public byte CodigoCargo { get; set; }

        public string NombreCargo { get; set; }

        public byte TipoContrato { get; set; }

        public decimal Salario { get; set; }

        public ulong Trab_CodigoTrabajador { get; set; }

        //Pago
        public byte Forma { get; set; }

        public byte Metodo { get; set; }

        public string Banco { get; set; }

        public string TipoCuenta { get; set; }

        public string NumeroCuenta { get; set; }

        //Salud

        public decimal s_Porcentaje { get; set; }

        public decimal s_ValorBase { get; set; }

        public decimal s_Deduccion { get; set; }


        //FondoPension

        public decimal FP_Porcentaje { get; set; }

        public decimal FP_ValorBase { get; set; }

        public decimal FP_Deduccion { get; set; }

        //FondoSP
        public decimal FSP_Porcentaje { get; set; }

        public decimal FSP_Deduccion { get; set; }

        //Basico
        public byte DiasTrabajados { get; set; }

        public decimal SalarioTrabajado { get; set; }

        //VacacionesComunes
        public System.DateTime FechaInicio { get; set; }

        public System.DateTime FechaFin { get; set; }

        public byte Cantidad { get; set; }

        public decimal Pago { get; set; }

        //Bonificaciones

        public decimal BonificacionNS { get; set; }

        //Deducciones

        public decimal RetencionFuente { get; set; }

        public decimal AFC { get; set; }

        public decimal Deuda { get; set; }

        public decimal devengadosTotal { get; set; }

        public decimal deduccionesTotal { get; set; }

        public decimal comprobanteTotal { get; set; }

    }
}
