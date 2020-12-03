using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Gosocket.Dian.Infrastructure
{
    public class CertificateManager
    {
    }

    public class ExportCertificateRepsonse : CertificateResponse
    {
        public string Base64Data { get; set; }
        public string Password { get; set; }
    }

    public abstract class CertificateResponse
    {
        public bool Success { get; set; }
        public string Name { get; set; }
        public string Error { get; set; }
    }
}
