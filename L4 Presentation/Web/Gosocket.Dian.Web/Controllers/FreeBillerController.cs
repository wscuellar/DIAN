﻿using Gosocket.Dian.Web.Models.FreeBiller;
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
using Gosocket.Dian.Application.FreeBiller;
using Gosocket.Dian.Domain.Utils;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Common.Resources;
using System.Net;
using Gosocket.Dian.Domain.Sql.FreeBiller;
using Gosocket.Dian.Web.Common;
using System.Text.RegularExpressions;

namespace Gosocket.Dian.Web.Controllers
{
    public class FreeBillerController : Controller
    {

        #region variables privadas

        /// <summary>
        /// Servicio que contiene las operaciones de los usuarios de Entrega Factura.
        /// </summary>
        private readonly UserService userService = new UserService();

        /// <summary>
        /// Servicio para obtener la informacion de Tipos de documento.
        /// Tabla: IdentificationType.
        /// </summary>
        private readonly IdentificationTypeService identificationTypeService = new IdentificationTypeService();

        /// <summary>
        /// Servicio para obtener los perfiles de Facturador gratuito.
        /// Tabla: ProfilesFreeBiller.
        /// </summary>
        private readonly ProfileService profileService = new ProfileService();

        private readonly ClaimsDbService claimsDbService = new ClaimsDbService();

        /// <summary>
        /// Listas parametrica "estaticas" para no tener que consultar muchas veces la DB.
        /// Tipos de Documento.
        /// </summary>
        private static List<SelectListItem> staticTypeDoc { get; set; }

        /// <summary>
        /// Listas parametrica "estaticas" para no tener que consultar muchas veces la DB.
        /// Perfiles.
        /// </summary>
        private static List<SelectListItem> staticProfiles { get; set; }

        /// <summary>
        /// Identificador para poder guardar Claims con el Perfil del usuario.
        /// </summary>
        private const string CLAIMPROFILE = "PROFILE_FREEBILLER";

        /// <summary>
        /// Identificador para poder guardar el usuario con el rol indicado en la aplicación de Entrega Factura.
        /// </summary>
        private const string ROLEFREEBILLER = "UsuarioFacturadorGratuito";
        #endregion

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
        /// <summary>
        /// Cargue de información para editar la condición Activo o Inactivo de un Usuario.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> EditUserActive(string id, string activo)
        {
            var user = await userManager.FindByIdAsync(id);
            int dt = 0;
            if (activo == "true")
            {
                dt = 1;
            }
            if (user == null)
            {
                ViewBag.message = "No se encontro el ID";
                ResponseMessage resultx = new ResponseMessage()
                {
                    Message = "Not Found",
                    MessageType = "alert",
                    Code = 200
                };
                return Json(resultx, JsonRequestBehavior.AllowGet);
            }
            else
            {
                user.Active = Convert.ToByte(dt);
                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    ResponseMessage resultx = new ResponseMessage()
                    {
                        Message = "Estado actualizado correctamente",
                        MessageType = "alert",
                        Code = 200
                    };
                    return Json(resultx, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ResponseMessage resultx = new ResponseMessage()
                    {
                        Message = "¡Ocurrió un problema al cambiar de estado.!",
                        MessageType = "alert",
                        Code = 200
                    };
                    return Json(resultx, JsonRequestBehavior.AllowGet);
                }
            }

        }

        // GET: FreeBiller
        public ActionResult FreeBillerUser()
        {
            ViewBag.activo = false;
            UserFiltersFreeBillerModel model = new UserFiltersFreeBillerModel();
            staticTypeDoc = this.GetTypesDoc();
            staticProfiles = this.GetProfiles();

            model.DocTypes = staticTypeDoc;
            model.Profiles = staticProfiles;
            model.UserContainer = this.GetUsers(new UserFiltersFreeBillerModel() { ProfileId = 0, DocNumber = null, FullName = null, DocTypeId = 0, Page = 1, PageSize = 10 });
            return View(model);

        }

        [HttpPost]
        public ActionResult FreeBillerUser(UserFiltersFreeBillerModel model)
        {
            model.DocTypes = staticTypeDoc;
            model.Profiles = staticProfiles;
            model.UserContainer = GetUsers(model);
            return View(model);
        }

