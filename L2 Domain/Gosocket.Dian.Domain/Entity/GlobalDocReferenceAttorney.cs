using Microsoft.WindowsAzure.Storage.Table;
using System;

public class GlobalDocReferenceAttorney : TableEntity
{
    public GlobalDocReferenceAttorney() { }

    public GlobalDocReferenceAttorney(string pk, string rk) : base(pk, rk)
    {

    }

    public bool Active { get; set; }
    public string Actor { get; set; }
    public string EffectiveDate { get; set; }
    public string EndDate { get; set; }
    public string FacultityCode { get; set; }
    public string IssuerAttorney { get; set; }
    public string SenderCode { get; set; }
    public string StartDate { get; set; }
    public string DocReferencedEndAthorney { get; set; }
}

