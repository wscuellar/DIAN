using Ionic.Zip;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;

namespace Gosocket.Dian.Infrastructure.Utils
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

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="zipFileName"></param>
        /// <returns></returns>
        public static byte[] CreateMultipleZip(List<Tuple<string, byte[]>> files, string zipFileName)
        {
            var zipFile = new ZipFile { CompressionLevel = CompressionLevel.BestCompression };
            foreach (var file in files)
            {
                if (file != null)
                {
                    zipFile.AddEntry(file.Item1, file.Item2);
                }
            }
            zipFile.Name = zipFileName + ".zip";
            using (var memoryStream = new MemoryStream())
            {
                zipFile.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static byte[] ExtractZip(this byte[] zipBytes)
        {
            ZipFile zipFile = ZipFile.Read(new MemoryStream(zipBytes));
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
}