        /// <summary>
        /// Cargue de información para editar.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditOrViewFreeBillerUser(string id, bool isEdit = true)
        {
            var tdocs = GetTypesDoc();
            ViewBag.tdocs = tdocs.Select(p => new SelectListItem() { Value = p.Value.ToString(), Text = p.Text }).ToList<SelectListItem>();
            UserService user = new UserService();
            var data = user.Get(id);
            UserFreeBillerModel model = new UserFreeBillerModel();
            model.IsEdit = isEdit;
            model.Id = data.Id;
            model.Name = data.UserName;
            model.Email = data.Email;
            model.LastUpdate = data.LastUpdated;
            model.Profiles = this.GetProfiles();
            model.LastName =string.Empty;
            model.FullName = data.Name;
            model.NumberDoc = data.IdentificationId;
            model.ProfileId = Convert.ToInt32(data.Claims.First().ClaimValue);
            model.TypeDocId = Convert.ToString(data.IdentificationTypeId);
            model.IsActive = false;
            model.TypesDoc = this.GetTypesDoc();
            model.MenuOptionsByProfile = profileService.GetOptionsByProfile(model.ProfileId);
            model.Password = data.PasswordHash;
            return View(model);
        }

        #region EditFreeBillerUser

        /// <summary>
        /// Metodo asincrono para editar la información..
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> EditFreeBillerUser(UserFreeBillerModel model)
        {

            //Valida si el modelo trae errores
            StringBuilder errors = new StringBuilder();
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var item in allErrors)
                    errors.AppendLine(item.ErrorMessage);
                return Json(new ResponseMessage(errors.ToString(), TextResources.alertType, (int)HttpStatusCode.BadRequest), JsonRequestBehavior.AllowGet);
            }

            //validar si existe un Usuario con el id
            ApplicationUser user = await userManager.FindByIdAsync(model.Id);
            if (user == null)
                return Json(new ResponseMessage(TextResources.UserDoesntExist, TextResources.alertType, (int)HttpStatusCode.BadRequest), JsonRequestBehavior.AllowGet);

            // Actualiza los datos del usuario ingresados en el modelo
            user.Name = model.FullName;
            user.IdentificationTypeId = Convert.ToInt32(model.TypeDocId);
            user.IdentificationId = model.NumberDoc;
            user.Email = model.Email;
            user.UserName = model.Email;
            //user.PasswordHash = userManager.PasswordHasher.HashPassword(model.Password);
            IdentityResult identityResult = await userManager.UpdateAsync(user);
            if (identityResult.Succeeded)
            {
                // Actualiza el claim
                _ = userService.UpdateUserClaim(new ClaimsDb() { ClaimValue = model.ProfileId.ToString(), UserId = user.Id });

                // Actualiza perfil
                _ = userService.UserFreeBillerUpdate(new Domain.Sql.UsersFreeBillerProfile() { ProfileFreeBillerId = model.ProfileId, UserId = user.Id });

                SendMailEdit(model);
                ResponseMessage resultx = new ResponseMessage(TextResources.UserUpdatedSuccess, TextResources.alertType);
                resultx.RedirectTo = Url.Action("FreeBillerUser", "FreeBiller");
                return Json(resultx, JsonRequestBehavior.AllowGet);
            }

