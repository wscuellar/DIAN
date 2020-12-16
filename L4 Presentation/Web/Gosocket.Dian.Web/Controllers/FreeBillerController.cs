using Gosocket.Dian.Web.Models.FreeBiller;
using Gosocket.Dian.Web.Utils;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Globalization;
using Gosocket.Dian.Web.Models;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System.Text;
using Gosocket.Dian.Application;

namespace Gosocket.Dian.Web.Controllers
{
    public class FreeBillerController : Controller
    {
        /// <summary>
        /// Servicio que contiene las operaciones de los usuarios de Entrega Factura.
        /// </summary>
        private readonly UserService userService = new UserService();

        /// <summary>
        /// Servicio para obtener la informacion de Tipos de documento.
        /// Tabla: IdentificationType.
        /// </summary>
        private readonly IdentificationTypeService identificationTypeService = new IdentificationTypeService();
        private ApplicationUserManager userManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                userManager = value;
            }
        }

        // GET: FreeBiller
        public ActionResult FreeBillerUser()
        {
            UserFiltersFreeBillerModel model = new UserFiltersFreeBillerModel();
            model.DocTypes = this.GetTypesDoc();
            model.Profiles = this.DataPerfiles();
            model.Users = this.Datausuarios(); //new List<UserFreeBillerModel>();
            return View(model);
        }

        [HttpPost]
        public ActionResult FreeBillerUser(UserFiltersFreeBillerModel model)
        {

            var algo = model;
            return RedirectToAction("FreeBillerUser");
        }


        public ActionResult EditFreeBillerUser(UserFreeBillerModel model)
        {
            return View();
        }

        public ActionResult CreateUser()
        {
            UserFreeBillerModel model = new UserFreeBillerModel();
            model.TypesDoc = this.GetTypesDoc();
            model.Profiles = this.DataPerfiles();

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateUser(UserFreeBillerModel model)
        {
            var uCompany = userService.Get(User.Identity.GetUserId());


            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var item in allErrors)
                    ModelState.AddModelError("", item.ErrorMessage);

                return View(model);
            }

            //if (uCompany == null)
            //{
            //    ModelState.AddModelError("", "El Usuario no tiene una Empresa Asociada!");
            //    return View(model);
            //}

            //if (string.IsNullOrEmpty(uCompany.Code))
            //{
            //    ModelState.AddModelError("", "El Usuario no tiene una Empresa Asociada!");
            //    return View(model);
            //}

            model.FullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.FullName);
            var user = new ApplicationUser
            {
                CreatorNit = uCompany?.Code ?? "999999999",
                IdentificationTypeId = Convert.ToInt32(model.TypeDocId),
                IdentificationId = model.NumberDoc,
                Name = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                CreatedBy = User.Identity.GetUserId(),
                CreationDate = DateTime.Now,
                UpdatedBy = User.Identity.Name,
                LastUpdated = DateTime.Now,
                //PasswordHash = UserManager.PasswordHasher.HashPassword(model.Email.Split('@')[0])
            };
            model.Password = userManager.PasswordHasher.HashPassword(model.Email.Split('@')[0]);

            if (!ModelState.IsValid)
            {
                foreach (var item in ModelState.Values.SelectMany(v => v.Errors))
                    ModelState.AddModelError("", item.ErrorMessage);
            }

            if (Convert.ToInt32(model.TypeDocId) <= 0)
            {
                ModelState.AddModelError("", "Por favor seleccione el Tipo de Documento");

                return View(model);
            }

            //validar si ya existe un Usuario con el tipo documento y documento suministrados
            var vUserDB = userService.FindUserByIdentificationAndTypeId(Convert.ToInt32(model.TypeDocId), model.NumberDoc);

            if (vUserDB != null)
            {
                ModelState.AddModelError("", "Ya existe un Usuario con el Tipo de Documento y Documento suministrados");
                return View(model);
            }

            IdentityResult result = userManager.Create(user, model.Password);

            if (result.Succeeded)
            {
                model.Id = user.Id;
                ViewBag.messageAction = "Usuario Registrado exitosamente!";

                userService.RegisterExternalUserTrazability(JsonConvert.SerializeObject(new ExternalUserViewModel()
                {
                    Id = user.Id,
                    IdentificationTypeId = Convert.ToInt32(model.TypeDocId),
                    IdentificationId = model.NumberDoc,
                    Names = model.FullName,
                    Email = model.Email,
                    CreatorNit = user.CreatorNit
                }), "Creación");

                //var resultUser = userManager.AddToRoleAsync(user.Id, Roles.UsuarioExterno);

                // Revisar calse estatica para colocar el valor del nuevo rol.
                var resultRole = userManager.AddToRole(user.Id, "UsuarioFacturadorGratuito");


                //Envio de notificacion por correo
                var envio = SendMailCreate(model);

                if (!resultRole.Succeeded)
                {
                    ModelState.AddModelError("", "El Usario no puedo ser asignado al role 'Usuario Facturador Gratuito'");

                    userManager.Delete(user);

                    return View(model);
                }

                //var affected = _permisionService.AddOrUpdate(permissions);

                //userService.RegisterExternalUserTrazability(JsonConvert.SerializeObject(new ExternalUserViewModel()
                //{
                //    IdentificationTypeId = model.IdentificationTypeId,
                //    IdentificationId = model.IdentificationId,
                //    Names = model.Names,
                //    Email = model.Email
                //}) + ", permisos: " + JsonConvert.SerializeObject(permissions), "Creación de Permisos");

                //Aquí va el modal de ok.
                return RedirectToAction("FreeBillerUser");
            }
            else
            {
                ViewBag.messageAction = "No se pudo Registrar el Usuario!";

                if (!result.Succeeded)
                {
                    foreach (var item in result.Errors)
                        ModelState.AddModelError(string.Empty, item);

                    foreach (var item in ModelState)
                        if (item.Key.Contains("Code"))
                            item.Value.Errors.Clear();

                    return View(model);
                }

            }

            return View(model);
        }


        /// <summary>
        /// Enviar notificacion email para creacion de usuario externo.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool SendMailCreate(UserFreeBillerModel model)
        {
            var emailService = new Gosocket.Dian.Application.EmailService();
            StringBuilder message = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            message.Append("<span style='font-size:24px;'><b>Comunicación de servicio</b></span></br>");
            message.Append("</br> <span style='font-size:18px;'><b>Se ha generado una clave de acceso al Catalogo de DIAN</b></span></br>");
            message.AppendFormat("</br> Señor (a) usuario (a): {0}", model.FullName);
            message.Append("</br> A continuación, se entrega la clave para realizar tramites y gestión de solicitudes recepción documentos electrónicos.");
            message.AppendFormat("</br> Clave de acceso: {0}", model.Password);

            message.Append("</br> <span style='font-size:10px;'>Te recordamos que esta dirección de correo electrónico es utilizada solamente con fines informativos. Por favor no respondas con consultas, ya que estas no podrán ser atendidas. Así mismo, los trámites y consultas en línea que ofrece la entidad se deben realizar únicamente a través del portal www.dian.gov.co</span>");

            //Nombre del documento, estado, observaciones
            dic.Add("##CONTENT##", message.ToString());

            emailService.SendEmail(model.Email, "DIAN - Creacion de Usuario Registrado", dic);

            return true;
        }


        #region data test
        private List<SelectListItem> DataPerfiles()
        {
            return new List<SelectListItem> {
                new SelectListItem{ Value="1", Text= "Administrador (TODOS)" },
                new SelectListItem{ Value="2", Text= "Contador" },
                new SelectListItem{ Value="3", Text= "Facturador" },
                new SelectListItem{ Value="4", Text= "Fiscal" }
            };
        }

        /// <summary>
        /// Método encargado de obtener los tipos de documentos y asignarlos a una lista para 
        /// usarlos en las vistas.
        /// </summary>
        /// <returns>
        /// List<SelectListItem([Code],[Description])>
        /// </returns>
        private List<SelectListItem> GetTypesDoc()
        {
            List<SelectListItem> selectTypesId = new List<SelectListItem>();
            var types = identificationTypeService.List()?.ToList();

            if (types?.Count > 0)
            {
                foreach (var item in types)
                {
                    selectTypesId.Add(
                        new SelectListItem
                        {
                            Value = item.Code,
                            Text = item.Description
                        });
                }

            }

            return selectTypesId;
        }

        private List<UserFreeBillerModel> Datausuarios()
        {
            return new List<UserFreeBillerModel>{
                new UserFreeBillerModel{
                    Id = new Guid().ToString(),
                    FullName= "Pepito perez",
                    DescriptionTypeDoc = "Cedula de ciudadanía",
                    DescriptionProfile = "Facturador",
                    NumberDoc = "1000223674",
                    //LastUpdate = DateTime.Now,
                    IsActive = true
                },
                new UserFreeBillerModel{
                    Id = new Guid().ToString(),
                    FullName= "lala lolo",
                    DescriptionTypeDoc = "Cedula de ciudadanía",
                    DescriptionProfile = "Fiscal",
                    NumberDoc = "45722258",
                    LastUpdate = DateTime.Now,
                    IsActive = false
                }
            };
        }
        #endregion
    }
}