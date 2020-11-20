﻿using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gosocket.Dian.Web.Models
{
    public class ExternalUserViewModel
    {
        public ExternalUserViewModel()
        {
            IdentificationTypes = new List<IdentificationTypeListViewModel>();
            Roles = new List<IdentityUserRole>();
        }

        public string Id { get; set; }

        //[DataType(DataType.Text)]
        //[Display(Name = "Nombre de Usuario")]
        ////[Required(ErrorMessage = "{0} es requerido.")]
        //public string UserName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Nombres")]
        [Required(ErrorMessage = "{0} es requerido.")]
        public string Names { get; set; }

        [Display(Name = "Tipo de Identificación")]
        [Required(ErrorMessage = "{0} es requerido.")]
        public int IdentificationTypeId { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Número de documento")]
        [Required(ErrorMessage = "{0} es requerido.")]
        public string IdentificationId { get; set; }

        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "{0} es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "{0} es requerida")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        public byte Active { get; set; }
        public string UpdatedBy { get; set; }
        public string ActiveDescription { get; set; }
        public DateTime LastUpdated { get; set; }

        public List<IdentificationTypeListViewModel> IdentificationTypes { get; set; }
        
        public List<IdentityUserRole> Roles { get; set; }

    }
}