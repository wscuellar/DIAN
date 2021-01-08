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
        private List<SelectListItem> staticTypeDoc { get; set; }

        /// <summary>
        /// Listas parametrica "estaticas" para no tener que consultar muchas veces la DB.
        /// Perfiles.
        /// </summary>
        private List<SelectListItem> staticProfiles { get; set; }

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
            this.staticTypeDoc = this.GetTypesDoc();
            this.staticProfiles = this.GetProfiles();
            
            model.DocTypes = this.staticTypeDoc;
            model.Profiles = this.staticProfiles;
            model.Users = this.GetUsers();
            foreach (var item in model.Users.ToList())
            {
                var activo = item.IsActive;
                ViewBag.activo = activo;
            }
            return View(model);
          
            return View();
        }
       
        [HttpPost]
        public ActionResult FreeBillerUser(UserFiltersFreeBillerModel model)
        {
            model.Profiles = this.staticProfiles;
            if (model.DocTypeId > default(int) && !string.IsNullOrWhiteSpace(model.DocNumber))
            {
               model.Users = this.GetUsersByDocument(model.DocTypeId, model.DocNumber);
            }

            return View(model);
        }

        /// <summary>
        /// Cargue de información para editar.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditFreeBillerUser(string id)
        {
            var tdocs = GetTypesDoc();
            var MenuProfiles = GetMenuProfile(id);
            //perfiles y permisos
            if (MenuProfiles != null)
            {
                ViewBag.valor = true;
                foreach (var item in MenuProfiles)
                {
                    for (int i = 0; i<= MenuProfiles.Count; i++)
                    {
                        if (item.Text == i.ToString())
                        {
                            if(i == 1)
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
            }else
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
            model.TypesDoc = this.staticTypeDoc;
            model.Profiles = this.staticProfiles;
            model.IsActive = true;

            if (model.IsActive)
            {
                ViewBag.activo = true;
            }
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
                Code = model.NumberDoc,
                Name = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                CreatedBy = User.Identity.GetUserId(),
                CreationDate = DateTime.Now,
                UpdatedBy = User.Identity.Name,
                LastUpdated = DateTime.Now,
                Active = 1
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
            var vUserDB = userService.FindUserByIdentificationAndTypeId(string.Empty, Convert.ToInt32(model.TypeDocId), model.NumberDoc);

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


                // Revisar calse estatica para colocar el valor del nuevo rol.
                var resultRole = userManager.AddToRole(user.Id, ROLEFREEBILLER);

                // Claim para reconocer el perfi del nuevo usuario para el Facturador Gratuito.
                userManager.AddClaim(user.Id, new System.Security.Claims.Claim(CLAIMPROFILE, model.ProfileId.ToString()));
                

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
                foreach (var item in users.Where(x => x.Id == id).ToList() )
                {
                    foreach(var item2 in userIdsFreeBiller.Where(x => x.UserId == item.Id).ToList())
                    {
                        listUsers.Add(new UserFreeBillerModel
                        {
                            Id = item2.ClaimValue
                        });
                    }
                    
                }
            }
            List <SelectListItem> selectProfiles = new List<SelectListItem>();
            var types = profileService.GetMenuOptions().ToList();
           

            if (types?.Count > 0)
            {
                foreach(var item in listUsers.ToList())
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
        private List<UserFreeBillerModel> GetUsers()
        {
            List<UserFreeBillerModel> listUsers = new List<UserFreeBillerModel>();
            List<ClaimsDb> userIdsFreeBiller = claimsDbService.GetUserIdsByClaimType(CLAIMPROFILE);

            var users = userService.GetUsers(userIdsFreeBiller.Select(u=> u.UserId).ToList());

            if (users != null)
            {
                foreach (var item in users)
                {
                    string perfilId = userIdsFreeBiller.FirstOrDefault(u => u.UserId == item.Id).ClaimValue;
                    listUsers.Add(new UserFreeBillerModel
                    {
                        Id = item.Id,
                        FullName = item.Name,
                        DescriptionTypeDoc = this.staticTypeDoc.FirstOrDefault(td => td.Value == item.IdentificationTypeId.ToString()).Text,
                        DescriptionProfile = this.staticProfiles.FirstOrDefault(td => td.Value == perfilId).Text,
                        NumberDoc = item.IdentificationId,
                        LastUpdate = item.LastUpdated,
                        IsActive = Convert.ToBoolean(item.Active)
                    });
                }

            }

            return listUsers;
        }

        /// <summary>
        /// Obtiene todos los usuarios creados para el facturador gratuito.
        /// </summary>
        /// <returns>List<UserFreeBillerModel></returns>
        private List<UserFreeBillerModel> GetUsersByDocument(int typeDocId, string numberDoc)
        {
            List<UserFreeBillerModel> listUsers = new List<UserFreeBillerModel>();
            List<ClaimsDb> userIdsFreeBiller = claimsDbService.GetUserIdsByClaimType(CLAIMPROFILE);

            var users = userService.GetUsers(userIdsFreeBiller.Select(u => u.UserId).ToList());

            if (users != null)
            {
                foreach (var item in users.Where(u=> u.IdentificationTypeId == typeDocId && u.IdentificationId == numberDoc))
                {
                    string perfilId = userIdsFreeBiller.FirstOrDefault(u => u.UserId == item.Id).ClaimValue;
                    listUsers.Add(new UserFreeBillerModel
                    {
                        Id = item.Id,
                        FullName = item.Name,
                        DescriptionTypeDoc = this.staticTypeDoc.FirstOrDefault(td => td.Value == item.IdentificationTypeId.ToString()).Text,
                        DescriptionProfile = this.staticProfiles.FirstOrDefault(td => td.Value == perfilId).Text,
                        NumberDoc = item.IdentificationId,
                        LastUpdate = item.LastUpdated,
                        IsActive = Convert.ToBoolean(item.Active)
                    });
                }

            }

            return listUsers;
        }

        /// <summary>
        /// Obtiene todos los usuarios creados para el facturador gratuito dependiendo del nombre.
        /// </summary>
        /// <returns>List<UserFreeBillerModel></returns>
        private List<UserFreeBillerModel> GetUsersByName(string name)
        {
            List<UserFreeBillerModel> listUsers = new List<UserFreeBillerModel>();
            List<ClaimsDb> userIdsFreeBiller = claimsDbService.GetUserIdsByClaimType(CLAIMPROFILE);

            var users = userService.GetUsers(userIdsFreeBiller.Select(u => u.UserId).ToList());

            if (users != null)
            {
                foreach (var item in users.Where(u => u.Name.ToLower().Contains(name.ToLower())))
                {
                    string perfilId = userIdsFreeBiller.FirstOrDefault(u => u.UserId == item.Id).ClaimValue;
                    listUsers.Add(new UserFreeBillerModel
                    {
                        Id = item.Id,
                        FullName = item.Name,
                        DescriptionTypeDoc = this.staticTypeDoc.FirstOrDefault(td => td.Value == item.IdentificationTypeId.ToString()).Text,
                        DescriptionProfile = this.staticProfiles.FirstOrDefault(td => td.Value == perfilId).Text,
                        NumberDoc = item.IdentificationId,
                        LastUpdate = item.LastUpdated,
                        IsActive = Convert.ToBoolean(item.Active)
                    });
                }

            }

            return listUsers;
        }

        /// <summary>
        /// Obtiene todos los usuarios creados para el facturador gratuito dependiendo del nombre.
        /// </summary>
        /// <returns>List<UserFreeBillerModel></returns>
        private List<UserFreeBillerModel> GetUsersByProfile(int profileId)
        {
            List<UserFreeBillerModel> listUsers = new List<UserFreeBillerModel>();
            List<ClaimsDb> userIdsFreeBiller = claimsDbService.GetUserIdsByClaimType(CLAIMPROFILE);

            var users = userService.GetUsers(userIdsFreeBiller.Where(u=> u.ClaimValue == profileId.ToString()).Select(u => u.UserId).ToList());

            if (users != null)
            {
                foreach (var item in users)
                {
                    string perfilId = userIdsFreeBiller.FirstOrDefault(u => u.UserId == item.Id).ClaimValue;
                    listUsers.Add(new UserFreeBillerModel
                    {
                        Id = item.Id,
                        FullName = item.Name,
                        DescriptionTypeDoc = this.staticTypeDoc.FirstOrDefault(td => td.Value == item.IdentificationTypeId.ToString()).Text,
                        DescriptionProfile = this.staticProfiles.FirstOrDefault(td => td.Value == perfilId).Text,
                        NumberDoc = item.IdentificationId,
                        LastUpdate = item.LastUpdated,
                        IsActive = Convert.ToBoolean(item.Active)
                    });
                }

            }

            return listUsers;
        }
    }
    
}