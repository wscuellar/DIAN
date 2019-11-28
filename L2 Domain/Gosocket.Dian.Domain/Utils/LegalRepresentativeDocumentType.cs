using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gosocket.Dian.Domain.Utils
{
    public static class LegalRepresentativeDocumentType
    {
        public static Dictionary<int, string> TypesList = new Dictionary<int, string>() {
            { 11, "Registro civil" },
            { 12, "Tarjeta de identidad" },
            { 13, "Cédula de ciudadanía" },
            { 21, "Tarjeta de extranjería" },
            { 22, "Cédula de extranjería" },
            { 31, "NIT" },
            { 41, "Pasaporte" },
            { 42, "Documento de identificación extranjero" },
            { 91, "NUIP" }
        };
    }
}
