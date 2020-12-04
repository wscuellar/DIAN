//using Gosocket.Dian.Domain.Entity;
//using Gosocket.Dian.Infrastructure;
//using Gosocket.Dian.Services.Utils;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace Gosocket.Dian.TestProject.Export
//{
//    [TestClass]
//    public class ExportTest
//    {
//        private static readonly TableManager globalDocValidatorRuntimeTableManager = new TableManager("GlobalDocValidatorRuntime");
//        [TestMethod]
//        public void TestExportValidationExecutionTime()
//        {
//            var list = new List<ExecutionTimeValidation>();
//            var result = globalDocValidatorRuntimeTableManager.FindAll<GlobalDocValidatorRuntime>();
//            var items = result.GroupBy(r => r.PartitionKey);
//            foreach (var group in items)
//            {
//                var upload = group.FirstOrDefault(r => r.RowKey == "UPLOAD");
//                var end = group.FirstOrDefault(r => r.RowKey == "END");
//                if (upload != null && end != null)
//                {
//                    list.Add(new ExecutionTimeValidation
//                    {
//                        DocumentKey = end.PartitionKey,
//                        ExecutionTime = end.Timestamp.Subtract(upload.Timestamp).TotalSeconds,
//                        Date = end.Timestamp
//                    });
//                }

//            }

//            list = list.OrderByDescending(l => l.Date).ToList();
//            var csv = StringUtil.ToCSV(list);
//            File.WriteAllText($@"D:\ValidarDianXmls\validation_execution_time.csv", csv);
//        }
//    }
//}
