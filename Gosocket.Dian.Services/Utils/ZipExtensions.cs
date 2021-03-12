using Gosocket.Dian.Domain.Domain;
using Ionic.Zip;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gosocket.Dian.Services.Utils
{
    public static class ZipExtensions
    {
        public static byte[] CreateZip(this byte[] bytes, string name, string ext)
        {
            var zipFile = new ZipFile { CompressionLevel = CompressionLevel.BestCompression };
            zipFile.AddEntry(name + "." + ext, bytes);
            zipFile.Name = name + ".zip";
            using (var memoryStream = new MemoryStream())
            {
                zipFile.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static byte[] CreateMultipleZip(this byte[] xmlBytes, string fileName, byte[] cdrBytes, string cdrName)
        {
            var zipFile = new ZipFile { CompressionLevel = CompressionLevel.BestCompression };
            zipFile.AddEntry(fileName, xmlBytes);
            zipFile.AddEntry(cdrName, cdrBytes);
            zipFile.Name = fileName + ".zip";
            using (var memoryStream = new MemoryStream())
            {
                zipFile.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static byte[] CreateMultipleZip(string zipName, List<ResponseApplicationResponse> responses)
        {
            var zipFile = new ZipFile { CompressionLevel = CompressionLevel.BestCompression };
            zipFile.Name = $"{zipName}.zip";

            byte[] resultArray = new byte[] { };

            foreach (var item in responses.Distinct())
            {
                if (item.Content == null) continue;
                zipFile.AddEntry($"{item.DocumentKey}.xml", item.Content);                
            }

            using (var memoryStream = new MemoryStream())
            {
                zipFile.Save(memoryStream);
                resultArray = memoryStream.ToArray();
            }

            return resultArray;
        }

        public static byte[] ExtractZip(this byte[] zipBytes)
        {
            using (ZipFile zipFile = ZipFile.Read(new MemoryStream(zipBytes)))
            {
                byte[] numArray = null;
                foreach (ZipEntry zipEntry in zipFile)
                {
                    using (MemoryStream input = new MemoryStream())
                    {
                        zipEntry.Extract(input);
                        input.Position = 0L;
                        numArray = input.ReadFully();
                    }
                }
                return numArray;
            }

        }

        public static List<XmlBytesArrayParams> ExtractMultipleZip(this byte[] zipBytes, int maxFiles = 50)
        {
            List<XmlBytesArrayParams> listXmls = new List<XmlBytesArrayParams>();

            try
            {
                using (ZipFile zipFile = ZipFile.Read(new MemoryStream(zipBytes)))
                {
                    var files = zipFile.Entries.Where(o => !o.FileName.Contains("_MACOSX") && o.FileName != null && o.FileName.EndsWith(".xml"));

                    if (files.Count() == 0)
                    {
                        XmlBytesArrayParams param = new XmlBytesArrayParams
                        {
                            XmlFileName = "Archivo Comprimido",
                            MaxQuantityAllowedFailed = true,
                            XmlErrorCode = "EMPTY",
                            XmlErrorMessage = $"El archivo ZIP se encuentra vacío."
                        };
                        listXmls.Add(param);

                        return listXmls;
                    }

                    if (files.Count() > maxFiles)
                    {
                        XmlBytesArrayParams param = new XmlBytesArrayParams
                        {
                            XmlFileName = "Archivo Comprimido",
                            MaxQuantityAllowedFailed = true,
                            XmlErrorMessage = $"Cantidad de documentos dentro del lote ZIP excede máximo permitido: {maxFiles}"
                        };
                        listXmls.Add(param);

                        return listXmls;
                    }

                    Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 1 }, zipEntry =>
                    {
                        try
                        {
                            XmlBytesArrayParams param = new XmlBytesArrayParams();

                            using (MemoryStream input = new MemoryStream())
                            {
                                param.HasError = false;
                                zipEntry.Extract(input);
                                input.Position = 0L;
                                var name = new FileInfo(zipEntry.FileName).Name;
                                param.XmlFileName = name.Split('.').First();
                                param.XmlBytes = input.ReadFully();

                                if (param.XmlBytes.Length == 0)
                                    param.HasError = true;
                                param.XmlErrorMessage = param.HasError ? "El xml adjuntado se encuentra vacío." : "OK.";
                                listXmls.Add(param);
                            }
                        }
                        catch (Exception ex)
                        {
                            XmlBytesArrayParams param = new XmlBytesArrayParams();
                            var name = new FileInfo(zipEntry.FileName).Name;
                            param.XmlFileName = name.Split('.').First();
                            param.HasError = true;
                            param.XmlErrorMessage = $"Error descomprimiendo el fichero: {ex.Message}";
                            listXmls.Add(param);
                        }
                    });
                    return listXmls;
                }
            }
            catch (Exception)
            {
                XmlBytesArrayParams param = new XmlBytesArrayParams
                {
                    XmlFileName = "Archivo Comprimido",
                    UnzipError = true,
                    XmlErrorMessage = $"El archivo está corrupto."
                };
                listXmls.Add(param);
                return listXmls;
            }
        }

        public static bool ZipContainsXmlFiles(this byte[] zipBytes)
        {
            using (ZipFile zipFile = ZipFile.Read(new MemoryStream(zipBytes)))
            {
                var files = zipFile.Entries.Where(o => !o.FileName.Contains("_MACOSX") && o.FileName != null && o.FileName.EndsWith(".xml"));
                return files.Count() > 0;
            }
        }
    }
}