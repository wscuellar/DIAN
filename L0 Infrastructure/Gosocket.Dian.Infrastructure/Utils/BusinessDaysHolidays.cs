using System;
using System.Globalization;

namespace Gosocket.Dian.Infrastructure.Utils
{

    public class BusinessDaysHolidays
    {
        //Lista de días festivos excluidos los fines de semana
        private static readonly DateTime[] bankHolidays=
        {
            DateTime.ParseExact("01/01/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime.ParseExact("21/03/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),            
            DateTime.ParseExact("14/04/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime.ParseExact("15/04/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime.ParseExact("30/05/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),            
            DateTime.ParseExact("20/06/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime.ParseExact("27/06/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),            
            DateTime.ParseExact("04/07/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime.ParseExact("20/07/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime.ParseExact("15/08/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),            
            DateTime.ParseExact("17/10/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime.ParseExact("07/11/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime.ParseExact("14/11/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime.ParseExact("08/12/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTime.ParseExact("25/12/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture),
        };
        /// <summary>
        /// Calcula el número de días hábiles, teniendo en cuenta:
        /// - fines de semana (sábados y domingos)
        /// - festivos a mitad de semana
        /// </summary>
        /// <param name = "firstDay"> Primer día del intervalo de tiempo </param>
        /// <param name = "lastDay"> Último día del intervalo de tiempo </param>
        /// <returns> Número de días hábiles durante el 'lapso' </returns>
        public static int BusinessDaysUntil(DateTime firstDay, DateTime lastDay)
        {
            firstDay = firstDay.Date;
            lastDay = lastDay.Date;
            if (firstDay > lastDay)
                throw new ArgumentException("Incorrect last day " + lastDay);

            TimeSpan span = lastDay - firstDay;
            int businessDays = span.Days;
            int fullWeekCount = businessDays / 7;
            //  averigüe si hay fines de semana durante el tiempo que exceden las semanas completas
            if (businessDays > fullWeekCount * 7)
            {
                // estamos aquí para averiguar si hay un fin de semana de 1 día o 2 días
                // en el intervalo de tiempo restante después de restar las semanas completas
                int firstDayOfWeek = firstDay.DayOfWeek == DayOfWeek.Sunday
                    ? 7 : (int)firstDay.DayOfWeek;
                int lastDayOfWeek = lastDay.DayOfWeek == DayOfWeek.Sunday
                    ? 7 : (int)lastDay.DayOfWeek;
                if (lastDayOfWeek < firstDayOfWeek)
                    lastDayOfWeek += 7;
                if (firstDayOfWeek <= 6)
                {
                    if (lastDayOfWeek >= 7)//  Tanto el sábado como el domingo están en el intervalo de tiempo restante
                        businessDays -= 2;
                    else if (lastDayOfWeek >= 6)// Solo el sábado está en el intervalo de tiempo restante
                        businessDays -= 1;
                }
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Solo el domingo está en el intervalo de tiempo restante
                    businessDays -= 1;
            }

            // restar los fines de semana durante las semanas completas del intervalo
            businessDays -= fullWeekCount + fullWeekCount;

            // restar el número de festivos durante el intervalo de tiempo
            foreach (DateTime bankHoliday in bankHolidays)
            {
                DateTime bh = bankHoliday.Date;
                if (firstDay <= bh && bh <= lastDay)
                    --businessDays;
            }

            return businessDays;
        }
    }
}
