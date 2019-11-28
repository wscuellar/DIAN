using Gosocket.Dian.Services.ServicesGroup;
using System;
using System.IO;

namespace Gosocket.Dian.Consoles
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DianPAServices service = new DianPAServices();
                var allZipBytes = File.ReadAllBytes(@"D:\ValidarDianXmls\DianXmlsALot50.zip");
                var responses = service.SendBillAsync2("DianXmlsALot50", allZipBytes);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
