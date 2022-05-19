using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gosocket.Dian.Web.Models
{
    public class DocumentStatsViewModel
    {
        public DocumentStatsViewModel()
        {
            GlobalDailyTotals = new List<Tuple<DateTime, long, long, long, long>>();
            GlobalMonthlyTotals = new List<Tuple<DateTime, long, long, long, long>>();
           
        }

        public int PendingContributors { get; set; }
        public int RegisteredContributors { get; set; }
        public int EnabledContributors { get; set; }
        public string TotalContributors { get; set; }


        public string GlobalTotal { get; set; }
        public long GlobalTotalAccepted { get; set; }
        public long GlobalTotalNotification { get; set; }
        public long GlobalTotalRejected { get; set; }

        public int ContributorAcceptanceStatusId { get; set; }
        public DateTime? ContributorProductionDate { get; set; }
        public DateTime? ContributorHabilitationDate { get; set; }
        public string ContributorTestSetMessageInfo { get; set; }

        public List<Tuple<DateTime, long, long, long, long>> GlobalDailyTotals { get; set; }
        public List<Tuple<DateTime, long, long, long, long>> GlobalMonthlyTotals { get; set; }


        public string JsonDate { get; set; }
        public string JsonAll { get; set; }
        public string JsonSumAll { get; set; }
        public string JsonAccepted { get; set; }
        public string JsonNotification { get; set; }
        public string JsonRejected { get; set; }

        public string JsonMonthDate { get; set; }
        public string JsonMonthSumAll { get; set; }
        public string JsonMonthAll { get; set; }
        public string JsonMonthAccepted { get; set; }
        public string JsonMonthNotification { get; set; }
        public string JsonMonthRejected { get; set; }

        public int TotalDocumentsEmitted { get; set; }
        public int TotalDocumentsReceived { get; set; }

        public int ContributorTypeId { get; set; }
    }
}