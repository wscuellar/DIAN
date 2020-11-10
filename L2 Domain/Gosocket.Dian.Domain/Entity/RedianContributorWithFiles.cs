using System;
using System.Collections.Generic;

namespace Gosocket.Dian.Domain.Entity
{

    public class AdminRadianFilter
    {

        public int Id { get; set; }

        public string Code { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public int Type { get; set; }

        public string RadianState { get; set; }

        public int Page { get; set; }

        public int Length { get; set; }


    }


    public class RadianAdmin
    {
        public List<RedianContributorWithTypes> contributors { get; set; }

        public List<RadianContributorType> Types { get; set; }

    }

    public class RedianContributorWithTypes
    {

        public int Id { get; set; }

        public string Code { get; set; }

        public string TradeName { get; set; }

        public string BusinessName { get; set; }

        public string AcceptanceStatusName { get; set; }


    }
}
