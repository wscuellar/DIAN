﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Infrastructure.Utils
{

    public class BusinessDaysHolidays
    {
        //Lista de días festivos excluidos los fines de semana
        private static readonly DateTime[] bankHolidays=
        { 
            Convert.ToDateTime("1/01/2020"),
            Convert.ToDateTime("6/01/2020"),
            Convert.ToDateTime("23/03/2020"),
            Convert.ToDateTime("9/04/2020"),
            Convert.ToDateTime("10/04/2020"),
            Convert.ToDateTime("1/05/2020"),
            Convert.ToDateTime("25/05/2020"),
            Convert.ToDateTime("15/06/2020"),
            Convert.ToDateTime("22/06/2020"),
            Convert.ToDateTime("29/06/2020"),
            Convert.ToDateTime("20/07/2020"),
            Convert.ToDateTime("7/08/2020"),
            Convert.ToDateTime("17/08/2020"),
            Convert.ToDateTime("12/10/2020"),
            Convert.ToDateTime("2/11/2020"),
            Convert.ToDateTime("16/11/2020"),
            Convert.ToDateTime("8/12/2020"),
            Convert.ToDateTime("25/12/2020")
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
            int businessDays = span.Days + 1;
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
