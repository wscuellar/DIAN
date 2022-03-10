﻿using Gosocket.Dian.Domain.Domain;
using Gosocket.Dian.Functions.Utils;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Cuds;
using Gosocket.Dian.Services.Utils.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using OpenHtmlToPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using QRCoder;
using System.Drawing;
using Gosocket.Dian.DataContext;

namespace Gosocket.Dian.Functions.Pdf
{
    public static class PdfDS
    {
        private static readonly TableManager tableManagerGlobalDocValidatorDocumentMeta = new TableManager("GlobalDocValidatorDocumentMeta");

        [FunctionName("PdfDS")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                // Definir contenedor de parámetro
                string base64Xml = req.GetQueryNameValuePairs()
                    .FirstOrDefault(q => string.Compare(q.Key, "base64Xml", true) == 0)
                    .Value;

                // Obtener parámetros de consulta
                dynamic data = await req.Content.ReadAsAsync<object>();

                // Establecer nombre para consultar la cadena o los datos del cuerpo
                base64Xml = base64Xml ?? data?.base64Xml;

                if (base64Xml == null)
                    return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a base64Xml on the query string or in the request body");

                // Descargar Bytes de XML a partir de base64Xml
                var requestObj = new { base64Xml };
                var response = await Utils.Utils.DownloadXmlAsync(requestObj);
                //if (!response.Success)
                //    throw new Exception(response.Message);
                var base44 = base64Xml;// "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPEludm9pY2UgeG1sbnM6Y2FjPSJ1cm46b2FzaXM6bmFtZXM6c3BlY2lmaWNhdGlvbjp1Ymw6c2NoZW1hOnhzZDpDb21tb25BZ2dyZWdhdGVDb21wb25lbnRzLTIiIHhtbG5zOmNiYz0idXJuOm9hc2lzOm5hbWVzOnNwZWNpZmljYXRpb246dWJsOnNjaGVtYTp4c2Q6Q29tbW9uQmFzaWNDb21wb25lbnRzLTIiIHhtbG5zOmRzPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwLzA5L3htbGRzaWcjIiB4bWxuczpleHQ9InVybjpvYXNpczpuYW1lczpzcGVjaWZpY2F0aW9uOnVibDpzY2hlbWE6eHNkOkNvbW1vbkV4dGVuc2lvbkNvbXBvbmVudHMtMiIgeG1sbnM6c3RzPSJkaWFuOmdvdjpjbzpmYWN0dXJhZWxlY3Ryb25pY2E6U3RydWN0dXJlcy0yLTEiIHhtbG5zOnhhZGVzMTQxPSJodHRwOi8vdXJpLmV0c2kub3JnLzAxOTAzL3YxLjQuMSMiIHhtbG5zOnhhZGVzPSJodHRwOi8vdXJpLmV0c2kub3JnLzAxOTAzL3YxLjMuMiMiIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhtbG5zPSJ1cm46b2FzaXM6bmFtZXM6c3BlY2lmaWNhdGlvbjp1Ymw6c2NoZW1hOnhzZDpJbnZvaWNlLTIiIHhzaTpzY2hlbWFMb2NhdGlvbj0idXJuOm9hc2lzOm5hbWVzOnNwZWNpZmljYXRpb246dWJsOnNjaGVtYTp4c2Q6SW52b2ljZS0yIGh0dHA6Ly9kb2NzLm9hc2lzLW9wZW4ub3JnL3VibC9vcy1VQkwtMi4xL3hzZC9tYWluZG9jL1VCTC1JbnZvaWNlLTIuMS54c2QiPgoJPGV4dDpVQkxFeHRlbnNpb25zPgoJCTxleHQ6VUJMRXh0ZW5zaW9uPgoJCQk8ZXh0OkV4dGVuc2lvbkNvbnRlbnQ+CgkJCQk8c3RzOkRpYW5FeHRlbnNpb25zPgoJCQkJCTxzdHM6SW52b2ljZUNvbnRyb2w+CgkJCQkJCTxzdHM6SW52b2ljZUF1dGhvcml6YXRpb24+MTg3NjQwMTE4MTczNzc8L3N0czpJbnZvaWNlQXV0aG9yaXphdGlvbj4gPCEtLWF1dG9yaXphY2lvbiBkZSBudW1lcmFjaW9uLS0+CgkJCQkJCTxzdHM6QXV0aG9yaXphdGlvblBlcmlvZD4KCQkJCQkJCTxjYmM6U3RhcnREYXRlPjIwMjEtMDMtMjQ8L2NiYzpTdGFydERhdGU+CgkJCQkJCQk8Y2JjOkVuZERhdGU+MjAyMi0wMy0yNDwvY2JjOkVuZERhdGU+CgkJCQkJCTwvc3RzOkF1dGhvcml6YXRpb25QZXJpb2Q+CgkJCQkJCTxzdHM6QXV0aG9yaXplZEludm9pY2VzPgoJCQkJCQkJPHN0czpQcmVmaXg+UEU8L3N0czpQcmVmaXg+CgkJCQkJCQk8c3RzOkZyb20+MTwvc3RzOkZyb20+CgkJCQkJCQk8c3RzOlRvPjUwMDA8L3N0czpUbz4KCQkJCQkJPC9zdHM6QXV0aG9yaXplZEludm9pY2VzPgoJCQkJCTwvc3RzOkludm9pY2VDb250cm9sPgoJCQkJCTxzdHM6SW52b2ljZVNvdXJjZT4KCQkJCQkJPGNiYzpJZGVudGlmaWNhdGlvbkNvZGUgbGlzdEFnZW5jeUlEPSI2IiBsaXN0QWdlbmN5TmFtZT0iVW5pdGVkIE5hdGlvbnMgRWNvbm9taWMgQ29tbWlzc2lvbiBmb3IgRXVyb3BlIiBsaXN0U2NoZW1lVVJJPSJ1cm46b2FzaXM6bmFtZXM6c3BlY2lmaWNhdGlvbjp1Ymw6Y29kZWxpc3Q6Z2M6Q291bnRyeUlkZW50aWZpY2F0aW9uQ29kZS0yLjEiPkNPPC9jYmM6SWRlbnRpZmljYXRpb25Db2RlPgoJCQkJCTwvc3RzOkludm9pY2VTb3VyY2U+CgkJCQkJPHN0czpTb2Z0d2FyZVByb3ZpZGVyPgoJCQkJCQk8c3RzOlByb3ZpZGVySUQgc2NoZW1lQWdlbmN5SUQ9IjE5NSIgc2NoZW1lQWdlbmN5TmFtZT0iQ08sIERJQU4gKERpcmVjY2nDs24gZGUgSW1wdWVzdG9zIHkgQWR1YW5hcyBOYWNpb25hbGVzKSIgc2NoZW1lSUQ9IjIiIHNjaGVtZU5hbWU9IjMxIj45MDA4NTAyNTU8L3N0czpQcm92aWRlcklEPiA8IS0tbml0IGRlbCBmYWJyaWNhbnRlIGRlbCBzb2Z0d2FyZS0tPgoJCQkJCQk8c3RzOlNvZnR3YXJlSUQgc2NoZW1lQWdlbmN5SUQ9IjE5NSIgc2NoZW1lQWdlbmN5TmFtZT0iQ08sIERJQU4gKERpcmVjY2nDs24gZGUgSW1wdWVzdG9zIHkgQWR1YW5hcyBOYWNpb25hbGVzKSI+YmM3ZjBhNzEtYTYyMi00NmRkLTk1ZmItMWU4MmI4Y2YyZGVkPC9zdHM6U29mdHdhcmVJRD4KCQkJCQk8L3N0czpTb2Z0d2FyZVByb3ZpZGVyPgoJCQkJCTxzdHM6U29mdHdhcmVTZWN1cml0eUNvZGUgc2NoZW1lQWdlbmN5SUQ9IjE5NSIgc2NoZW1lQWdlbmN5TmFtZT0iQ08sIERJQU4gKERpcmVjY2nDs24gZGUgSW1wdWVzdG9zIHkgQWR1YW5hcyBOYWNpb25hbGVzKSI+M2Y1M2U3OGNkMzU5YWRiYzNiMGY4N2FkZTUwMDk3Y2VkZDM2ZWU0NGY5NGFjMGY0YTU4NzZhOTBkYTliYmE0MWYzOTU0YzBiOWYyNjNmZTUzZmU0ZDM1YzhhNWZhMmNlPC9zdHM6U29mdHdhcmVTZWN1cml0eUNvZGU+CgkJCQkJPHN0czpBdXRob3JpemF0aW9uUHJvdmlkZXI+CgkJCQkJCTxzdHM6QXV0aG9yaXphdGlvblByb3ZpZGVySUQgc2NoZW1lQWdlbmN5SUQ9IjE5NSIgc2NoZW1lQWdlbmN5TmFtZT0iQ08sIERJQU4gKERpcmVjY2nDs24gZGUgSW1wdWVzdG9zIHkgQWR1YW5hcyBOYWNpb25hbGVzKSIgc2NoZW1lSUQ9IjQiIHNjaGVtZU5hbWU9IjMxIj44MDAxOTcyNjg8L3N0czpBdXRob3JpemF0aW9uUHJvdmlkZXJJRD4KCQkJCQk8L3N0czpBdXRob3JpemF0aW9uUHJvdmlkZXI+CgkJCQkJPHN0czpRUkNvZGU+TnVtRmFjOiBGRTYyMSBGZWNGYWM6IDIwMjEtMDUtMjQgSG9yRmFjOiAxNTowNTo0MC0wNTowMCBOaXRGYWM6IDkwMDg1MDI1NSBEb2NBZHE6IDY3NjI0MTQgVmFsRmFjOiAzMjAxNi44MSBWYWxJdmE6IDYwODMuMTkgVmFsT3Ryb0ltOiAwLjAwIFZhbFRvbEZhYzogMzgxMDAuMDAgQ1VGRTogODQ3NDM1MThlOGZlNzVkNDE1MDljNTBmNTU5ZTk1NTQxNzNjYTkyZGM5YmM1YTBlNzVjMmNiM2YyZWQ2OWYwZGMzMThlZTFhNDY3YjQ5NjZkNGJiN2NlNTg4YzJlZWM3PC9zdHM6UVJDb2RlPgoJCQkJPC9zdHM6RGlhbkV4dGVuc2lvbnM+CgkJCTwvZXh0OkV4dGVuc2lvbkNvbnRlbnQ+CgkJPC9leHQ6VUJMRXh0ZW5zaW9uPgoJCTxleHQ6VUJMRXh0ZW5zaW9uPgoJCSAgICA8ZXh0OkV4dGVuc2lvbkNvbnRlbnQ+CgkJCSAgPEZhYnJpY2FudGVTb2Z0d2FyZT4KCQkJICAgIDxJbmZvcm1hY2lvbkRlbEZhYnJpY2FudGVEZWxTb2Z0d2FyZT4KCQkJCSAgICA8TmFtZT5Ob21icmVBcGVsbGlkbzwvTmFtZT4gPCEtLW5vbWJyZSB5IGFwZWxsaWRvcyBkZWwgZmFicmljYW50ZSBkZWwgc29mdHdhcmUtLT4KCQkJCQk8VmFsdWU+RXJpY2sgUmljbzwvVmFsdWU+CgkJCQkJPE5hbWU+UmF6b25Tb2NpYWw8L05hbWU+IDwhLS1SYXpvbiBzb2NpYWwgZGVsIGZhYnJpY2FudGUgZGVsIHNvZnR3YXJlLS0+CgkJCQkJPFZhbHVlPkNoaWEuc2FzPC9WYWx1ZT4KCQkJCQk8TmFtZT5Ob21icmVTb2Z0d2FyZTwvTmFtZT4gPCEtLW5vbWJyZSBkZWwgc29mdHdhcmUtLT4KCQkJCQk8VmFsdWU+VGhlUG9zPC9WYWx1ZT4KCQkJCTwvSW5mb3JtYWNpb25EZWxGYWJyaWNhbnRlRGVsU29mdHdhcmU+CgkJCSAgPC9GYWJyaWNhbnRlU29mdHdhcmU+CgkJCTwvZXh0OkV4dGVuc2lvbkNvbnRlbnQ+CgkJPC9leHQ6VUJMRXh0ZW5zaW9uPgoJICAgIDxleHQ6VUJMRXh0ZW5zaW9uPgoJCSAgICA8ZXh0OkV4dGVuc2lvbkNvbnRlbnQ+CgkJCSAgPFZlbmVmaWNpb3NDb21wcmFkb3I+CgkJCSAgIDxJbmZvcm1hY2lvbkJlbmVmaWNpb3NDb21wcmFkb3I+CgkJCSAgICAgICAgPE5hbWU+Q29kaWdvPC9OYW1lPiA8IS0tQ29kaWdvIGRlbCBjb21wcmFkb3ItLT4KCQkJCQk8VmFsdWU+Nzk5MDc3NTk8L1ZhbHVlPgoJCQkJCTxOYW1lPk5vbWJyZXNBcGVsbGlkb3M8L05hbWU+IDwhLS1Ob21icmVzIHkgYXBlbGxpZG9zIGRlbCBjb21wcmFkb3ItLT4KCQkJCQk8VmFsdWU+RWRpc29uIEhlcm5hbmRlejwvVmFsdWU+CgkJCQkgICAgPE5hbWU+UHVudG9zPC9OYW1lPiA8IS0tQ2FudGlkYWQgZGUgUHVudG9zIGFjdW11bGFkb3MgcG9yIGVsIGNvbXByYWRvci0tPgoJCQkJCTxWYWx1ZT4xMDA8L1ZhbHVlPgoJCQkJPC9JbmZvcm1hY2lvbkJlbmVmaWNpb3NDb21wcmFkb3I+CgkJCSAgPC9WZW5lZmljaW9zQ29tcHJhZG9yPgoJCQk8L2V4dDpFeHRlbnNpb25Db250ZW50PgoJCTwvZXh0OlVCTEV4dGVuc2lvbj4KCQk8ZXh0OlVCTEV4dGVuc2lvbj4KCQkgICAgPGV4dDpFeHRlbnNpb25Db250ZW50PgoJCQkgIDxQdW50b1ZlbnRhPgoJCQkgICA8SW5mb3JtYWNpb25DYWphVmVudGE+CgkJCSAgICAgICAgPE5hbWU+UGxhY2FDYWphPC9OYW1lPiA8IS0tUGxhY2EgZGUgaW52ZW50YXJpbyBkZSBsYSBDYWphLS0+CgkJCQkJPFZhbHVlPk5vIFJlZ2lzdHJhZGE8L1ZhbHVlPgoJCQkJCTxOYW1lPlViaWNhY2nDs25DYWphPC9OYW1lPiA8IS0tVWJpY2FjacOzbiBkZSBsYSBjYWphIEFMTUFDRU4tLT4KCQkJCQk8VmFsdWU+R2lsYmFyY28gRW5jb3JlIDQgTDEgLSBNYW5ndWUgcmEgMTcgQUM8L1ZhbHVlPgoJCQkgICAgICAgIDxOYW1lPkNhamVybzwvTmFtZT4gPCEtLURhdG9zIGRlbCBDYWplcm8gbyBWZW5kZWRvci0tPgoJCQkJCTxWYWx1ZT5ub21icmUgZGVsIGNhamVybzwvVmFsdWU+CgkJCQkJPE5hbWU+VGlwb0NhamE8L05hbWU+IDwhLS1UaXBvIGRlIENhamEtLT4KCQkJCQk8VmFsdWU+Q2FqYSBkZSBhcG95bzwvVmFsdWU+CgkJCQkgICAgPE5hbWU+Q8OzZGlnb1ZlbnRhPC9OYW1lPiA8IS0tQ8OzZGlnbyBkZSBsYSBWZW50YS0tPgoJCQkJCTxWYWx1ZT43NDM5OTI8L1ZhbHVlPgoJCQkJCTxOYW1lPlN1YlRvdGFsPC9OYW1lPiA8IS0tU3VidG90YWwgZGUgbGEgdmVudGEtLT4KCQkJCQk8VmFsdWU+NzI5OTAwPC9WYWx1ZT4KCQkJCTwvSW5mb3JtYWNpb25DYWphVmVudGE+CgkJCSAgPC9QdW50b1ZlbnRhPgoJCQk8L2V4dDpFeHRlbnNpb25Db250ZW50PgoJCTwvZXh0OlVCTEV4dGVuc2lvbj4KCTwvZXh0OlVCTEV4dGVuc2lvbnM+Cgk8Y2JjOlVCTFZlcnNpb25JRD5VQkwgMi4xPC9jYmM6VUJMVmVyc2lvbklEPgoJPGNiYzpDdXN0b21pemF0aW9uSUQ+MTwvY2JjOkN1c3RvbWl6YXRpb25JRD4KCTxjYmM6UHJvZmlsZUlEPkRJQU4gMi4xPC9jYmM6UHJvZmlsZUlEPgoJPGNiYzpQcm9maWxlRXhlY3V0aW9uSUQ+MTwvY2JjOlByb2ZpbGVFeGVjdXRpb25JRD4KCTxjYmM6SUQ+UEUwMDE8L2NiYzpJRD4gIDwhLS1jb25zZWN1dGl2by0tPgoJPGNiYzpVVUlEIHNjaGVtZUFnZW5jeUlEPSIxOTUiIHNjaGVtZUFnZW5jeU5hbWU9IkNPLCBESUFOIChEaXJlY2Npw7NuIGRlIEltcHVlc3RvcyB5IEFkdWFuYXMgTmFjaW9uYWxlcykiIHNjaGVtZUlEPSIxIiBzY2hlbWVOYW1lPSJDVVBFLVNIQTM4NCI+ODQ3NDM1MThlOGZlNzVkNDE1MDljNTBmNTU5ZTk1NTQxNzNjYTkyZGM5YmM1YTBlNzVjMmNiM2YyZWQ2OWYwZGMzMThlZTFhNDY3YjQ5NjZkNGJiN2NlNTg4YzJlZWM3PC9jYmM6VVVJRD4KCTxjYmM6SXNzdWVEYXRlPjIwMjEtMDUtMjQ8L2NiYzpJc3N1ZURhdGU+IDwhLS1mZWNoYSBkZSBleHBlZGljaW9uLS0+Cgk8Y2JjOklzc3VlVGltZT4xNTowNTo0MC0wNTowMDwvY2JjOklzc3VlVGltZT4gIDwhLS1ob3JhIGRlIGV4cGVkaWNpb24tLT4KCTxjYmM6RHVlRGF0ZT4yMDIxLTA1LTI0PC9jYmM6RHVlRGF0ZT4KCTxjYmM6SW52b2ljZVR5cGVDb2RlIG5hbWU9IkZhY3R1cmEgdGlwbyBwdW50byBkZSB2ZW50YSBQT1MiPjIwPC9jYmM6SW52b2ljZVR5cGVDb2RlPiA8IS0taWRlbnRpZmljYWRvciBkZWwgZG9jdW1lbnRvLS0+Cgk8Y2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlPkNPUDwvY2JjOkRvY3VtZW50Q3VycmVuY3lDb2RlPgoJPGNiYzpMaW5lQ291bnROdW1lcmljPjE8L2NiYzpMaW5lQ291bnROdW1lcmljPgoJPGNhYzpBY2NvdW50aW5nU3VwcGxpZXJQYXJ0eT4KCTxjYmM6QWRkaXRpb25hbEFjY291bnRJRD4xPC9jYmM6QWRkaXRpb25hbEFjY291bnRJRD4gPCEtLTE9IFBlcnNvbmEgSnVyaWRpY2EsIDI9IFBlcnNvbmEgTmF0dXJhbCwgMz0gTk8gSURFTlRJRklDQURPLS0+CgkJPGNhYzpQYXJ0eT4KCQkJPGNhYzpQYXJ0eU5hbWU+CgkJCQk8Y2JjOk5hbWU+U2VydmlwYXJraW5nIFZpcmdlbiBkZWwgQ2FybWVuPC9jYmM6TmFtZT4gPCEtLW5vbWJyZSB5IGFwZWxsaWRvcyBkZWwgZW1pc29yLS0+CgkJCTwvY2FjOlBhcnR5TmFtZT4KCQkJPGNhYzpQYXJ0eVRheFNjaGVtZT4KCQkJCTxjYmM6UmVnaXN0cmF0aW9uTmFtZT5TZXJ2aXBhcmtpbmcgVmlyZ2VuIGRlbCBDYXJtZW48L2NiYzpSZWdpc3RyYXRpb25OYW1lPiA8IS0tcmF6b24gc29jaWFsIGRlbCBlbWlzb3ItLT4KCQkJCTxjYmM6Q29tcGFueUlEIHNjaGVtZUFnZW5jeUlEPSIxOTUiIHNjaGVtZUFnZW5jeU5hbWU9IkNPLCBESUFOIChEaXJlY2Npw7NuIGRlIEltcHVlc3RvcyB5IEFkdWFuYXMgTmFjaW9uYWxlcykiIHNjaGVtZUlEPSIyIiBzY2hlbWVOYW1lPSIzMSI+OTAwODUwMjU1PC9jYmM6Q29tcGFueUlEPiA8IS0tbml0IGRlbCBlbWlzb3ItLT4KCQkJCTxjYmM6VGF4TGV2ZWxDb2RlPk8tMjM8L2NiYzpUYXhMZXZlbENvZGU+ICA8IS0tY2FsaWRhZCBkZWwgcmV0ZW5lZG9yLS0+CgkJCQk8Y2FjOlJlZ2lzdHJhdGlvbkFkZHJlc3M+CgkJCQkJPGNiYzpJRD4yNTQ3MzwvY2JjOklEPgoJCQkJCTxjYmM6Q2l0eU5hbWU+TU9TUVVFUkE8L2NiYzpDaXR5TmFtZT4KCQkJCQk8Y2JjOlBvc3RhbFpvbmU+MjUwMDQwPC9jYmM6UG9zdGFsWm9uZT4KCQkJCQk8Y2JjOkNvdW50cnlTdWJlbnRpdHkvPgoJCQkJCTxjYmM6Q291bnRyeVN1YmVudGl0eUNvZGU+MjU8L2NiYzpDb3VudHJ5U3ViZW50aXR5Q29kZT4KCQkJCQk8Y2FjOkFkZHJlc3NMaW5lPgoJCQkJCQk8Y2JjOkxpbmU+RnJlbnRlIGEgUGFzdGFzIERvcmlhIEFsIGxhZG8gZGUgQm9kZWdhcyBTYW4gQ2FybG9zIElJSTwvY2JjOkxpbmU+CgkJCQkJPC9jYWM6QWRkcmVzc0xpbmU+CgkJCQkJPGNhYzpDb3VudHJ5PgoJCQkJCQk8Y2JjOklkZW50aWZpY2F0aW9uQ29kZT5DTzwvY2JjOklkZW50aWZpY2F0aW9uQ29kZT4KCQkJCQkJPGNiYzpOYW1lIGxhbmd1YWdlSUQ9ImVzIj5Db2xvbWJpYTwvY2JjOk5hbWU+CgkJCQkJPC9jYWM6Q291bnRyeT4KCQkJCTwvY2FjOlJlZ2lzdHJhdGlvbkFkZHJlc3M+CgkJCQk8Y2FjOlRheFNjaGVtZT4KCQkJCQk8Y2JjOklEPjAxPC9jYmM6SUQ+CgkJCQkJPGNiYzpOYW1lPklWQTwvY2JjOk5hbWU+CgkJCQk8L2NhYzpUYXhTY2hlbWU+CgkJCTwvY2FjOlBhcnR5VGF4U2NoZW1lPgoJCQk8Y2FjOlBhcnR5TGVnYWxFbnRpdHk+CgkJCQk8Y2JjOlJlZ2lzdHJhdGlvbk5hbWU+U2VydmlwYXJraW5nIFZpcmdlbiBkZWwgQ2FybWVuPC9jYmM6UmVnaXN0cmF0aW9uTmFtZT4KCQkJCTxjYmM6Q29tcGFueUlEIHNjaGVtZUFnZW5jeUlEPSIxOTUiIHNjaGVtZUFnZW5jeU5hbWU9IkNPLCBESUFOIChEaXJlY2Npw7NuIGRlIEltcHVlc3RvcyB5IEFkdWFuYXMgTmFjaW9uYWxlcykiIHNjaGVtZUlEPSIyIiBzY2hlbWVOYW1lPSIzMSI+OTAwODUwMjU1PC9jYmM6Q29tcGFueUlEPgoJCQkJPGNhYzpDb3Jwb3JhdGVSZWdpc3RyYXRpb25TY2hlbWU+CgkJCQkJPGNiYzpJRD5QRTwvY2JjOklEPgoJCQkJPC9jYWM6Q29ycG9yYXRlUmVnaXN0cmF0aW9uU2NoZW1lPgoJCQk8L2NhYzpQYXJ0eUxlZ2FsRW50aXR5PgoJCQk8Y2FjOkNvbnRhY3Q+CgkJCQk8Y2JjOlRlbGVwaG9uZT4zMTEyMzM2NzM1PC9jYmM6VGVsZXBob25lPgoJCQkJPGNiYzpFbGVjdHJvbmljTWFpbD4uPC9jYmM6RWxlY3Ryb25pY01haWw+CgkJCTwvY2FjOkNvbnRhY3Q+CgkJPC9jYWM6UGFydHk+Cgk8L2NhYzpBY2NvdW50aW5nU3VwcGxpZXJQYXJ0eT4KCTxjYWM6QWNjb3VudGluZ0N1c3RvbWVyUGFydHk+Cgk8Y2JjOkFkZGl0aW9uYWxBY2NvdW50SUQ+MjwvY2JjOkFkZGl0aW9uYWxBY2NvdW50SUQ+CgkJPGNhYzpQYXJ0eT4KCQk8Y2FjOlBhcnR5SWRlbnRpZmljYXRpb24+CgkJICAgIDxjYmM6SUQgc2NoZW1lTmFtZT0iMTMiIHNjaGVtZUlEPSIiPjk4MzU0MTA5PC9jYmM6SUQ+CgkJPC9jYWM6UGFydHlJZGVudGlmaWNhdGlvbj4KICAgICAgICAgPGNhYzpQYXJ0eU5hbWU+CiAgICAgICAgICAgIDxjYmM6TmFtZT5Vc3VhcmlvIEZpbmFsPC9jYmM6TmFtZT4KICAgICAgICAgPC9jYWM6UGFydHlOYW1lPgoJCSA8Y2FjOlBhcnR5VGF4U2NoZW1lPgoJCQkJPGNiYzpSZWdpc3RyYXRpb25OYW1lPlVzdWFyaW8gRmluYWw8L2NiYzpSZWdpc3RyYXRpb25OYW1lPiA8IS0tVXN1YXJpbyBGaW5hbCB2YWxvciBmaWpvLS0+CgkJCQk8Y2JjOkNvbXBhbnlJRCBzY2hlbWVBZ2VuY3lJRD0iMTk1IiBzY2hlbWVBZ2VuY3lOYW1lPSJDTywgRElBTiAoRGlyZWNjacOzbiBkZSBJbXB1ZXN0b3MgeSBBZHVhbmFzIE5hY2lvbmFsZXMpIiBzY2hlbWVJRD0iIiBzY2hlbWVOYW1lPSIxMyI+OTgzNTQxMDk8L2NiYzpDb21wYW55SUQ+IDwhLS1uaXQgZGVsIGFkcXVpcmllbnRlIHZhbG9yIGZpam8tLT4KCQkJCTxjYWM6VGF4U2NoZW1lPgoJCQkJCTxjYmM6SUQ+MDE8L2NiYzpJRD4gPCEtLXZhbG9yIGZpam8tLT4KCQkJCQk8Y2JjOk5hbWU+SVZBPC9jYmM6TmFtZT48IS0tdmFsb3IgZmlqby0tPgoJCQkJPC9jYWM6VGF4U2NoZW1lPgoJCQk8L2NhYzpQYXJ0eVRheFNjaGVtZT4KICAgICAgPC9jYWM6UGFydHk+Cgk8L2NhYzpBY2NvdW50aW5nQ3VzdG9tZXJQYXJ0eT4KCTxjYWM6UGF5bWVudE1lYW5zPgoJCTxjYmM6SUQ+MTwvY2JjOklEPgoJCTxjYmM6UGF5bWVudE1lYW5zQ29kZT4xMDwvY2JjOlBheW1lbnRNZWFuc0NvZGU+ICA8IS0tbWVkaW8gZGUgcGFnby0tPgoJCTxjYmM6UGF5bWVudER1ZURhdGU+MjAyMS0wNS0yNDwvY2JjOlBheW1lbnREdWVEYXRlPgoJPC9jYWM6UGF5bWVudE1lYW5zPgoJPGNhYzpQYXltZW50RXhjaGFuZ2VSYXRlPgoJCTxjYmM6U291cmNlQ3VycmVuY3lDb2RlPkNPUDwvY2JjOlNvdXJjZUN1cnJlbmN5Q29kZT4KCQk8Y2JjOlNvdXJjZUN1cnJlbmN5QmFzZVJhdGU+MS4wMDwvY2JjOlNvdXJjZUN1cnJlbmN5QmFzZVJhdGU+CgkJPGNiYzpUYXJnZXRDdXJyZW5jeUNvZGU+Q09QPC9jYmM6VGFyZ2V0Q3VycmVuY3lDb2RlPgoJCTxjYmM6VGFyZ2V0Q3VycmVuY3lCYXNlUmF0ZT4xLjAwPC9jYmM6VGFyZ2V0Q3VycmVuY3lCYXNlUmF0ZT4KCQk8Y2JjOkNhbGN1bGF0aW9uUmF0ZT4xLjAwPC9jYmM6Q2FsY3VsYXRpb25SYXRlPgoJCTxjYmM6RGF0ZT4yMDIxLTA1LTI0PC9jYmM6RGF0ZT4KCTwvY2FjOlBheW1lbnRFeGNoYW5nZVJhdGU+Cgk8Y2FjOlRheFRvdGFsPgoJCTxjYmM6VGF4QW1vdW50IGN1cnJlbmN5SUQ9IkNPUCI+NjA4My4xOTwvY2JjOlRheEFtb3VudD4KCQk8Y2FjOlRheFN1YnRvdGFsPgoJCQk8Y2JjOlRheGFibGVBbW91bnQgY3VycmVuY3lJRD0iQ09QIj4zMjAxNi44MTwvY2JjOlRheGFibGVBbW91bnQ+CgkJCTxjYmM6VGF4QW1vdW50IGN1cnJlbmN5SUQ9IkNPUCI+NjA4My4xOTwvY2JjOlRheEFtb3VudD4KCQkJPGNhYzpUYXhDYXRlZ29yeT4KCQkJCTxjYmM6UGVyY2VudD4xOS4wMDwvY2JjOlBlcmNlbnQ+CgkJCQk8Y2FjOlRheFNjaGVtZT4KCQkJCQk8Y2JjOklEPjAxPC9jYmM6SUQ+CgkJCQkJPGNiYzpOYW1lPklWQTwvY2JjOk5hbWU+CgkJCQk8L2NhYzpUYXhTY2hlbWU+CgkJCTwvY2FjOlRheENhdGVnb3J5PgoJCTwvY2FjOlRheFN1YnRvdGFsPgoJCTxjYWM6VGF4U3VidG90YWw+CgkJCTxjYmM6VGF4YWJsZUFtb3VudCBjdXJyZW5jeUlEPSJDT1AiPjMyMDE2LjgxPC9jYmM6VGF4YWJsZUFtb3VudD4KCQkJPGNiYzpUYXhBbW91bnQgY3VycmVuY3lJRD0iQ09QIj42MDgzLjE5PC9jYmM6VGF4QW1vdW50PgoJCQk8Y2FjOlRheENhdGVnb3J5PgoJCQkJPGNiYzpQZXJjZW50PjguMDA8L2NiYzpQZXJjZW50PgoJCQkJPGNhYzpUYXhTY2hlbWU+CgkJCQkJPGNiYzpJRD4wMTwvY2JjOklEPgoJCQkJCTxjYmM6TmFtZT5JVkE8L2NiYzpOYW1lPgoJCQkJPC9jYWM6VGF4U2NoZW1lPgoJCQk8L2NhYzpUYXhDYXRlZ29yeT4KCQk8L2NhYzpUYXhTdWJ0b3RhbD4KCTwvY2FjOlRheFRvdGFsPgoJPGNhYzpUYXhUb3RhbD4KCQk8Y2JjOlRheEFtb3VudCBjdXJyZW5jeUlEPSJDT1AiPjYwODMuMTk8L2NiYzpUYXhBbW91bnQ+CgkJPGNhYzpUYXhTdWJ0b3RhbD4KCQkJPGNiYzpUYXhhYmxlQW1vdW50IGN1cnJlbmN5SUQ9IkNPUCI+MzIwMTYuODE8L2NiYzpUYXhhYmxlQW1vdW50PgoJCQk8Y2JjOlRheEFtb3VudCBjdXJyZW5jeUlEPSJDT1AiPjYwODMuMTk8L2NiYzpUYXhBbW91bnQ+CgkJCTxjYWM6VGF4Q2F0ZWdvcnk+CgkJCQk8Y2JjOlBlcmNlbnQ+MTkuMDA8L2NiYzpQZXJjZW50PgoJCQkJPGNhYzpUYXhTY2hlbWU+CgkJCQkJPGNiYzpJRD4wNDwvY2JjOklEPgoJCQkJCTxjYmM6TmFtZT5JTkM8L2NiYzpOYW1lPgoJCQkJPC9jYWM6VGF4U2NoZW1lPgoJCQk8L2NhYzpUYXhDYXRlZ29yeT4KCQk8L2NhYzpUYXhTdWJ0b3RhbD4KCQk8Y2FjOlRheFN1YnRvdGFsPgoJCQk8Y2JjOlRheGFibGVBbW91bnQgY3VycmVuY3lJRD0iQ09QIj4zMjAxNi44MTwvY2JjOlRheGFibGVBbW91bnQ+CgkJCTxjYmM6VGF4QW1vdW50IGN1cnJlbmN5SUQ9IkNPUCI+NjA4My4xOTwvY2JjOlRheEFtb3VudD4KCQkJPGNhYzpUYXhDYXRlZ29yeT4KCQkJCTxjYmM6UGVyY2VudD44LjAwPC9jYmM6UGVyY2VudD4KCQkJCTxjYWM6VGF4U2NoZW1lPgoJCQkJCTxjYmM6SUQ+MDQ8L2NiYzpJRD4KCQkJCQk8Y2JjOk5hbWU+SU5DPC9jYmM6TmFtZT4KCQkJCTwvY2FjOlRheFNjaGVtZT4KCQkJPC9jYWM6VGF4Q2F0ZWdvcnk+CgkJPC9jYWM6VGF4U3VidG90YWw+Cgk8L2NhYzpUYXhUb3RhbD4KCTxjYWM6TGVnYWxNb25ldGFyeVRvdGFsPgoJCTxjYmM6TGluZUV4dGVuc2lvbkFtb3VudCBjdXJyZW5jeUlEPSJDT1AiPjMyMDE2LjgxPC9jYmM6TGluZUV4dGVuc2lvbkFtb3VudD4KCQk8Y2JjOlRheEV4Y2x1c2l2ZUFtb3VudCBjdXJyZW5jeUlEPSJDT1AiPjMyMDE2LjgxPC9jYmM6VGF4RXhjbHVzaXZlQW1vdW50PgoJCTxjYmM6VGF4SW5jbHVzaXZlQW1vdW50IGN1cnJlbmN5SUQ9IkNPUCI+MzgxMDAuMDA8L2NiYzpUYXhJbmNsdXNpdmVBbW91bnQ+CgkJPGNiYzpDaGFyZ2VUb3RhbEFtb3VudCBjdXJyZW5jeUlEPSJDT1AiPjAuMDA8L2NiYzpDaGFyZ2VUb3RhbEFtb3VudD4KCQk8Y2JjOlByZXBhaWRBbW91bnQgY3VycmVuY3lJRD0iQ09QIj4wLjAwPC9jYmM6UHJlcGFpZEFtb3VudD4KCQk8Y2JjOlBheWFibGVBbW91bnQgY3VycmVuY3lJRD0iQ09QIj4zODEwMC4wMDwvY2JjOlBheWFibGVBbW91bnQ+ICA8IS0tdmFsb3IgdG90YWwtLT4KCTwvY2FjOkxlZ2FsTW9uZXRhcnlUb3RhbD4KCTxjYWM6SW52b2ljZUxpbmU+CgkJPGNiYzpJRD4xPC9jYmM6SUQ+CgkJPGNiYzpJbnZvaWNlZFF1YW50aXR5IHVuaXRDb2RlPSI5NCI+MTwvY2JjOkludm9pY2VkUXVhbnRpdHk+ICA8IS0tY2FudGlkYWQtLT4gICA8IS0tbGFzIGJvbHNhcyBzZSBpbmZvcm1hcmlhbiBjb21vIHVuIHByb2R1Y3RvLS0+CgkJPGNiYzpMaW5lRXh0ZW5zaW9uQW1vdW50IGN1cnJlbmN5SUQ9IkNPUCI+MzIwMTYuODEwMDwvY2JjOkxpbmVFeHRlbnNpb25BbW91bnQ+CgkJPGNiYzpGcmVlT2ZDaGFyZ2VJbmRpY2F0b3I+ZmFsc2U8L2NiYzpGcmVlT2ZDaGFyZ2VJbmRpY2F0b3I+CgkJPGNhYzpBbGxvd2FuY2VDaGFyZ2U+IDwhLS1HcnVwbyBwYXJhIGluZm9ybWFyIFJFQ0FSR09TIG8gREVTQ1VFTlRPUy0tPgoJCQk8Y2JjOklEPjE8L2NiYzpJRD4KCQkJPGNiYzpDaGFyZ2VJbmRpY2F0b3I+ZmFsc2U8L2NiYzpDaGFyZ2VJbmRpY2F0b3I+IDwhLS1zaSBlbCB2YWxvcyBkZSBlc3RlIGVsZW1lbnRvIGVzIFRSVUUgc2lnbmlmaWNhIHF1ZSBlcyB1biByZWNhcmdvLS0+IDwhLS1zaSBlbCB2YWxvcyBkZSBlc3RlIGVsZW1lbnRvIGVzIEZBTFNFIHNpZ25pZmljYSBxdWUgZXMgdW4gZGVzY3VlbnRvLS0+CgkJCTxjYmM6QWxsb3dhbmNlQ2hhcmdlUmVhc29uPkRlc2N1ZW50byBwb3IgY2xpZW50ZSBmcmVjdWVudGU8L2NiYzpBbGxvd2FuY2VDaGFyZ2VSZWFzb24+CgkJCTxjYmM6TXVsdGlwbGllckZhY3Rvck51bWVyaWM+MTA8L2NiYzpNdWx0aXBsaWVyRmFjdG9yTnVtZXJpYz4KCQkJPGNiYzpBbW91bnQgY3VycmVuY3lJRD0iQ09QIj4xMDAwMDwvY2JjOkFtb3VudD4KCQkJPGNiYzpCYXNlQW1vdW50IGN1cnJlbmN5SUQ9IkNPUCI+MTAwMDAwLjAwPC9jYmM6QmFzZUFtb3VudD4KCQk8L2NhYzpBbGxvd2FuY2VDaGFyZ2U+CgkJPGNhYzpUYXhUb3RhbD4KCQkJPGNiYzpUYXhBbW91bnQgY3VycmVuY3lJRD0iQ09QIj42MDgzLjE5PC9jYmM6VGF4QW1vdW50PgoJCQk8Y2FjOlRheFN1YnRvdGFsPgoJCQkJPGNiYzpUYXhhYmxlQW1vdW50IGN1cnJlbmN5SUQ9IkNPUCI+MzIwMTYuODE8L2NiYzpUYXhhYmxlQW1vdW50PgoJCQkJPGNiYzpUYXhBbW91bnQgY3VycmVuY3lJRD0iQ09QIj42MDgzLjE5PC9jYmM6VGF4QW1vdW50PgoJCQkJPGNhYzpUYXhDYXRlZ29yeT4KCQkJCQk8Y2JjOlBlcmNlbnQ+MTkuMDA8L2NiYzpQZXJjZW50PgoJCQkJCTxjYWM6VGF4U2NoZW1lPgoJCQkJCQk8Y2JjOklEPjAxPC9jYmM6SUQ+CgkJCQkJCTxjYmM6TmFtZT5JVkE8L2NiYzpOYW1lPiAgPCEtLWltcHVlc3RvcyBJVkEsIElOQy0tPgoJCQkJCTwvY2FjOlRheFNjaGVtZT4KCQkJCTwvY2FjOlRheENhdGVnb3J5PgoJCQk8L2NhYzpUYXhTdWJ0b3RhbD4KCQk8L2NhYzpUYXhUb3RhbD4KCQk8Y2FjOkl0ZW0+CgkJCTxjYmM6RGVzY3JpcHRpb24+U2VydmljaW8gZGUgUGFycXVlYWRlcm8gLSBQbGFjYSBTVkE1NjY8L2NiYzpEZXNjcmlwdGlvbj4gIDwhLS1kZXNjcmlwY2lvbiBkZWwgYmllbiBvIGVsIHNlcnZpY2lvLS0+CgkJCTxjYWM6U3RhbmRhcmRJdGVtSWRlbnRpZmljYXRpb24+CgkJCQk8Y2JjOklEIHNjaGVtZUlEPSI5OTkiPjE8L2NiYzpJRD4KCQkJPC9jYWM6U3RhbmRhcmRJdGVtSWRlbnRpZmljYXRpb24+CgkJPC9jYWM6SXRlbT4KCQk8Y2FjOlByaWNlPgoJCQk8Y2JjOlByaWNlQW1vdW50IGN1cnJlbmN5SUQ9IkNPUCI+MzIwMTYuODE8L2NiYzpQcmljZUFtb3VudD4gIDwhLS12YWxvciB1bml0YXJpby0tPgoJCQk8Y2JjOkJhc2VRdWFudGl0eSB1bml0Q29kZT0iOTQiPjEuMDA8L2NiYzpCYXNlUXVhbnRpdHk+ICA8IS0tdW5pZGFkLS0+CgkJPC9jYWM6UHJpY2U+Cgk8L2NhYzpJbnZvaWNlTGluZT4KCTxjYWM6SW52b2ljZUxpbmU+CgkJPGNiYzpJRD4xPC9jYmM6SUQ+CgkJPGNiYzpJbnZvaWNlZFF1YW50aXR5IHVuaXRDb2RlPSI5NCI+MTwvY2JjOkludm9pY2VkUXVhbnRpdHk+ICA8IS0tY2FudGlkYWQtLT4gICA8IS0tbGFzIGJvbHNhcyBzZSBpbmZvcm1hcmlhbiBjb21vIHVuIHByb2R1Y3RvLS0+CgkJPGNiYzpMaW5lRXh0ZW5zaW9uQW1vdW50IGN1cnJlbmN5SUQ9IkNPUCI+MTAwPC9jYmM6TGluZUV4dGVuc2lvbkFtb3VudD4KCQk8Y2FjOlRheFRvdGFsPgoJCQk8Y2JjOlRheEFtb3VudCBjdXJyZW5jeUlEPSJDT1AiPjYwODMuMTk8L2NiYzpUYXhBbW91bnQ+CgkJCTxjYWM6VGF4U3VidG90YWw+CgkJCQk8Y2JjOlRheGFibGVBbW91bnQgY3VycmVuY3lJRD0iQ09QIj4zMjAxNi44MTwvY2JjOlRheGFibGVBbW91bnQ+CgkJCQk8Y2JjOlRheEFtb3VudCBjdXJyZW5jeUlEPSJDT1AiPjYwODMuMTk8L2NiYzpUYXhBbW91bnQ+CgkJCQk8Y2FjOlRheENhdGVnb3J5PgoJCQkJCTxjYmM6UGVyY2VudD4xOS4wMDwvY2JjOlBlcmNlbnQ+CgkJCQkJPGNhYzpUYXhTY2hlbWU+CgkJCQkJCTxjYmM6SUQ+MDE8L2NiYzpJRD4KCQkJCQkJPGNiYzpOYW1lPklWQTwvY2JjOk5hbWU+ICA8IS0taW1wdWVzdG9zIElWQSwgSU5DLS0+CgkJCQkJPC9jYWM6VGF4U2NoZW1lPgoJCQkJPC9jYWM6VGF4Q2F0ZWdvcnk+CgkJCTwvY2FjOlRheFN1YnRvdGFsPgoJCTwvY2FjOlRheFRvdGFsPgoJCTxjYWM6SXRlbT4KCQkJPGNiYzpEZXNjcmlwdGlvbj5Cb2xzYTwvY2JjOkRlc2NyaXB0aW9uPiAgPCEtLWRlc2NyaXBjaW9uIGRlbCBiaWVuIG8gZWwgc2VydmljaW8tLT4KCQkJPGNhYzpTdGFuZGFyZEl0ZW1JZGVudGlmaWNhdGlvbj4KCQkJCTxjYmM6SUQgc2NoZW1lSUQ9Ijk5OSI+MTwvY2JjOklEPgoJCQk8L2NhYzpTdGFuZGFyZEl0ZW1JZGVudGlmaWNhdGlvbj4KCQk8L2NhYzpJdGVtPgoJCTxjYWM6UHJpY2U+CgkJCTxjYmM6UHJpY2VBbW91bnQgY3VycmVuY3lJRD0iQ09QIj4zMjAxNi44MTwvY2JjOlByaWNlQW1vdW50PiAgPCEtLXZhbG9yIHVuaXRhcmlvLS0+CgkJCTxjYmM6QmFzZVF1YW50aXR5IHVuaXRDb2RlPSI5NCI+MS4wMDwvY2JjOkJhc2VRdWFudGl0eT4gIDwhLS11bmlkYWQtLT4KCQk8L2NhYzpQcmljZT4KCTwvY2FjOkludm9pY2VMaW5lPgo8L0ludm9pY2U+Cgo=";
                var xmlBytes = Convert.FromBase64String(base44);
                var xmldecoded = Encoding.UTF8.GetString(Convert.FromBase64String(base44));
                var invoceParser = new XmlToDocumentoSoporteParser();





                var invoceDs = invoceParser.Parser(Encoding.UTF8.GetBytes(xmldecoded));


                XElement xelement = XElement.Load(new StringReader(xmldecoded));
                XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
                XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

                var invoiceLineNodes = xelement.Elements(cac + "InvoiceLine");
                var html = GetTemplate(xelement.Elements(cbc + "InvoiceTypeCode").FirstOrDefault().Value);
                var qr = GenerateQrBase64ForDocument(invoceDs.Cuds);
                html = CruzarLogosEnHeader(html, invoceDs.NitAbs);
                html = await FillDocumentData(html, xelement);
                

                html = await CruzarModeloDetallesProductos(html, invoiceLineNodes.ToList(), xelement.Elements(cbc + "IssueDate").FirstOrDefault().Value);
                html = FillReferenceData(html, xelement);
                html = CruzarModeloNotasFinales(html, xelement);
                html = html.Replace("{QrCodeBase64}", qr);


                byte[] bytes = OpenHtmlToPdf.Pdf
                       .From(html)
                       .WithGlobalSetting("orientation", "Portrait")
                       .WithObjectSetting("web.defaultEncoding", "utf-8")
                       .OfSize(PaperSize.A4)
                       .Content();

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);



