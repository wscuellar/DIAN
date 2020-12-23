using Microsoft.WindowsAzure.Storage.Table;
using System;

public class GlobalDocHolderExchange : TableEntity
{
    public GlobalDocHolderExchange() { }

    public GlobalDocHolderExchange(string pk, string rk) : base(pk, rk)
    {

    }

    public DateTime Timestamp { get; set; }
    public bool Active { get; set; }
    public string CorporateStockAmount { get; set; }
    public string GlobalDocumentId { get; set; }
    public string PartyLegalEntity { get; set; }
}


