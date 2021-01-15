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
using Gosocket.Dian.Application.FreeBiller;
using Gosocket.Dian.Domain.Utils;
using System.Threading.Tasks;
using Gosocket.Dian.Domain.Entity;
using Gosocket.Dian.Common.Resources;
using System.Net;

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
                return View("Not found");
            }
            else
            {
                user.Active = Convert.ToByte(dt);

                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("FreeBillerUser");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.ToString());
                }
                return RedirectToAction("FreeBillerUser");
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
            model.Users = this.GetUsers(new UserFiltersFreeBillerModel() { ProfileId = 0, DocNumber = null, FullName= null, DocTypeId = 0 });  
            foreach (var item in model.Users.ToList())
            {
                var activo = item.IsActive;
                ViewBag.activo = activo;
            }
            return View(model);

        }

        [HttpPost]
        public ActionResult FreeBillerUser(UserFiltersFreeBillerModel model)
        {
            model.DocTypes = staticTypeDoc;
            model.Profiles = staticProfiles;
            model.Users = GetUsers(model);
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
            var MenuProfiles = GetMenuProfile(id);
            //perfiles y permisos
            if (MenuProfiles != null)
            {
                ViewBag.valor = true;
                foreach (var item in MenuProfiles)
                {
                    for (int i = 0; i <= MenuProfiles.Count; i++)
                    {
                        if (item.Text == i.ToString())
                        {
                            if (i == 1)
                            {
                                ViewBag.menu1 = item.Text;
                            }
                            if (i == 2)
                            {
                                ViewBag.menu2 = item.Text;
                            }
                            if (i == 3)
                            {
                                ViewBag.menu3 = item.Text;
                            }
                            if (i == 4)
                            {
                                ViewBag.menu4 = item.Text;
                            }
                            if (i == 5)
                            {
                                ViewBag.menu5 = item.Text;
                            }
                            if (i == 6)
                            {
                                ViewBag.menu6 = item.Text;
                            }
                            if (i == 7)
                            {
                                ViewBag.menu7 = item.Text;
                            }
                            if (i == 8)
                            {
                                ViewBag.menu8 = item.Text;
                            }
                            if (i == 9)
                            {
                                ViewBag.menu9 = item.Text;
                            }
                            if (i == 10)
                            {
                                ViewBag.menu10 = item.Text;
                            }
                            if (i == 11)
                            {
                                ViewBag.menu11 = item.Text;
                            }
                            if (i == 12)
                            {
                                ViewBag.menu12 = item.Text;
                            }
                            if (i == 13)
                            {
                                ViewBag.menu13 = item.Text;
                            }
                            if (i == 14)
                            {
                                ViewBag.menu14 = item.Text;
                            }
                            if (i == 15)
                            {
                                ViewBag.menu15 = item.Text;
                            }
                            if (i == 16)
                            {
                                ViewBag.menu16 = item.Text;
                            }
                            if (i == 17)
                            {
                                ViewBag.menu17 = item.Text;
                            }
                            if (i == 18)
                            {
                                ViewBag.menu18 = item.Text;
                            }
                            if (i == 19)
                            {
                                ViewBag.menu19 = item.Text;
                            }
                            if (i == 20)
                            {
                                ViewBag.menu20 = item.Text;
                            }
                            if (i == 21)
                            {
                                ViewBag.menu21 = item.Text;
                            }
                            if (i == 22)
                            {
                                ViewBag.menu22 = item.Text;
                            }
                            if (i == 23)
                            {
                                ViewBag.menu23 = item.Text;
                            }
                            if (i == 24)
                            {
                                ViewBag.menu24 = item.Text;
                            }
                            if (i == 25)
                            {
                                ViewBag.menu25 = item.Text;
                            }
                            if (i == 26)
                            {
                                ViewBag.menu26 = item.Text;
                            }
                            if (i == 27)
                            {
                                ViewBag.menu27 = item.Text;
                            }
                            if (i == 28)
                            {
                                ViewBag.menu28 = item.Text;
                            }
                            if (i == 29)
                            {
                                ViewBag.menu29 = item.Text;
                            }
                            if (i == 30)
                            {
                                ViewBag.menu30 = item.Text;
                            }
                            if (i == 31)
                            {
                                ViewBag.menu31 = item.Text;
                            }
                            if (i == 32)
                            {
                                ViewBag.menu31 = item.Text;
                            }
                        }
                    }


                }

            }


            //dropdown
            ViewBag.tdocs = tdocs.Select(p => new SelectListItem() { Value = p.Value.ToString(), Text = p.Text }).ToList<SelectListItem>();
            UserService user = new UserService();
            var data = user.Get(id);
            UserFreeBillerModel model = new UserFreeBillerModel();
            model.IsEdit = isEdit == false ? false : true;
            model.Id = data.Id;
            model.Name = data.UserName;
            model.Email = data.Email;
            model.LastUpdate = data.LastUpdated;
            model.Profiles = this.GetProfiles();
            model.LastName = "Perez";
            model.FullName = data.Name;
            model.NumberDoc = data.IdentificationId;
            model.ProfileId = 1;
            model.TypeDocId = Convert.ToString(data.IdentificationTypeId);
            model.IsActive = false;
            return View(model);
        }

        /// <summary>
        /// Metodo asincrono para editar la información..
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> EditFreeBillerUser(UserFreeBillerModel model)
        {

            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                ViewBag.message = "No se encontro el ID";
                return View("Not found");
            }
            else
            {
                user.Name = model.FullName;
                user.IdentificationTypeId = Convert.ToInt32(model.TypeDocId);
                user.IdentificationId = model.NumberDoc;
                user.Email = model.Email;
                user.PasswordHash = model.Password;
                var result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    //Envio de notificacion por correo
                    SendMailCreate(model);
                    return RedirectToAction("FreeBillerUser");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.ToString());
                }
                return View(model);
            }
        }

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
            model.Password = userManager.PasswordHasher.HashPassword(model.Email.Split('@')[0]);


            //validar si ya existe un Usuario con el tipo documento y documento suministrados en AspNetUser
            var vUserDB = userService.FindUserByIdentificationAndTypeId(string.Empty, user.IdentificationTypeId, model.NumberDoc);
            if (vUserDB != null)
                return Json(new ResponseMessage(TextResources.UserExistingDoc, TextResources.alertType,(int)HttpStatusCode.BadRequest), JsonRequestBehavior.AllowGet);

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
                _ = userService.UserFreeBillerUpdate(new Domain.Sql.UsersFreeBillerProfile() { ProfileFreeBillerId = model.ProfileId, UserId = user.Id });

                //Envio de notificacion por correo
                _ = SendMailCreate(model);

                //Aquí va el modal de ok.
                ResponseMessage resultx = new ResponseMessage(TextResources.UserCreatedSuccess, TextResources.alertType);
                resultx.RedirectTo = Url.Action("FreeBillerUser", "FreeBillerController");
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
                            Value = item.Code,
                            Text = item.Description
                        });
                }

            }

            return selectTypesId;
        } 
        #endregion

        /// <summary>
        /// Método encargado de obtener todos los perfiles para los usuarios del Facturador Gratuito.
        /// </summary>
        /// <returns>List<SelectListItem></returns>
        private List<SelectListItem> GetMenuProfile(string id)
        {
            List<UserFreeBillerModel> listUsers = new List<UserFreeBillerModel>();
            List<ClaimsDb> userIdsFreeBiller = claimsDbService.GetUserIdsByClaimType(CLAIMPROFILE);

            var users = userService.GetUsers(userIdsFreeBiller.Select(u => u.UserId).ToList());

            if (users != null)
            {
                foreach (var item in users.Where(x => x.Id == id).ToList())
                {
                    foreach (var item2 in userIdsFreeBiller.Where(x => x.UserId == item.Id).ToList())
                    {
                        listUsers.Add(new UserFreeBillerModel
                        {
                            Id = item2.ClaimValue
                        });
                    }

                }
            }
            List<SelectListItem> selectProfiles = new List<SelectListItem>();
            var types = profileService.GetMenuOptions().ToList();


            if (types?.Count > 0)
            {
                foreach (var item in listUsers.ToList())
                {
                    var OptProf = profileService.GetMenuOptionsByProfile().Where(x => x.ProfileId == Convert.ToInt32(item.Id)).ToList();
                    foreach (var item1 in OptProf.ToList())
                    {
                        selectProfiles.Add(
                            new SelectListItem
                            {
                                Value = item1.ProfileId.ToString(),
                                Text = item1.MenuOptionId.ToString()
                            });
                    }
                }


            }

            return selectProfiles;
        }

        /// <summary>
        /// Obtiene todos los usuarios creados para el facturador gratuito.
        /// </summary>
        /// <returns>List<UserFreeBillerModel></returns>
        private List<UserFreeBillerModel> GetUsers(UserFiltersFreeBillerModel model)
        {
            List<ApplicationUser> users = userService.UserFreeBillerProfile(t => (model.DocTypeId == 0 || t.IdentificationTypeId == model.DocTypeId)
                                              && (model.DocNumber == null || t.IdentificationId == model.DocNumber)
                                              && (model.FullName == null || t.Name.ToLower().Contains(model.FullName.ToLower()))
                                               , model.ProfileId);

            List<ClaimsDb> userIdsFreeBiller = claimsDbService.GetUserIdsByClaimType(CLAIMPROFILE);
            return (from item in users
                    join cl in userIdsFreeBiller on item.Id equals cl.UserId
                    select new UserFreeBillerModel()
                    {
                        Id = item.Id,
                        FullName = item.Name,
                        DescriptionTypeDoc = staticTypeDoc.FirstOrDefault(td => td.Value == item.IdentificationTypeId.ToString()).Text,
                        DescriptionProfile = staticProfiles.FirstOrDefault(td => td.Value == cl.ClaimValue).Text,
                        NumberDoc = item.IdentificationId,
                        LastUpdate = item.LastUpdated,
                        IsActive = Convert.ToBoolean(item.Active)
                    }).ToList();
        }


    }

}