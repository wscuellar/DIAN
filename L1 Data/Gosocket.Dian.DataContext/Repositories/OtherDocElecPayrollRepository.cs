using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.DataContext.Repositories
{
    public class OtherDocElecPayrollRepository : IOtherDocElecPayrollRepository
    {
        public Guid AddOrUpdate(OtherDocElecPayroll otherDocElecPayroll)
        {
            using (var context = new SqlDBContext())
            {
                OtherDocElecPayroll otherDocElecPayrollInstance = context.OtherDocElecPayroll.FirstOrDefault(c => c.Id == otherDocElecPayroll.Id);

                if (otherDocElecPayrollInstance != null)
                {
                    otherDocElecPayrollInstance.Id = otherDocElecPayroll.Id;
                    //otherDocElecPayrollInstance.Name = otherDocElecPayroll.Name;
                    //otherDocElecPayrollInstance.Pin = otherDocElecPayroll.Pin;
                    //otherDocElecPayrollInstance.Url = otherDocElecPayroll.Url;
                    //otherDocElecPayrollInstance.Status = otherDocElecPayroll.Status;
                    //otherDocElecPayrollInstance.Deleted = otherDocElecPayroll.Deleted;
                    //otherDocElecPayrollInstance.Updated = otherDocElecPayroll.Updated;
                    //otherDocElecPayrollInstance.OtherDocElecSoftwareStatusId = otherDocElecPayroll.OtherDocElecSoftwareStatusId;
                    context.Entry(otherDocElecPayrollInstance).State = EntityState.Modified;
                }
                else
                {
                    otherDocElecPayroll.Id = otherDocElecPayroll.Id == Guid.Empty ? Guid.NewGuid() : otherDocElecPayroll.Id;
                    //otherDocElecPayroll.Updated = DateTime.Now;
                    //otherDocElecPayroll.OtherDocElecSoftwareStatusId = 1;
                    context.Entry(otherDocElecPayroll).State = EntityState.Added;
                }

                context.SaveChanges();

                return otherDocElecPayrollInstance != null ? otherDocElecPayrollInstance.Id : otherDocElecPayroll.Id;
            }
        }

        public List<OtherDocElecPayroll> GetByFilters(int take, DateTime? monthStart, DateTime? monthEnd, double? enumerationStart, double? enumerationEnd,
            string employeeDocType, string employeeDocNumber, string firstSurname, double? employeeSalaryStart, double? employeeSalaryEnd, string employeeCity)
        {
            string sqlQuery = string.Empty;
            string sqlQueryFROM = $" SELECT TOP {take} * FROM OtherDocElecPayroll ";
            string sqlQueryConditions = string.Empty;
            string sqlQueryOrderBy = "ORDER BY DESC Timestamp";

            if (enumerationStart.HasValue)
            {
                sqlQueryConditions += $" (Consecutivo BETWEEN {enumerationStart.HasValue} AND {enumerationEnd.Value}) AND ";
            }

            if (monthStart.HasValue)
            {
                sqlQueryConditions += $" (FechaPagoInicio BETWEEN {enumerationStart.HasValue} AND {enumerationEnd.Value}) AND ";

                // por alguna razón el tiempo lo inicia con 05:...
                sqlQueryConditions = sqlQueryConditions.Replace("T05", "T00");
            }

            if (!string.IsNullOrWhiteSpace(employeeDocType))
            {
                sqlQueryConditions += $" TipoDocumento = {employeeDocType} AND ";
            }

            if (!string.IsNullOrWhiteSpace(employeeDocNumber))
            {
                sqlQueryConditions += $" NumeroDocumento = {employeeDocNumber} AND ";
            }

            if (!string.IsNullOrWhiteSpace(firstSurname))
            {
                sqlQueryConditions += $" PrimerApellido = {firstSurname} AND ";
            }

            if (employeeSalaryStart.HasValue)
            {
                sqlQueryConditions += $" Sueldo BETWEEN {employeeSalaryStart.HasValue} AND {employeeSalaryEnd.Value} AND ";
            }

            if (!string.IsNullOrWhiteSpace(employeeCity))
            {
                sqlQueryConditions += $" LugarTrabajoMunicipioCiudad = {employeeCity} AND ";
            }

            if (string.IsNullOrEmpty(sqlQueryConditions))
            {
                sqlQuery = $"{sqlQueryFROM} {sqlQueryOrderBy}";
            }
            else
            {
                sqlQueryConditions = sqlQueryConditions.Substring(0, sqlQueryConditions.Length - 4);

                sqlQuery = $"{sqlQueryFROM} WHERE {sqlQueryConditions} {sqlQueryOrderBy}";
            }

            using (var context = new SqlDBContext())
            {
                List<OtherDocElecPayroll> otherDocElecPayrolls = new List<OtherDocElecPayroll>();

                otherDocElecPayrolls = context.OtherDocElecPayroll.SqlQuery(sqlQuery).ToList<OtherDocElecPayroll>();

                return otherDocElecPayrolls; 
            }
        }
    }
}
