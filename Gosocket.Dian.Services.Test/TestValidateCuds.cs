using Gosocket.Dian.Services.Cuds;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Gosocket.Dian.Services.Test
{
    [TestClass]
    public class TestValidateCuds
    {
        //[Ignore]
        [TestMethod]
        public void Should_ValidateCudsDian_ConTrackIdSofia0803222()
        {
        /*
        DS236000000
        2022-02-18
        13:34:59-05:00
        3899000.00
        01
        322430.00
        4152176.00
        1020
        20227859
        12345
        2
         */
        /*
        CodImp: "01"
        Cuds: "63930c174e440328eccc4545712ed9f26ef3d7923e6b528a25df11b6f76dcd9f8e457a1b7b72bd3d85f210c3ed3ba924"
        DocumentType: "05"
        FecDs: "2022-02-18"
        HorDs: "13:34:59-05:00"
        NitAbs: "20227859"
        NumDs: "DS236000000"
        NumSno: "1020"
        SoftwareId: "963a4c64-d2a8-4edd-ada9-331eb47be323"
        SoftwarePin: null
        TipoAmb: "2"
        ValDs: "3899000.00"
        ValImp: "432530.00"
        ValTol: "4152176.00"*/
            Console.WriteLine("Nuevo Test");
            var invoiceDsTest = new DocumentoSoporte()
            {
                NumDs = "DS236000000",
                FecDs = "2022-02-18",
                HorDs = "13:34:59-05:00",
                ValDs = "3899000.00",
                CodImp = "01",
                ValImp = "432530.00",
                ValTol = "4152176.00",
                NumSno = "1020",
                NitAbs = "20227859",
                SoftwarePin = "12345",//
                TipoAmb = "2"
            };

            var cudsEsperado = "b42c77ce05c477e20ed50aa84f43019dd36019d6564dbbd650c50acdf531fd1945809d7da8a97f1cf6681eabdd8d4358";
            Console.WriteLine($"Cuds e:{cudsEsperado}");
            Console.WriteLine($"Cuds c:{invoiceDsTest.ToCombinacionToCuds()}");
            Console.WriteLine($"Cuds r:{invoiceDsTest.ToCombinacionToCuds().EncryptSHA384()}");

            Assert.AreEqual(cudsEsperado, invoiceDsTest.ToCombinacionToCuds().EncryptSHA384());
        }

        [TestMethod]
        public void Should_ValidateCudsDian_ConTrackIdWilliamXml()
        {
            Console.WriteLine("Nuevo Test");
            var invoiceDsTest = new DocumentoSoporte()
            {
                NumDs = "DS236000000",
                FecDs = "2022-02-18",
                HorDs = "13:34:59-05:00",
                ValDs = "3899000.00",
                CodImp = "01",
                ValImp = "432530.00",
                ValTol = "4152176.00",
                NumSno = "1020",
                NitAbs = "20227859",
                SoftwarePin = "12345",//
                TipoAmb = "2"
            };

            var cudsEsperado  = "63930c174e440328eccc4545712ed9f26ef3d7923e6b528a25df11b6f76dcd9f8e457a1b7b72bd3d85f210c3ed3ba924";
            Console.WriteLine($"Cuds e:{cudsEsperado}");
            Console.WriteLine($"Cuds r:{invoiceDsTest.ToCombinacionToCuds().EncryptSHA384()}");

            Assert.AreNotEqual(cudsEsperado, invoiceDsTest.ToCombinacionToCuds().EncryptSHA384());
        }


        [TestMethod]
        public void Should_ValidateCudsDian_Ok()
        {
            Console.WriteLine("Nuevo Test");
            var invoiceDsTest = new DocumentoSoporte()
            {
                NumDs = "DS236000000",
                FecDs = "2022-02-18",
                HorDs = "13:34:59-05:00",
                ValDs = "3899000.00",
                CodImp = "01",
                ValImp = "322430.00",
                ValTol = "4152178.00",
                NumSno = "1020",
                NitAbs = "20227859",
                SoftwarePin = "12345",//
                TipoAmb = "2"
            };

            //var composicionCudsEsperada = "00000000012020-10-2414:04:35-05:0015000.000119.0016350.009003730768355990123451";
            //Console.WriteLine($"Combinación Cuds e:{composicionCudsEsperada}");
            //Console.WriteLine($"Combinación Cuds r:{invoiceDsTest.ToCombinacionToCuds()}");
            //Assert.AreEqual(composicionCudsEsperada, invoiceDsTest.ToCombinacionToCuds());

            var cudsEsperado = "63930c174e440328eccc4545712ed9f26ef3d7923e6b528a25df11b6f76dcd9f8e457a1b7b72bd3d85f210c3ed3ba924";
            Console.WriteLine($"Cuds e:{cudsEsperado}");
            Console.WriteLine($"Cuds r:{invoiceDsTest.ToCombinacionToCuds().EncryptSHA384()}");

            Assert.AreEqual(cudsEsperado, invoiceDsTest.ToCombinacionToCuds().EncryptSHA384());
        }

        [TestMethod]
        public void Should_ValidateCuds_Ok()
        {
            Console.WriteLine("Nuevo Test");
            var invoiceDsTest = new DocumentoSoporte()
            {
                NumDs = "0000000001",
                FecDs = "2020-10-24",
                HorDs = "14:04:35-05:00",
                ValDs = "15000.00",
                CodImp = "01",
                ValImp = "19.00",
                ValTol = "16350.00",
                NumSno = "900373076",
                NitAbs = "8355990",
                SoftwarePin = "12345",
                TipoAmb = "1"
            };

            var composicionCudsEsperada = "00000000012020-10-2414:04:35-05:0015000.000119.0016350.009003730768355990123451";
            Console.WriteLine($"Combinación Cuds e:{composicionCudsEsperada}");
            Console.WriteLine($"Combinación Cuds r:{invoiceDsTest.ToCombinacionToCuds()}");
            Assert.AreEqual(composicionCudsEsperada, invoiceDsTest.ToCombinacionToCuds());
            
            var cudsEsperado = "bf4bb6920d5054ac065ddb7e6df0398e63e3ba2ff29cb341edd7d46ee8f2ea1802f84aaca91a19a24623e5e3baff3a71";
            Console.WriteLine($"Cuds e:{cudsEsperado}");
            Console.WriteLine($"Cuds r:{invoiceDsTest.ToCombinacionToCuds().EncryptSHA384()}");
            
            Assert.AreEqual(cudsEsperado, invoiceDsTest.ToCombinacionToCuds().EncryptSHA384());
        }

        [TestMethod]
        public void Should_Reader_Xml_Cuds()
        { 
            var xmlEjemplo = @"\\EjemplosXml\\Documento Soporte Invoice05 26-11-2021-firmado-SHA384.xml";
            var pathFull = ObtenerPath(xmlEjemplo);
            var xmlBytes=File.ReadAllBytes(pathFull);
            Console.WriteLine(pathFull);
            Console.WriteLine("Validar carga de bytes");
            Assert.IsNotNull(xmlBytes);
            var invoceParser = new XmlToDocumentoSoporteParser();
            var invoceDs=invoceParser.Parser(xmlBytes);
            Assert.IsNotNull(invoceDs);
            invoceDs.SoftwarePin = "37346";
            Console.WriteLine($"Cuds-{invoceDs.Cuds}");
            Console.WriteLine(invoceDs.ToCombinacionToCuds("*"));
            Console.WriteLine(invoceDs.Cuds);
            Console.WriteLine(invoceDs.ToCombinacionToCuds().EncryptSHA384());
            //Assert.AreEqual(invoceDs.Cuds, invoceDs.ToCombinacionToCuds().EncryptSHA384());

        }


        [TestMethod]
        public void Should_Reader_Xml_Cuds_14759()
        {
            var xmlEjemplo = @"\\EjemplosXml\\14759.xml";
            var pathFull = ObtenerPath(xmlEjemplo);
            var xmlBytes = File.ReadAllBytes(pathFull);
            Assert.IsNotNull(xmlBytes);
            var invoceParser = new XmlToDocumentoSoporteParser();
            var invoceDs = invoceParser.Parser(xmlBytes);
            Assert.IsNotNull(invoceDs);
            invoceDs.SoftwarePin = "81218";

            Console.WriteLine($"Cuds en Xml: {invoceDs.Cuds}");
            Console.WriteLine($"Cuds en Calculado: {invoceDs.ToCombinacionToCuds().EncryptSHA384()}");
            
            Console.WriteLine($"Parametros para armar CUDS separados por (*) {invoceDs.ToCombinacionToCuds("*")}");
            Console.WriteLine($"Parametros para armar CUDS separados {invoceDs.ToCombinacionToCuds("")}");

            Assert.AreEqual(invoceDs.Cuds, invoceDs.ToCombinacionToCuds().EncryptSHA384());

        }

        public string ObtenerPath(string nameFile) => AppDomain.CurrentDomain.BaseDirectory + nameFile;
    }
    
}
