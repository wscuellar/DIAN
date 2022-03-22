﻿using Gosocket.Dian.Services.Cuds;
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
        public string ObtenerPath(string nameFile) => AppDomain.CurrentDomain.BaseDirectory + nameFile;
    }
    
}
