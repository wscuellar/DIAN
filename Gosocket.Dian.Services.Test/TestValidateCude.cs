using Gosocket.Dian.Services.Cude;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Gosocket.Dian.Services.Test
{
    [TestClass]
    public class TestValidateCude
    {
        
        [TestMethod]
        public void Should_ValidateCude_Ok()
        {
            Console.WriteLine("Nuevo Test");
            var invoiceDsTest = new DocumentoEquivalente()
            {
                NumFac = "0000000001",
                FecFac = "2020-10-24",
                HorFac = "14:04:35-05:00",
                ValFac = "15000.00",
                CodImp1 = "01",
                ValImp1 = "19.00",
                CodImp2 = "01",
                ValImp2 = "19.00",
                CodImp3 = "01",
                ValImp3 = "19.00",
                ValTol = "16350.00",
                NumOfe = "900373076",
                NitAdq = "8355990",
                SoftwarePin = "12345",
                TipoAmb = "1"
            };

            var composicionCudsEsperada = "00000000012020-10-2414:04:35-05:0015000.000119.0016350.009003730768355990123451";
            Console.WriteLine($"Combinación Cuds e:{composicionCudsEsperada}");
            Console.WriteLine($"Combinación Cuds r:{invoiceDsTest.ToCombinacionToCude()}");
            Assert.AreEqual(composicionCudsEsperada, invoiceDsTest.ToCombinacionToCude());
            
            var cudsEsperado = "bf4bb6920d5054ac065ddb7e6df0398e63e3ba2ff29cb341edd7d46ee8f2ea1802f84aaca91a19a24623e5e3baff3a71";
            Console.WriteLine($"Cuds e:{cudsEsperado}");
            Console.WriteLine($"Cuds r:{invoiceDsTest.ToCombinacionToCude().EncryptSHA384()}");
            
            Assert.AreEqual(cudsEsperado, invoiceDsTest.ToCombinacionToCude().EncryptSHA384());
        }
      
        [TestMethod]
        public void Should_Reader_Xml_Cude()
        { 
            var xmlEjemplo = @"\\EjemplosXml\\Ejemplo_POS_DIAN.xml";
            var pathFull = ObtenerPath(xmlEjemplo);
            var xmlBytes=File.ReadAllBytes(pathFull);
            Console.WriteLine(pathFull);
            Console.WriteLine("Validar carga de bytes");
            Assert.IsNotNull(xmlBytes);
            var invoceParser = new XmlToDocumentoEquivalenteParser();
            var invoceDe=invoceParser.Parser(xmlBytes);
            Assert.IsNotNull(invoceDe);
            invoceDe.SoftwarePin = "37346";
            Console.WriteLine($"Cude-{invoceDe.Cude}");
            Console.WriteLine(invoceDe.ToCombinacionToCude("*"));
            Console.WriteLine(invoceDe.Cude);
            Console.WriteLine(invoceDe.ToCombinacionToCude().EncryptSHA384());

        }
        public string ObtenerPath(string nameFile) => AppDomain.CurrentDomain.BaseDirectory + nameFile;
    }
}