                result.Content = new ByteArrayContent(bytes);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/Binary");

                return result;
            }


            catch (System.Exception ex)
            {
                //return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.BadRequest);
                result.Content = new StringContent("No podemos generar el PDF en este momento debido al siguiente error: " + ex.Message);
                result.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("text/plain");

                return result;
            }
        }



        //private string CruzarModeloDescuentosYRecargos(List<XElement> model, string plantillaHtml)
        //{
        //    var rowDescuentosYRecargos = new StringBuilder();
        //    foreach (var detalle in model.DescuentosYRecargos)
        //    {
        //        rowDescuentosYRecargos.Append($@"
        //        <tr>
        //      <td class='text-right'>{detalle.Numero}</td>
        //      <td class='text-left'>{detalle.Tipo}</td>
        //      <td>{detalle.Codigo}</td>
        //      <td class='text-left'>{detalle.Descripcion}</td>
        //      <td>{detalle.Porcentaje:n2}</td>
        //      <td class='text-right'>{detalle.Valor:n2}</td>
        //     </tr>");
        //    }
        //    plantillaHtml = plantillaHtml.Replace("{RowsDescuentosYRecargos}", rowDescuentosYRecargos.ToString());
        //    return plantillaHtml;
        //}

        private static async Task<string> CruzarModeloDetallesProductos(string plantillaHtml, List<XElement> model, string fecha)
        {
            var rowDetalleProductosBuilder = new StringBuilder();
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            var cosmos = new CosmosDbManagerPayroll();
            decimal subTotal = 0;

            foreach (var detalle in model)
            {
                //<td>{Unidad.Where(x=>x.IdSubList.ToString()== detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").Attributes("unitCode").FirstOrDefault().Value).FirstOrDefault().CompositeName}</td>
                var unit = await cosmos.getUnidad(detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").Attributes("unitCode").FirstOrDefault().Value);
                rowDetalleProductosBuilder.Append($@"
                <tr>
		            <td>{detalle.Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cac + "Item").Elements(cac + "StandardItemIdentification").Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cac + "Item").Elements(cbc + "Description").FirstOrDefault().Value}</td>
		            <td>{unit.CompositeName}</td>
		            <td>{detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").FirstOrDefault().Value}</td>
                    <td>{detalle.Elements(cac + "Price").Elements(cbc + "PriceAmount").FirstOrDefault().Value}</td>
		            <td class='text-right'>{detalle.Elements(cac + "TaxTotal").Elements(cac + "TaxSubtotal").Elements(cbc + "TaxableAmount").FirstOrDefault().Value:n2}</td>
                    <td class='text-right'>{detalle.Elements(cac + "TaxTotal").Elements(cac + "TaxSubtotal").Elements(cac + "TaxCategory").Elements(cbc + "Percent").FirstOrDefault().Value:n2}</td>


		            <td>{detalle.Elements(cbc + "LineExtensionAmount").FirstOrDefault().Value}</td>

		            <td>{fecha:dd/MM/yyyy}</td>
	            </tr>");

                subTotal = subTotal + decimal.Parse(detalle.Elements(cac + "Price").Elements(cbc + "PriceAmount").FirstOrDefault().Value) * 
                                        decimal.Parse(detalle.Elements(cbc + "InvoicedQuantity").FirstOrDefault().Value);
            }
            plantillaHtml = plantillaHtml.Replace("{RowsDetalleProductos}", rowDetalleProductosBuilder.ToString());
            
            plantillaHtml = plantillaHtml.Replace("{SubTotal}", subTotal.ToString());
            return plantillaHtml;
        }


        public static string GenerateQrBase64ForDocument(string code)
        {
            var urlSiteDian = ConfigurationManager.GetValue("SiteDian");
            var urlToQr = $"{urlSiteDian}document/searchqr?documentkey={code}";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(urlToQr, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = new Bitmap(qrCode.GetGraphic(72), new Size(160, 160));
            var base64String = "";
            using (MemoryStream ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] imageBytes = ms.ToArray();
                base64String = Convert.ToBase64String(imageBytes);
            }
            var qrBase64 = $@"data:image/png;base64,{base64String}";
            return qrBase64;
        }

        public static async Task<string> FillDocumentData(string Html, XElement model)
        {
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
            XNamespace def = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            var cosmos = new CosmosDbManagerPayroll();
            var DocumentType = await cosmos.getDocumentType();
            var Regimen = await cosmos.getRegimen();

            var UUID = model.Elements(cbc + "UUID");
            Html = Html.Replace("{CodigoUnicoDocumentoSoporte}", UUID.FirstOrDefault().Value);

            var ID = model.Elements(cbc + "ID");
            Html = Html.Replace("{NumeroDocumentoSoporte}", ID.FirstOrDefault().Value);

            var IssueDate = model.Elements(cbc + "IssueDate");
            var IssueTime = model.Elements(cbc + "IssueTime");
            Html = Html.Replace("{FechaGeneracion}", IssueDate.FirstOrDefault().Value + " " + IssueTime.FirstOrDefault().Value);

            var DueDate = model.Elements(cbc + "DueDate");
            Html = Html.Replace("{FechaVencimiento}", DueDate.FirstOrDefault().Value);

            var AccountingCustomerPartyName = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyName").Elements(cbc + "Name");
            Html = Html.Replace("{AdquirienteRazonSocial}", AccountingCustomerPartyName.FirstOrDefault().Value);

            var CompanyID = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "CompanyID");
            Html = Html.Replace("{AdquirienteNit}", CompanyID.FirstOrDefault().Value);

            var AdquirienteTipoContribuyente = model.Elements(cac + "AccountingCustomerParty").Elements(cbc + "AdditionalAccountID");
            Html = Html.Replace("{AdquirienteTipoContribuyente}", AdquirienteTipoContribuyente.FirstOrDefault().Value == "3" ? "NO IDENTIFICADO" : AdquirienteTipoContribuyente.FirstOrDefault().Value == "2" ? "Persona Natural" : "Persona Juridica");

            var AccountingSupplierPartyName = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyName").Elements(cbc + "Name");
            Html = Html.Replace("{VendedorRazonSocial}", AccountingSupplierPartyName.FirstOrDefault().Value);

            //falta tipo de documento




            var VendedorNumeroDocumento = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "CompanyID");
            Html = Html.Replace("{VendedorTipoDocumento}", DocumentType.Where(x => x.IdDocumentType.ToString() == VendedorNumeroDocumento.FirstOrDefault().Attribute("schemeName").Value).FirstOrDefault().CompositeName);
            Html = Html.Replace("{VendedorNumeroDocumento}", VendedorNumeroDocumento.FirstOrDefault().Value);



            var VendedorTipoContribuyente = model.Elements(cac + "AccountingSupplierParty").Elements(cbc + "AdditionalAccountID");
            Html = Html.Replace("{VendedorTipoContribuyente}", VendedorTipoContribuyente.FirstOrDefault().Value == "3" ? "NO IDENTIFICADO" : VendedorTipoContribuyente.FirstOrDefault().Value == "2" ? "Persona Natural" : "Persona Juridica");


            var VendedorRegimenFiscal = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "TaxLevelCode");
            Html = Html.Replace("{VendedorRegimenFiscal}", Regimen.Where(x => x.IdSubList.ToString() == VendedorRegimenFiscal.FirstOrDefault().Value).FirstOrDefault().CompositeName);

            //falta regimen fiscal
            var VendedorResponsabilidadTributaria = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "TaxScheme").Elements(cbc + "Name");
            var VendedorResponsabilidadTributariaID = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "TaxScheme").Elements(cbc + "ID");
            Html = Html.Replace("{VendedorResponsabilidadTributaria}", VendedorResponsabilidadTributariaID.FirstOrDefault().Value + "-" + VendedorResponsabilidadTributaria.FirstOrDefault().Value);

            var Moneda = model.Elements(cbc + "DocumentCurrencyCode");
            Html = Html.Replace("{Moneda}", Moneda.FirstOrDefault().Value);

            var TasaCambio = model.Elements(cac + "PaymentExchangeRate").Elements(cbc + "CalculationRate");
            Html = Html.Replace("{TasaCambio}", TasaCambio.FirstOrDefault().Value);


            //falta caculos subtotal
           

            var TotalBrutoDocumento = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "TaxExclusiveAmount");
            Html = Html.Replace("{TotalBrutoDocumento}", TotalBrutoDocumento.FirstOrDefault().Value);

            var TotalIVA = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "TaxInclusiveAmount");//resta subtotal ? 
            Html = Html.Replace("{TotalIVA}", (decimal.Parse(TotalIVA.FirstOrDefault().Value) - decimal.Parse(TotalBrutoDocumento.FirstOrDefault().Value)).ToString());

            var TotalNetoDocumento = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "TaxInclusiveAmount");//resta subtotal ? 
            Html = Html.Replace("{TotalNetoDocumento}", TotalIVA.FirstOrDefault().Value);

            var DescuentoGlobal = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "AllowanceTotalAmount");//resta subtotal ? 
            if (DescuentoGlobal.Any())
            {
                Html = Html.Replace("{DescuentoGlobal}", DescuentoGlobal.FirstOrDefault().Value);
            }
            else
            {
                Html = Html.Replace("{DescuentoGlobal}", string.Empty);
            }

            var RecargoGlobal = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "ChargeTotalAmount");//resta subtotal ? 
            if(RecargoGlobal.Any())
                Html = Html.Replace("{RecargoGlobal}", RecargoGlobal.FirstOrDefault().Value);
            else
                Html = Html.Replace("{RecargoGlobal}", string.Empty);

            var TotalFactura = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "PayableAmount");//resta subtotal ? 
            Html = Html.Replace("{TotalFactura}", TotalFactura.FirstOrDefault().Value);

            var fab = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Where(x => x.FirstNode.ToString().Contains("FabricanteSoftware"));
             var info = fab.Where(x => x.FirstNode.ToString().Contains("InformacionDelFabricanteDelSoftware"));
            var soft = info.Descendants().Elements(def+"Value").ToArray();
            Html = Html.Replace("{FabricanteRazon}", soft[1].Value);
            Html = Html.Replace("{FabricanteNombre}", soft[0].Value);
            Html = Html.Replace("{FabricanteSoftware}", soft[2].Value);

            //var FabricanteRazon = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext+ "ExtensionContent")
            //    .Elements("FabricanteSoftware").Elements("InformacionDelFabricanteDelSoftware").Elements( "Name");//resta subtotal ? 
            //Html = Html.Replace("{TotalFactura}", FabricanteRazon.FirstOrDefault().Value);

            var NumeroAutorizacion = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent")
                .Elements(sts + "DianExtensions").Elements(sts + "InvoiceControl").Elements(sts + "InvoiceAuthorization");

            Html = Html.Replace("{NumeroAutorizacion}", NumeroAutorizacion.FirstOrDefault().Value);


             var RangoDesde = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent")
                .Elements(sts + "DianExtensions").Elements(sts + "InvoiceControl").Elements(sts + "AuthorizedInvoices").Elements(sts + "From");

            Html = Html.Replace("{RangoDesde}", RangoDesde.FirstOrDefault().Value);


             var RangoHasta = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent")
                .Elements(sts + "DianExtensions").Elements(sts + "InvoiceControl").Elements(sts + "AuthorizedInvoices").Elements(sts + "From");

            Html = Html.Replace("{RangoHasta}", RangoHasta.FirstOrDefault().Value);

              var Vigencia = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent")
                .Elements(sts + "DianExtensions").Elements(sts + "InvoiceControl").Elements(sts + "AuthorizationPeriod").Elements(cbc + "EndDate");

            Html = Html.Replace("{Vigencia}", Vigencia.FirstOrDefault().Value);

            return Html;
        }


        public static string FillReferenceData(string Html, XElement model)
        {
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";


            var Recargos = model.Elements(cac + "AllowanceCharge").ToList();
            if (Recargos.Count == 0)
            {

                Html = Html.Replace("{RowsDescuentosYRecargos}", String.Empty);

            }
            else
            {
                var rowDescuentosYRecargos = new StringBuilder();
                foreach (var detalle in Recargos)
                {

                    var tipo = detalle.Elements(cbc + "ChargeIndicator").FirstOrDefault().Value.ToUpper() == "TRUE" ? "Recargo" : "Descuento";
                    rowDescuentosYRecargos.Append($@"
                <tr>
		            <td class='text-right'>{detalle.Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td class='text-left'>{tipo}</td>
		            <td>{detalle.Elements(cbc + "AllowanceChargeReasonCode").FirstOrDefault().Value}</td>
		            <td class='text-left'>{detalle.Elements(cbc + "AllowanceChargeReason").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cbc + "MultiplierFactorNumeric").FirstOrDefault().Value}</td>
		            <td class='text-right'>{detalle.Elements(cbc + "Amount").FirstOrDefault().Value:n2}</td>
	            </tr>");
                }
                Html = Html.Replace("{RowsDescuentosYRecargos}", rowDescuentosYRecargos.ToString());
                return Html;
            }
            

            return Html;
        }


        public static string CruzarLogosEnHeader(string plantillaHtml, string identificationUser)
        {
            var fileManager = new FileManager();
            var fileManagerBiller = new FileManager("GlobalStorageBiller");

            MemoryStream logoDianStream = new MemoryStream(fileManager.GetBytes("radian-dian-logos", "Logo-DIAN-2020-color.jpg"));
            string logoDianaStrBase64 = Convert.ToBase64String(logoDianStream.ToArray());
            var logoDianBase64 = $@"data:image/png;base64,{logoDianaStrBase64}";

            MemoryStream logoStream = new MemoryStream(fileManagerBiller.GetBytesBiller("logo", $"{identificationUser}.jpg") ?? fileManager.GetBytes("radian-dian-logos", "Logo-DIAN-2020-color.jpg"));
            string logoStrBase64 = Convert.ToBase64String(logoStream.ToArray());
            var logoBase64 = $@"data:image/png;base64,{logoStrBase64}";



            plantillaHtml = plantillaHtml.Replace("{imgLogoDian}", logoDianBase64);
            /*WebConfig: clave MainStorage*/


            plantillaHtml = plantillaHtml.Replace("{imgLogoEmpresa}", logoBase64);
            return plantillaHtml;
        }
        private static string CruzarModeloNotasFinales(string Html, XElement obj)
        {
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            var rowReferencias = new StringBuilder();
            var model = obj.Elements(cac + "Note").ToList();

            foreach (var detalle in model)
            {
                rowReferencias.Append($@"
                <tr>
		            <td colspan='2'>{detalle.Value}</td>
	            </tr>");
            }
            Html = Html.Replace("{RowsNotasFinales}", rowReferencias.ToString());
            return Html;
        }
    
    private static string GetTemplate(string tipo)
        {
            var fileManager = new FileManager();
            if (tipo == "20")
                return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentPOS_template.html");
            else return null;
        }


    }
}