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
        public string FechaIngreso { get; set; }
        public string FechaPagoInicio { get; set; }
        public string FechaPagoFin { get; set; }
        public string TiempoLaborado { get; set; }
        public string FechaLiquidacion { get; set; }
        public string FechaGen { get; set; }

        //NumeroSecuenciaXML
        public string CodigoTrabajador { get; set; }
        public string Prefijo { get; set; }
        public string Consecutivo { get; set; }
        public string Numero { get; set; }

        //LugarGeneracionXML
        public string Pais { get; set; }
        public string DepartamentoEstado { get; set; }
        public string MunicipioCiudad { get; set; }
        public string Idioma { get; set; }

        //ProveedorXml
        public string NIT { get; set; }
        public string DV { get; set; }
        public string SoftwareID { get; set; }
        public string SoftwareSC { get; set; }

        //CodigoQR
        public string CodigoQR { get; set; }

        //InformacionGeneral
        public string Ambiente { get; set; }
        public string CUNE { get; set; }
        public string EncripCUNE { get; set; }
        public string Info_FechaGen { get; set; }
        public string HoraGen { get; set; }
        public string PeriodoNomina { get; set; }
        public string TRM { get; set; }
        public string TipoMoneda { get; set; }
        public string TipoNomina { get; set; }
        public string Version { get; set; }

        //Notas
        public string Notas { get; set; }

        //ReemplazandoPredecesor
        public string NumeroPred { get; set; }
        public string CUNEPred { get; set; }
        public System.DateTime? FechaGenPred { get; set; }

        //Empleador
        public string Emp_RazonSocial { get; set; }
        public string Emp_NIT { get; set; }
        public string Emp_DV { get; set; }
        public string Emp_Pais { get; set; }
        public string Emp_DepartamentoEstado { get; set; }
        public string Emp_MunicipioCiudad { get; set; }
        public string Emp_Direccion { get; set; }

        //Trabajador
        public string TipoTrabajador { get; set; }
        public string SubTipoTrabajador { get; set; }
        public bool AltoRiesgoPension { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string PrimerNombre { get; set; }
        public string OtrosNombres { get; set; }
        public string LugarTrabajoPais { get; set; }
        public string LugarTrabajoDepartamentoEstado { get; set; }
        public string LugarTrabajoMunicipioCiudad { get; set; }
        public string LugarTrabajoDireccion { get; set; }
        public bool SalarioIntegral { get; set; }
        public string TipoContrato { get; set; }
        public double Sueldo { get; set; }
        public string Trab_CodigoTrabajador { get; set; }

        //Pago
        public string Forma { get; set; }
        public string Metodo { get; set; }
        public string Banco { get; set; }
        public string TipoCuenta { get; set; }
        public string NumeroCuenta { get; set; }

        //Devengados
        //Basico
        public string DiasTrabajados { get; set; }
        public string SalarioTrabajado { get; set; }

        //HE
        public string HED { get; set; }
        public string HEN { get; set; }
        public string HRN { get; set; }
        public string HEDDF { get; set; }
        public string HRDDF { get; set; }
        public string HENDF { get; set; }
        public string HRNDF { get; set; }

        //VacacionesComunes
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string Cantidad { get; set; }
        public string Pago { get; set; }

        //Primas
        public string Pri_Cantidad { get; set; }
        public string Pri_Pago { get; set; }
        public string Pri_PagoNS { get; set; }

        //Cesantías
        public string Ces_Pago { get; set; }
        public string Ces_PagoIntereses { get; set; }
        public string Ces_Porcentaje { get; set; }

        //Incapacidades
        public string Inc_Cantidad { get; set; }
        public string Inc_Pago { get; set; }

        //Licencias
        public string Lic_Cantidad { get; set; }
        public string Lic_Pago { get; set; }

        //Transporte
        public string AuxTransporte { get; set; }

        //Bonificaciones
        public string BonificacionNS { get; set; }
        public string BonificacionS { get; set; }

        //Comisiones
        public string Comisiones { get; set; }

        //Compensaciones
        public string CompensacionE { get; set; }
        public string CompensacionO { get; set; }

        //Deducciones
        //Salud
        public string s_Porcentaje { get; set; }
        public string s_ValorBase { get; set; }
        public string s_Deduccion { get; set; }

        //FondoPension
        public string FP_Porcentaje { get; set; }
        public string FP_ValorBase { get; set; }
        public string FP_Deduccion { get; set; }

        //Retefuente
        public string RetencionFuente { get; set; }

        //FondoSP
        public string FSP_Porcentaje { get; set; }
        public string FSP_Deduccion { get; set; }
        public string FSP_DeduccionSub { get; set; }
        public string FSP_PorcentajeSub { get; set; }

        // ...
        public string AFC { get; set; }
        public string Deuda { get; set; }
        public double DevengadosTotal { get; set; }
        public double DeduccionesTotal { get; set; }
        public double ComprobanteTotal { get; set; }

        //Novedad
        public bool Novedad { get; set; }
    }
}
