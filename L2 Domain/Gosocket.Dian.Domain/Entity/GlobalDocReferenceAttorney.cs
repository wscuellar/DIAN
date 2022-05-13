using Microsoft.WindowsAzure.Storage.Table;

namespace Gosocket.Dian.Domain.Entity
{
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
        public string AttorneyType { get; set; }
        public string IssuerAttorneyName { get; set; }
        public string SenderName { get; set; }
        public string SerieAndNumber { get; set; }
        public string ResponseCodeListID { get; set; }
        public string SchemeID { get; set; }
        public string IssuerAttorneyPersonId { get; set; }
    }
}
