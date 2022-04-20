using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Common;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Infrastructure;
using Gosocket.Dian.Services.Utils;
using Gosocket.Dian.Services.Utils.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace Gosocket.Dian.Functions.Others
{
    public static class QualificationStatusGlobalOtherDocElecOperation
    {
        private static readonly TableManager TableManagerGlobalOtherDocElecOperation = new TableManager("GlobalOtherDocElecOperation");
        private static readonly TableManager TableManagerGlobalTestSetOthersDocumentsResult = new TableManager("GlobalTestSetOthersDocumentsResult");//10169");
        private static readonly TableManager contributorActivationTableManager = new TableManager("GlobalContributorActivation");
        private static readonly TableManager TableManagerGlobalOtherDocElecOperationProd = new TableManager("GlobalOtherDocElecOperation", ConfigurationManager.GetValue("GlobalStorageProd"));
        private static readonly TableManager TableManagerGlobalTestSetOthersDocumentsResultProd = new TableManager("GlobalTestSetOthersDocumentsResult", ConfigurationManager.GetValue("GlobalStorageProd"));
        private static readonly TableManager TableManagerGlobalSoftwareProd = new TableManager("GlobalSoftware", ConfigurationManager.GetValue("GlobalStorageProd"));
        private static readonly OthersDocsElecSoftwareService othersDocsElecSoftwareService = new OthersDocsElecSoftwareService();
        private static readonly OthersElectronicDocumentsService othersElectronicDocumentsService = new OthersElectronicDocumentsService();
        private static readonly SoftwareService softwareService = new SoftwareService();
        private static readonly ContributorService contributorService = new ContributorService();
        private static readonly string sqlConnectionStringProd = ConfigurationManager.GetValue("SqlConnectionProd");

        [FunctionName("CorrectQualificationStatus")]
        public static async Task<EventResponse> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var data = await req.Content.ReadAsAsync<RequestObject>();

            if (data == null)
                return new EventResponse { Code = "400", Message = "Request body is empty." };
            //NIT/S de los participantes separados por "|"
            if (string.IsNullOrEmpty(data.Nits))
                return new EventResponse { Code = "400", Message = "Please pass a nits in the request body." };
            //ModeTest: "1" ejecuta la funcion en modo de consulta, no genera update; "0" ejecuta la funcion actualizando las tablas.
            if (string.IsNullOrEmpty(data.ModeTest))
                data.ModeTest = "0";

            var response = new EventResponse
            {
                Code = ((int)EventValidationMessage.Success).ToString(),
                Message = EnumHelper.GetEnumDescription(EventValidationMessage.Success),
            };

            var arrayTasks = new List<Task>();
            var nitSwIdOperMod = new List<Tuple<string, string, int, string>>();
            Tuple<string, string, int, string> itemTuple = null;
            GlobalOtherDocElecOperation itemGODEO;
            List<string> messages = new List<string>();
            string _StateOld = string.Empty;
            bool QualificationProd = false;
            Domain.Contributor contributor = null;
            List<NitProcesed> ListNitsProcess = new List<NitProcesed>();
            //List<NitDatos> ListNitDatos = new List<NitDatos>();
            IEnumerable<String> nits = data.Nits.Split('|').AsEnumerable();

            try
            {
                #region #actualizacontributorid
                if (data.ModeTest == "2")
                {
                    List<Tuple<String, int, int, int>> ListData = new List<Tuple<string, int, int, int>>();
                    ListData.Add(new Tuple<string, int, int, int>("1116261717", 1, 1, 44362));
                    ListData.Add(new Tuple<string, int, int, int>("1116261717", 1, 2, 180366));
                    ListData.Add(new Tuple<string, int, int, int>("13060584", 1, 1, 95185));
                    ListData.Add(new Tuple<string, int, int, int>("13060584", 1, 2, 157452));
                    ListData.Add(new Tuple<string, int, int, int>("15431620", 1, 1, 8716));
                    ListData.Add(new Tuple<string, int, int, int>("15431620", 1, 2, 8717));
                    ListData.Add(new Tuple<string, int, int, int>("15959491", 1, 1, 27695));
                    ListData.Add(new Tuple<string, int, int, int>("15959491", 1, 2, 27696));
                    ListData.Add(new Tuple<string, int, int, int>("16215590", 1, 1, 36259));
                    ListData.Add(new Tuple<string, int, int, int>("16215590", 1, 2, 15252));
                    ListData.Add(new Tuple<string, int, int, int>("19326888", 1, 1, 51312));
                    ListData.Add(new Tuple<string, int, int, int>("19326888", 1, 2, 133607));
                    ListData.Add(new Tuple<string, int, int, int>("19390632", 1, 1, 42636));
                    ListData.Add(new Tuple<string, int, int, int>("19390632", 1, 2, 54977));
                    ListData.Add(new Tuple<string, int, int, int>("25613044", 1, 1, 101825));
                    ListData.Add(new Tuple<string, int, int, int>("25613044", 1, 2, 203045));
                    ListData.Add(new Tuple<string, int, int, int>("43055922", 1, 1, 24824));
                    ListData.Add(new Tuple<string, int, int, int>("43055922", 1, 2, 58479));
                    ListData.Add(new Tuple<string, int, int, int>("48574672", 1, 1, 101860));
                    ListData.Add(new Tuple<string, int, int, int>("48574672", 1, 2, 203058));
                    ListData.Add(new Tuple<string, int, int, int>("63304321", 1, 1, 64989));
                    ListData.Add(new Tuple<string, int, int, int>("63304321", 1, 2, 115155));
                    ListData.Add(new Tuple<string, int, int, int>("6873403", 1, 1, 22454));
                    ListData.Add(new Tuple<string, int, int, int>("6873403", 1, 2, 124575));
                    ListData.Add(new Tuple<string, int, int, int>("70694218", 1, 1, 57127));
                    ListData.Add(new Tuple<string, int, int, int>("70694218", 1, 2, 89339));
                    ListData.Add(new Tuple<string, int, int, int>("800115005", 1, 1, 43176));
                    ListData.Add(new Tuple<string, int, int, int>("800115005", 1, 2, 43172));
                    ListData.Add(new Tuple<string, int, int, int>("800146814", 1, 1, 9278));
                    ListData.Add(new Tuple<string, int, int, int>("800146814", 1, 2, 58195));
                    ListData.Add(new Tuple<string, int, int, int>("800207224", 1, 2, 121414));
                    ListData.Add(new Tuple<string, int, int, int>("800216484", 1, 1, 203087));
                    ListData.Add(new Tuple<string, int, int, int>("800216484", 1, 2, 203178));
                    ListData.Add(new Tuple<string, int, int, int>("802021369", 1, 1, 33193));
                    ListData.Add(new Tuple<string, int, int, int>("802021369", 1, 2, 33708));
                    ListData.Add(new Tuple<string, int, int, int>("811029042", 1, 1, 103516));
                    ListData.Add(new Tuple<string, int, int, int>("811029042", 1, 2, 114661));
                    ListData.Add(new Tuple<string, int, int, int>("812007286", 1, 1, 4775));
                    ListData.Add(new Tuple<string, int, int, int>("812007286", 1, 2, 113307));
                    ListData.Add(new Tuple<string, int, int, int>("814003420", 1, 1, 3009));
                    ListData.Add(new Tuple<string, int, int, int>("814003420", 1, 2, 168019));
                    ListData.Add(new Tuple<string, int, int, int>("816004480", 1, 1, 15187));
                    ListData.Add(new Tuple<string, int, int, int>("816004480", 1, 2, 158971));
                    ListData.Add(new Tuple<string, int, int, int>("819001302", 1, 1, 151947));
                    ListData.Add(new Tuple<string, int, int, int>("819001302", 1, 2, 151934));
                    ListData.Add(new Tuple<string, int, int, int>("830030986", 1, 1, 159796));
                    ListData.Add(new Tuple<string, int, int, int>("830030986", 1, 2, 221662));
                    ListData.Add(new Tuple<string, int, int, int>("830036470", 1, 1, 38775));
                    ListData.Add(new Tuple<string, int, int, int>("830036470", 1, 2, 157611));
                    ListData.Add(new Tuple<string, int, int, int>("830086720", 1, 2, 121625));
                    ListData.Add(new Tuple<string, int, int, int>("860007229", 1, 1, 13188));
                    ListData.Add(new Tuple<string, int, int, int>("860007229", 1, 2, 13248));
                    ListData.Add(new Tuple<string, int, int, int>("860014987", 1, 1, 21259));
                    ListData.Add(new Tuple<string, int, int, int>("860014987", 1, 2, 157757));
                    ListData.Add(new Tuple<string, int, int, int>("860066875", 1, 1, 533));
                    ListData.Add(new Tuple<string, int, int, int>("860066875", 1, 2, 532));
                    ListData.Add(new Tuple<string, int, int, int>("890300524", 1, 1, 102952));
                    ListData.Add(new Tuple<string, int, int, int>("890300524", 1, 2, 202013));
                    ListData.Add(new Tuple<string, int, int, int>("890303254", 1, 1, 1367));
                    ListData.Add(new Tuple<string, int, int, int>("890303254", 1, 2, 1347));
                    ListData.Add(new Tuple<string, int, int, int>("891401093", 1, 1, 15728));
                    ListData.Add(new Tuple<string, int, int, int>("891401093", 1, 2, 15719));
                    ListData.Add(new Tuple<string, int, int, int>("891408974", 1, 1, 85153));
                    ListData.Add(new Tuple<string, int, int, int>("891408974", 1, 2, 88568));
                    ListData.Add(new Tuple<string, int, int, int>("891700203", 1, 1, 98115));
                    ListData.Add(new Tuple<string, int, int, int>("891700203", 1, 2, 153876));
                    ListData.Add(new Tuple<string, int, int, int>("891801193", 1, 1, 9944));
                    ListData.Add(new Tuple<string, int, int, int>("891801193", 1, 2, 30275));
                    ListData.Add(new Tuple<string, int, int, int>("900011819", 1, 2, 88918));
                    ListData.Add(new Tuple<string, int, int, int>("900011885", 1, 2, 174884));
                    ListData.Add(new Tuple<string, int, int, int>("900021682", 1, 1, 76724));
                    ListData.Add(new Tuple<string, int, int, int>("900021682", 1, 2, 91207));
                    ListData.Add(new Tuple<string, int, int, int>("900038575", 1, 2, 179807));
                    ListData.Add(new Tuple<string, int, int, int>("900056422", 1, 1, 36203));
                    ListData.Add(new Tuple<string, int, int, int>("900056422", 1, 2, 200178));
                    ListData.Add(new Tuple<string, int, int, int>("900169782", 1, 1, 38605));
                    ListData.Add(new Tuple<string, int, int, int>("900169782", 1, 2, 39943));
                    ListData.Add(new Tuple<string, int, int, int>("900197552", 1, 1, 158037));
                    ListData.Add(new Tuple<string, int, int, int>("900197552", 1, 2, 158033));
                    ListData.Add(new Tuple<string, int, int, int>("900205886", 1, 1, 149557));
                    ListData.Add(new Tuple<string, int, int, int>("900205886", 1, 2, 149558));
                    ListData.Add(new Tuple<string, int, int, int>("900337310", 1, 1, 61670));
                    ListData.Add(new Tuple<string, int, int, int>("900337310", 1, 2, 61668));
                    ListData.Add(new Tuple<string, int, int, int>("900361697", 1, 2, 26573));
                    ListData.Add(new Tuple<string, int, int, int>("900417923", 1, 1, 32633));
                    ListData.Add(new Tuple<string, int, int, int>("900417923", 1, 2, 32784));
                    ListData.Add(new Tuple<string, int, int, int>("900438679", 1, 2, 76038));
                    ListData.Add(new Tuple<string, int, int, int>("900450994", 1, 1, 63719));
                    ListData.Add(new Tuple<string, int, int, int>("900450994", 1, 2, 63720));
                    ListData.Add(new Tuple<string, int, int, int>("900474088", 1, 1, 45751));
                    ListData.Add(new Tuple<string, int, int, int>("900474088", 1, 2, 114045));
                    ListData.Add(new Tuple<string, int, int, int>("900478500", 1, 1, 4588));
                    ListData.Add(new Tuple<string, int, int, int>("900478500", 1, 2, 137061));
                    ListData.Add(new Tuple<string, int, int, int>("900521455", 1, 1, 6451));
                    ListData.Add(new Tuple<string, int, int, int>("900521455", 1, 2, 223726));
                    ListData.Add(new Tuple<string, int, int, int>("900530718", 1, 2, 242856));
                    ListData.Add(new Tuple<string, int, int, int>("900535078", 1, 2, 161329));
                    ListData.Add(new Tuple<string, int, int, int>("900582372", 1, 1, 78316));
                    ListData.Add(new Tuple<string, int, int, int>("900582372", 1, 2, 78317));
                    ListData.Add(new Tuple<string, int, int, int>("900598357", 1, 1, 177940));
                    ListData.Add(new Tuple<string, int, int, int>("900598357", 1, 2, 179810));
                    ListData.Add(new Tuple<string, int, int, int>("900601276", 1, 1, 15140));
                    ListData.Add(new Tuple<string, int, int, int>("900601276", 1, 2, 15137));
                    ListData.Add(new Tuple<string, int, int, int>("900633198", 1, 1, 20107));
                    ListData.Add(new Tuple<string, int, int, int>("900633198", 1, 2, 20111));
                    ListData.Add(new Tuple<string, int, int, int>("900676334", 1, 2, 201325));
                    ListData.Add(new Tuple<string, int, int, int>("900697924", 1, 2, 42573));
                    ListData.Add(new Tuple<string, int, int, int>("900699319", 1, 2, 136829));
                    ListData.Add(new Tuple<string, int, int, int>("900701816", 1, 1, 73237));
                    ListData.Add(new Tuple<string, int, int, int>("900701816", 1, 2, 75592));
                    ListData.Add(new Tuple<string, int, int, int>("900740317", 1, 1, 146242));
                    ListData.Add(new Tuple<string, int, int, int>("900740317", 1, 2, 146225));
                    ListData.Add(new Tuple<string, int, int, int>("900772711", 1, 1, 103561));
                    ListData.Add(new Tuple<string, int, int, int>("900772711", 1, 2, 103555));
                    ListData.Add(new Tuple<string, int, int, int>("900781986", 1, 1, 12830));
                    ListData.Add(new Tuple<string, int, int, int>("900781986", 1, 2, 20861));
                    ListData.Add(new Tuple<string, int, int, int>("900785720", 1, 1, 1888));
                    ListData.Add(new Tuple<string, int, int, int>("900785720", 1, 2, 114585));
                    ListData.Add(new Tuple<string, int, int, int>("900819287", 1, 1, 100411));
                    ListData.Add(new Tuple<string, int, int, int>("900819287", 1, 2, 178535));
                    ListData.Add(new Tuple<string, int, int, int>("900922437", 1, 2, 119172));
                    ListData.Add(new Tuple<string, int, int, int>("900961711", 1, 1, 13214));
                    ListData.Add(new Tuple<string, int, int, int>("900961711", 1, 2, 15472));
                    ListData.Add(new Tuple<string, int, int, int>("900988309", 1, 2, 29187));
                    ListData.Add(new Tuple<string, int, int, int>("901023043", 1, 2, 19909));
                    ListData.Add(new Tuple<string, int, int, int>("901029762", 1, 1, 15037));
                    ListData.Add(new Tuple<string, int, int, int>("901029762", 1, 2, 12750));
                    ListData.Add(new Tuple<string, int, int, int>("901048549", 1, 1, 54738));
                    ListData.Add(new Tuple<string, int, int, int>("901048549", 1, 2, 115131));
                    ListData.Add(new Tuple<string, int, int, int>("901107406", 1, 1, 22947));
                    ListData.Add(new Tuple<string, int, int, int>("901107406", 1, 2, 28699));
                    ListData.Add(new Tuple<string, int, int, int>("901118295", 1, 2, 201919));
                    ListData.Add(new Tuple<string, int, int, int>("901132953", 1, 1, 31393));
                    ListData.Add(new Tuple<string, int, int, int>("901132953", 1, 2, 113445));
                    ListData.Add(new Tuple<string, int, int, int>("901133642", 1, 1, 85321));
                    ListData.Add(new Tuple<string, int, int, int>("901133642", 1, 2, 116215));
                    ListData.Add(new Tuple<string, int, int, int>("901143611", 1, 1, 119046));
                    ListData.Add(new Tuple<string, int, int, int>("901143611", 1, 2, 137668));
                    ListData.Add(new Tuple<string, int, int, int>("901153500", 1, 1, 106549));
                    ListData.Add(new Tuple<string, int, int, int>("901153500", 1, 2, 132029));
                    ListData.Add(new Tuple<string, int, int, int>("901165175", 1, 2, 3176));
                    ListData.Add(new Tuple<string, int, int, int>("901227025", 1, 1, 10967));
                    ListData.Add(new Tuple<string, int, int, int>("901227025", 1, 2, 55298));
                    ListData.Add(new Tuple<string, int, int, int>("901368769", 1, 2, 61660));
                    ListData.Add(new Tuple<string, int, int, int>("901378861", 1, 2, 165692));
                    ListData.Add(new Tuple<string, int, int, int>("901405704", 1, 2, 29827));
                    ListData.Add(new Tuple<string, int, int, int>("901458700", 1, 2, 22279));
                    ListData.Add(new Tuple<string, int, int, int>("901490114", 1, 2, 217549));

                    IEnumerable<GlobalOtherDocElecOperation> listGlobalOtherDocElecOperation = TableManagerGlobalOtherDocElecOperation.GetRowsContainsInPartitionKeys<GlobalOtherDocElecOperation>(ListData.Select(d => d.Item1));
                    var listGlobalOtherDocElecOperationDif = from godeo in listGlobalOtherDocElecOperation
                                                             join ld in ListData on new { godeo.PartitionKey, godeo.ContributorTypeId, godeo.OperationModeId } equals new { PartitionKey = ld.Item1, ContributorTypeId = ld.Item2, OperationModeId = ld.Item3 }
                                                             where godeo.OtherDocElecContributorId != ld.Item4
                                                             select new { godeo, ld };
                    foreach (var item in listGlobalOtherDocElecOperationDif)
                    {
                        messages.Add(string.Format("Se actualizo el nit {0}", item.godeo.PartitionKey));
                        item.godeo.OtherDocElecContributorId = item.ld.Item4;
                        arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(item.godeo));
                    }

                    IEnumerable<GlobalTestSetOthersDocumentsResult> listGlobalTestSetOthersDocumentsResult = TableManagerGlobalTestSetOthersDocumentsResult.GetRowsContainsInPartitionKeys<GlobalTestSetOthersDocumentsResult>(ListData.Select(d => d.Item1));
                    var listGlobalTestSetOthersDocumentsResultDif = from godeo in listGlobalTestSetOthersDocumentsResult
                                                                    join ld in ListData on new { godeo.PartitionKey, godeo.ContributorTypeId, OperationModeId = godeo.RowKey.Split('|')[0] } equals new { PartitionKey = ld.Item1, ContributorTypeId = ld.Item2.ToString(), OperationModeId = ld.Item3.ToString() }
                                                                    where godeo.OtherDocElecContributorId != ld.Item4
                                                                    select new { godeo, ld };
                    foreach (var item1 in listGlobalTestSetOthersDocumentsResultDif)
                    {
                        messages.Add(string.Format("Se actualizo el nit {0}", item1.godeo.PartitionKey));
                        item1.godeo.OtherDocElecContributorId = item1.ld.Item4;
                        arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item1.godeo));
                    }
                }
                #endregion
                #region #consultaoperations
                else if (data.ModeTest == "3")
                {


                    List<string> ListData = new List<string>();
                    ListData.Add("19284945");
                    ListData.Add("811033128");
                    ListData.Add("830053465");
                    ListData.Add("900109996");
                    ListData.Add("900263163");
                    ListData.Add("900398839");
                    ListData.Add("900457433");
                    ListData.Add("900489437");
                    ListData.Add("900721612");
                    ListData.Add("900771544");
                    ListData.Add("900822849");
                    ListData.Add("900842654");
                    ListData.Add("900882001");
                    ListData.Add("900997586");
                    ListData.Add("901055347");
                    ListData.Add("901105672");
                    ListData.Add("901277238");
                    ListData.Add("901286328");
                    ListData.Add("901302805");
                    ListData.Add("901364339");
                    ListData.Add("901384873");
                    ListData.Add("901386267");
                    ListData.Add("901404737");
                    ListData.Add("901418112");
                    ListData.Add("901478570");
                    ListData.Add("901481071");
                    ListData.Add("901484777");
                    ListData.Add("901491167");
                    ListData.Add("901556408");


                    while (ListData.Count > 0)
                    {
                        IEnumerable<GlobalOtherDocElecOperation> listGlobalOtherDocElecOperation = TableManagerGlobalOtherDocElecOperation.GetRowsContainsInPartitionKeys<GlobalOtherDocElecOperation>(ListData.Take(100));
                        foreach (var item in listGlobalOtherDocElecOperation)
                        {
                            messages.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}'", item.PartitionKey, item.RowKey, item.OperationModeId, item.OtherDocElecContributorId, item.SoftwareId, item.State, item.Deleted));

                        }
                        ListData.RemoveRange(0, ListData.Count > 100 ? 100 : ListData.Count);
                    }


                    //IEnumerable<GlobalTestSetOthersDocumentsResult> listGlobalTestSetOthersDocumentsResult = TableManagerGlobalTestSetOthersDocumentsResult.GetRowsContainsInPartitionKeys<GlobalTestSetOthersDocumentsResult>(ListData.Select(d => d.Item1));
                    //var listGlobalTestSetOthersDocumentsResultDif = from godeo in listGlobalTestSetOthersDocumentsResult
                    //                                                join ld in ListData on new { godeo.PartitionKey, godeo.ContributorTypeId, godeo.OperationModeId } equals new { PartitionKey = ld.Item1, ContributorTypeId = ld.Item2.ToString(), OperationModeId = ld.Item3.ToString() }
                    //                                                where godeo.OtherDocElecContributorId != ld.Item4
                    //                                                select new { godeo, ld };
                    //foreach (var item1 in listGlobalTestSetOthersDocumentsResultDif)
                    //{
                    //    item1.godeo.OtherDocElecContributorId = item1.ld.Item4;
                    //    arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item1.godeo));
                    //}
                }
                #endregion
                #region #consultaoperations
                else if (data.ModeTest == "4")
                {
                    List<string> ListData = nits.ToList();
                    while (ListData.Count > 0)
                    {
                        IEnumerable<GlobalOtherDocElecOperation> listGlobalOtherDocElecOperation = TableManagerGlobalOtherDocElecOperation.GetRowsContainsInPartitionKeys<GlobalOtherDocElecOperation>(ListData.Take(100));
                        foreach (var item in listGlobalOtherDocElecOperation)
                        {
                            messages.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}'", item.PartitionKey, item.RowKey, item.OperationModeId, item.OtherDocElecContributorId, item.SoftwareId, item.State, item.Deleted));

                        }
                        ListData.RemoveRange(0, ListData.Count > 100 ? 100 : ListData.Count);
                    }


                    //IEnumerable<GlobalTestSetOthersDocumentsResult> listGlobalTestSetOthersDocumentsResult = TableManagerGlobalTestSetOthersDocumentsResult.GetRowsContainsInPartitionKeys<GlobalTestSetOthersDocumentsResult>(ListData.Select(d => d.Item1));
                    //var listGlobalTestSetOthersDocumentsResultDif = from godeo in listGlobalTestSetOthersDocumentsResult
                    //                                                join ld in ListData on new { godeo.PartitionKey, godeo.ContributorTypeId, godeo.OperationModeId } equals new { PartitionKey = ld.Item1, ContributorTypeId = ld.Item2.ToString(), OperationModeId = ld.Item3.ToString() }
                    //                                                where godeo.OtherDocElecContributorId != ld.Item4
                    //                                                select new { godeo, ld };
                    //foreach (var item1 in listGlobalTestSetOthersDocumentsResultDif)
                    //{
                    //    item1.godeo.OtherDocElecContributorId = item1.ld.Item4;
                    //    arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item1.godeo));
                    //}
                }
                #endregion
                #region #actualizacontributoridProd
                else if (data.ModeTest == "5")
                {
                    List<Tuple<String, int, int, int>> ListData = new List<Tuple<string, int, int, int>>();
                    ListData.Add(new Tuple<string, int, int, int>("1116261717", 1, 2, 160119));
                    ListData.Add(new Tuple<string, int, int, int>("13060584", 1, 2, 130069));
                    ListData.Add(new Tuple<string, int, int, int>("15431620", 1, 2, 23972));
                    ListData.Add(new Tuple<string, int, int, int>("15959491", 1, 2, 78499));
                    ListData.Add(new Tuple<string, int, int, int>("16215590", 1, 2, 51129));
                    ListData.Add(new Tuple<string, int, int, int>("19326888", 1, 2, 130721));
                    ListData.Add(new Tuple<string, int, int, int>("19390632", 1, 2, 78292));
                    ListData.Add(new Tuple<string, int, int, int>("25613044", 1, 2, 188376));
                    ListData.Add(new Tuple<string, int, int, int>("43055922", 1, 2, 26254));
                    ListData.Add(new Tuple<string, int, int, int>("48574672", 1, 2, 188388));
                    ListData.Add(new Tuple<string, int, int, int>("63304321", 1, 2, 79047));
                    ListData.Add(new Tuple<string, int, int, int>("6873403", 1, 2, 104683));
                    ListData.Add(new Tuple<string, int, int, int>("70694218", 1, 2, 50411));
                    ListData.Add(new Tuple<string, int, int, int>("800115005", 1, 2, 104705));
                    ListData.Add(new Tuple<string, int, int, int>("800146814", 1, 2, 25695));
                    ListData.Add(new Tuple<string, int, int, int>("800207224", 1, 2, 87038));
                    ListData.Add(new Tuple<string, int, int, int>("800216484", 1, 2, 188572));
                    ListData.Add(new Tuple<string, int, int, int>("802021369", 1, 2, 6979));
                    ListData.Add(new Tuple<string, int, int, int>("811029042", 1, 2, 78164));
                    ListData.Add(new Tuple<string, int, int, int>("812007286", 1, 2, 76750));
                    ListData.Add(new Tuple<string, int, int, int>("814003420", 1, 2, 160316));
                    ListData.Add(new Tuple<string, int, int, int>("816004480", 1, 2, 158546));
                    ListData.Add(new Tuple<string, int, int, int>("819001302", 1, 2, 158736));
                    ListData.Add(new Tuple<string, int, int, int>("830030986", 1, 2, 213549));
                    ListData.Add(new Tuple<string, int, int, int>("830036470", 1, 2, 130346));
                    ListData.Add(new Tuple<string, int, int, int>("830086720", 1, 2, 87012));
                    ListData.Add(new Tuple<string, int, int, int>("860007229", 1, 2, 7324));
                    ListData.Add(new Tuple<string, int, int, int>("860014987", 1, 2, 130096));
                    ListData.Add(new Tuple<string, int, int, int>("860066875", 1, 2, 5932));
                    ListData.Add(new Tuple<string, int, int, int>("890300524", 1, 2, 186947));
                    ListData.Add(new Tuple<string, int, int, int>("890303254", 1, 2, 77362));
                    ListData.Add(new Tuple<string, int, int, int>("891401093", 1, 2, 77421));
                    ListData.Add(new Tuple<string, int, int, int>("891408974", 1, 2, 50152));
                    ListData.Add(new Tuple<string, int, int, int>("891700203", 1, 2, 129596));
                    ListData.Add(new Tuple<string, int, int, int>("891801193", 1, 2, 5470));
                    ListData.Add(new Tuple<string, int, int, int>("900011819", 1, 2, 137154));
                    ListData.Add(new Tuple<string, int, int, int>("900011885", 1, 2, 152220));
                    ListData.Add(new Tuple<string, int, int, int>("900021682", 1, 2, 52082));
                    ListData.Add(new Tuple<string, int, int, int>("900038575", 1, 2, 158868));
                    ListData.Add(new Tuple<string, int, int, int>("900056422", 1, 2, 185897));
                    ListData.Add(new Tuple<string, int, int, int>("900169782", 1, 2, 23744));
                    ListData.Add(new Tuple<string, int, int, int>("900197552", 1, 2, 131219));
                    ListData.Add(new Tuple<string, int, int, int>("900205886", 1, 2, 158577));
                    ListData.Add(new Tuple<string, int, int, int>("900337310", 1, 2, 129725));
                    ListData.Add(new Tuple<string, int, int, int>("900361697", 1, 2, 26127));
                    ListData.Add(new Tuple<string, int, int, int>("900417923", 1, 2, 25414));
                    ListData.Add(new Tuple<string, int, int, int>("900438679", 1, 2, 41080));
                    ListData.Add(new Tuple<string, int, int, int>("900450994", 1, 2, 79677));
                    ListData.Add(new Tuple<string, int, int, int>("900474088", 1, 2, 77133));
                    ListData.Add(new Tuple<string, int, int, int>("900478500", 1, 2, 105105));
                    ListData.Add(new Tuple<string, int, int, int>("900521455", 1, 2, 214363));
                    ListData.Add(new Tuple<string, int, int, int>("900530718", 1, 2, 235270));
                    ListData.Add(new Tuple<string, int, int, int>("900535078", 1, 2, 134392));
                    ListData.Add(new Tuple<string, int, int, int>("900582372", 1, 2, 53013));
                    ListData.Add(new Tuple<string, int, int, int>("900598357", 1, 2, 158944));
                    ListData.Add(new Tuple<string, int, int, int>("900601276", 1, 2, 52896));
                    ListData.Add(new Tuple<string, int, int, int>("900633198", 1, 2, 25827));
                    ListData.Add(new Tuple<string, int, int, int>("900676334", 1, 2, 186553));
                    ListData.Add(new Tuple<string, int, int, int>("900697924", 1, 2, 72521));
                    ListData.Add(new Tuple<string, int, int, int>("900699319", 1, 2, 104764));
                    ListData.Add(new Tuple<string, int, int, int>("900701816", 1, 2, 77422));
                    ListData.Add(new Tuple<string, int, int, int>("900740317", 1, 2, 130842));
                    ListData.Add(new Tuple<string, int, int, int>("900772711", 1, 2, 104487));
                    ListData.Add(new Tuple<string, int, int, int>("900781986", 1, 2, 130680));
                    ListData.Add(new Tuple<string, int, int, int>("900785720", 1, 2, 77991));
                    ListData.Add(new Tuple<string, int, int, int>("900819287", 1, 2, 157250));
                    ListData.Add(new Tuple<string, int, int, int>("900922437", 1, 2, 83536));
                    ListData.Add(new Tuple<string, int, int, int>("900961711", 1, 2, 159704));
                    ListData.Add(new Tuple<string, int, int, int>("900988309", 1, 2, 3659));
                    ListData.Add(new Tuple<string, int, int, int>("901023043", 1, 2, 2862));
                    ListData.Add(new Tuple<string, int, int, int>("901029762", 1, 2, 50838));
                    ListData.Add(new Tuple<string, int, int, int>("901048549", 1, 2, 79446));
                    ListData.Add(new Tuple<string, int, int, int>("901107406", 1, 2, 5338));
                    ListData.Add(new Tuple<string, int, int, int>("901118295", 1, 2, 186776));
                    ListData.Add(new Tuple<string, int, int, int>("901132953", 1, 2, 76840));
                    ListData.Add(new Tuple<string, int, int, int>("901133642", 1, 2, 79801));
                    ListData.Add(new Tuple<string, int, int, int>("901143611", 1, 2, 105896));
                    ListData.Add(new Tuple<string, int, int, int>("901153500", 1, 1, 130098));
                    ListData.Add(new Tuple<string, int, int, int>("901153500", 1, 2, 99338));
                    ListData.Add(new Tuple<string, int, int, int>("901165175", 1, 2, 25508));
                    ListData.Add(new Tuple<string, int, int, int>("901227025", 1, 2, 25013));
                    ListData.Add(new Tuple<string, int, int, int>("901368769", 1, 2, 242743));
                    ListData.Add(new Tuple<string, int, int, int>("901378861", 1, 2, 242516));
                    ListData.Add(new Tuple<string, int, int, int>("901405704", 1, 2, 242688));
                    ListData.Add(new Tuple<string, int, int, int>("901458700", 1, 2, 242711));
                    ListData.Add(new Tuple<string, int, int, int>("901490114", 1, 2, 242890));


                    IEnumerable<GlobalOtherDocElecOperation> listGlobalOtherDocElecOperationProd = TableManagerGlobalOtherDocElecOperationProd.GetRowsContainsInPartitionKeys<GlobalOtherDocElecOperation>(ListData.Select(d => d.Item1));
                    var listGlobalOtherDocElecOperationDifProd = from godeo in listGlobalOtherDocElecOperationProd
                                                                 join ld in ListData on new { godeo.PartitionKey, godeo.ContributorTypeId, godeo.OperationModeId } equals new { PartitionKey = ld.Item1, ContributorTypeId = ld.Item2, OperationModeId = ld.Item3 }
                                                                 where godeo.OtherDocElecContributorId != ld.Item4
                                                                 select new { godeo, ld };
                    foreach (var item in listGlobalOtherDocElecOperationDifProd)
                    {
                        messages.Add(string.Format("Se actualizo el nit {0}", item.godeo.PartitionKey));
                        item.godeo.OtherDocElecContributorId = item.ld.Item4;
                        arrayTasks.Add(TableManagerGlobalOtherDocElecOperationProd.InsertOrUpdateAsync(item.godeo));
                    }

                    IEnumerable<GlobalTestSetOthersDocumentsResult> listGlobalTestSetOthersDocumentsResultProd = TableManagerGlobalTestSetOthersDocumentsResultProd.GetRowsContainsInPartitionKeys<GlobalTestSetOthersDocumentsResult>(ListData.Select(d => d.Item1));
                    var listGlobalTestSetOthersDocumentsResultDifProd = from godeo in listGlobalTestSetOthersDocumentsResultProd
                                                                        join ld in ListData on new { godeo.PartitionKey, godeo.ContributorTypeId, OperationModeId = godeo.RowKey.Split('|')[0] } equals new { PartitionKey = ld.Item1, ContributorTypeId = ld.Item2.ToString(), OperationModeId = ld.Item3.ToString() }
                                                                        where godeo.OtherDocElecContributorId != ld.Item4
                                                                        select new { godeo, ld };
                    foreach (var item1 in listGlobalTestSetOthersDocumentsResultDifProd)
                    {
                        messages.Add(string.Format("Se actualizo el nit {0}", item1.godeo.PartitionKey));
                        item1.godeo.OtherDocElecContributorId = item1.ld.Item4;
                        arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResultProd.InsertOrUpdateAsync(item1.godeo));
                    }
                }
                #endregion
                #region #Softwarepartitiondiferenterow
                else if (data.ModeTest == "6")
                {
                    GlobalSoftware globalSoftwareAux = null;
                    IEnumerable<GlobalSoftware> listGlobalSoftwareProd = TableManagerGlobalSoftwareProd.GetRowsByAnyFilter<GlobalSoftware>(" Timestamp ge datetime'2022-04-01T00:00:00.000Z' ");
                    foreach (GlobalSoftware globalSoftware in listGlobalSoftwareProd.Where(gs => !gs.PartitionKey.Equals(gs.RowKey)))
                    {
                        globalSoftwareAux = new GlobalSoftware();
                        globalSoftwareAux.PartitionKey = globalSoftware.PartitionKey;
                        globalSoftwareAux.RowKey = globalSoftware.RowKey;
                        globalSoftwareAux.ETag = globalSoftware.ETag;
                        globalSoftware.RowKey = globalSoftware.PartitionKey;
                        globalSoftware.Timestamp = DateTime.Now;
                        arrayTasks.Add(TableManagerGlobalSoftwareProd.InsertOrUpdateAsync(globalSoftware));
                        arrayTasks.Add(TableManagerGlobalSoftwareProd.DeleteAsync(globalSoftwareAux));
                        messages.Add(String.Format("Se actualizo el software base '{0}', software id '{1}'", globalSoftware.PartitionKey, globalSoftware.Id));
                    }
                }
                #endregion
                else
                {
                    List<string> nitsAux = nits.ToList();
                    while (nitsAux.Count > 0)
                    {

                        IEnumerable<GlobalOtherDocElecOperation> listGlobalOtherDocElecOperation = TableManagerGlobalOtherDocElecOperation.GetRowsContainsInPartitionKeys<GlobalOtherDocElecOperation>(nitsAux.Take(50));
                        var listGlobalOtherDocElecOperationByType = listGlobalOtherDocElecOperation.Where(godeo => (!godeo.Deleted));
                        nitSwIdOperMod = listGlobalOtherDocElecOperationByType.Select(godeo => new Tuple<string, string, int, string>(godeo.PartitionKey, godeo.RowKey, godeo.OperationModeId, godeo.State)).ToList();
                        if (nitSwIdOperMod.Count() > 0)
                        {

                            List<GlobalTestSetOthersDocumentsResult> listGlobalTestSetOthersDocumentsResult = TableManagerGlobalTestSetOthersDocumentsResult.GetRowsContainsInPartitionRowKey<GlobalTestSetOthersDocumentsResult>(nitSwIdOperMod).ToList();
                            //aceptados mayores o iguales al requerido y total aceptados mayores o iguales al total requerido
                            foreach (var item in listGlobalTestSetOthersDocumentsResult.Where(gtsodr => (!gtsodr.Deleted) &&
                                (gtsodr.ElectronicPayrollAjustmentAccepted >= gtsodr.ElectronicPayrollAjustmentAcceptedRequired) &&
                                (gtsodr.OthersDocumentsAccepted >= gtsodr.OthersDocumentsAcceptedRequired) &&
                                (gtsodr.TotalDocumentAccepted >= gtsodr.TotalDocumentAcceptedRequired)))
                            {

                                _StateOld = item.State;
                                if (item.Status != 1)
                                {
                                    AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item.PartitionKey, tieneHabilitados = true });
                                    item.Status = 1;
                                    item.StatusDescription = "Aceptado";
                                    item.State = "Aceptado";
                                    item.Timestamp = DateTime.Now;
                                    //Actualiza GlobalTestSetOthersDocumentsResult
                                    if (data.ModeTest == "0")
                                        arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item));
                                    messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", item.PartitionKey, item.RowKey, item.RowKey.Split('|')[0], _StateOld, "Aceptado"));
                                }
                                itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item.PartitionKey && nso.Item2 == item.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item.RowKey.Split('|')[0]).FirstOrDefault();
                                if (itemTuple != null)// && itemTuple.Item4 != "Habilitado")
                                {
                                    itemGODEO = listGlobalOtherDocElecOperationByType.Where(godeo => godeo.PartitionKey == item.PartitionKey &&
                                        godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3 && godeo.SoftwareId == item.SoftwareId).FirstOrDefault();
                                    if (itemGODEO != null)
                                    {
                                        //ListNitDatos.Add(new NitDatos() { Nit = itemGODEO.PartitionKey, BaseSoftwareId = itemGODEO.RowKey, OperationModeId = itemGODEO.OperationModeId, OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = itemGODEO.SoftwareId });
                                        if (itemTuple.Item4 != "Habilitado")
                                        {
                                            AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item.PartitionKey, tieneHabilitados = true });
                                            itemGODEO.State = "Habilitado";
                                            itemGODEO.Timestamp = DateTime.Now;
                                            //Actualiza GlobalOtherDocElecOperation
                                            if (data.ModeTest == "0")
                                                arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                            messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, itemTuple.Item4, "Habilitado"));
                                            if (data.ModeTest == "0")
                                            {
                                                arrayTasks.Add(Task.Run(() =>
                                                {
                                                    //Actualiza othersDocsElecSoftware
                                                    var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.None);
                                                    if (_guid.Result == Guid.Empty)
                                                        messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado Aceptado", item.SoftwareId));
                                                    else
                                                        messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item.SoftwareId));
                                                }));
                                                arrayTasks.Add(Task.Run(() =>
                                                {
                                                    //Actualiza OtherDocElecContributorOperations
                                                    var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item.SoftwareId), OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado, Deleted = false }, Domain.Common.OtherDocElecState.none);
                                                    if (_ContributorOperationsId.Result == 0)
                                                        messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado Habilitado", item.SoftwareId));
                                                    else
                                                        messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item.SoftwareId));
                                                }));
                                            }
                                            else
                                            {
                                                messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado Aceptado", item.SoftwareId));
                                                messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado Habilitado", item.SoftwareId));
                                            }
                                        }
                                        contributor = contributorService.GetByCode(itemGODEO.PartitionKey);
                                        if (contributor == null)
                                            messages.Add(String.Format("contributor - No se encontro con Code : {0}", itemGODEO.PartitionKey));
                                        else
                                        {
                                            //si no esta habilitado en produccion, se envia para su habilitacion
                                            var filter = new Domain.Sql.OtherDocElecContributorOperations()
                                            {
                                                OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId,
                                                SoftwareId = new Guid(item.SoftwareId),
                                                OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                                Deleted = false
                                            };
                                            if (!othersElectronicDocumentsService.QualifiedContributor(filter
                                                ,
                                                new Domain.Sql.OtherDocElecContributor() { ContributorId = contributor.Id, OtherDocElecOperationModeId = itemGODEO.OperationModeId, Description = itemGODEO.PartitionKey },
                                                sqlConnectionStringProd))
                                            {
                                                IEnumerable<GlobalOtherDocElecOperation> listGlobalOtherDocElecOperationProd = TableManagerGlobalOtherDocElecOperationProd.FindByPartition<GlobalOtherDocElecOperation>(itemGODEO.PartitionKey);
                                                var listGlobalOtherDocElecOperationProdNoDeleted = listGlobalOtherDocElecOperationProd.Where(godeo => godeo.SoftwareId != itemGODEO.SoftwareId && (!godeo.Deleted) && godeo.RowKey == itemGODEO.RowKey);
                                                if (listGlobalOtherDocElecOperationProdNoDeleted.Any())
                                                {
                                                    AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item.PartitionKey, SoftwareEqualBase = true });
                                                    if (data.ModeTest == "0")
                                                    {
                                                        var cop = listGlobalOtherDocElecOperationProdNoDeleted.FirstOrDefault();
                                                        if (cop != null)
                                                        {
                                                            cop.OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId;
                                                            arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(cop));


                                                            var listGlobalTestSetOthersDocumentsResultProd = TableManagerGlobalTestSetOthersDocumentsResultProd.GetRowsContainsInPartitionRowKey<GlobalTestSetOthersDocumentsResult>(new List<Tuple<string, string, int, string>>() { new Tuple<string, string, int, string>(cop.PartitionKey, cop.RowKey, cop.OperationModeId, cop.State) }).ToList();
                                                            if (listGlobalTestSetOthersDocumentsResultProd.Any())
                                                            {
                                                                var odrp = listGlobalTestSetOthersDocumentsResultProd.FirstOrDefault();
                                                                if (odrp != null)
                                                                {
                                                                    odrp.OtherDocElecContributorId = item.OtherDocElecContributorId;
                                                                    odrp.ProviderId = item.ProviderId;
                                                                    arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(odrp));
                                                                }

                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {

                                                    AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item.PartitionKey, tieneHabilitados = true });
                                                    if (data.ModeTest == "0")
                                                    {
                                                        arrayTasks.Add(Task.Run(async () =>
                                                        {
                                                            var con = contributor;
                                                            var ig = itemGODEO;
                                                            var it = item;
                                                            Domain.Sql.OtherDocElecSoftware software = softwareService.GetOtherDocSoftware(new Guid(it.SoftwareId));
                                                            if (software == null)
                                                                messages.Add(String.Format("OtherDocElecSoftware - No se encontro con SoftwareId : {0}", it.SoftwareId));
                                                            else
                                                            {
                                                                #region migracion SQL
                                                                try
                                                                {
                                                                    var requestObject = new
                                                                    {
                                                                        code = ig.PartitionKey,
                                                                        contributorId = con.Id,
                                                                        contributorTypeId = ig.ContributorTypeId,
                                                                        softwareId = ig.RowKey,
                                                                        softwareType = ig.OperationModeId,
                                                                        softwareUser = software.SoftwareUser,
                                                                        softwarePassword = software.SoftwarePassword,
                                                                        pin = software.Pin,
                                                                        url = software.Url,
                                                                        softwareName = software.Name,
                                                                        enabled = othersElectronicDocumentsService.QualifiedContributor(
                                                                            new Domain.Sql.OtherDocElecContributorOperations()
                                                                            {
                                                                                OtherDocElecContributorId = ig.OtherDocElecContributorId,
                                                                                SoftwareId = new Guid(it.SoftwareId),
                                                                                OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                                                                Deleted = false
                                                                            }, new Domain.Sql.OtherDocElecContributor() { ContributorId = con.Id, OtherDocElecOperationModeId = ig.OperationModeId, Description = ig.PartitionKey },

                                                                            string.Empty),
                                                                        contributorOpertaionModeId = ig.OperationModeId
                                                                        ,
                                                                        otherDocElecContributorId = ig.OtherDocElecContributorId
                                                                    };


                                                                    string functionPath = ConfigurationManager.GetValue("SendToActivateOtherDocumentContributorUrl");

                                                                    var activation = await ApiHelpers.ExecuteRequestAsync<SendToActivateContributorResponse>(functionPath, requestObject);

                                                                    if (activation.Success)
                                                                    {
                                                                        messages.Add(String.Format("OtherDocElecContributor - ContributorId : {0}, ContributorTypeId : {1}, SoftwareId : {2},  SoftwareType : {3}, Se Activo",
                                                                        con.Id, ig.ContributorTypeId, ig.SoftwareId, ig.OperationModeId));

                                                                        var guid = Guid.NewGuid().ToString();
                                                                        var contributorActivation = new GlobalContributorActivation(con.Code, guid)
                                                                        {
                                                                            Success = true,
                                                                            ContributorCode = Convert.ToString(ig.OtherDocElecContributorId),
                                                                            ContributorTypeId = Convert.ToInt32(ig.ContributorTypeId),
                                                                            OperationModeId = Convert.ToInt32(ig.OperationModeId),
                                                                            OperationModeName = "OTHERDOCUMENTS",
                                                                            SentToActivateBy = "Function",
                                                                            SoftwareId = ig.RowKey,
                                                                            SendDate = DateTime.UtcNow,
                                                                            TestSetId = it.Id,
                                                                            Request = JsonConvert.SerializeObject(requestObject)
                                                                        };
                                                                        var contAct = await contributorActivationTableManager.InsertOrUpdateAsync(contributorActivation);
                                                                        if (contAct)
                                                                            messages.Add(String.Format("GlobalContributorActivation - ContributorCode : {0}, ContributorTypeId : {1}, OperationModeId : {2},  SoftwareId : {3}, Se Activo",
                                                                            ig.OtherDocElecContributorId, ig.ContributorTypeId, ig.OperationModeId, ig.SoftwareId));
                                                                        else
                                                                            messages.Add(String.Format("GlobalContributorActivation - ContributorCode : {0}, ContributorTypeId : {1}, OperationModeId : {2},  SoftwareId : {3}, NO Se Activo",
                                                                            ig.OtherDocElecContributorId, ig.ContributorTypeId, ig.OperationModeId, ig.SoftwareId));
                                                                    }
                                                                    else
                                                                        messages.Add(String.Format("SendToActivateOtherDocumentContributorUrl - ContributorId : {0}, ContributorTypeId : {1}, SoftwareId : {2},  SoftwareType : {3}, Se presento el siguiente error : {4}",
                                                                        con.Id, ig.ContributorTypeId, ig.SoftwareId, ig.OperationModeId, activation.Message));
                                                                }

                                                                catch (Exception ex)
                                                                {
                                                                    messages.Add(String.Format("OtherDocElecContributor - Error al enviar a activar contributor con id : {0}", con.Id));
                                                                    log.Error($"Error al enviar a activar OtherDocument contribuyente con Code {ig.PartitionKey} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);

                                                                }
                                                                #endregion
                                                                //}
                                                            }
                                                        }));
                                                    }
                                                    else
                                                        messages.Add(String.Format("OtherDocElecContributor - OtherDocElecContributorId : {0} , SoftwareId : {1}, SE ENVIARIA HABILITAR EN PRODUCCION", itemGODEO.OtherDocElecContributorId, item.SoftwareId));
                                                }
                                            }
                                            else
                                            {
                                                AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item.PartitionKey, tieneEnProduccion = true });
                                                //ListNitDatos.Add(new NitDatos() { Nit = itemGODEO.PartitionKey, BaseSoftwareId = itemGODEO.RowKey, OperationModeId = itemGODEO.OperationModeId, OtherDocElecContributorId = filter.OtherDocElecContributorId, SoftwareId = itemGODEO.SoftwareId, AzureProd = true  });
                                                messages.Add(String.Format("OtherDocElecContributor - PartitionKey : {0}, OtherDocElecContributorId : {1} , SoftwareId : {2}, YA ESTA HABILITADO EN PRODUCCION", item.PartitionKey, itemGODEO.OtherDocElecContributorId, item.SoftwareId));
                                            }
                                        }
                                    }
                                }

                            }


                            //aceptados menores al requerido y aceptados faltantes menores o iguales al faltante de total requerido
                            foreach (var item1 in listGlobalTestSetOthersDocumentsResult.Where(gtsodr => (!gtsodr.Deleted) &&
                                ((gtsodr.TotalDocumentAccepted < gtsodr.TotalDocumentAcceptedRequired) ||
                                    ((gtsodr.OthersDocumentsAccepted < gtsodr.OthersDocumentsAcceptedRequired) || (gtsodr.ElectronicPayrollAjustmentAccepted < gtsodr.ElectronicPayrollAjustmentAcceptedRequired))) &&
                                (gtsodr.TotalDocumentsRejected <= (gtsodr.TotalDocumentRequired - gtsodr.TotalDocumentAcceptedRequired))
                                ))
                            {

                                QualificationProd = false;

                                itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item1.PartitionKey && nso.Item2 == item1.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item1.RowKey.Split('|')[0]).FirstOrDefault();

                                itemGODEO = listGlobalOtherDocElecOperationByType.Where(godeo => godeo.PartitionKey == item1.PartitionKey &&
                                    godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3 && godeo.SoftwareId == item1.SoftwareId).FirstOrDefault();
                                if (itemGODEO != null)
                                {
                                    //if (itemGODEO.State == "Habilitado")
                                    //{
                                    contributor = contributorService.GetByCode(itemGODEO.PartitionKey);
                                    if (contributor == null)
                                    {
                                        //para que no cambie estado
                                        QualificationProd = true;
                                        messages.Add(String.Format("contributor - No se encontro con Code : {0}", itemGODEO.PartitionKey));
                                    }
                                    else
                                    {
                                        //Valida si el participante esta habilitado en produccion
                                        var filter = new Domain.Sql.OtherDocElecContributorOperations()
                                        {
                                            OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId,
                                            SoftwareId = new Guid(item1.SoftwareId),
                                            OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                            Deleted = false
                                        };
                                        if (othersElectronicDocumentsService.QualifiedContributor(
                                        filter, new Domain.Sql.OtherDocElecContributor() { ContributorId = contributor.Id, OtherDocElecOperationModeId = itemGODEO.OperationModeId, Description = itemGODEO.PartitionKey },

                                        sqlConnectionStringProd))
                                        {
                                            //ListNitDatos.Add(new NitDatos() { Nit = itemGODEO.PartitionKey, BaseSoftwareId = itemGODEO.RowKey, OperationModeId = itemGODEO.OperationModeId, OtherDocElecContributorId = filter.OtherDocElecContributorId, SoftwareId = itemGODEO.SoftwareId, AzureProd = true });
                                            AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item1.PartitionKey, tieneEnProduccion = true, FailsSetOfTests = true });
                                            QualificationProd = true;
                                            messages.Add(String.Format("OtherDocElecContributorOperations - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, OtherDocElecContributorId : {3}, SoftwareId : {4}, No pasa set de pruebas, Habilitado en producción", item1.PartitionKey, item1.RowKey, item1.OperationModeId, itemGODEO.OtherDocElecContributorId, item1.SoftwareId));
                                        }
                                    }
                                    //}
                                    if (!QualificationProd)
                                    {
                                        IEnumerable<GlobalOtherDocElecOperation> listGlobalOtherDocElecOperationProd = TableManagerGlobalOtherDocElecOperationProd.FindByPartition<GlobalOtherDocElecOperation>(itemGODEO.PartitionKey);
                                        var listGlobalOtherDocElecOperationProdNoDeleted = listGlobalOtherDocElecOperationProd.Where(godeo => godeo.SoftwareId != itemGODEO.SoftwareId && (!godeo.Deleted) && godeo.RowKey == itemGODEO.RowKey);
                                        if (listGlobalOtherDocElecOperationProdNoDeleted.Any())
                                        {
                                            AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item1.PartitionKey, SoftwareEqualBase = true });
                                            if (data.ModeTest == "0")
                                            {
                                                var cop = listGlobalOtherDocElecOperationProdNoDeleted.FirstOrDefault();
                                                if (cop != null)
                                                {
                                                    cop.OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId;
                                                    arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(cop));


                                                    var listGlobalTestSetOthersDocumentsResultProd = TableManagerGlobalTestSetOthersDocumentsResultProd.GetRowsContainsInPartitionRowKey<GlobalTestSetOthersDocumentsResult>(new List<Tuple<string, string, int, string>>() { new Tuple<string, string, int, string>(cop.PartitionKey, cop.RowKey, cop.OperationModeId, cop.State) }).ToList();
                                                    if (listGlobalTestSetOthersDocumentsResultProd.Any())
                                                    {
                                                        var odrp = listGlobalTestSetOthersDocumentsResultProd.FirstOrDefault();
                                                        if (odrp != null)
                                                        {
                                                            odrp.OtherDocElecContributorId = item1.OtherDocElecContributorId;
                                                            odrp.ProviderId = item1.ProviderId;
                                                            arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(odrp));
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (item1.Status != 0)
                                            {
                                                AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item1.PartitionKey, tieneEnProceso = true });
                                                _StateOld = item1.State;
                                                item1.Status = 0;
                                                item1.StatusDescription = "En proceso";
                                                item1.State = "En proceso";
                                                item1.Timestamp = DateTime.Now;
                                                //Actualiza GlobalTestSetOthersDocumentsResult
                                                if (data.ModeTest == "0")
                                                    arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item1));
                                                messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", item1.PartitionKey, item1.RowKey, item1.RowKey.Split('|')[0], _StateOld, "En proceso"));
                                            }
                                            if (itemTuple != null && itemTuple.Item4 != "En pruebas")
                                            {
                                                AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item1.PartitionKey, tieneEnProceso = true });
                                                itemGODEO.State = "En pruebas";
                                                itemGODEO.Timestamp = DateTime.Now;
                                                //Actualiza GlobalOtherDocElecOperation
                                                if (data.ModeTest == "0")
                                                    arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                                messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, itemTuple.Item4, "En pruebas"));
                                                if (data.ModeTest == "0")
                                                {
                                                    arrayTasks.Add(Task.Run(() =>
                                                    {
                                                        //Actualiza othersDocsElecSoftware
                                                        var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item1.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.InProcess, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.None);
                                                        if (_guid.Result == Guid.Empty)
                                                            messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado En proceso", item1.SoftwareId));
                                                        else
                                                            messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item1.SoftwareId));
                                                    }));
                                                    arrayTasks.Add(Task.Run(() =>
                                                    {
                                                        //Actualiza OtherDocElecContributorOperations
                                                        var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item1.SoftwareId), OperationStatusId = (int)Domain.Common.OtherDocElecState.Test, Deleted = false }, Domain.Common.OtherDocElecState.none);
                                                        if (_ContributorOperationsId.Result == 0)
                                                            messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado En pruebas", item1.SoftwareId));
                                                        else
                                                            messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item1.SoftwareId));
                                                    }));
                                                }
                                                else
                                                {
                                                    messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado En proceso", item1.SoftwareId));
                                                    messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado En pruebas", item1.SoftwareId));
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //existe en prod, actualiza estados
                                        _StateOld = item1.State;
                                        if (item1.Status != 1)
                                        {
                                            AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item1.PartitionKey, tieneHabilitados = true });
                                            item1.Status = 1;
                                            item1.StatusDescription = "Aceptado";
                                            item1.State = "Aceptado";
                                            item1.Timestamp = DateTime.Now;
                                            //Actualiza GlobalTestSetOthersDocumentsResult
                                            if (data.ModeTest == "0")
                                                arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item1));
                                            messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", item1.PartitionKey, item1.RowKey, item1.RowKey.Split('|')[0], _StateOld, "Aceptado"));
                                        }
                                        itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item1.PartitionKey && nso.Item2 == item1.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item1.RowKey.Split('|')[0]).FirstOrDefault();
                                        if (itemTuple != null)// && itemTuple.Item4 != "Habilitado")
                                        {
                                            itemGODEO = listGlobalOtherDocElecOperationByType.Where(godeo => godeo.PartitionKey == item1.PartitionKey &&
                                                godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3 && godeo.SoftwareId == item1.SoftwareId).FirstOrDefault();
                                            if (itemGODEO != null)
                                            {
                                                if (itemTuple.Item4 != "Habilitado")
                                                {
                                                    AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item1.PartitionKey, tieneHabilitados = true });
                                                    itemGODEO.State = "Habilitado";
                                                    itemGODEO.Timestamp = DateTime.Now;
                                                    //Actualiza GlobalOtherDocElecOperation
                                                    if (data.ModeTest == "0")
                                                        arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                                    messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, itemTuple.Item4, "Habilitado"));
                                                    if (data.ModeTest == "0")
                                                    {
                                                        arrayTasks.Add(Task.Run(() =>
                                                        {
                                                            //Actualiza othersDocsElecSoftware
                                                            var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item1.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.None);
                                                            if (_guid.Result == Guid.Empty)
                                                                messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado Aceptado", item1.SoftwareId));
                                                            else
                                                                messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item1.SoftwareId));
                                                        }));
                                                        arrayTasks.Add(Task.Run(() =>
                                                        {
                                                            //Actualiza OtherDocElecContributorOperations
                                                            var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item1.SoftwareId), OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado, Deleted = false }, Domain.Common.OtherDocElecState.none);
                                                            if (_ContributorOperationsId.Result == 0)
                                                                messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado Habilitado", item1.SoftwareId));
                                                            else
                                                                messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item1.SoftwareId));
                                                        }));
                                                    }
                                                    else
                                                    {
                                                        messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado Aceptado", item1.SoftwareId));
                                                        messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado Habilitado", item1.SoftwareId));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //aceptados menores al requerido y aceptados faltantes mayores al faltante de total requerido
                            foreach (var item2 in listGlobalTestSetOthersDocumentsResult.Where(gtsodr => (!gtsodr.Deleted) &&
                                (gtsodr.TotalDocumentAccepted < gtsodr.TotalDocumentAcceptedRequired) &&
                                (gtsodr.TotalDocumentsRejected > (gtsodr.TotalDocumentRequired - gtsodr.TotalDocumentAcceptedRequired))))
                            {

                                QualificationProd = false;

                                itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item2.PartitionKey && nso.Item2 == item2.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item2.RowKey.Split('|')[0]).FirstOrDefault();

                                itemGODEO = listGlobalOtherDocElecOperationByType.Where(godeo => godeo.PartitionKey == item2.PartitionKey &&
                                    godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3 && godeo.SoftwareId == item2.SoftwareId).FirstOrDefault();
                                if (itemGODEO != null)
                                {
                                    //if (itemGODEO.State == "Habilitado")
                                    //{
                                    contributor = contributorService.GetByCode(itemGODEO.PartitionKey);
                                    if (contributor == null)
                                    {
                                        //para que no cambie estado
                                        QualificationProd = true;
                                        messages.Add(String.Format("contributor - No se encontro con Code : {0}", itemGODEO.PartitionKey));
                                    }
                                    else
                                    {
                                        //Valida si el participante esta habilitado en produccion
                                        var filter = new Domain.Sql.OtherDocElecContributorOperations()
                                        {
                                            OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId,
                                            SoftwareId = new Guid(item2.SoftwareId),
                                            OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                            Deleted = false
                                        };
                                        if (othersElectronicDocumentsService.QualifiedContributor(
                                        filter, new Domain.Sql.OtherDocElecContributor() { ContributorId = contributor.Id, OtherDocElecOperationModeId = itemGODEO.OperationModeId, Description = itemGODEO.PartitionKey },
                                        sqlConnectionStringProd))
                                        {
                                            //ListNitDatos.Add(new NitDatos() { Nit = itemGODEO.PartitionKey, BaseSoftwareId = itemGODEO.RowKey, OperationModeId = itemGODEO.OperationModeId, OtherDocElecContributorId = filter.OtherDocElecContributorId, SoftwareId = itemGODEO.SoftwareId, AzureProd = true });
                                            AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item2.PartitionKey, tieneEnProduccion = true, FailsSetOfTests = true });
                                            QualificationProd = true;
                                            messages.Add(String.Format("OtherDocElecContributorOperations - OtherDocElecContributorId : {0}, SoftwareId : {1}, No pasa set de pruebas, Habilitado en producción", itemGODEO.OtherDocElecContributorId, item2.SoftwareId));
                                        }
                                    }
                                    //}
                                    if (!QualificationProd)
                                    {
                                        if (item2.Status != 2)
                                        {
                                            AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item2.PartitionKey, tieneEnProceso = true });
                                            _StateOld = item2.State;
                                            item2.Status = 2;
                                            item2.StatusDescription = "Rechazado";
                                            item2.State = "Rechazado";
                                            item2.Timestamp = DateTime.Now;
                                            //Actualiza GlobalTestSetOthersDocumentsResult
                                            if (data.ModeTest == "0")
                                                arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item2));
                                            messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", item2.PartitionKey, item2.RowKey, item2.RowKey.Split('|')[0], _StateOld, "Rechazado"));
                                        }
                                        if (itemTuple != null && itemTuple.Item4 != "En pruebas")
                                        {
                                            AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item2.PartitionKey, tieneEnProceso = true });
                                            itemGODEO.State = "En pruebas";
                                            itemGODEO.Timestamp = DateTime.Now;
                                            //Actualiza GlobalOtherDocElecOperation
                                            if (data.ModeTest == "0")
                                                arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                            messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, itemTuple.Item4, "En pruebas"));
                                            if (data.ModeTest == "0")
                                            {
                                                arrayTasks.Add(Task.Run(() =>
                                                {
                                                    //Actualiza othersDocsElecSoftware
                                                    var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item2.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Rejected, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.None);
                                                    if (_guid.Result == Guid.Empty)
                                                        messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado Rechazado", item2.SoftwareId));
                                                    else
                                                        messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item2.SoftwareId));
                                                }));
                                                arrayTasks.Add(Task.Run(() =>
                                                {
                                                    //Actualiza OtherDocElecContributorOperations
                                                    var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item2.SoftwareId), OperationStatusId = (int)Domain.Common.OtherDocElecState.Cancelado, Deleted = false }, Domain.Common.OtherDocElecState.none);
                                                    if (_ContributorOperationsId.Result == 0)
                                                        messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado Cancelado (Rechazado)", item2.SoftwareId));
                                                    else
                                                        messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item2.SoftwareId));
                                                }));
                                            }
                                            else
                                            {
                                                messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado Rechazado", item2.SoftwareId));
                                                messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado Cancelado (Rechazado)", item2.SoftwareId));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //existe en prod, actualiza estados
                                        _StateOld = item2.State;
                                        if (item2.Status != 1)
                                        {
                                            AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item2.PartitionKey, tieneHabilitados = true });
                                            item2.Status = 1;
                                            item2.StatusDescription = "Aceptado";
                                            item2.State = "Aceptado";
                                            item2.Timestamp = DateTime.Now;
                                            //Actualiza GlobalTestSetOthersDocumentsResult
                                            if (data.ModeTest == "0")
                                                arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item2));
                                            messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", item2.PartitionKey, item2.RowKey, item2.RowKey.Split('|')[0], _StateOld, "Aceptado"));
                                        }
                                        itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item2.PartitionKey && nso.Item2 == item2.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item2.RowKey.Split('|')[0]).FirstOrDefault();
                                        if (itemTuple != null)// && itemTuple.Item4 != "Habilitado")
                                        {
                                            itemGODEO = listGlobalOtherDocElecOperationByType.Where(godeo => godeo.PartitionKey == item2.PartitionKey &&
                                                godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3 && godeo.SoftwareId == item2.SoftwareId).FirstOrDefault();
                                            if (itemGODEO != null)
                                            {
                                                if (itemTuple.Item4 != "Habilitado")
                                                {
                                                    AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item2.PartitionKey, tieneHabilitados = true });
                                                    itemGODEO.State = "Habilitado";
                                                    itemGODEO.Timestamp = DateTime.Now;
                                                    //Actualiza GlobalOtherDocElecOperation
                                                    if (data.ModeTest == "0")
                                                        arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                                    messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, itemTuple.Item4, "Habilitado"));
                                                    if (data.ModeTest == "0")
                                                    {
                                                        arrayTasks.Add(Task.Run(() =>
                                                        {
                                                            //Actualiza othersDocsElecSoftware
                                                            var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item2.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.None);
                                                            if (_guid.Result == Guid.Empty)
                                                                messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado Aceptado", item2.SoftwareId));
                                                            else
                                                                messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item2.SoftwareId));
                                                        }));
                                                        arrayTasks.Add(Task.Run(() =>
                                                        {
                                                            //Actualiza OtherDocElecContributorOperations
                                                            var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item2.SoftwareId), OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado, Deleted = false }, Domain.Common.OtherDocElecState.none);
                                                            if (_ContributorOperationsId.Result == 0)
                                                                messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado Habilitado", item2.SoftwareId));
                                                            else
                                                                messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item2.SoftwareId));
                                                        }));
                                                    }
                                                    else
                                                    {
                                                        messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado Aceptado", item2.SoftwareId));
                                                        messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado Habilitado", item2.SoftwareId));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        nitsAux.RemoveRange(0, nitsAux.Count > 50 ? 50 : nitsAux.Count);
                    }
                    //ListNitsProcess.Clear();
                    //trae los nits no procesados
                    IEnumerable<string> listNitsNoProcess = (from item3 in nits
                                                             join item4 in ListNitsProcess on item3 equals item4.Nit into ResultProcess
                                                             from item5 in ResultProcess.DefaultIfEmpty()
                                                             where item5 is null
                                                             select item3).AsEnumerable();
                    if (listNitsNoProcess.Count() > 0)
                    {
                        //consulta los registros en produccion por el nit, ya estan habilitados
                        IEnumerable<GlobalOtherDocElecOperation> listGlobalOtherDocElecOperationProd = TableManagerGlobalOtherDocElecOperationProd.GetRowsContainsInPartitionKeys<GlobalOtherDocElecOperation>(listNitsNoProcess);
                        var listGlobalOtherDocElecOperationProdNoDeleted = listGlobalOtherDocElecOperationProd.Where(godeo => (!godeo.Deleted));
                        //Arma una lista de filtros por PartitionKey, RowKey, OperationModeId
                        List<Dictionary<string, string>> nitSwIdOperModProd = new List<Dictionary<string, string>>();
                        listGlobalOtherDocElecOperationProdNoDeleted.ToList().ForEach(f => nitSwIdOperModProd.Add(new Dictionary<string, string>() { { "PartitionKey", string.Format("'{0}'",f.PartitionKey) },
                            { "RowKey", string.Format("'{0}'",f.RowKey) }, {  "OperationModeId", string.Format("{0}",f.OperationModeId) } }));
                        if (nitSwIdOperModProd.Count() > 0)
                        {
                            //consulta los registros en habilitacion por PartitionKey, RowKey, OperationModeId
                            var listGlobalOtherDocElecOperationHab = TableManagerGlobalOtherDocElecOperation.GetRowsContainsInAnyFilter<GlobalOtherDocElecOperation>(nitSwIdOperModProd);
                            nitSwIdOperMod = null;
                            nitSwIdOperMod = listGlobalOtherDocElecOperationHab.Select(godeo => new Tuple<string, string, int, string>(godeo.PartitionKey, godeo.RowKey, godeo.OperationModeId, godeo.State)).ToList();
                            if (nitSwIdOperMod.Count() > 0)
                            {
                                //consulta el registro en azure hab, de los registro habilitados, no borrados en azure prod
                                var listGlobalTestSetOthersDocumentsResultHab = TableManagerGlobalTestSetOthersDocumentsResult.GetRowsContainsInPartitionRowKey<GlobalTestSetOthersDocumentsResult>(nitSwIdOperMod).ToList();
                                //aceptados menores al requerido y aceptados faltantes menores o iguales al faltante de total requerido
                                foreach (var item8 in listGlobalTestSetOthersDocumentsResultHab.Where(gtsodr =>
                                    ((gtsodr.TotalDocumentAccepted < gtsodr.TotalDocumentAcceptedRequired) ||
                                        ((gtsodr.OthersDocumentsAccepted < gtsodr.OthersDocumentsAcceptedRequired) || (gtsodr.ElectronicPayrollAjustmentAccepted < gtsodr.ElectronicPayrollAjustmentAcceptedRequired))) &&
                                    (gtsodr.TotalDocumentsRejected <= (gtsodr.TotalDocumentRequired - gtsodr.TotalDocumentAcceptedRequired))
                                    ))
                                {
                                    AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item8.PartitionKey, FailsSetOfTests = true });
                                }

                                //alguno de los los documentos aceptados mayores o iguales al requerido y total aceptados y rechazados mayores o iguales al total requerido
                                foreach (var item6 in listGlobalTestSetOthersDocumentsResultHab//.Where(gtsodr => 
                                                                                               //((gtsodr.ElectronicPayrollAjustmentAccepted >= gtsodr.ElectronicPayrollAjustmentAcceptedRequired) ||
                                                                                               //(gtsodr.OthersDocumentsAccepted >= gtsodr.OthersDocumentsAcceptedRequired)) &&
                                                                                               //(gtsodr.TotalDocumentAccepted >= gtsodr.TotalDocumentAcceptedRequired))
                                )
                                {

                                    _StateOld = item6.State;
                                    if (item6.Status != 1 || item6.StatusDescription != "Aceptado" || item6.State != "Aceptado" || item6.Deleted)
                                    {
                                        AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item6.PartitionKey, tieneHabilitados = true });
                                        item6.Status = 1;
                                        item6.StatusDescription = "Aceptado";
                                        item6.State = "Aceptado";
                                        item6.Timestamp = DateTime.Now;
                                        item6.Deleted = false;
                                        //Actualiza GlobalTestSetOthersDocumentsResult
                                        if (data.ModeTest == "0")
                                            arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item6));
                                        messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", item6.PartitionKey, item6.RowKey, item6.RowKey.Split('|')[0], _StateOld, "Aceptado"));

                                    }
                                    itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item6.PartitionKey && nso.Item2 == item6.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item6.RowKey.Split('|')[0]).FirstOrDefault();
                                    if (itemTuple != null)
                                    {
                                        itemGODEO = listGlobalOtherDocElecOperationHab.Where(godeo => godeo.PartitionKey == item6.PartitionKey &&
                                            godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3 && godeo.SoftwareId == item6.SoftwareId).FirstOrDefault();
                                        if (itemGODEO != null)
                                        {
                                            //ListNitDatos.Add(new NitDatos() { Nit = itemGODEO.PartitionKey, BaseSoftwareId = itemGODEO.RowKey, OperationModeId = itemGODEO.OperationModeId, OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = itemGODEO.SoftwareId });
                                            if (item6.State != "Habilitado" || item6.Deleted)
                                            {
                                                AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item6.PartitionKey, tieneHabilitados = true });


                                                itemGODEO.State = "Habilitado";
                                                itemGODEO.Timestamp = DateTime.Now;
                                                itemGODEO.Deleted = false;
                                                //Actualiza GlobalOtherDocElecOperation
                                                if (data.ModeTest == "0")
                                                    arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                                messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, StateOld : {3}, StateNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, itemTuple.Item4, "Habilitado"));
                                                if (data.ModeTest == "0")
                                                {
                                                    arrayTasks.Add(Task.Run(() =>
                                                    {
                                                        //Actualiza othersDocsElecSoftware
                                                        var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item6.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.None);
                                                        if (_guid.Result == Guid.Empty)
                                                            messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado Aceptado", item6.SoftwareId));
                                                        else
                                                            messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item6.SoftwareId));
                                                    }));
                                                    arrayTasks.Add(Task.Run(() =>
                                                    {
                                                        //Actualiza OtherDocElecContributorOperations
                                                        var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item6.SoftwareId), OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado, Deleted = false }, Domain.Common.OtherDocElecState.none);
                                                        if (_ContributorOperationsId.Result == 0)
                                                            messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado Habilitado", item6.SoftwareId));
                                                        else
                                                            messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item6.SoftwareId));
                                                    }));
                                                }
                                                else
                                                {
                                                    messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado Aceptado", item6.SoftwareId));
                                                    messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado Habilitado", item6.SoftwareId));
                                                }
                                            }
                                            contributor = contributorService.GetByCode(itemGODEO.PartitionKey);
                                            if (contributor == null)
                                                messages.Add(String.Format("contributor - No se encontro con Code : {0}", itemGODEO.PartitionKey));
                                            else
                                            {
                                                //si no esta habilitado en produccion, se envia para su habilitacion
                                                var filter = new Domain.Sql.OtherDocElecContributorOperations()
                                                {

                                                    OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId,
                                                    SoftwareId = new Guid(item6.SoftwareId),
                                                    OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                                    Deleted = false,
                                                };
                                                if (!othersElectronicDocumentsService.QualifiedContributor(
                                                    filter,
                                                    new Domain.Sql.OtherDocElecContributor() { ContributorId = contributor.Id, OtherDocElecOperationModeId = itemGODEO.OperationModeId, Description = itemGODEO.PartitionKey },
                                                    sqlConnectionStringProd))
                                                {
                                                    AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item6.PartitionKey, tieneHabilitados = true });

                                                    if (data.ModeTest == "0")
                                                    {
                                                        arrayTasks.Add(Task.Run(async () =>
                                                        {
                                                            var con = contributor;
                                                            var ig = itemGODEO;
                                                            var it = item6;
                                                            Domain.Sql.OtherDocElecSoftware software = softwareService.GetOtherDocSoftware(new Guid(it.SoftwareId));
                                                            if (software == null)
                                                                messages.Add(String.Format("OtherDocElecSoftware - No se encontro con SoftwareId : {0}", it.SoftwareId));
                                                            else
                                                            {
                                                                #region migracion SQL
                                                                try
                                                                {
                                                                    var requestObject = new
                                                                    {
                                                                        code = ig.PartitionKey,
                                                                        contributorId = con.Id,
                                                                        contributorTypeId = ig.ContributorTypeId,
                                                                        softwareId = ig.RowKey,
                                                                        softwareType = ig.OperationModeId,
                                                                        softwareUser = software.SoftwareUser,
                                                                        softwarePassword = software.SoftwarePassword,
                                                                        pin = software.Pin,
                                                                        url = software.Url,
                                                                        softwareName = software.Name,
                                                                        enabled = othersElectronicDocumentsService.QualifiedContributor(
                                                                            new Domain.Sql.OtherDocElecContributorOperations()
                                                                            {
                                                                                OtherDocElecContributorId = ig.OtherDocElecContributorId,
                                                                                SoftwareId = new Guid(it.SoftwareId),
                                                                                OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado,
                                                                                Deleted = false
                                                                            }, new Domain.Sql.OtherDocElecContributor() { ContributorId = con.Id, OtherDocElecOperationModeId = ig.OperationModeId, Description = ig.PartitionKey },

                                                                            string.Empty),
                                                                        contributorOpertaionModeId = ig.OperationModeId
                                                                        ,
                                                                        otherDocElecContributorId = ig.OtherDocElecContributorId
                                                                    };


                                                                    string functionPath = ConfigurationManager.GetValue("SendToActivateOtherDocumentContributorUrl");

                                                                    var activation = await ApiHelpers.ExecuteRequestAsync<SendToActivateContributorResponse>(functionPath, requestObject);

                                                                    if (activation.Success)
                                                                    {
                                                                        messages.Add(String.Format("OtherDocElecContributor - ContributorId : {0}, ContributorTypeId : {1}, SoftwareId : {2},  SoftwareType : {3}, Se Activo",
                                                                        con.Id, ig.ContributorTypeId, ig.SoftwareId, ig.OperationModeId));

                                                                        var guid = Guid.NewGuid().ToString();
                                                                        var contributorActivation = new GlobalContributorActivation(con.Code, guid)
                                                                        {
                                                                            Success = true,
                                                                            ContributorCode = Convert.ToString(ig.OtherDocElecContributorId),
                                                                            ContributorTypeId = Convert.ToInt32(ig.ContributorTypeId),
                                                                            OperationModeId = Convert.ToInt32(ig.OperationModeId),
                                                                            OperationModeName = "OTHERDOCUMENTS",
                                                                            SentToActivateBy = "Function",
                                                                            SoftwareId = ig.RowKey,
                                                                            SendDate = DateTime.UtcNow,
                                                                            TestSetId = it.Id,
                                                                            Request = JsonConvert.SerializeObject(requestObject)
                                                                        };
                                                                        var contAct = await contributorActivationTableManager.InsertOrUpdateAsync(contributorActivation);
                                                                        if (contAct)
                                                                            messages.Add(String.Format("GlobalContributorActivation - ContributorCode : {0}, ContributorTypeId : {1}, OperationModeId : {2},  SoftwareId : {3}, Se Activo",
                                                                            ig.OtherDocElecContributorId, ig.ContributorTypeId, ig.OperationModeId, ig.SoftwareId));
                                                                        else
                                                                            messages.Add(String.Format("GlobalContributorActivation - ContributorCode : {0}, ContributorTypeId : {1}, OperationModeId : {2},  SoftwareId : {3}, NO Se Activo",
                                                                            ig.OtherDocElecContributorId, ig.ContributorTypeId, ig.OperationModeId, ig.SoftwareId));
                                                                    }
                                                                    else
                                                                        messages.Add(String.Format("SendToActivateOtherDocumentContributorUrl - ContributorId : {0}, ContributorTypeId : {1}, SoftwareId : {2},  SoftwareType : {3}, Se presento el siguiente error : {4}",
                                                                        con.Id, ig.ContributorTypeId, ig.SoftwareId, ig.OperationModeId, activation.Message));
                                                                }

                                                                catch (Exception ex)
                                                                {
                                                                    messages.Add(String.Format("OtherDocElecContributor - Error al enviar a activar contributor con id : {0}", con.Id));
                                                                    log.Error($"Error al enviar a activar OtherDocument contribuyente con Code {ig.PartitionKey} en producción _________ {ex.Message} _________ {ex.StackTrace} _________ {ex.Source}", ex);

                                                                }
                                                                #endregion
                                                                //}
                                                            }
                                                        }));
                                                    }
                                                    else
                                                        messages.Add(String.Format("OtherDocElecContributor - OtherDocElecContributorId : {0} , SoftwareId : {1}, SE ENVIARIA HABILITAR EN PRODUCCION", itemGODEO.OtherDocElecContributorId, item6.SoftwareId));
                                                }
                                                else
                                                {
                                                    //ListNitDatos.Add(new NitDatos() { Nit = itemGODEO.PartitionKey, BaseSoftwareId = itemGODEO.RowKey, OperationModeId = itemGODEO.OperationModeId, OtherDocElecContributorId = filter.OtherDocElecContributorId, SoftwareId = itemGODEO.SoftwareId, AzureProd = true });
                                                    AddNitProcesed(ListNitsProcess, new NitProcesed() { Nit = item6.PartitionKey, tieneEnProduccion = true });
                                                    messages.Add(String.Format("OtherDocElecContributor - OtherDocElecContributorId : {0} , SoftwareId : {1}, YA ESTA HABILITADO EN PRODUCCION", itemGODEO.OtherDocElecContributorId, item6.SoftwareId));
                                                }
                                            }

                                        }
                                    }
                                }

                                //si total de aceptados es menor al total de aceptados requeridos
                                //foreach (var item7 in listGlobalTestSetOthersDocumentsResultHab//.Where(gtsodr => (gtsodr.TotalDocumentAccepted < gtsodr.TotalDocumentAcceptedRequired))
                                //    )
                                //{
                                //    _StateOld = item7.Deleted ? "Eliminado": "No Eliminado";
                                //    item7.Status = 1;
                                //    item7.StatusDescription = "Aceptado";
                                //    item7.State = "Aceptado";
                                //    item7.Timestamp = DateTime.Now;
                                //    item7.Deleted = false;
                                //    //Actualiza GlobalTestSetOthersDocumentsResult
                                //    if (data.ModeTest == "0")
                                //        arrayTasks.Add(TableManagerGlobalTestSetOthersDocumentsResult.InsertOrUpdateAsync(item7));
                                //    messages.Add(String.Format("GlobalTestSetOthersDocumentsResult - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, DeletedOld : {3}, DeletedNew : {4}", item7.PartitionKey, item7.RowKey, item7.RowKey.Split('|')[0], _StateOld, "No Eliminado"));
                                //    itemTuple = nitSwIdOperMod.Where(nso => nso.Item1 == item7.PartitionKey && nso.Item2 == item7.RowKey.Split('|')[1] && Convert.ToString(nso.Item3) == item7.RowKey.Split('|')[0]).FirstOrDefault();
                                //    if (itemTuple != null)
                                //    {
                                //        itemGODEO = listGlobalOtherDocElecOperationHab.Where(godeo => godeo.PartitionKey == item7.PartitionKey &&
                                //            godeo.RowKey == itemTuple.Item2 && godeo.OperationModeId == itemTuple.Item3).FirstOrDefault();
                                //        if (itemGODEO != null)
                                //        {
                                //            _StateOld = itemGODEO.Deleted ? "Eliminado" : "No Eliminado";
                                //            itemGODEO.State = "Habilitado";
                                //            itemGODEO.Timestamp = DateTime.Now;
                                //            itemGODEO.Deleted = false;
                                //            //Actualiza GlobalOtherDocElecOperation
                                //            if (data.ModeTest == "0")
                                //                arrayTasks.Add(TableManagerGlobalOtherDocElecOperation.InsertOrUpdateAsync(itemGODEO));
                                //            messages.Add(String.Format("GlobalOtherDocElecOperation - PartitionKey : {0}, RowKey : {1}, OperationModeId : {2}, DeletedOld : {3}, DeletedNew : {4}", itemGODEO.PartitionKey, itemGODEO.RowKey, itemGODEO.OperationModeId, _StateOld, "No Eliminado"));
                                //            if (data.ModeTest == "0")
                                //            {
                                //                arrayTasks.Add(Task.Run(() =>
                                //                {
                                //                    //Actualiza othersDocsElecSoftware
                                //                    var _guid = othersDocsElecSoftwareService.UpdateSoftwareStatusId(new Domain.Sql.OtherDocElecSoftware() { Id = new Guid(item7.SoftwareId), OtherDocElecSoftwareStatusId = (int)Domain.Common.OtherDocElecSoftwaresStatus.Accepted, Status = true, Deleted = false }, Domain.Common.OtherDocElecSoftwaresStatus.Accepted);
                                //                    if (_guid.Result == Guid.Empty)
                                //                        messages.Add(String.Format("OtherDocElecSoftware - No se pudo actualizar el Id : {0}, al estado Aceptado", item7.SoftwareId));
                                //                    else
                                //                        messages.Add(String.Format("OtherDocElecSoftware - Id : {0}", item7.SoftwareId));
                                //                }));
                                //                arrayTasks.Add(Task.Run(() =>
                                //                {
                                //                    //Actualiza OtherDocElecContributorOperations
                                //                    var _ContributorOperationsId = othersElectronicDocumentsService.UpdateOtherDocElecContributorOperationStatusId(new Domain.Sql.OtherDocElecContributorOperations() { 
                                //                        OtherDocElecContributorId = itemGODEO.OtherDocElecContributorId, SoftwareId = new Guid(item7.SoftwareId), 
                                //                        OperationStatusId = (int)Domain.Common.OtherDocElecState.Habilitado, Deleted = false }, Domain.Common.OtherDocElecState.Habilitado);
                                //                    if (_ContributorOperationsId.Result == 0)
                                //                        messages.Add(String.Format("OtherDocElecContributorOperations - No se pudo actualizar con el SoftwareId : {0}, al estado Habilitado", item7.SoftwareId));
                                //                    else
                                //                        messages.Add(String.Format("OtherDocElecContributorOperations - SoftwareId : {0}", item7.SoftwareId));
                                //                }));
                                //            }
                                //            else
                                //            {
                                //                messages.Add(String.Format("OtherDocElecSoftware - Actualizar el Id : {0}, al estado En Proceso", item7.SoftwareId));
                                //                messages.Add(String.Format("OtherDocElecContributorOperations - Actualizar con el SoftwareId : {0}, al estado En Pruebas", item7.SoftwareId));
                                //            }

                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                }

                Task.WhenAll(arrayTasks).Wait();

                if (data.ModeTest == "2")
                {
                }
                else
                {

                    if (ListNitsProcess.Any(np => np.tieneHabilitados || np.tieneEnProceso))
                        messages.Add(string.Format("Habilitar, migrar, o cambiar estado: {0}", String.Join("|", ListNitsProcess.Where(np => np.tieneHabilitados || np.tieneEnProceso).Select(np => np.Nit).ToArray())));

                    if (ListNitsProcess.Any(np => np.tieneEnProduccion && !np.tieneHabilitados && !np.tieneEnProceso))
                        messages.Add(string.Format("Sin cambios: {0}", String.Join("|", ListNitsProcess.Where(np => np.tieneEnProduccion && !np.tieneHabilitados && !np.tieneEnProceso).Select(np => np.Nit).ToArray())));

                    if (ListNitsProcess.Any(np => np.FailsSetOfTests))
                        messages.Add(string.Format("No pasa set de pruebas: {0}", String.Join("|", ListNitsProcess.Where(np => np.FailsSetOfTests).Select(np => np.Nit).ToArray())));

                    if (ListNitsProcess.Any(np => np.SoftwareEqualBase))
                        messages.Add(string.Format("Software distinto en azure prod con igual base: {0}", String.Join("|", ListNitsProcess.Where(np => np.SoftwareEqualBase).Select(np => np.Nit).ToArray())));


                    List<string> ns = ListNitsProcess.Select(np => np.Nit).ToList();
                    if (nits.Any(n => !ns.Contains(n)))
                        messages.Add(string.Format("otros: {0}", String.Join("|", nits.Where(n => !ns.Contains(n)).ToArray())));

                    //messages.Add(string.Format("base de datos, software habilitados: {0}", String.Join("|", ListNitDatos.Select(nd => string.Format("'{0}','{1}',{2},{3},'{4}',{5}", nd.Nit, nd.BaseSoftwareId, nd.OperationModeId, nd.OtherDocElecContributorId, nd.SoftwareId, nd.AzureProd ? 1: 0)).ToArray())));
                }
            }
            catch (Exception ex)
            {

                log.Error(ex.Message + "_________" + ex.StackTrace + "_________" + ex.Source, ex);
                response.Code = ((int)EventValidationMessage.Error).ToString();
                response.Message = ex.Message;
                messages.Add("Se presento el siguiente error:");
                messages.Add(ex.Message);
            }
            finally
            {
                if (messages.Count > 0)
                    response.Message = string.Format("Se actualizo los siguientes registros: {0} {1}", "\n\r", string.Join("\n\r", messages));
                if (File.Exists("C://HRR//temp//result.txt"))
                {
                    File.Delete("C://HRR//temp//result.txt");
                }

                FileStream fs = File.Create("C://HRR//temp//result.txt");
                UnicodeEncoding uniEncoding = new UnicodeEncoding();
                fs.Write(uniEncoding.GetBytes(response.Message), 0, uniEncoding.GetByteCount(response.Message));
                fs.Close();
            }
            return response;
        }

        public static void AddNitProcesed(List<NitProcesed> listNitsProcess, NitProcesed nitProcesed)
        {
            NitProcesed np = listNitsProcess.Where(npa => npa.Nit == nitProcesed.Nit).FirstOrDefault();
            if (np == null)
                listNitsProcess.Add(nitProcesed);
            else
            {
                if (nitProcesed.tieneHabilitados)
                    np.tieneHabilitados = nitProcesed.tieneHabilitados;
                if (nitProcesed.tieneEnProduccion)
                    np.tieneEnProduccion = nitProcesed.tieneEnProduccion;
                if (nitProcesed.tieneEnProceso)
                    np.tieneEnProceso = nitProcesed.tieneEnProceso;
                if (nitProcesed.FailsSetOfTests)
                    np.FailsSetOfTests = nitProcesed.FailsSetOfTests;
                if (nitProcesed.SoftwareEqualBase)
                    np.SoftwareEqualBase = nitProcesed.SoftwareEqualBase;

            }
        }
        public class RequestObject
        {
            [JsonProperty(PropertyName = "nits")]
            public string Nits { get; set; }

            [JsonProperty(PropertyName = "modeTest")]
            public string ModeTest { get; set; }
        }

        public class NitDatos
        {
            public string Nit { get; set; }
            public string SoftwareId { get; set; }
            public string BaseSoftwareId { get; set; }
            public int OperationModeId { get; set; }
            public int OtherDocElecContributorId { get; set; }
            public bool AzureProd { get; set; }
        }
        public class NitProcesed
        {
            public string Nit { get; set; }
            public bool tieneHabilitados { get; set; }
            public bool tieneEnProceso { get; set; }
            public bool tieneEnProduccion { get; set; }
            public bool FailsSetOfTests { get; set; }
            public bool SoftwareEqualBase { get; set; }
        }

        class SendToActivateContributorResponse
        {
            [JsonProperty(PropertyName = "success")]
            public bool Success { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }

            [JsonProperty(PropertyName = "detail")]
            public string Detail { get; set; }

            [JsonProperty(PropertyName = "trace")]
            public string Trace { get; set; }

            [JsonProperty(PropertyName = "testSetId")]
            public string TestSetId { get; set; }
        }

    }


}
