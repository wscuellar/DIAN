using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Gosocket.Dian.TestProject2.Services
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestDate()
        {
            string result = DateNormalized("Documento generado el: 03/02/2021 18:37:27", "Documento generado el:");
            Assert.IsNotNull(result);
        }

        private string DateNormalized(string currentDate, string dataReplace)
        {
            string[] dateparts = currentDate.Replace(dataReplace, "").Trim().Split('/');
            string[] hours = dateparts[2].Split(':');
            string[] years = hours[0].Split(' ');
            string[] rest = years[1].Split(':');
            int year = Convert.ToInt32(years[0]);
            int month = Convert.ToInt32(dateparts[1]);
            int day = Convert.ToInt32(dateparts[0]);
            int hour = Convert.ToInt32(rest[0]);
            int min = Convert.ToInt32(hours[1]);
            int sec = Convert.ToInt32(hours[2]);
            return dataReplace + " " + (new DateTime(year, month, day, hour, min, sec).AddHours(-5)).ToString("dd/MM/yyyy HH:mm:ss");

        }
    }
}
