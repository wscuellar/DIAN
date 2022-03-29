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
                ValImp1 = "0.00",
                CodImp2 = "04",
                ValImp2 = "0.00",
                CodImp3 = "03",
                ValImp3 = "0.00",
                ValTol = "17000.00",
                NumOfe = "900373076",
                NitAdq = "8355990",
                SoftwarePin = "12345",
                TipoAmb = "1"
            };

            var composicionCudsEsperada = "00000000012020-10-2414:04:35-05:0015000.00010.00040.00030.0017000.009003730768355990123451";
            Console.WriteLine($"Combinación Cude esperado:{composicionCudsEsperada}");
            Console.WriteLine($"Combinación Cude real    :{invoiceDsTest.ToCombinacionToCude()}");
            Assert.AreEqual(composicionCudsEsperada, invoiceDsTest.ToCombinacionToCude());
            
            var cudsEsperado = "92c29e6b17a584f7a67ea53e5666980bde27bc1c2b2aefbc8ba42366ff11a246982ad27075e623429efd5ed1fbcbb57b";
            Console.WriteLine($"Cuds esperado:{cudsEsperado}");
            Console.WriteLine($"Cuds real    :{invoiceDsTest.ToCombinacionToCude().EncryptSHA384()}");
            
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
            invoceDe.SoftwarePin = "12345";
            Console.WriteLine($"Cude-{invoceDe.Cude}");
            Console.WriteLine(invoceDe.ToCombinacionToCude("*"));
            Console.WriteLine(invoceDe.Cude);
            Console.WriteLine(invoceDe.ToCombinacionToCude().EncryptSHA384());
            //Assert.AreEqual(invoceDe.Cude, invoceDe.ToCombinacionToCude().EncryptSHA384());
        }
        public string ObtenerPath(string nameFile) => AppDomain.CurrentDomain.BaseDirectory + nameFile;

        [TestMethod]
        public void Should_ValidateCude_Ok_Case_Bug_13620()
        {
            Console.WriteLine("Nuevo Test [Bug 13620]");
            var invoiceDsTest = new DocumentoEquivalente()
            {
                NumFac = "DBV01",
                FecFac = "2022-03-14",
                HorFac = "15:05:40-05:00",
                ValFac = "20753.80",
                CodImp1 = "01",
                ValImp1 = "19.00",
                CodImp2 = "04",
                ValImp2 = "0.00",
                CodImp3 = "03",
                ValImp3 = "0.00",
                ValTol = "24697.02",
                NumOfe = "800197268",
                NitAdq = "856987456",
                SoftwarePin = "12345",
                TipoAmb = "2"
            };

            var composicionCudsEsperada = "DBV012022-03-1415:05:40-05:0020753.800119.00040.00030.0024697.02800197268856987456123452";
            Console.WriteLine($"Combinación Cude esperado:{composicionCudsEsperada}");
            Console.WriteLine($"Combinación Cude real    :{invoiceDsTest.ToCombinacionToCude()}");
            Assert.AreEqual(composicionCudsEsperada, invoiceDsTest.ToCombinacionToCude());

            var cudsEsperado = "6b0ea6e48a74ce59faeed218f96b5e1af55101bcfdc60db549f77d5a4e0046a03d36b0189dfef671aef1904f6baf4a3b";
            Console.WriteLine($"Cuds esperado:{cudsEsperado}");
            Console.WriteLine($"Cuds real    :{invoiceDsTest.ToCombinacionToCude().EncryptSHA384()}");

            Assert.AreEqual(cudsEsperado, invoiceDsTest.ToCombinacionToCude().EncryptSHA384());
        }
    }
}
