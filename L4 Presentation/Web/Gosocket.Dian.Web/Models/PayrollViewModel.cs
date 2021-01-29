﻿using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class PayrollViewModel
    {
        [Display(Name = "CUNE")]
        public string CUNE { get; set; }
        [Display(Name = "Mes Validacion")]
        public string MesValidacion { get; set; }
        public List<MesModel> MesesValidacion { get; set; }
        [Display(Name = "Del")]
        public string RangoNumeracionMenor { get; set; }
        [Display(Name = "Al")]
        public string RangoNumeracionMayor { get; set; }
        [Display(Name = "Letra Primer Apellido")]
        public string LetraPrimerApellido { get; set; }
        public List<LetraModel> LetrasPrimerApellido { get; set; }
        [Display(Name = "Tipo Documento")]
        public string TipoDocumento { get; set; }
        public List<TipoDocumentoModel> TiposDocumento { get; set; }
        [Display(Name = "Numero Documento")]
        public string NumeroDocumento { get; set; }
        public string Ciudad { get; set; }
        public List<CiudadModelList.CiudadModel> Ciudades { get; set; }
        [Display(Name = "Rango Salarial")]
        public string RangoSalarial { get; set; }
        public List<RangoSalarialModel> RangosSalarial { get; set; }
        public string Ordenar { get; set; }
        public List<OrdenarModel> Ordenadores { get; set; }
        public string Mensaje { get; set; }
        public List<DocumentViewPayroll> Payrolls { get; set; }
    }
    public class DocumentViewPayroll
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string link { get; set; }
        public string NumeroNomina { get; set; }
        public string ApellidosNombre { get; set; }
        public string TipoDocumento { get; set; }
        public string NoDocumento { get; set; }
        public string Salario { get; set; }
        public string Devengado { get; set; }
        public string Deducido { get; set; }
        public string ValorTotal { get; set; }
        public string MesValidacion { get; set; }
        public bool? Novedad { get; set; }
        public string NumeroAjuste { get; set; }
        public string Resultado { get; set; }
        public string Ciudad { get; set; }
    }

    public class LetraModel
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static List<LetraModel> List()
        {
            return new List<LetraModel>()
            {
                new LetraModel() { Code = "00", Name = "Todas" },
                new LetraModel() { Code = "01", Name = "A" },
                new LetraModel() { Code = "02", Name = "B" },
                new LetraModel() { Code = "03", Name = "C" },
                new LetraModel() { Code = "04", Name = "D" },
                new LetraModel() { Code = "05", Name = "E" },
                new LetraModel() { Code = "06", Name = "F" },
                new LetraModel() { Code = "07", Name = "G" },
                new LetraModel() { Code = "08", Name = "H" },
                new LetraModel() { Code = "09", Name = "I" },
                new LetraModel() { Code = "10", Name = "J" },
                new LetraModel() { Code = "11", Name = "K" },
                new LetraModel() { Code = "12", Name = "L" },
                new LetraModel() { Code = "13", Name = "M" },
                new LetraModel() { Code = "14", Name = "N" },
                new LetraModel() { Code = "15", Name = "Ñ" },
                new LetraModel() { Code = "16", Name = "O" },
                new LetraModel() { Code = "17", Name = "P" },
                new LetraModel() { Code = "18", Name = "Q" },
                new LetraModel() { Code = "19", Name = "R" },
                new LetraModel() { Code = "20", Name = "S" },
                new LetraModel() { Code = "21", Name = "T" },
                new LetraModel() { Code = "22", Name = "V" },
                new LetraModel() { Code = "23", Name = "W" },
                new LetraModel() { Code = "24", Name = "X" },
                new LetraModel() { Code = "25", Name = "Y" },
                new LetraModel() { Code = "26", Name = "Z" }
            };
        }
    }

    public class TipoDocumentoModel
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static List<TipoDocumentoModel> List()
        {
            return new List<TipoDocumentoModel>()
            {
                new TipoDocumentoModel() { Code = "00", Name = "Todos" },
                new TipoDocumentoModel() { Code = "01", Name = "CC - Cédula de Ciudadanía" },
                new TipoDocumentoModel() { Code = "02", Name = "CE - Cédula de Extranjería" },
                new TipoDocumentoModel() { Code = "03", Name = "No. Identificación Personal" },
                new TipoDocumentoModel() { Code = "04", Name = "No. Identificación Tributaria" },
                new TipoDocumentoModel() { Code = "05", Name = "TI - Tarjeta de identidad" },
                new TipoDocumentoModel() { Code = "06", Name = "PS - Pasaporte" }
            };
        }
    }

    public class RangoSalarialModel
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static List<RangoSalarialModel> List()
        {
            return new List<RangoSalarialModel>()
            {
                new RangoSalarialModel() { Code = "00", Name = "Todos" },
                new RangoSalarialModel() { Code = "01", Name = "0 - 1.000.000" },
                new RangoSalarialModel() { Code = "02", Name = "1.000.001 - 2.000.000" },
                new RangoSalarialModel() { Code = "03", Name = "2.000.001- 3.000.000" },
                new RangoSalarialModel() { Code = "04", Name = "3.000.001- 5.000.000" },
                new RangoSalarialModel() { Code = "05", Name = "5.000.001 - 10.000.000" },
                new RangoSalarialModel() { Code = "06", Name = "10.000.001 - 20.000.000" },
                new RangoSalarialModel() { Code = "07", Name = "20.000.001 o mas" }
            };
        }
    }

    public class MesModel
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static List<MesModel> List()
        {
            return new List<MesModel>()
            {
                new MesModel() { Code = "00", Name = "Todos" },
                new MesModel() { Code = "01", Name = "Enero" },
                new MesModel() { Code = "02", Name = "Febrero" },
                new MesModel() { Code = "03", Name = "Marzo" },
                new MesModel() { Code = "04", Name = "Abril" },
                new MesModel() { Code = "05", Name = "Mayo" },
                new MesModel() { Code = "06", Name = "Junio" },
                new MesModel() { Code = "07", Name = "Julio" },
                new MesModel() { Code = "08", Name = "Agosto" },
                new MesModel() { Code = "09", Name = "Septiembre" },
                new MesModel() { Code = "10", Name = "Octubre" },
                new MesModel() { Code = "11", Name = "Noviembre" },
                new MesModel() { Code = "12", Name = "Diciembre" }
            };
        }
    }

    public class OrdenarModel
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static List<OrdenarModel> List()
        {
            return new List<OrdenarModel>()
            {
                new OrdenarModel() { Code = "00", Name = "Sin Orden" },
                new OrdenarModel() { Code = "01", Name = "Mayor Valor" },
                new OrdenarModel() { Code = "02", Name = "Menor Valor" },
                new OrdenarModel() { Code = "03", Name = "A-Z" },
                new OrdenarModel() { Code = "04", Name = "Z-A" },
            };
        }
    }

    public class CiudadModelList
    {
        public TableManager municipalityTableManager = new TableManager("MunicipalityByCode", ConfigurationManager.GetValue("GlobalBillerStorage"));
        public class CiudadModel
        {
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public List<CiudadModel> List()
        {
            List<CiudadModel> result = new List<CiudadModel>();
            result.Add(new CiudadModel() { Code = "00", Name = "Todas" });
            var cities = municipalityTableManager.FindAll<MunicipalityByCode>();
            foreach(var city in cities)
                result.Add(new CiudadModel() { Code = city.Code, Name = city.Name });
            return result;
        }

    }
}