            foreach (var item in identityResult.Errors)
                errors.Append(item);
            return Json(new ResponseMessage(errors.ToString(), TextResources.alertType), JsonRequestBehavior.AllowGet);

        }

        #endregion

        public ActionResult CreateUser()
        {
            UserFreeBillerModel model = new UserFreeBillerModel();
            model.TypesDoc = this.GetTypesDoc();
            model.Profiles = this.GetProfiles();

            model.IsActive = true;

            if (model.IsActive)
            {
                ViewBag.activo = true;
            }
            model.MenuOptionsByProfile = profileService.GetOptionsByProfile(0);
            return View(model);
        }

        [HttpPost]
        public JsonResult CreateUser(UserFreeBillerModel model)
        {

            //Valida si el modelo trae errores
            StringBuilder errors = new StringBuilder();
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var item in allErrors)
                    errors.AppendLine(item.ErrorMessage);
                return Json(new ResponseMessage(errors.ToString(), TextResources.alertType, (int)HttpStatusCode.BadRequest), JsonRequestBehavior.AllowGet);
            }

            //identifica el usuario que esta crean los nuevos registros
            var uCompany = userService.Get(User.Identity.GetUserId());
            if (uCompany == null)
                return Json(new ResponseMessage(TextResources.UserWithOutCompany, TextResources.alertType, (int)HttpStatusCode.BadRequest), JsonRequestBehavior.AllowGet);

            //Crea nuevo registro para AspNetUser
            model.FullName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.FullName);
            model.Password = CreateStringPassword(model);
            var user = new ApplicationUser
            {
                CreatorNit = uCompany.Code,
                IdentificationTypeId = Convert.ToInt32(model.TypeDocId),
                IdentificationId = model.NumberDoc,
                Code = model.NumberDoc,
                Name = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                CreatedBy = User.Identity.GetUserId(),
                CreationDate = DateTime.Now,
                UpdatedBy = User.Identity.Name,
                LastUpdated = DateTime.Now,
                Active = 1
            };
            


            //validar si ya existe un Usuario con el tipo documento y documento suministrados en AspNetUser
            var vUserDB = userService.FindUserByIdentificationAndTypeId(string.Empty, user.IdentificationTypeId, model.NumberDoc);
            if (vUserDB != null)
                return Json(new ResponseMessage(TextResources.UserExistingDoc, TextResources.alertType, (int)HttpStatusCode.BadRequest), JsonRequestBehavior.AllowGet);

            //Crea el registro del nuevo usuario
            IdentityResult identification = userManager.Create(user, model.Password);

            //Si la creacion fue exitosa  
            if (identification.Succeeded)
            {
                model.Id = user.Id;
                //En el log de azure tabla globalogger hace los registros de acceso.
                userService.RegisterExternalUserTrazability(JsonConvert.SerializeObject(new ExternalUserViewModel()
                {
                    Id = user.Id,
                    IdentificationTypeId = Convert.ToInt32(model.TypeDocId),
                    IdentificationId = model.NumberDoc,
                    Names = model.FullName,
                    Email = model.Email,
                    CreatorNit = user.CreatorNit
                }), "Creación");

                // Revisar calse estatica para colocar el valor del nuevo rol.
                var resultRole = userManager.AddToRole(user.Id, ROLEFREEBILLER);
                if (!resultRole.Succeeded)
                {
                    userManager.Delete(user);
                    return Json(new ResponseMessage(TextResources.UserRoleFail, TextResources.alertType, (int)HttpStatusCode.BadRequest), JsonRequestBehavior.AllowGet);
                }

                // Claim para reconocer el perfi del nuevo usuario para el Facturador Gratuito.
                userManager.AddClaim(user.Id, new System.Security.Claims.Claim(CLAIMPROFILE, model.ProfileId.ToString()));

                //incluir el asociacion del registro. UserFreeBillerProfile
                _ = userService.UserFreeBillerUpdate(
                    new Domain.Sql.UsersFreeBillerProfile() 
                    { 
                        ProfileFreeBillerId = model.ProfileId, 
                        UserId = user.Id, 
                        CompanyCode = User.ContributorCode(),
                        CompanyIdentificationType = User.IdentificationTypeId()
                    });

                //Envio de notificacion por correo
                _ = SendMailCreate(model);

                //Aquí va el modal de ok.
                ResponseMessage resultx = new ResponseMessage(TextResources.UserCreatedSuccess, TextResources.alertType);
                resultx.RedirectTo = Url.Action("FreeBillerUser", "FreeBiller");
                return Json(resultx, JsonRequestBehavior.AllowGet);
            }

            foreach (var item in identification.Errors)
                errors.Append(item);

            return Json(new ResponseMessage(errors.ToString(), TextResources.alertType), JsonRequestBehavior.AllowGet);
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


        /// <summary>
        /// Enviar notificacion email para creacion de usuario externo.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool SendMailEdit(UserFreeBillerModel model)
        {
            var emailService = new Gosocket.Dian.Application.EmailService();
            StringBuilder message = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            message.Append("<span style='font-size:24px;'><b>Comunicación de servicio</b></span></br>");
            message.Append("</br> <span style='font-size:18px;'><b>Se ha realizado una actualizacion a sus datos de usuario</b></span></br>");
            message.AppendFormat("</br> Señor (a) usuario (a): {0}", model.FullName);
            
            message.Append("</br> <span style='font-size:10px;'>Te recordamos que esta dirección de correo electrónico es utilizada solamente con fines informativos. Por favor no respondas con consultas, ya que estas no podrán ser atendidas. Así mismo, los trámites y consultas en línea que ofrece la entidad se deben realizar únicamente a través del portal www.dian.gov.co</span>");

            //Nombre del documento, estado, observaciones
            dic.Add("##CONTENT##", message.ToString());

            emailService.SendEmail(model.Email, "DIAN - Edición de Usuario Registrado", dic);

            return true;
        }

        #region Carga de combos

        /// <summary>
        /// Método encargado de obtener los perfiles del Facturador Gratuito y asignarlos a 
        /// una lista para usarlos en las vistas.
        /// </summary>
        /// <returns>List<SelectListItem([Id],[Name])></returns>
        private List<SelectListItem> GetProfiles()
        {
            List<SelectListItem> selectTypesId = new List<SelectListItem>();
            var profiles = profileService.GetAll();
            if (profiles?.Count > 0)
            {
                profiles.Insert(0, new Domain.Sql.FreeBiller.Profile() { Id = 0, Name = "Seleccione..." });
                foreach (var item in profiles)
                {
                    selectTypesId.Add(
                        new SelectListItem
                        {
                            Value = item.Id.ToString(),
                            Text = item.Name
                        });
                }
            }

            return selectTypesId;
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
            var types = identificationTypeService.List().ToList();
            types.Insert(0, new Domain.IdentificationType() { Code = "0", Description = "Seleccione..." });

            if (types.Count > 0)
            {
                foreach (var item in types)
                {
                    selectTypesId.Add(
                        new SelectListItem
                        {
                            Value = item.Id.ToString(),
                            Text = item.Description
                        });
                }

            }

            return selectTypesId;
        }
        #endregion


        /// <summary>
        /// Obtiene todos los usuarios creados para el facturador gratuito.
        /// </summary>
        /// <returns>List<UserFreeBillerModel></returns>
        private UserFreeBillerContainerModel GetUsers(UserFiltersFreeBillerModel model)
        {
            string companyCode = User.ContributorCode();
            List<ApplicationUser> users = userService.UserFreeBillerProfile(t => (model.DocTypeId == 0 || t.IdentificationTypeId == model.DocTypeId)
                                              && (model.DocNumber == null || t.IdentificationId == model.DocNumber)
                                              && (model.FullName == null || t.Name.ToLower().Contains(model.FullName.ToLower()))
                                               , companyCode, model.ProfileId);

            List<ClaimsDb> userIdsFreeBiller = claimsDbService.GetUserIdsByClaimType(CLAIMPROFILE);
            var query = from item in users
                        join cl in userIdsFreeBiller on item.Id equals cl.UserId
                        join td in staticTypeDoc on item.IdentificationTypeId.ToString() equals td.Value
                        join pr in staticProfiles on cl.ClaimValue equals pr.Value
                        select new UserFreeBillerModel()
                        {
                            Id = item.Id,
                            FullName = item.Name,
                            DescriptionTypeDoc = td.Text,
                            DescriptionProfile = pr.Text,
                            NumberDoc = item.IdentificationId,
                            LastUpdate = item.LastUpdated,
                            IsActive = Convert.ToBoolean(item.Active)
                        };
            int totalCount = query.Count();
            int skip = (model.Page - 1) * model.PageSize;
            IQueryable<UserFreeBillerModel> sql = query.Skip(skip).Take(model.PageSize).AsQueryable();
            return new UserFreeBillerContainerModel()
            {
                TotalCount = totalCount,
                Users = sql.ToList()
            };
        }


        [HttpPost]
        public JsonResult GetMenuOptionsByProfile(int profileId)
        {
            List<MenuOptions> hierarchy = profileService.GetOptionsByProfile(profileId);
            return Json(hierarchy, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult GetIdsByProfile(int profileId)
        {
            List<MenuOptionsByProfiles> hierarchy = profileService.GetMenuOptionsByProfile(profileId);
            var ids = hierarchy.Select(t => t.MenuOptionId);
            return Json(ids, JsonRequestBehavior.AllowGet);
        }

        #region CreateStringPassword

        private string CreateStringPassword(UserFreeBillerModel model)
        {
            string result = model.FullName.Substring(1, 1).ToUpper();
            result =  $"{result}{model.Email.Split('@')[0]}{Guid.NewGuid().ToString("d").Substring(1, 4)}**";
            return result;
        }

        #endregion

    }

}