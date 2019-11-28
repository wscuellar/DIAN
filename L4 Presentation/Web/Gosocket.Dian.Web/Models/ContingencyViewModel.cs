using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Gosocket.Dian.Web.Models
{
    public class ContingencyTableViewModel
    {
        public ContingencyTableViewModel()
        {
            Page = 0;
            Length = 10;
            StartDateString = new DateTime(2019, 05, 01).ToString("dd-MM-yyyy");
            EndDateString = DateTime.UtcNow.ToString("dd-MM-yyyy");
            Contingencies = new List<ContingencyViewModel>();
        }
        public int Page { get; set; }
        public int Length { get; set; }

        [Display(Name = "Fecha inicio de contingencia")]
        [Required(ErrorMessage = "Fecha inicio de contingencia es requerida.")]
        public string StartDateString { get; set; }
        [Display(Name = "Fecha final de contingencia")]
        [Required(ErrorMessage = "Fecha final de contingencia es requerida.")]
        public string EndDateString { get; set; }

        public List<ContingencyViewModel> Contingencies { get; set; }

    }

    public class ContingencyViewModel
    {
        public ContingencyViewModel()
        {
            StartDateString = DateTime.UtcNow.Date.Add(new TimeSpan(0, 0, 0)).ToString("dd-MM-yyyy HH:mm");
            EndDateString = DateTime.UtcNow.Date.Add(new TimeSpan(23, 59, 0)).ToString("dd-MM-yyyy HH:mm");
        }

        public bool Active { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        [Display(Name = "Fecha inicio de contingencia")]
        [Required(ErrorMessage = "Fecha inicio de contingencia es requerida.")]
        public string StartDateString { get; set; }
        [Display(Name = "Fecha final de contingencia")]
        [Required(ErrorMessage = "Fecha final de contingencia es requerida.")]
        public string EndDateString { get; set; }

        [Display(Name = "Motivo de contingencia")]
        [Required(ErrorMessage = "Motivo de contingencia es requerido.")]
        public string Reason { get; set; }

        public long StartDateNumber { get; set; }
        public long EndDateNumber { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool Deleted { get; set; }
        public DateTime DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }

        public static void SetDateNumber(ref ContingencyViewModel model)
        {
            model.StartDateString = model.StartDateString.Replace(" ", "T");
            model.EndDateString = model.EndDateString.Replace(" ", "T");
            model.StartDateNumber = long.Parse(DateTime.ParseExact(model.StartDateString, "dd-MM-yyyyTHH:mm", CultureInfo.InvariantCulture).ToString("yyyyMMddHHmmss"));
            model.EndDateNumber = long.Parse(DateTime.ParseExact(model.EndDateString, "dd-MM-yyyyTHH:mm", CultureInfo.InvariantCulture).ToString("yyyyMMddHHmmss"));
        }

        public static bool IsValidDate(string date)
        {
            try
            {
                date = date.Replace(" ", "T");
                if (!date.Contains("T")) date = $"{date}T00:00";
                DateTime.ParseExact(date, "dd-MM-yyyyTHH:mm", CultureInfo.InvariantCulture).ToString("yyyyMMddHHmm");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}