using Gosocket.Dian.Domain.Domain;
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
using Newtonsoft.Json;
using System.Globalization;

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


				param data = await req.Content.ReadAsAsync<param>();



				// Establecer nombre para consultar la cadena o los datos del cuerpo
				var base64Xml = data.base64Xml;
				var FechaValidacionDIAN = data.FechaValidacionDIAN;
				var FechaGeneracionDIAN = data.FechaGeneracionDIAN;
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
				XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
				XNamespace def = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
				XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
				var tp = xelement.Elements(cbc + "InvoiceTypeCode");
				var tipo = tp.Any() ? tp.FirstOrDefault().Value : "Nota";
				var invoiceLineNodes = xelement.Elements(cac + "InvoiceLine");
				var html = GetTemplate(tipo);
				var qr = GenerateQrBase64ForDocument(invoceDs.Cuds);
				html = CruzarLogosEnHeader(html, invoceDs.NitAbs);
				html = await FillDocumentData(html, xelement);

				if (tipo == "20")
					html = await CruzarModeloDetallesProductos(html, invoiceLineNodes.ToList(), xelement.Elements(cbc + "IssueDate").FirstOrDefault().Value);
				else if (tipo == "40")
					html = await CruzarModeloDetallesProductosComplete(html, invoiceLineNodes.ToList(), tipo);

				html = FillReferenceData(html, xelement);
				html = CruzarModeloNotasFinales(html, xelement);
				if (tipo != "Nota")
				{

					html = CruzarReferencias(html, xelement);
				}
				else
				{
					var CreditLineNodes = xelement.Elements(cac + "CreditNoteLine");
					html = await FillTransporteA(html, xelement, xelement.Elements(cbc + "IssueDate").FirstOrDefault().Value);
					html = await CruzarModeloDetallesProductosComplete(html, CreditLineNodes.ToList(), tipo);
					html =  CruzarReferenciasNota(html, xelement);
				}

				if (tipo == "50" || tipo == "55" || tipo == "45" || tipo == "32" ||tipo == "27" || tipo == "30" )
				{
					html = await FillTransporteA(html, xelement, xelement.Elements(cbc + "IssueDate").FirstOrDefault().Value);
					html = await CruzarModeloDetallesProductosComplete(html, invoiceLineNodes.ToList(), tipo);
				}

				if (tipo == "35")
				{
					//html = await FillTransporteA(html, xelement, xelement.Elements(cbc + "IssueDate").FirstOrDefault().Value);
					html = await CruzarModeloDetallesProductosComplete(html, invoiceLineNodes.ToList(), tipo);
					html = FillTransporteT(html, xelement);
				}

				if (tipo == "60"){
					html = await FillTransporteA(html, xelement, xelement.Elements(cbc + "IssueDate").FirstOrDefault().Value);
					html = await CruzarModeloDetallesProductosContador(html, invoiceLineNodes.ToList(), xelement);
				}
				html = html.Replace("{QrCodeBase64}", qr);
				html = html.Replace("{FechaValidacionDIAN}", FechaValidacionDIAN);
				html = html.Replace("{FechaGeneracionDIAN}", FechaGeneracionDIAN);
				if (ConfigurationManager.GetValue("Environment") != "Prod")
				{

					var rep = "PHRhYmxlIGNsYXNzPSJoZWFkZXItdGFibGUiPg==";
					var rep2 = "ICA8cCBzdHlsZT0nY29sb3I6bGlnaHRncmV5Ow0KICAgIHBvc2l0aW9uOiBhYnNvbHV0ZTtmb250LXNpemU6MTIwcHg7DQogICAgdHJhbnNmb3JtOnJvdGF0ZSgzMDBkZWcpOw0KICAgIC13ZWJraXQtdHJhbnNmb3JtOnJvdGF0ZSgzMDBkZWcpO3otaW5kZXg6IC0xO3BhZGRpbmctdG9wOiA1MCU7JyA+U2luIHZhbG9yIGZpc2NhbCA8L3A+";
					byte[] dataa = Convert.FromBase64String(rep);
					byte[] dataa2 = Convert.FromBase64String(rep2);
					string decodedString = Encoding.UTF8.GetString(dataa);
					string decodedString2 = Encoding.UTF8.GetString(dataa2);

					html = html.Replace(decodedString, decodedString2 + decodedString);

				}
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
				var taxt = detalle.Elements(cac + "TaxTotal").Elements(cac + "TaxSubtotal").Elements(cac + "TaxCategory").Elements(cbc + "Percent");
				var tax = taxt.Any() ? taxt.FirstOrDefault().Value : "";
				var TaxableAmount = detalle.Elements(cac + "TaxTotal").Elements(cac + "TaxSubtotal").Elements(cbc + "TaxableAmount");
				var taxAmo = TaxableAmount.Any() ? TaxableAmount.FirstOrDefault().Value : "";
				rowDetalleProductosBuilder.Append($@"
                <tr>
		            <td>{detalle.Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cac + "Item").Elements(cac + "StandardItemIdentification").Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cac + "Item").Elements(cbc + "Description").FirstOrDefault().Value}</td>
		            <td>{unit.CompositeName}</td>
		            <td>{detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").FirstOrDefault().Value}</td>
                    <td>{detalle.Elements(cac + "Price").Elements(cbc + "PriceAmount").FirstOrDefault().Value}</td>
		            <td class='text-right'>{taxAmo:n2}</td>
                    <td class='text-right'>{tax:n2}</td>


		            <td>{detalle.Elements(cbc + "LineExtensionAmount").FirstOrDefault().Value}</td>

		            <td>{fecha:dd/MM/yyyy}</td>
	            </tr>");

				subTotal = subTotal + decimal.Parse(detalle.Elements(cac + "Price").Elements(cbc + "PriceAmount").FirstOrDefault().Value.ToString().Split('.')[0]) *
										decimal.Parse(detalle.Elements(cbc + "InvoicedQuantity").FirstOrDefault().Value);
			}
			plantillaHtml = plantillaHtml.Replace("{RowsDetalleProductos}", rowDetalleProductosBuilder.ToString());

			plantillaHtml = plantillaHtml.Replace("{SubTotal}", String.Format("{0:n}", subTotal.ToString()));
			return plantillaHtml;
		}
		private static async Task<string> FillTransporteA(string plantillaHtml, XElement model, string fecha)
		{
			var rowDetalleProductosBuilder = new StringBuilder();
			XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
			XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
			var cosmos = new CosmosDbManagerPayroll();
			var Regimen = await cosmos.getRegimen();
			var Depto = await cosmos.getDepartament();
			var DocumentType = await cosmos.getDocumentType();
			var tipoOper = await cosmos.getTipoOperacion();
			var TipoOperacion = model.Elements(cbc + "CustomizationID");
			var TipoDoc = model.Elements(cbc + "InvoiceTypeCode");
			if (TipoOperacion.Any())
                
				  plantillaHtml = plantillaHtml.Replace("{TipoOperacion}", tipoOper.Where(x => x.IdSubList == TipoOperacion.FirstOrDefault().Value).FirstOrDefault().CompositeName);


			var EmisorRazonSocial = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "RegistrationName");
			if (EmisorRazonSocial.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorRazonSocial}", EmisorRazonSocial.FirstOrDefault().Value);

			var EmisorNombreComercial = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyName").Elements(cbc + "Name");
			if (EmisorNombreComercial.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorNombreComercial}", EmisorNombreComercial.FirstOrDefault().Value);



			var EmisorNit = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "CompanyID");
			if (EmisorNit.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorNit}", EmisorNit.FirstOrDefault().Value);


			var EmisorTipoContribuyente = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "CompanyID");
			if (EmisorTipoContribuyente.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorTipoContribuyente}", EmisorTipoContribuyente.FirstOrDefault().Value == "3" ? "NO IDENTIFICADO" : EmisorTipoContribuyente.FirstOrDefault().Value == "2" ? "Persona Natural" : "Persona Juridica");


			var EmisorRegimenFiscal = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "TaxLevelCode");

			if (EmisorRegimenFiscal.Count() != 0)
			{
				var regimen = EmisorRegimenFiscal.FirstOrDefault().Value.Split(';');
				var regim = new StringBuilder();
				for (int i = 0; i < regimen.Count(); i++)
				{
					try
					{
						regim.Append(Regimen.Where(x => x.IdSubList.ToString() == regimen[i]).FirstOrDefault().IdSubList + ";");
					}
					catch (Exception)
					{

						regim.Append(regimen[i] + ";");
					}



				}
				plantillaHtml = plantillaHtml.Replace("{EmisorRegimenFiscal}", regim.ToString().Substring(0, regim.ToString().Length - 1));
			}

			var EmisorResponsabilidadTributaria = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "TaxScheme").Elements(cbc + "Name");
			if (EmisorResponsabilidadTributaria.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorResponsabilidadTributaria}", EmisorResponsabilidadTributaria.FirstOrDefault().Value);




			var EmisorPais = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cac + "Country").Elements(cbc + "Name");
			if (EmisorPais.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorPais}", EmisorPais.FirstOrDefault().Value);
			else
				plantillaHtml = plantillaHtml.Replace("{EmisorPais}", string.Empty);

			var EmisorDepartamento = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cbc + "CountrySubentityCode");
			if (EmisorDepartamento.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorDepartamento}", Depto.Where(x => x.IdDepartament == EmisorDepartamento.FirstOrDefault().Value).FirstOrDefault().NameDepartament);
			else
				plantillaHtml = plantillaHtml.Replace("{EmisorDepartamento}", string.Empty);
			//AdquirienteRegimenFiscal

			var EmisorCiudad = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cbc + "CityName");
			if (EmisorCiudad.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorCiudad}", EmisorCiudad.FirstOrDefault().Value);
			else
				plantillaHtml = plantillaHtml.Replace("{EmisorCiudad}", string.Empty);

			var EmisorDireccion = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cac + "AddressLine").Elements(cbc + "Line");
			if (EmisorDireccion.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorDireccion}", EmisorDireccion.FirstOrDefault().Value);
			else
				plantillaHtml = plantillaHtml.Replace("{EmisorDireccion}", string.Empty);

			var EmisorCorreo = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "Contact").Elements(cbc + "ElectronicMail");
			if (EmisorCorreo.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorCorreo}", EmisorCorreo.FirstOrDefault().Value);
			else
				plantillaHtml = plantillaHtml.Replace("{EmisorCorreo}", string.Empty);

			var EmisorTelefono = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "Contact").Elements(cbc + "Telephone");
			if (EmisorTelefono.Any())
				plantillaHtml = plantillaHtml.Replace("{EmisorTelefono}", EmisorTelefono.FirstOrDefault().Value);
			else
				plantillaHtml = plantillaHtml.Replace("{EmisorTelefono}", string.Empty);






			var AdquirienteNombre = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyName").Elements(cbc + "Name");
			if (AdquirienteNombre.Any())
				plantillaHtml = plantillaHtml.Replace("{AdquirienteNombre}", AdquirienteNombre.FirstOrDefault().Value);

			var AccountingCustomerPartyName = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "RegistrationName");
			if (AccountingCustomerPartyName.Any())
			{
				plantillaHtml = plantillaHtml.Replace("{AdquirienteRazon}", AccountingCustomerPartyName.FirstOrDefault().Value);
				plantillaHtml = plantillaHtml.Replace("{NombreEntidad}", AccountingCustomerPartyName.FirstOrDefault().Value);
			}
			else
			{
				plantillaHtml = plantillaHtml.Replace("{AdquirienteRazon}", string.Empty);
				plantillaHtml = plantillaHtml.Replace("{NombreEntidad}", string.Empty);
			}

			//var AdquirienteTipoDocumento = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "CompanyID");
			//if (AdquirienteTipoDocumento.Any())
			//	plantillaHtml = plantillaHtml.Replace("{AdquirienteTipoDocumento}", AdquirienteTipoDocumento.FirstOrDefault().Value);


			var VendedorNumeroDocumento = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "CompanyID");
			if (VendedorNumeroDocumento.Any())
			{
				plantillaHtml = plantillaHtml.Replace("{AdquirienteTipoDocumento}", DocumentType.Where(x => x.IdDocumentType.ToString() == VendedorNumeroDocumento.FirstOrDefault().Attribute("schemeName").Value).FirstOrDefault().NameDocumentType);
				plantillaHtml = plantillaHtml.Replace("{AdquirienteNumeroDocumento}", VendedorNumeroDocumento.FirstOrDefault().Value);
				plantillaHtml = plantillaHtml.Replace("{EntidadTipoDocumento}", DocumentType.Where(x => x.IdDocumentType.ToString() == VendedorNumeroDocumento.FirstOrDefault().Attribute("schemeName").Value).FirstOrDefault().CompositeName);
				plantillaHtml = plantillaHtml.Replace("{AdquirienteNit}", VendedorNumeroDocumento.FirstOrDefault().Value);
			}
			else
			{
				plantillaHtml = plantillaHtml.Replace("{AdquirienteTipoDocumento}", string.Empty);
				plantillaHtml = plantillaHtml.Replace("{AdquirienteNumeroDocumento}", string.Empty);
				plantillaHtml = plantillaHtml.Replace("{EntidadTipoDocumento}", string.Empty);
				plantillaHtml = plantillaHtml.Replace("{AdquirienteNit}", string.Empty);
			}

			var AdquirienteRegimenFiscal = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "TaxLevelCode");

			if (AdquirienteRegimenFiscal.Count() != 0)
			{
				var regimen = AdquirienteRegimenFiscal.FirstOrDefault().Value.Split(';');
				var regim = new StringBuilder();
				for (int i = 0; i < regimen.Count(); i++)
				{

					regim.Append(Regimen.Where(x => x.IdSubList.ToString() == regimen[i]).FirstOrDefault().IdSubList + ";");

				}
				plantillaHtml = plantillaHtml.Replace("{AdquirienteRegimen}", regim.ToString().Substring(0, regim.ToString().Length - 1));
			}
			else
			{
				plantillaHtml = plantillaHtml.Replace("{AdquirienteRegimen}", string.Empty);

			}

			

			var AdquirienteResponsabilidadTributaria = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "TaxScheme").Elements(cbc + "Name");
			if (AdquirienteResponsabilidadTributaria.Any())
				plantillaHtml = plantillaHtml.Replace("{AdquirienteResponsabilidad}", AdquirienteResponsabilidadTributaria.FirstOrDefault().Value);
			else
				plantillaHtml = plantillaHtml.Replace("{AdquirienteResponsabilidad}", string.Empty);


            if (TipoDoc.Any())
            {
				if (TipoDoc.FirstOrDefault().Value == "55" || TipoDoc.FirstOrDefault().Value == "50" || TipoDoc.FirstOrDefault().Value == "27")
				{
					var AdquirientePais = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PhysicalLocation").Elements(cac + "Address").Elements(cac + "Country").Elements(cbc + "Name");
					if (AdquirientePais.Any())
						plantillaHtml = plantillaHtml.Replace("{AdquirientePais}", AdquirientePais.FirstOrDefault().Value);
					else
						plantillaHtml = plantillaHtml.Replace("{AdquirientePais}", string.Empty);

					var AdquirienteDepartamento = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PhysicalLocation").Elements(cac + "Address").Elements(cbc + "CountrySubentityCode");
					if (AdquirienteDepartamento.Any())
						plantillaHtml = plantillaHtml.Replace("{AdquirienteDepartamento}", Depto.Where(x => x.IdDepartament == AdquirienteDepartamento.FirstOrDefault().Value).FirstOrDefault().NameDepartament);
					else
						plantillaHtml = plantillaHtml.Replace("{AdquirienteDepartamento}", string.Empty);
					//AdquirienteRegimenFiscal

					var AdquirienteCiudad = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PhysicalLocation").Elements(cac + "Address").Elements(cbc + "CityName");
					if (AdquirienteCiudad.Any())
						plantillaHtml = plantillaHtml.Replace("{AdquirienteCiudad}", AdquirienteCiudad.FirstOrDefault().Value);
					else
						plantillaHtml = plantillaHtml.Replace("{AdquirienteCiudad}", string.Empty);

					var AdquirienteDireccion = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PhysicalLocation").Elements(cac + "Address").Elements(cac + "AddressLine").Elements(cbc + "Line");
					if (AdquirienteDireccion.Any())
						plantillaHtml = plantillaHtml.Replace("{AdquirienteDireccion}", AdquirienteDireccion.FirstOrDefault().Value);
					else
						plantillaHtml = plantillaHtml.Replace("{AdquirienteDireccion}", string.Empty);
				}
				else
				{
					var AdquirientePais = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cac + "Country").Elements(cbc + "Name");
					if (AdquirientePais.Any())
						plantillaHtml = plantillaHtml.Replace("{AdquirientePais}", AdquirientePais.FirstOrDefault().Value);
					else
						plantillaHtml = plantillaHtml.Replace("{AdquirientePais}", string.Empty);

					var AdquirienteDepartamento = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cbc + "CountrySubentityCode");
					if (AdquirienteDepartamento.Any())
						plantillaHtml = plantillaHtml.Replace("{AdquirienteDepartamento}", Depto.Where(x => x.IdDepartament == AdquirienteDepartamento.FirstOrDefault().Value).FirstOrDefault().NameDepartament);
					else
						plantillaHtml = plantillaHtml.Replace("{AdquirienteDepartamento}", string.Empty);
					//AdquirienteRegimenFiscal

					var AdquirienteCiudad = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cbc + "CityName");
					if (AdquirienteCiudad.Any())
						plantillaHtml = plantillaHtml.Replace("{AdquirienteCiudad}", AdquirienteCiudad.FirstOrDefault().Value);
					else
						plantillaHtml = plantillaHtml.Replace("{AdquirienteCiudad}", string.Empty);

					var AdquirienteDireccion = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cac + "AddressLine").Elements(cbc + "Line");
					if (AdquirienteDireccion.Any())
						plantillaHtml = plantillaHtml.Replace("{AdquirienteDireccion}", AdquirienteDireccion.FirstOrDefault().Value);
					else
						plantillaHtml = plantillaHtml.Replace("{AdquirienteDireccion}", string.Empty);
				}
			}
			else
			{
				var AdquirientePais = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cac + "Country").Elements(cbc + "Name");
				if (AdquirientePais.Any())
					plantillaHtml = plantillaHtml.Replace("{AdquirientePais}", AdquirientePais.FirstOrDefault().Value);
				else
					plantillaHtml = plantillaHtml.Replace("{AdquirientePais}", string.Empty);

				var AdquirienteDepartamento = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cbc + "CountrySubentityCode");
				if (AdquirienteDepartamento.Any())
					plantillaHtml = plantillaHtml.Replace("{AdquirienteDepartamento}", Depto.Where(x => x.IdDepartament == AdquirienteDepartamento.FirstOrDefault().Value).FirstOrDefault().NameDepartament);
				else
					plantillaHtml = plantillaHtml.Replace("{AdquirienteDepartamento}", string.Empty);
				//AdquirienteRegimenFiscal

				var AdquirienteCiudad = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cbc + "CityName");
				if (AdquirienteCiudad.Any())
					plantillaHtml = plantillaHtml.Replace("{AdquirienteCiudad}", AdquirienteCiudad.FirstOrDefault().Value);
				else
					plantillaHtml = plantillaHtml.Replace("{AdquirienteCiudad}", string.Empty);

				var AdquirienteDireccion = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cac + "AddressLine").Elements(cbc + "Line");
				if (AdquirienteDireccion.Any())
					plantillaHtml = plantillaHtml.Replace("{AdquirienteDireccion}", AdquirienteDireccion.FirstOrDefault().Value);
				else
					plantillaHtml = plantillaHtml.Replace("{AdquirienteDireccion}", string.Empty);
			}


			var AdquirienteCorreo = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "Contact").Elements(cbc + "ElectronicMail");
			if (AdquirienteCorreo.Any())
				plantillaHtml = plantillaHtml.Replace("{AdquirienteCorreo}", AdquirienteCorreo.FirstOrDefault().Value);
			else
				plantillaHtml = plantillaHtml.Replace("{AdquirienteCorreo}", string.Empty);




			return plantillaHtml;
		}
		private static async Task<string> CruzarModeloDetallesProductosComplete(string plantillaHtml, List<XElement> model, string tipoD)
		{
			NumberFormatInfo formato = new CultureInfo("es-AR").NumberFormat;
			formato.CurrencyGroupSeparator = ".";
			formato.NumberDecimalSeparator = ",";

			var rowDetalleProductosBuilder = new StringBuilder();
			XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
			XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
			var cosmos = new CosmosDbManagerPayroll();
			decimal subTotal = 0;			
			decimal DescDet = 0;
			decimal RecDet = 0;
			var subTotalTotal = "";
			


			foreach (var detalle in model)
			{
				decimal descunetoTotal = 0;
				decimal recargototal = 0;
				var numinf = new NumberFormatInfo { NumberDecimalSeparator = "." };
				//<td>{Unidad.Where(x=>x.IdSubList.ToString()== detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").Attributes("unitCode").FirstOrDefault().Value).FirstOrDefault().CompositeName}</td>
				var unit = await cosmos.getUnidad(detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").Attributes("unitCode").FirstOrDefault().Value);

				var ivaValor =tipoD!="Nota"? detalle.Elements(cac + "TaxTotal").Elements(cac + "TaxSubtotal").Elements(cbc + "TaxableAmount") : detalle.Elements(cac + "TaxTotal").Elements(cbc + "TaxAmount"); 
				var ivaValorGran =tipoD!="Nota"? detalle.Elements(cac + "TaxTotal").ToList() : detalle.Elements(cac + "TaxTotal").ToList(); 
				var IvaVal = ivaValor.Count() == 0 ? "" : ivaValor.FirstOrDefault().Value;

				var ivaPorc = detalle.Elements(cac + "TaxTotal").Elements(cac + "TaxSubtotal").Elements(cac + "TaxCategory").Elements(cbc + "Percent");
				var IvaPor = ivaPorc.Count() == 0 ? "" : ivaPorc.FirstOrDefault().Value;
				if (tipoD == "55" || tipoD == "50" || tipoD == "45" || tipoD == "27" || tipoD == "32")
				{
					
                    foreach (var item in ivaValorGran)
                    {
						var tipo = item.Elements(cac + "TaxSubtotal").Elements(cac + "TaxCategory").Elements(cac + "TaxScheme").Elements(cbc + "ID").FirstOrDefault().Value;
                        if (tipo =="01")
                        {
					      ivaValor = item.Elements(cbc + "TaxAmount");
                        }

                    }

					IvaVal = ivaValor.Count() == 0 ? "0" : ivaValor.FirstOrDefault().Value;

				}
				var Desc = "";
				var Reca = "";
				var recargos = detalle.Elements(cac + "AllowanceCharge").ToList();
				foreach (var item in recargos)
				{
					var tipo = item.Elements(cbc + "ChargeIndicator").FirstOrDefault().Value;
					if (tipo.ToUpper() == "TRUE")
					{
						Reca = item.Elements(cbc + "Amount").FirstOrDefault().Value;

						recargototal = decimal.Parse(Reca, numinf);
						RecDet = RecDet + decimal.Parse(Reca, numinf);
					}
					else
					{
						Desc = item.Elements(cbc + "Amount").FirstOrDefault().Value;

						descunetoTotal = decimal.Parse(Desc, numinf);
						DescDet = DescDet + decimal.Parse(Desc, numinf);
                        
					}
				}
				var fechaPeriodo = detalle.Elements(cac + "InvoicePeriod").Elements(cbc + "StartDate");

				var FechaPeriodo = fechaPeriodo.Any() ? fechaPeriodo.FirstOrDefault().Value : "";

				var fechaPeriodofinal = detalle.Elements(cac + "InvoicePeriod").Elements(cbc + "EndDate");

				var FechaPeriodofinal = fechaPeriodofinal.Any() ? fechaPeriodofinal.FirstOrDefault().Value : "";

				var Descr = detalle.Elements(cac + "InvoicePeriod").Elements(cbc + "Description");

				var Descrip = Descr.Any() ? Descr.FirstOrDefault().Value : "";

				var desc2 = detalle.Elements(cac + "Item").Elements(cbc + "Description");
				var des2 = desc2.Any() ? desc2.FirstOrDefault().Value : "";

				var preciounitario = detalle.Elements(cac + "Price").Elements(cbc + "PriceAmount");
				decimal Preciounitario = decimal.Parse(preciounitario.FirstOrDefault().Value, numinf);

				var valortotal = detalle.Elements(cbc + "LineExtensionAmount");
				decimal Valortotal = decimal.Parse(valortotal.FirstOrDefault().Value, numinf);


				decimal ivaVal = decimal.Parse(IvaVal, numinf);

				if ( tipoD == "Nota" || tipoD == "27"|| tipoD == "32")
				{
					rowDetalleProductosBuilder.Append($@"
                <tr>
		            <td>{detalle.Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cac + "Item").Elements(cac + "StandardItemIdentification").Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{des2}</td>
		            <td>{unit.CompositeName}</td>
		            <td>{detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").FirstOrDefault().Value}</td>
                    <td>{Preciounitario.ToString("N", formato)}</td>
					<td class='text-right'>{descunetoTotal.ToString("N", formato)}</td>
                    <td class='text-right'>{recargototal.ToString("N", formato)}</td>
		            <td class='text-right'>{ivaVal.ToString("N", formato)}</td>
                    <td class='text-right'>{IvaPor:n2}</td>


		            <td style='word-wrap: break-word;'>{Valortotal.ToString("N", formato)}</td>

	            </tr>");
				}else if(tipoD == "55" || tipoD == "50")
				{
					

					rowDetalleProductosBuilder.Append($@"
					 <tr>
		            <td>{detalle.Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cac + "Item").Elements(cac + "StandardItemIdentification").Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{des2}</td>
		            <td>{unit.CompositeName}</td>
		            <td>{detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").FirstOrDefault().Value}</td>
                    <td>{Preciounitario.ToString("N", formato)}</td>
					<td class='text-right'>{descunetoTotal.ToString("N", formato)}</td>
                    <td class='text-right'>{recargototal.ToString("N", formato)}</td>
		            <td class='text-right'>{ivaVal.ToString("N", formato)}</td>
                    <td class='text-right'>{IvaPor:n2}</td>


		            <td style='word-wrap: break-word;'>{Valortotal.ToString("N", formato)}</td>

					 </tr>");
				}
				else if (tipoD == "45")
				{
					rowDetalleProductosBuilder.Append($@"
                <tr>
		            <td>{detalle.Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cac + "Item").Elements(cac + "StandardItemIdentification").Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cac + "Item").Elements(cbc + "Description").FirstOrDefault().Value}</td>
		            <td>{unit.CompositeName}</td>
		            <td>{detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").FirstOrDefault().Value}</td>
                    <td>{Preciounitario.ToString("N", formato)}</td>
					<td class='text-right'>{descunetoTotal.ToString("N", formato)}</td>
                    <td class='text-right'>{recargototal.ToString("N", formato)}</td>
		            <td class='text-right'>{ivaVal.ToString("N", formato)}</td>
                    <td class='text-right'>{IvaPor:n2}</td>


		            
		            <td>{FechaPeriodo:dd/MM/yyyy}</td>
					<td>{FechaPeriodofinal:dd/MM/yyyy}</td>
					<td>{Valortotal.ToString("N", formato)}</td>
	            </tr>");
				}
				else
				{
					rowDetalleProductosBuilder.Append($@"
                <tr>
		            <td>{detalle.Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cac + "Item").Elements(cac + "StandardItemIdentification").Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td>{detalle.Elements(cac + "Item").Elements(cbc + "Description").FirstOrDefault().Value}</td>
		            <td>{unit.CompositeName}</td>
		            <td>{detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").FirstOrDefault().Value}</td>
                   <td>{Preciounitario.ToString("N", formato)}</td>
					<td class='text-right'>{descunetoTotal.ToString("N", formato)}</td>
                    <td class='text-right'>{recargototal.ToString("N", formato)}</td>
		            <td class='text-right'>{ivaVal.ToString("N", formato)}</td>
                    <td class='text-right'>{IvaPor:n2}</td>


		            <td>{Valortotal.ToString("N", formato)}</td>
					<td>{Descrip}</td>
		            <td>{FechaPeriodo:dd/MM/yyyy}</td>
	            </tr>");
				}

				if (tipoD == "Nota")
					subTotal = subTotal + decimal.Parse(detalle.Elements(cac + "Price").Elements(cbc + "PriceAmount").FirstOrDefault().Value.ToString(), numinf) *
										decimal.Parse(detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").FirstOrDefault().Value, numinf);
				else
					if (tipoD == "")
					subTotalTotal = detalle.Elements(cac + "TaxTotal").Elements(cac + "TaxSubtotal").Elements(cbc + "TaxableAmount").FirstOrDefault().Value.ToString();


				else
					
				subTotal = subTotal + decimal.Parse(detalle.Elements(cac + "Price").Elements(cbc + "PriceAmount").FirstOrDefault().Value.ToString(), numinf) *
									decimal.Parse(detalle.Elements(cbc + "InvoicedQuantity").FirstOrDefault().Value.ToString(), numinf);

					


			}
			plantillaHtml = plantillaHtml.Replace("{RowsDetalleProductos}", rowDetalleProductosBuilder.ToString());
			//var dub = detalle.Elements(cac + "Price").Elements(cbc + "PriceAmount").FirstOrDefault().Value.ToString("0,0", System.Globalization.CultureInfo.InvariantCulture);

			subTotal = decimal.Round(subTotal, 2);
			DescDet = decimal.Round(DescDet, 2);
			RecDet = decimal.Round(RecDet, 2);
			if (tipoD =="55" || tipoD == "50" || tipoD == "45")
            {				
				plantillaHtml = plantillaHtml.Replace("{SubTotal}", subTotal.ToString("N", formato));
				plantillaHtml = plantillaHtml.Replace("{DescuentoDetalle}", DescDet.ToString("N", formato));
				plantillaHtml = plantillaHtml.Replace("{RecargoDetalle}", RecDet.ToString("N", formato));

			}
            else
            {

			  plantillaHtml = plantillaHtml.Replace("{SubTotal}", subTotal.ToString("N", formato));
				plantillaHtml = plantillaHtml.Replace("{DescuentoDetalle}", DescDet.ToString("N", formato));
				plantillaHtml = plantillaHtml.Replace("{RecargoDetalle}", RecDet.ToString("N", formato));
			}

			return plantillaHtml;
		}
		public static string Reverse(string text) { char[] cArray = text.ToCharArray(); string reverse = String.Empty; for (int i = cArray.Length - 1; i > -1; i--) { reverse += cArray[i]; } return reverse; }

		
		private static async Task<string> CruzarModeloDetallesProductosContador(string plantillaHtml, List<XElement> model, XElement element)
		{
			NumberFormatInfo formato = new CultureInfo("es-AR").NumberFormat;
			var numinf = new NumberFormatInfo { NumberDecimalSeparator = "." };
			formato.CurrencyGroupSeparator = ".";
			formato.NumberDecimalSeparator = ",";
			var rowDetalleProductosBuilder = new StringBuilder();

			XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
			XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
			XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
			XNamespace def = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
			XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
			var cosmos = new CosmosDbManagerPayroll();
			decimal subTotal = 0;
			decimal DescDet = 0;
			decimal RecDet = 0;
			var _bandera = 0;
			var OtrasEntidades = "";
			/// Invoice / ext:UBLExtensions / ext:UBLExtension / ext:ExtensionContent / Services_SPD / SubscriberConsumption / ConsumptionSection / SubInvoiceLines / SubInvoiceLine / Balance / Transactions / Transaction / CreditLineAmount
			var AcuerdosPagos = element.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "Services_SPD").Elements(def + "SubscriberConsumption").Elements(def + "ConsumptionSection")
						.Elements(def + "SubInvoiceLines").Elements(def + "SubInvoiceLine").Elements(def + "Balance").Elements(def + "Transactions").Elements(def + "Transaction").Elements(def + "CreditLineAmount");

			var listaCont = new List<Cont>();

			var contador = element.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "Services_SPD").Elements(def + "SubscriberConsumption").Elements(def + "ConsumptionSection")
						.Elements(def + "SPDDebitsForPartialConsumption").Elements(def + "SPDDebitForPartialConsumption").Elements(def + "UtilityMeter").Elements(def + "MeterNumber");

			foreach (var item in contador)
			{
				var node = item.Parent.Parent.Parent.Parent.Parent.Parent.Element(def + "ID");

				listaCont.Add(new Cont() { numero = item.Value, id = node.Value });

			}
			string[] info = new string[20];
			int continfo = 0;
			foreach (var item in AcuerdosPagos)
			{
				info[continfo] = item.Value;
				continfo = continfo + 1;
			}


			var _bande = 0;
			foreach (var detalle in model)
			{				
				//<td>{Unidad.Where(x=>x.IdSubList.ToString()== detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").Attributes("unitCode").FirstOrDefault().Value).FirstOrDefault().CompositeName}</td>
				var unit = await cosmos.getUnidad(detalle.Elements(cac + "Price").Elements(cbc + "BaseQuantity").Attributes("unitCode").FirstOrDefault().Value);

				var ivaValor = detalle.Elements(cac + "TaxTotal").Elements(cac + "TaxSubtotal").Elements(cbc + "TaxAmount");
				var IvaVal = ivaValor.Count() == 0 ? "" : ivaValor.FirstOrDefault().Value;

				var ivaPorc = detalle.Elements(cac + "TaxTotal").Elements(cac + "TaxSubtotal").Elements(cac + "TaxCategory").Elements(cbc + "Percent");
				var IvaPor = ivaPorc.Count() == 0 ? "" : ivaPorc.FirstOrDefault().Value;
				var Desc = "";
				var Reca = "";
				var recargos = detalle.Elements(cac + "AllowanceCharge").ToList();
				foreach (var item in recargos)
				{
					var tipo = item.Elements(cbc + "ChargeIndicator").FirstOrDefault().Value;
					if (tipo.ToUpper() == "TRUE")
					{
						Reca = item.Elements(cbc + "Amount").FirstOrDefault().Value;

						RecDet = RecDet + decimal.Parse(Reca, numinf);
					}
					else
					{
						Desc = item.Elements(cbc + "Amount").FirstOrDefault().Value;
						DescDet = DescDet + decimal.Parse(Desc, numinf);
					}
				}
				var fechaPeriodo = detalle.Elements(cac + "InvoicePeriod").Elements(cbc + "StartDate");

				var FechaPeriodo = fechaPeriodo.Any() ? fechaPeriodo.FirstOrDefault().Value : "";
				var Descr = detalle.Elements(cac + "InvoicePeriod").Elements(cbc + "Description");

				var Descrip = Descr.Any() ? Descr.FirstOrDefault().Value : "";
				
				var cont = listaCont.Where(x => x.id == element.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "Services_SPD").Elements(def + "ID").FirstOrDefault().Value);
				var conta = cont.Any() ? cont.FirstOrDefault().numero : "";

				decimal ivaVal = decimal.Parse(IvaVal, numinf);
				ivaVal = decimal.Round(ivaVal, 2);
				DescDet = decimal.Round(DescDet, 2);
				RecDet = decimal.Round(RecDet, 2);

				if (_bande < listaCont.Count())
                {
					rowDetalleProductosBuilder.Append($@"
					<tr>
						<td>{detalle.Elements(cbc + "ID").FirstOrDefault().Value}</td>
						<td>{detalle.Elements(cac + "Item").Elements(cac + "StandardItemIdentification").Elements(cbc + "ID").FirstOrDefault().Value}</td>
						<td>{detalle.Elements(cac + "Item").Elements(cbc + "Description").FirstOrDefault().Value}</td>
						<td>{unit.IdSubList}</td>
						<td>{detalle.Elements(cbc + "InvoicedQuantity").FirstOrDefault().Value}</td>
						<td>{detalle.Elements(cbc + "LineExtensionAmount").FirstOrDefault().Value}</td>		            
						<td>{detalle.Elements(cac + "Price").Elements(cbc + "PriceAmount").FirstOrDefault().Value}</td>
						<td>{DescDet.ToString("N", formato)}</td>
						<td>{info[_bande]}</td>
						<td class='text-right'>{RecDet.ToString("N", formato)}</td>
						<td class='text-right'>{ivaVal.ToString("N", formato)}</td>
						<td class='text-right'>{IvaPor:n2}</td>
						<td style='word-wrap: break-word;'>{detalle.Elements(cbc + "LineExtensionAmount").FirstOrDefault().Value}</td>		
						<td style='word-wrap: break-word;'>{listaCont[_bande].numero}</td>
		    
					</tr>");

					subTotal = subTotal + decimal.Parse(detalle.Elements(cac + "Price").Elements(cbc + "PriceAmount").FirstOrDefault().Value.ToString(), numinf) *
									decimal.Parse(detalle.Elements(cbc + "InvoicedQuantity").FirstOrDefault().Value.ToString(), numinf);
				}
				
				_bande = _bande + 1;
			}
			plantillaHtml = plantillaHtml.Replace("{RowsDetalleProductos}", rowDetalleProductosBuilder.ToString());
			subTotal = decimal.Round(subTotal, 2);
			DescDet = decimal.Round(DescDet, 2);
			RecDet = decimal.Round(RecDet, 2);

			plantillaHtml = plantillaHtml.Replace("{SubTotal}", subTotal.ToString("N", formato));
			decimal acuerdosPagos = 0;

			if (AcuerdosPagos.Any())
			{
				acuerdosPagos = decimal.Parse(AcuerdosPagos.FirstOrDefault().Value, numinf);
				acuerdosPagos = decimal.Round(acuerdosPagos, 2);
				plantillaHtml = plantillaHtml.Replace("{AcuerdosPagos}", acuerdosPagos.ToString("N", formato));
			}
			else
			{
				plantillaHtml = plantillaHtml.Replace("{AcuerdosPagos}", string.Empty);
			}
			
			if (OtrasEntidades.Any())
				plantillaHtml = plantillaHtml.Replace("{OtrasEntidades}", OtrasEntidades);
			else
				plantillaHtml = plantillaHtml.Replace("{OtrasEntidades}", string.Empty);

			plantillaHtml = plantillaHtml.Replace("{DescuentoDetalle}", DescDet.ToString("N", formato));
			plantillaHtml = plantillaHtml.Replace("{RecargoDetalle}", RecDet.ToString("N", formato));
			return plantillaHtml;
		}

		public static string GenerateQrBase64ForDocument(string code)
		{
			//var urlSiteDian = ConfigurationManager.GetValue("SiteDian");
			var urlSiteDian = "https://gtpa-web-prototype-test-uat-utnxse.azurewebsites.net/";
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
			NumberFormatInfo formato = new CultureInfo("es-AR").NumberFormat;
			formato.CurrencyGroupSeparator = ".";
			formato.NumberDecimalSeparator = ",";
			var cosmos = new CosmosDbManagerPayroll();
			var numinf = new NumberFormatInfo { NumberDecimalSeparator = "." };
			var DocumentType = await cosmos.getDocumentType();
			var FormasPago = await cosmos.getFormapago();
			var MediosPago = await cosmos.getPaymentMethod();
			var Regimen = await cosmos.getRegimen();
			var Depto = await cosmos.getDepartament();
			var UUID = model.Elements(cbc + "UUID");
			var typeDocument = model.Elements(cbc + "InvoiceTypeCode");
			var typeNote = model.Elements(cbc + "CreditNoteTypeCode");
			Html = Html.Replace("{CodigoUnicoDocumentoSoporte}", UUID.FirstOrDefault().Value);

			var ID = model.Elements(cbc + "ID");
			Html = Html.Replace("{NumeroDocumentoSoporte}", ID.FirstOrDefault().Value);

			var IssueDate = model.Elements(cbc + "IssueDate");
			var IssueTime = model.Elements(cbc + "IssueTime");
			Html = Html.Replace("{FechaGeneracion}", IssueDate.FirstOrDefault().Value + " " + IssueTime.FirstOrDefault().Value);

			var DueDate = model.Elements(cbc + "DueDate");
			if (DueDate.Any())
				Html = Html.Replace("{FechaVencimiento}", DueDate.FirstOrDefault().Value);

			var AccountingCustomerPartyName = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyName").Elements(cbc + "Name");
			if (AccountingCustomerPartyName.Any())
				Html = Html.Replace("{AdquirienteRazonSocial}", AccountingCustomerPartyName.FirstOrDefault().Value);

			var CompanyID = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "CompanyID");
			if (CompanyID.Any())
				Html = Html.Replace("{AdquirienteNit}", CompanyID.FirstOrDefault().Value);

			var AdquirienteTipoContribuyente = model.Elements(cac + "AccountingCustomerParty").Elements(cbc + "AdditionalAccountID");
			if (AdquirienteTipoContribuyente.Any())
				Html = Html.Replace("{AdquirienteTipoContribuyente}", AdquirienteTipoContribuyente.FirstOrDefault().Value == "3" ? "NO IDENTIFICADO" : AdquirienteTipoContribuyente.FirstOrDefault().Value == "2" ? "Persona Natural" : "Persona Juridica");
			

			var AccountingSupplierPartyName = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyName").Elements(cbc + "Name");
			if (AccountingSupplierPartyName.Any())
				Html = Html.Replace("{VendedorRazonSocial}", AccountingSupplierPartyName.FirstOrDefault().Value);

			//falta tipo de documento


			try
			{
				var VendedorNumeroDocumento = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "CompanyID");
				Html = Html.Replace("{VendedorTipoDocumento}", DocumentType.Where(x => x.IdDocumentType.ToString() == VendedorNumeroDocumento.FirstOrDefault().Attribute("schemeName").Value).FirstOrDefault().NameDocumentType);
				Html = Html.Replace("{EmisorTipoDocumento}", DocumentType.Where(x => x.IdDocumentType.ToString() == VendedorNumeroDocumento.FirstOrDefault().Attribute("schemeName").Value).FirstOrDefault().NameDocumentType);
				Html = Html.Replace("{EmisorTipoDocumento}", DocumentType.Where(x => x.IdDocumentType.ToString() == VendedorNumeroDocumento.FirstOrDefault().Attribute("schemeName").Value).FirstOrDefault().NameDocumentType);
				Html = Html.Replace("{VendedorNumeroDocumento}", VendedorNumeroDocumento.FirstOrDefault().Value);
			}
			catch (Exception)
			{

				Html = Html.Replace("{VendedorTipoDocumento}", string.Empty);
				Html = Html.Replace("{EmisorTipoDocumento}", string.Empty);
				Html = Html.Replace("{EmisorTipoDocumento}", string.Empty);
				Html = Html.Replace("{VendedorNumeroDocumento}", string.Empty);
			}





			var VendedorTipoContribuyente = model.Elements(cac + "AccountingSupplierParty").Elements(cbc + "AdditionalAccountID");
			Html = Html.Replace("{VendedorTipoContribuyente}", VendedorTipoContribuyente.FirstOrDefault().Value == "3" ? "NO IDENTIFICADO" : VendedorTipoContribuyente.FirstOrDefault().Value == "2" ? "Persona Natural" : "Persona Juridica");


			var VendedorRegimenFiscal = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "TaxLevelCode");
			if (VendedorRegimenFiscal.Count() != 0)
			{
				var regimen = VendedorRegimenFiscal.FirstOrDefault().Value.Split(';');
				var regim = new StringBuilder();
				for (int i = 0; i < regimen.Count(); i++)
				{
					try
					{
						regim.Append(Regimen.Where(x => x.IdSubList.ToString() == regimen[i]).FirstOrDefault().IdSubList + "\n ");

					}
					catch (Exception)
					{

						regim.Append(regimen[i] + "\n ");
					}



				}
				Html = Html.Replace("{VendedorRegimenFiscal}", regim.ToString());
			}


			//falta regimen fiscal
			var VendedorResponsabilidadTributaria = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "TaxScheme").Elements(cbc + "Name");
			var VendedorResponsabilidadTributariaID = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "TaxScheme").Elements(cbc + "ID");
			//Html = Html.Replace("{VendedorResponsabilidadTributaria}", VendedorResponsabilidadTributariaID.FirstOrDefault().Value + "-" + VendedorResponsabilidadTributaria.FirstOrDefault().Value);
			Html = Html.Replace("{VendedorResponsabilidadTributaria}", VendedorResponsabilidadTributaria.FirstOrDefault().Value);

			var Moneda = model.Elements(cbc + "DocumentCurrencyCode");
			Html = Html.Replace("{Moneda}", Moneda.FirstOrDefault().Value);

			var TasaCambio = model.Elements(cac + "PaymentExchangeRate").Elements(cbc + "CalculationRate");
			if (TasaCambio.Any())
				Html = Html.Replace("{TasaCambio}", TasaCambio.FirstOrDefault().Value);
			else
				Html = Html.Replace("{TasaCambio}", string.Empty);

			var Anticipo = model.Elements(cac + "WithholdingTaxTotal").Elements(cbc + "TaxAmount");
			decimal anticipos = 0;
			if (Anticipo.Any())
            {
				anticipos = decimal.Parse(Anticipo.FirstOrDefault().Value.ToString(), numinf);
				anticipos = decimal.Round(anticipos, 2);
				Html = Html.Replace("{Anticipo}", anticipos.ToString("N", formato));
            }
            else
            {

				Html = Html.Replace("{Anticipo}", string.Empty);
            }
			//ojito ese anticipo esta mal pero es porque en el xpath del excel de anticipos aparece esto asi que se deja
			decimal rentencion = 0;
			var Retenciones = model.Elements(cac + "WithholdingTaxTotal").Elements(cbc + "TaxAmount");
			if (Retenciones.Any())
            {
				rentencion = decimal.Parse(Retenciones.FirstOrDefault().Value.ToString(), numinf);
				rentencion = decimal.Round(rentencion, 2);
				Html = Html.Replace("{Retenciones}", rentencion.ToString("N", formato));
            }
            else
            {

				Html = Html.Replace("{Retenciones}", string.Empty);
            }

			//falta caculos subtotal
			
			decimal TotalIVA = 0;
			decimal impuesto = 0;
			decimal GlobalDescuento = 0;
			decimal GlobalRecargo = 0;
			var TotalBrutoDocumento = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "TaxExclusiveAmount");
			
			var ivaValorGran = model.Elements(cac + "TaxTotal").ToList();
			var TotalNetoDocumento = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "TaxInclusiveAmount");//resta subtotal ? 
			var TotalFactura = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "PayableAmount");//resta subtotal ? 
			var totalIvaString = "";
            if (typeDocument.Any())
            {
				
					foreach (var item in ivaValorGran)
					{
						var tipo = item.Elements(cac + "TaxSubtotal").Elements(cac + "TaxCategory").Elements(cac + "TaxScheme").Elements(cbc + "ID").FirstOrDefault().Value;
						if (tipo == "01")
						{
							totalIvaString  =  item.Elements(cbc + "TaxAmount").FirstOrDefault().Value;
							TotalIVA = TotalIVA + decimal.Parse(totalIvaString, numinf);
						}

					}
					if (typeDocument.FirstOrDefault().Value != "32")
					{

					  TotalBrutoDocumento = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "LineExtensionAmount");
					}
					else
					{
					TotalBrutoDocumento = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "TaxExclusiveAmount");
					}
					decimal TotalBrutodocumento = decimal.Parse(TotalBrutoDocumento.FirstOrDefault().Value, numinf);
					decimal TotalNetodocumento = decimal.Parse(TotalNetoDocumento.FirstOrDefault().Value, numinf);
					decimal Totalfactura = decimal.Parse(TotalFactura.FirstOrDefault().Value, numinf);
					TotalIVA = decimal.Round(TotalIVA, 2);
					Html = Html.Replace("{TotalIVA}", TotalIVA.ToString("N",formato));
					Html = Html.Replace("{TotalBrutoDocumento}", TotalBrutodocumento.ToString("N",formato));
					Html = Html.Replace("{TotalNetoDocumento}", TotalNetodocumento.ToString("N", formato));
					Html = Html.Replace("{TotalFactura}", Totalfactura.ToString("N", formato));
			
			
			}
			else
			{
				Html = Html.Replace("{TotalFactura}", Decimal.Parse(TotalFactura.FirstOrDefault().Value.ToString().Split('.')[0]).ToString("N0"));
				Html = Html.Replace("{TotalNetoDocumento}", Decimal.Parse(TotalNetoDocumento.FirstOrDefault().Value.ToString().Split('.')[0]).ToString("N0"));
				Html = Html.Replace("{TotalBrutoDocumento}", Decimal.Parse(TotalBrutoDocumento.FirstOrDefault().Value.ToString().Split('.')[0]).ToString("N0"));
				Html = Html.Replace("{TotalIVA}", Convert.ToString(TotalIVA).Replace(",", "."));
			}


			
			var OtrosImp = model.Elements(cac + "TaxTotal").Elements(cbc + "TaxAmount");//resta subtotal ? 
			if (OtrosImp.Any())
            {
				impuesto = decimal.Parse(OtrosImp.FirstOrDefault().Value.ToString(), numinf);
				impuesto =  decimal.Round(impuesto, 2);
				Html = Html.Replace("{OtrosImp}",impuesto.ToString("N", formato));
            }
            else
            {
				Html = Html.Replace("{OtrosImp}", string.Empty);

            }


			var TotalImpuestos = impuesto + TotalIVA;
			Html = Html.Replace("{TotalImpuestos}", TotalImpuestos.ToString("N", formato));

			
			

			var DescuentoGlobal = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "AllowanceTotalAmount");//resta subtotal ? 
			if (DescuentoGlobal.Any())
			{
				GlobalDescuento = decimal.Parse(DescuentoGlobal.FirstOrDefault().Value.ToString(), numinf);
				GlobalDescuento = decimal.Round(GlobalDescuento, 2);				
				Html = Html.Replace("{DescuentoGlobal}", GlobalDescuento.ToString("N", formato));
			}
			else
			{
				Html = Html.Replace("{DescuentoGlobal}", string.Empty);
			}

			var RecargoGlobal = model.Elements(cac + "LegalMonetaryTotal").Elements(cbc + "ChargeTotalAmount");//resta subtotal ? 
			if (RecargoGlobal.Any())
            {
				GlobalRecargo = decimal.Parse(RecargoGlobal.FirstOrDefault().Value.ToString(), numinf);
				GlobalRecargo = decimal.Round(GlobalRecargo, 2);
				Html = Html.Replace("{RecargoGlobal}", GlobalRecargo.ToString("N", formato));
            }
            else
            {

				Html = Html.Replace("{RecargoGlobal}", string.Empty);
            }

			

			//var (TotalFactura.FirstOrDefault().Value.ToString("N", new CultureInfo("is-IS")));

			

			try
			{

                if (typeDocument.Any())
                {
					if (typeDocument.FirstOrDefault().Value == "55" || typeDocument.FirstOrDefault().Value == "45" || typeDocument.FirstOrDefault().Value == "27" || typeDocument.FirstOrDefault().Value == "32")
					{
						var fab = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Where(x => x.FirstNode.ToString().Contains("InformacionDelFabricanteDelSoftware"));
						var info = fab.Where(x => x.FirstNode.ToString().Contains("InformacionDelFabricanteDelSoftware"));
						var soft = info.Descendants().Elements(def + "Value").ToArray();
						if (soft.Count() > 0)
						{
							Html = Html.Replace("{FabricanteRazon}", soft[1].Value);
							Html = Html.Replace("{FabricanteNombre}", soft[0].Value);
							Html = Html.Replace("{FabricanteSoftware}", soft[2].Value);
						}
					}
				}			
                else
                {
					var fab = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Where(x => x.FirstNode.ToString().Contains("FabricanteSoftware"));
					var info = fab.Where(x => x.FirstNode.ToString().Contains("InformacionDelFabricanteDelSoftware"));
					var soft = info.Descendants().Elements(def + "Value").ToArray();
					if (soft.Count() > 0)
					{
						Html = Html.Replace("{FabricanteRazon}", soft[1].Value);
						Html = Html.Replace("{FabricanteNombre}", soft[0].Value);
						Html = Html.Replace("{FabricanteSoftware}", soft[2].Value);
					}
				}
				
			}
			catch (Exception)
			{

				Html = Html.Replace("{FabricanteRazon}", string.Empty);
				Html = Html.Replace("{FabricanteNombre}", string.Empty);
				Html = Html.Replace("{FabricanteSoftware}", string.Empty);
			}

			try
			{
				var fab = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Where(x => x.FirstNode.ToString().Contains("BeneficiosComprador"));
				var info = fab.Where(x => x.FirstNode.ToString().Contains("InformacionBeneficiosComprador"));
				var soft = info.Descendants().Elements(def + "Value").ToArray();
				if (soft.Count() > 0)
				{
					Html = Html.Replace("{CodigoComprador}", soft[0].Value);
					Html = Html.Replace("{NombresComprador}", soft[1].Value);
					Html = Html.Replace("{Puntos}", soft[2].Value);
				}
			}
			catch (Exception)
			{

				Html = Html.Replace("{CodigoComprador}", string.Empty);
				Html = Html.Replace("{NombresComprador}", string.Empty);
				Html = Html.Replace("{Puntos}", string.Empty);
			}

			

			//var FabricanteRazon = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext+ "ExtensionContent")
			//    .Elements("FabricanteSoftware").Elements("InformacionDelFabricanteDelSoftware").Elements( "Name");//resta subtotal ? 
			//Html = Html.Replace("{TotalFactura}", FabricanteRazon.FirstOrDefault().Value);

			var NumeroAutorizacion = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent")
				.Elements(sts + "DianExtensions").Elements(sts + "InvoiceControl").Elements(sts + "InvoiceAuthorization");
			if (NumeroAutorizacion.Any())
				Html = Html.Replace("{NumeroAutorizacion}", NumeroAutorizacion.FirstOrDefault().Value);
			else
				Html = Html.Replace("{NumeroAutorizacion}", string.Empty);

			var RangoDesde = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent")
			   .Elements(sts + "DianExtensions").Elements(sts + "InvoiceControl").Elements(sts + "AuthorizedInvoices").Elements(sts + "From");
			if (RangoDesde.Any())
				Html = Html.Replace("{RangoDesde}", RangoDesde.FirstOrDefault().Value);
			else
				Html = Html.Replace("{RangoDesde}", string.Empty);

			var RangoHasta = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent")
			   .Elements(sts + "DianExtensions").Elements(sts + "InvoiceControl").Elements(sts + "AuthorizedInvoices").Elements(sts + "To");

			if (RangoHasta.Any())
				Html = Html.Replace("{RangoHasta}", RangoHasta.FirstOrDefault().Value);
			else
				Html = Html.Replace("{RangoHasta}", string.Empty);

			var Vigencia = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent")
			  .Elements(sts + "DianExtensions").Elements(sts + "InvoiceControl").Elements(sts + "AuthorizationPeriod").Elements(cbc + "EndDate");
			if (Vigencia.Any())
				Html = Html.Replace("{Vigencia}", Vigencia.FirstOrDefault().Value);
			else
				Html = Html.Replace("{Vigencia}", string.Empty);
			//Peajes

			var FormaPago = model.Elements(cac + "PaymentMeans").Elements(cbc + "ID");
			if (FormaPago.Any())
				Html = Html.Replace("{FormaPago}", FormasPago.Where(x => x.IdSubList == FormaPago.FirstOrDefault().Value).FirstOrDefault().ListName);

			var Medio = model.Elements(cac + "PaymentMeans").Elements(cbc + "PaymentMeansCode");
			if (Medio.Any())
				Html = Html.Replace("{MedioPago}", MediosPago.Where(x => x.IdPaymentMethod == Medio.FirstOrDefault().Value).FirstOrDefault().NamePaymentMethod);

			var FechaOrdenPedido = model.Elements(cac + "OrderReference").Elements(cbc + "IssueDate");
			if (FechaOrdenPedido.Any())
			{
				Html = Html.Replace("{FechaOrdenPedido}", FechaOrdenPedido.FirstOrDefault().Value);
				Html = Html.Replace("{FechaOrdenCompra}", FechaOrdenPedido.FirstOrDefault().Value);
			}
			else
			{
				Html = Html.Replace("{FechaOrdenPedido}", string.Empty);
				Html = Html.Replace("{FechaOrdenCompra}", string.Empty);
			}
			var OrdenPedido = model.Elements(cac + "OrderReference").Elements(cbc + "ID");
			if (OrdenPedido.Any())
			{
				Html = Html.Replace("{OrdenPedido}", OrdenPedido.FirstOrDefault().Value);
			
			}
			else
			{
				Html = Html.Replace("{OrdenPedido}", string.Empty);
			
			}

			var NumeroContrato = model.Elements(cac + "AccountingCustomerParty").Elements(cbc + "CustomerAssignedAccountID");

			if (NumeroContrato.Any())
			{

				Html = Html.Replace("{NumeroContrato}", NumeroContrato.FirstOrDefault().Value);
			}
			else
			{
				Html = Html.Replace("{NumeroContrato}", string.Empty);

			}

			var Estrato = model.Elements(cbc + "CustomizationID");

			if (Estrato.Any())
			{

				Html = Html.Replace("{Estrato}", Estrato.FirstOrDefault().Value);
			}
			else
			{
				Html = Html.Replace("{Estrato}", string.Empty);

			}

			var AdquirienteRegimenFiscal = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cbc + "TaxLevelCode");

			if (AdquirienteRegimenFiscal.Any())
				Html = Html.Replace("{AdquirienteRegimenFiscal}", AdquirienteRegimenFiscal.FirstOrDefault().Value);
			else
				Html = Html.Replace("{AdquirienteRegimenFiscal}", string.Empty);

			var AdquirienteResponsabilidadTributaria = model.Elements(cac + "AccountingCustomerParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "TaxScheme").Elements(cbc + "Name");
			if (AdquirienteResponsabilidadTributaria.Any())
				Html = Html.Replace("{AdquirienteResponsabilidadTributaria}", AdquirienteResponsabilidadTributaria.FirstOrDefault().Value);
			//var VendedorPais = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "PhysicalLocation").Elements(cac + "Address").Elements(cac + "Country").Elements(cbc + "Name");
			var VendedorPais = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cac + "Country").Elements(cbc + "Name");
			if (VendedorPais.Any())
				Html = Html.Replace("{VendedorPais}", VendedorPais.FirstOrDefault().Value);

			var VendedorDepartamento = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cbc + "CountrySubentityCode");
			if (VendedorDepartamento.Any())
				Html = Html.Replace("{VendedorDepartamento}", Depto.Where(x => x.IdDepartament == VendedorDepartamento.FirstOrDefault().Value).FirstOrDefault().NameDepartament);
			//AdquirienteRegimenFiscal

			var VendedorMunicipio = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cbc + "CityName");
			if (VendedorMunicipio.Any())
				Html = Html.Replace("{VendedorMunicipio}", VendedorMunicipio.FirstOrDefault().Value);

			var VendedorDireccion = model.Elements(cac + "AccountingSupplierParty").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cac + "AddressLine").Elements(cbc + "Line");
			if (VendedorDireccion.Any())
				Html = Html.Replace("{VendedorDireccion}", VendedorDireccion.FirstOrDefault().Value);

			//bolsa

			//var FechaCumplimiento = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "CustomTagGeneral").Elements(def + "Group").Elements(def + "AdditionalCollection").Elements(def + "Value");
			//if (FechaCumplimiento.Any())
			//	Html = Html.Replace("{FechaCumplimiento}", FechaCumplimiento.FirstOrDefault().Value);
			//else
			//	Html = Html.Replace("{FechaCumplimiento}", string.Empty);


			//espectaculos

			if (IssueDate.Any())
				Html = Html.Replace("{FechaDocumento}", IssueDate.FirstOrDefault().Value);
			else
				Html = Html.Replace("{FechaDocumento}", string.Empty);

			if (IssueTime.Any())
				Html = Html.Replace("{HoraDocumento}", IssueTime.FirstOrDefault().Value);
			else
				Html = Html.Replace("{HoraDocumento}", string.Empty);


			var NombreEvento = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "PublicShowsInformation").Elements(def + "EventInformation").Elements(def + "EventName");
			if (NombreEvento.Any())
				Html = Html.Replace("{NombreEvento}", NombreEvento.FirstOrDefault().Value);
			else
				Html = Html.Replace("{NombreEvento}", string.Empty);

			var PULEP = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "PublicShowsInformation").Elements(def + "EventInformation").Elements(def + "PULEPCode");
			if (PULEP.Any())
				Html = Html.Replace("{PULEP}", PULEP.FirstOrDefault().Value);
			else
				Html = Html.Replace("{PULEP}", string.Empty);


			var UbicacionBoleta = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "PublicShowsInformation").Elements(def + "EventInformation").Elements(def + "TicketLocation");
			if (UbicacionBoleta.Any())
				Html = Html.Replace("{UbicacionBoleta}", UbicacionBoleta.FirstOrDefault().Value);
			else
				Html = Html.Replace("{UbicacionBoleta}", string.Empty);

			var FechaEvento = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "PublicShowsInformation").Elements(def + "EventInformation").Elements(def + "DateEvent");
			if (FechaEvento.Any())
				Html = Html.Replace("{FechaEvento}", FechaEvento.FirstOrDefault().Value);
			else
				Html = Html.Replace("{FechaEvento}", string.Empty);


			var HoraEvento = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "PublicShowsInformation").Elements(def + "EventInformation").Elements(def + "TimeEvent");
			if (HoraEvento.Any())
				Html = Html.Replace("{HoraEvento}", HoraEvento.FirstOrDefault().Value);
			else
				Html = Html.Replace("{HoraEvento}", string.Empty);

			var EtapaCompra = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "PublicShowsInformation").Elements(def + "EventInformation").Elements(def + "SalePhase");
			if (EtapaCompra.Any())
				Html = Html.Replace("{EtapaCompra}", EtapaCompra.FirstOrDefault().Value);
			else
				Html = Html.Replace("{EtapaCompra}", string.Empty);

			var Retefuente = model.Elements(cac + "WithholdingTaxTotal").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cac + "AddressLine").Elements(cbc + "Line");
			if (Retefuente.Any())
				Html = Html.Replace("{Retefuente}", Retefuente.FirstOrDefault().Value);
			else
				Html = Html.Replace("{Retefuente}", string.Empty);


			var ReteIVA = model.Elements(cac + "WithholdingTaxTotal").Elements(cac + "Party").Elements(cac + "PartyTaxScheme").Elements(cac + "RegistrationAddress").Elements(cac + "AddressLine").Elements(cbc + "Line");
			if (ReteIVA.Any())
				Html = Html.Replace("{ReteIVA}", ReteIVA.FirstOrDefault().Value);
			else
				Html = Html.Replace("{ReteIVA}", string.Empty);

			//Juegos

			var DireccionJuegos = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "InfoEstablishment").Elements(def + "InfoPoint").Elements(def + "EstablishmentAddress");
			if (DireccionJuegos.Any())
				Html = Html.Replace("{DireccionJuegos}", DireccionJuegos.FirstOrDefault().Value);
			else
				Html = Html.Replace("{DireccionJuegos}", string.Empty);

			var ValorJuegos = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "InfoEstablishment").Elements(def + "InfoPoint").Elements(def + "MoneyDeposit");
			if (ValorJuegos.Any())
				Html = Html.Replace("{ValorJuegos}", ValorJuegos.FirstOrDefault().Value);
			else
				Html = Html.Replace("{ValorJuegos}", string.Empty);
			var instrumentos = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "InfoEstablishment").Elements(def + "GameInformation").ToList();

			if (instrumentos.Any())
			{
                foreach (var item in instrumentos)
                {
					var type = item.Elements(def + "TypeOfGame");
					var NumeroMesas = item.Elements(def + "GameTypeTotal");
					if (type.Any())
                    {
						if (type.FirstOrDefault().Value == "Mesa")
							Html = Html.Replace("{NumeroMesas}", NumeroMesas.FirstOrDefault().Value);
						
						if (type.FirstOrDefault().Value == "Bingo")
							Html = Html.Replace("{NumeroBingo}", NumeroMesas.FirstOrDefault().Value);
					
					}
                    else
                    {
						Html = Html.Replace("{NumeroMesas}", string.Empty);
						Html = Html.Replace("{NumeroBingo}", string.Empty);
					}
						
				}
				

			}
			else
			{
				Html = Html.Replace("{NumeroMesas}", string.Empty);
				Html = Html.Replace("{NumeroBingo}", string.Empty);
			}

			var CodigoComprador = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements("BeneficiosComprador").Elements("InformacionBeneficiosComprador").Elements("Value");

			if (CodigoComprador.Count() == 3)
			{
				Html = Html.Replace("{CodigoComprador}", CodigoComprador.ElementAt(0).Value);
				Html = Html.Replace("{NombresComprador}", CodigoComprador.ElementAt(1).Value);
				Html = Html.Replace("{Puntos}", CodigoComprador.ElementAt(2).Value);
			}
			else
			{
				Html = Html.Replace("{CodigoComprador}", string.Empty);
				Html = Html.Replace("{NombresComprador}", string.Empty);
				Html = Html.Replace("{Puntos}", string.Empty);
			}

			var Soft = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements("FabricanteSoftware").Elements("InformacionDelFabricanteDelSoftware").Elements("Value");

			if (Soft.Count() == 3)
			{
				Html = Html.Replace("{FabricanteRazon}", Soft.ElementAt(0).Value);
				Html = Html.Replace("{FabricanteNombre}", Soft.ElementAt(1).Value);
				Html = Html.Replace("{FabricanteSoftware}", Soft.ElementAt(2).Value);
			}
			else
			{
				Html = Html.Replace("{FabricanteRazon}", string.Empty);
				Html = Html.Replace("{FabricanteNombre}", string.Empty);
				Html = Html.Replace("{FabricanteSoftware}", string.Empty);
			}

			// aereo
			var rese = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "Interoperabilidad").Elements(def + "Group").Elements(def + "Collection").Elements(def + "AdditionalInformation").Elements(def + "Value");
			if (rese.Any())
			{
				Html = Html.Replace("{NumeroReserva}", rese.FirstOrDefault().Value);
			}
			else
			{
				Html = Html.Replace("{NumeroReserva}", string.Empty);
			}
			
			var Consecutivo = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "CustomTagGeneral").Elements(def + "Group").Elements(def + "AdditionalCollection").Elements(def + "Value");
			if (Consecutivo.Count()==2)
			{
				Html = Html.Replace("{Consecutivo}", Consecutivo.FirstOrDefault().Value);
				Html = Html.Replace("{FechaCumplimiento}", Consecutivo.ElementAt(1).Value);
			}
			else
			{
				Html = Html.Replace("{Consecutivo}", string.Empty);
				Html = Html.Replace("{FechaCumplimiento}", string.Empty);
			}

			var Pasa = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Elements(def + "Interoperabilidad").Elements(def + "Group").Elements(def + "Collectiontraveler").Elements(def + "AdditionalInformation").Elements(def + "Value");
			if (Pasa.Count() == 3)
			{
				Html = Html.Replace("{PasajeroNombre}", Pasa.ElementAt(1).Value);
				Html = Html.Replace("{PasajeroNumeroDocumento}", Pasa.ElementAt(0).Value);
			}
			else
			{
				Html = Html.Replace("{PasajeroNombre}", string.Empty);
				Html = Html.Replace("{PasajeroNumeroDocumento}", string.Empty);
			}

			//SPD

			var NumeroPago = model.Elements(cbc + "AccountingCostCode");
			if (NumeroPago.Any())
				Html = Html.Replace("{NumeroPago}", NumeroPago.FirstOrDefault().Value);
			else
				Html = Html.Replace("{NumeroPago}", string.Empty);

			var UltimaFechaPago = model.Elements(cbc + "TaxPointDate");

			if (UltimaFechaPago.Any())
				Html = Html.Replace("{UltimaFechaPago}", UltimaFechaPago.FirstOrDefault().Value);
			else
				Html = Html.Replace("{UltimaFechaPago}", string.Empty);

			var FechaVencimientoSPD = model.Elements(cbc + "DueDate");

			if (FechaVencimientoSPD.Any())
				Html = Html.Replace("{FechaVencimientoSPD}", FechaVencimientoSPD.FirstOrDefault().Value);
			else
				Html = Html.Replace("{FechaVencimientoSPD}", string.Empty);


			return Html;
		}


		public static string FillReferenceData(string Html, XElement model)
		{
			XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
			XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
			NumberFormatInfo formato = new CultureInfo("es-AR").NumberFormat;
			formato.CurrencyGroupSeparator = ".";
			formato.NumberDecimalSeparator = ",";			
			var numinf = new NumberFormatInfo { NumberDecimalSeparator = "." };

			var Recargos = model.Elements(cac + "AllowanceCharge").ToList();
			if (Recargos.Count == 0)
			{

				Html = Html.Replace(@"{RowsDescuentosYRecargos}", String.Empty);
				var rowDescuentosYRecargos = new StringBuilder();
				var rep = "CTx0cj4NCgkJPHRoID5Ocm8uPC90aD4NCgkJPHRoID5UaXBvPC90aD4NCgkJPHRoPkPDs2RpZ288L3RoPg0KCQk8dGg+RGVzY3JpcGNpw7NuPC90aD4NCgkJPHRoPiU8L3RoPg0KCQk8dGg+VmFsb3I8L3RoPg0KCTwvdHI+	";
				byte[] data = Convert.FromBase64String(rep);
				string decodedString = Encoding.UTF8.GetString(data);

				Html = Html.Replace(decodedString, String.Empty);
				//rep = Convert.FromBase64String(rep);
			}
			else
			{
				var rowDescuentosYRecargos = new StringBuilder();
				foreach (var detalle in Recargos)
				{

					decimal amutDeciTotal = 0;
					var tipo = detalle.Elements(cbc + "ChargeIndicator").FirstOrDefault().Value.ToUpper() == "TRUE" ? "Recargo" : "Descuento";
					var reason = detalle.Elements(cbc + "AllowanceChargeReasonCode").Any() ? detalle.Elements(cbc + "AllowanceChargeReasonCode").FirstOrDefault().Value : "";
					var porc = detalle.Elements(cbc + "MultiplierFactorNumeric");
					var porce = porc.Any() ? porc.FirstOrDefault().Value : "";
					var amutTotal = detalle.Elements(cbc + "Amount").FirstOrDefault().Value;

					amutDeciTotal = decimal.Parse(amutTotal, numinf);
					amutDeciTotal = decimal.Round(amutDeciTotal, 2);
					
					rowDescuentosYRecargos.Append($@"
                <tr>
		            <td class='text-right'>{detalle.Elements(cbc + "ID").FirstOrDefault().Value}</td>
		            <td class='text-left'>{tipo}</td>
		            <td>{reason}</td>
		            <td class='text-left'>{detalle.Elements(cbc + "AllowanceChargeReason").FirstOrDefault().Value}</td>
		            <td>{porce}</td>
		            <td class='text-right'>{amutDeciTotal.ToString("N", formato)}</td>
	            </tr>");
				}
				Html = Html.Replace("{RowsDescuentosYRecargos}", rowDescuentosYRecargos.ToString());
				return Html;
			}


			return Html;
		}
		public static string FillTransporteT(string Html, XElement model)
		{
			XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
			XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
			XNamespace ext = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2";
			XNamespace def = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
			XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";


			var InformacionAdicional = model.Elements(ext + "UBLExtensions").Elements(ext + "UBLExtension").Elements(ext + "ExtensionContent").Where(x => x.FirstNode.ToString().Contains("InformacionAdicional"));
			var info = InformacionAdicional.Where(x => x.FirstNode.ToString().Contains("InformacionTicket"));
			var InformacionTicket = info.Descendants().Elements(def + "Value").ToArray();

			if (InformacionTicket.Count() != 0)
			{
				Html = Html.Replace("{ModoTransporte}", InformacionTicket[0].ToString());
				Html = Html.Replace("{Placa}", InformacionTicket[1].ToString());
				Html = Html.Replace("{MedioTransporte}", InformacionTicket[2].ToString());
				Html = Html.Replace("{LugarOrigen}", InformacionTicket[3].ToString());
				Html = Html.Replace("{LugarDestino}", InformacionTicket[4].ToString());
			}
			else
			{

				Html = Html.Replace("{Placa}", string.Empty);
				Html = Html.Replace("{ModoTransporte}", string.Empty);
				Html = Html.Replace("{MedioTransporte}", string.Empty);
				Html = Html.Replace("{LugarOrigen}", string.Empty);
				Html = Html.Replace("{LugarDestino}", string.Empty);
			}



			//SPD

			var NumeroPago = model.Elements(cbc + "AccountingCostCode");
			if (NumeroPago.Any())
				Html = Html.Replace("{NumeroPago}", NumeroPago.FirstOrDefault().Value);
			else
				Html = Html.Replace("{NumeroPago}", string.Empty);


			var UltimaFechaPago = model.Elements(cbc + "TaxPointDate");
			if (UltimaFechaPago.Any())
				Html = Html.Replace("{UltimaFechaPago}", NumeroPago.FirstOrDefault().Value);
			else
				Html = Html.Replace("{UltimaFechaPago}", string.Empty);




			return Html;
		}


		public static string CruzarLogosEnHeader(string plantillaHtml, string identificationUser)
		{
			var fileManager = new FileManager();
			var fileManagerBiller = new FileManager("GlobalStorageBiller");

			MemoryStream logoDianStream = new MemoryStream(fileManager.GetBytes("radian-dian-logos", "Logo-DIAN-2020-color.jpg"));
			string logoDianaStrBase64 = Convert.ToBase64String(logoDianStream.ToArray());
			var logoDianBase64 = $@"data:image/png;base64,{logoDianaStrBase64}";
			MemoryStream logoStream = new MemoryStream();
			var logoempresa = fileManagerBiller.GetBytesBiller("logo", $"{identificationUser}.jpg");
			if (logoempresa != null) {
				logoStream = new MemoryStream(fileManagerBiller.GetBytesBiller("logo", $"{identificationUser}.jpg"));
			}

			string logoStrBase64 = Convert.ToBase64String(logoStream.ToArray());
			var logoBase64 = $@"data:image/png;base64,{logoStrBase64}";



			plantillaHtml = plantillaHtml.Replace("{imgLogoDian}", logoDianBase64);
			/*WebConfig: clave MainStorage*/
			if (logoempresa != null)
			{
				plantillaHtml = plantillaHtml.Replace("{imgLogoEmpresa}", logoBase64);
				
			}
			else
			{
				plantillaHtml = plantillaHtml.Replace("{imgLogoEmpresa}", string.Empty);
			}
			
			return plantillaHtml;
		}
		private static string CruzarModeloNotasFinales(string Html, XElement obj)
		{
			XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
			XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
			var rowReferencias = new StringBuilder();
			var model = obj.Elements(cac + "Note").ToList();
			if (model.Count() == 0)
			{ 
			model= obj.Elements(cbc + "Note").ToList();
			}
			if (!model.Any())
			{
				
				Html = Html.Replace("<td>{LineaNegocio}</td>", string.Empty);
				Html = Html.Replace("<td>Línea de negocio</td>", string.Empty);
				Html = Html.Replace("{RowsNotasFinales}", string.Empty);

			}
			foreach (var detalle in model)
			{
				rowReferencias.Append($@"
                <tr>
		            <td colspan='2'>{detalle.Value}</td>
	            </tr>");
			}
			Html = Html.Replace("{RowsNotasFinales}", rowReferencias.ToString());

			Html = Html.Replace("{LineaNegocio}", string.Empty);

			var linea = obj.Elements(cac + "Note");
			return Html;
		}



		private static string CruzarReferencias(string Html, XElement obj)
		{
			XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
			XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
			var rowReferencias = new StringBuilder();
			var model = obj.Elements(cac + "BillingReference").ToList();
			if (!model.Any())
			{
				Html = Html.Replace(@"{RowsReferencias}", String.Empty);
				var rowDescuentosYRecargos = new StringBuilder();
				var rep = "PHRyIGNsYXNzPSAic2VjdGlvbi1TdWJ0aXRsZSI+DQoJCTx0aCA+VGlwbyBkZSBkb2N1bWVudG88L3RoPg0KCQk8dGg+TsO6bWVybyByZWZlcmVuY2lhIDwvdGg+DQoJCTx0aD5GZWNoYSByZWZlcmVuY2lhPC90aD4NCg0KCTwvdHI+";
				byte[] data = Convert.FromBase64String(rep);
				string decodedString = Encoding.UTF8.GetString(data);

				Html = Html.Replace(decodedString, String.Empty);

			}
			foreach (var detalle in model)
			{
				var tip = detalle.Elements(cac + "CreditNoteDocumentReference").Elements(cbc + "DocumentType");
				var tipo = tip.Any() ? tip.FirstOrDefault().Value : "";
				rowReferencias.Append($@"
                <tr>
		            <td colspan='1'>{tipo}</td>
					<td colspan='1'>{detalle.Elements(cac + "CreditNoteDocumentReference").Elements(cbc + "ID").FirstOrDefault().Value}</td>
					<td colspan='1'>{detalle.Elements(cac + "CreditNoteDocumentReference").Elements(cbc + "IssueDate").FirstOrDefault().Value}</td>
	            </tr>");
			}
			Html = Html.Replace("{RowsReferencias}", rowReferencias.ToString());
			return Html;
		}
		private static string CruzarReferenciasNota(string Html, XElement obj)
		{
			XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
			XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
			List<ListDocumentType>  documentTypeNI = new List<ListDocumentType>();
			documentTypeNI.Add(new ListDocumentType { Id = "1" , Name="Devolución parcial de los bienes y/o no aceptación parcial del servicio" });
			documentTypeNI.Add(new ListDocumentType { Id = "2" , Name= "Anulación del documento equivalente POS" });
			documentTypeNI.Add(new ListDocumentType { Id = "3" , Name= "Rebaja o descuento parcial o total" });
			documentTypeNI.Add(new ListDocumentType { Id = "4" , Name= "Ajuste de precio" });
			documentTypeNI.Add(new ListDocumentType { Id = "5" , Name= "Otros" });
			documentTypeNI.Add(new ListDocumentType { Id = "" , Name= "" });

			var rowReferencias = new StringBuilder();
			
			
			var model = obj.Elements(cac + "BillingReference").ToList();
			if (!model.Any())
			{
				Html = Html.Replace(@"{RowsReferencias}", String.Empty);
				var rowDescuentosYRecargos = new StringBuilder();
				var rep = "PHRyIGNsYXNzPSAic2VjdGlvbi1TdWJ0aXRsZSI+DQoJCTx0aCA+VGlwbyBkZSBkb2N1bWVudG88L3RoPg0KCQk8dGg+TsO6bWVybyByZWZlcmVuY2lhIDwvdGg+DQoJCTx0aD5GZWNoYSByZWZlcmVuY2lhPC90aD4NCg0KCTwvdHI+";
				byte[] data = Convert.FromBase64String(rep);
				string decodedString = Encoding.UTF8.GetString(data);

				Html = Html.Replace(decodedString, String.Empty);

			}
			foreach (var detalle in model)
			{
				var tip = obj.Elements(cac + "DiscrepancyResponse").Elements(cbc + "ResponseCode");
				var tipo = tip.Any() ? tip.FirstOrDefault().Value : "";

				var desc = obj.Elements(cac + "DiscrepancyResponse").Elements(cbc + "Description");
				var des = desc.Any() ? desc.FirstOrDefault().Value : "";
				rowReferencias.Append($@"
                <tr>
		            <td colspan='1'>{documentTypeNI.Where(x=> x.Id == tipo).FirstOrDefault().Name}</td>
					<td colspan='1'>{detalle.Elements(cac + "InvoiceDocumentReference").Elements(cbc + "ID").FirstOrDefault().Value}</td>
					<td colspan='1'>{detalle.Elements(cac + "InvoiceDocumentReference").Elements(cbc + "IssueDate").FirstOrDefault().Value}</td>
					<td colspan='1'>{des}</td>
				 </tr>
				 <tr>
					<td class='text-left' colspan='3'>Cude: {detalle.Elements(cac + "InvoiceDocumentReference").Elements(cbc + "UUID").FirstOrDefault().Value}</td>
				 </tr>");
				}
			Html = Html.Replace("{RowsReferencias}", rowReferencias.ToString());
			return Html;
		}
		private static string GetTemplate(string tipo)
		{
			var fileManager = new FileManager();
			if (tipo == "20")
				return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentPOS_template.html");
			else if (tipo == "40")
				return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentPeaje_template.html");
			else if (tipo == "50")
				return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentAereo_template.html");
			else if (tipo == "35")
				return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentTransporte_template.html");
			else if (tipo == "55")
				return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentBolsa_template.html");
			else if (tipo == "45")
				return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentExtracto_template.html");
			else if ( tipo == "27")
				return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentBoleta_template.html");
			else if (tipo == "32")
				return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentJuegos_template.html");
			else if (tipo == "60")
				return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentSPD_template.html");
			else if (tipo == "Nota")
				return fileManager.GetText("dian", "configurations/SupportDocument/supportDocumentNota_template.html");
			else return null;
		}


	}

	partial class ListDocumentType
    {
		public string Id { get; set; }
		public string Name { get; set; }
	}

    partial class param
	{
		public string base64Xml { get; set; }
		public string FechaValidacionDIAN { get; set; }
		public string FechaGeneracionDIAN { get; set; }
	}
	partial class Cont
	{
		public string id { get; set; }
		public string numero { get; set; }

	}
}
