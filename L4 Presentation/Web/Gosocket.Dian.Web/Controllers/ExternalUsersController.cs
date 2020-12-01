﻿using Gosocket.Dian.Application;
using Gosocket.Dian.Domain;
using Gosocket.Dian.Domain.Sql;
using Gosocket.Dian.Interfaces;
using Gosocket.Dian.Interfaces.Services;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.Role;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.Controllers
{
    /// <summary>
    /// Yo como Representante legal 
    /// Quiero configurar usuarios
    /// Para que puedan ingresar al catalogo de validación sin necesidad(Facturando electrónicamente) de usar token
    /// </summary>
    [AllowAnonymous]
    //[CustomRoleAuthorization(CustomRoles = "Administrador, Super, UsuarioExterno")]
    public class ExternalUsersController : Controller
    {
        ApplicationDbContext _context;

        private UserService userService = new UserService();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private readonly IPermissionService _permisionService;
        private readonly IContributorService _contributorService;
        private IdentificationTypeService identificationTypeService = new IdentificationTypeService();

        public ExternalUsersController(IPermissionService permisionService, IContributorService contributorService)
        {
            _context = new ApplicationDbContext();
            _permisionService = permisionService;
            _contributorService = contributorService;
        }

        // GET: ExternalUsers
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult AddUser(string id = "")
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.ExternalUsersCreate;

            ViewBag.txtAccion = "Guardar";

            if (!string.IsNullOrEmpty(id))
                ViewBag.txtAccion = "Actualizar";

            var uCompany = userService.Get(User.Identity.GetUserId());

            this.LoadViewBags();

            if (uCompany == null)
            {
                ModelState.AddModelError("", "El Usuario no tiene una Empresa Asociada!");
                return View();
            }

            if (string.IsNullOrEmpty(uCompany.Code))
            {
                ModelState.AddModelError("", "El Usuario no tiene una Empresa Asociada!");
                return View();
            }

            this.LoadExternalUsersViewBags(uCompany.Code);

            ExternalUserViewModel model = null;
            ViewBag.PermissionList = null;

            if (!string.IsNullOrEmpty(id))
            {
                List<Permission> pe = _permisionService.GetPermissionsByUser(id);
                if (pe != null)
                {
                    ViewBag.PermissionList = pe.Where(p => p.UserId.Equals(id)).ToList().Select(s =>
                    new PermissionViewModel
                    {
                        Id = s.Id,
                        UserId = id,
                        MenuId = s.MenuId,
                        SubMenuId = s.SubMenuId,
                        State = s.State,
                        CreatedBy = s.CreatedBy,
                        UpdatedBy = s.UpdatedBy
                    }).ToList();
                }

                var userBD = UserManager.FindById(id);

                if (userBD != null)
                {
                    model = new ExternalUserViewModel()
                    {
                        Id = userBD.Id,
                        IdentificationTypeId = userBD.IdentificationTypeId,
                        IdentificationId = userBD.IdentificationId,
                        Names = userBD.Name,
                        Email = userBD.Email,
                        //Roles = userBD.Roles.ToList(),
                        Active = userBD.Active,
                        IdentificationTypes = identificationTypeService.List()
                        .Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList()
                    };
                }
                else
                {
                    model = new ExternalUserViewModel()
                    {
                        IdentificationTypes = identificationTypeService.List()
                        .Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                    };
                }

            }
            else
            {
                model = new ExternalUserViewModel()
                {
                    IdentificationTypes = identificationTypeService.List()
                        .Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList(),
                };
            }

            if (model.Active == 1)
                ViewBag.txtActive = "Desactivar";
            else
                ViewBag.txtActive = "Activar";

            return View(model);
        }

        /// <summary>
        /// Moock del menu de la aplicación
        /// </summary>
        /// <returns></returns>
        private List<MenuViewModel> MenuApp()
        {
            return new List<MenuViewModel>()
            {
                new MenuViewModel() { Id=1, Name= "Tablero", Title="Dashboard" },
                new MenuViewModel() { Id=2, Name= "Vallidador", Title="Validator", Options =  new List<SubMenuViewModel>()
                        {
                        new SubMenuViewModel() { MenuId = 2, Id=1, Title="Reglas", Name = "Reglas" },
                        new SubMenuViewModel() { MenuId = 2, Id=2, Title="Configuraciones", Name = "Configuraciones" },
                        new SubMenuViewModel() { MenuId = 2, Id=3, Title="Validar Documento", Name = "Validar Documento" },
                        new SubMenuViewModel() { MenuId = 2, Id=4, Title="Consultar Validación", Name = "Consultar Validación" }
                        }
                    },
                new MenuViewModel() { Id=3, Name= "Documentos", Title="Document", Options = new List<SubMenuViewModel>()
                    {
                        new SubMenuViewModel() { MenuId = 3, Id=5, Title="Consultar", Name = "Consultar" },
                        new SubMenuViewModel() { MenuId = 3, Id=6, Title="Rango de Númeración", Name = "Rango de Númeración" },
                        new SubMenuViewModel() { MenuId = 3, Id=7, Title="Exportar", Name = "Exportar" }
                    }
                },
                new MenuViewModel() { Id=4, Name= "Participantes", Title="Participants", Options = new List<SubMenuViewModel>(){
                        new SubMenuViewModel() { MenuId = 4, Id=8, Name="Proveedores Tecnológicos", Title="Proveedores Tecnológicos" },
                        new SubMenuViewModel() { MenuId = 4, Id=9, Name="Proveedores Autorizados", Title="Proveedores Autorizados" },
                        new SubMenuViewModel() { MenuId = 4, Id=10, Name="Facturadores Electrónicos", Title="Facturadores Electrónicos" },
                        new SubMenuViewModel() { MenuId = 4, Id=11, Name="Representantes Legales", Title="Representantes Legales" },
                        new SubMenuViewModel() { MenuId = 4, Id=12, Name="Softwares", Title="Softwares" },
                        new SubMenuViewModel() { MenuId = 4, Id=13, Name="Contribuyentes", Title="Contribuyentes" },
                        new SubMenuViewModel() { MenuId = 4, Id=14, Name="Gestionar Solicitudes Recepción por Lote - Asíncrono", Title="Gestionar Solicitudes Recepción por Lote - Asíncrono" }
                    }
                },
                new MenuViewModel() { Id=5, Name= "Configuración", Title="Settings", Options = new List<SubMenuViewModel>(){
                        new SubMenuViewModel() { MenuId = 5, Id=15, Name="Tipos de Ficheros", Title="Tipos de Ficheros" },
                        new SubMenuViewModel() { MenuId = 5, Id=16, Name="Set de Pruebas", Title="Set de Pruebas" },
                        new SubMenuViewModel() { MenuId = 5, Id=17, Name="Administrar Contingencias", Title="Administrar Contingencias" },
                        new SubMenuViewModel() { MenuId = 5, Id=18, Name="Usuarios", Title="Usuarios" }
                    }
                },
                new MenuViewModel() { Id=6, Name= "Business Intelligence", Title="Business Intelligence"},
                new MenuViewModel() { Id=7, Name= "Facturador Gratuito", Title="Facturador Gratuito"},
            };
        }

        /// <summary>
        /// Listas para llenar el Modelo de la Vista
        /// </summary>
        /// <param name="nit">Nit de la empresa del Representante Legal o Nit de la persona Natural registrada en el Rut</param>
        private void LoadExternalUsersViewBags(string nit)
        {
            //ViewBag.ExternalUsersList = _context.Users.Where(u => u.Roles.Any(r => r.RoleId == role.Id) && u.Code == nit).ToList()
            ViewBag.ExternalUsersList = _context.Users.Where(u => u.CreatorNit == nit).ToList()
                .Select(u =>
                new ExternalUserViewModel
                {
                    Id = u.Id,
                    IdentificationTypeId = u.IdentificationTypeId,
                    IdentificationId = u.IdentificationId,
                    Names = u.Name,
                    Email = u.Email,
                    //Roles = u.Roles.ToList(),
                    Active = u.Active,
                    IdentificationTypes = identificationTypeService.List()
                        .Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList()
                }).ToList();
        }

        public void LoadViewBags()
        {
            ViewBag.IdentificationTypesList = identificationTypeService.List()
                        .Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList();

            //var roles = new SelectList(_context.Roles.Where(u => u.Name.Contains("UsuarioExterno"))
            //                                .ToList(), "Id", "Name");
            var role = _context.Roles.FirstOrDefault(u => u.Name.Contains(Roles.UsuarioExterno));

            //var userExt2 = _context.Users.Where(u => u.Roles.Any(r => r.RoleId == role.Id)).ToList();

            //ViewBag.Menu = this.MenuApp();
            ViewBag.Menu = _permisionService.GetAppMenu().Select(m =>
                new MenuViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Title = m.Title,
                    Description = m.Description,
                    Options = _permisionService.GetSubMenusByMenuId(m.Id).Select(s =>
                        new SubMenuViewModel()
                        {
                            Id = s.Id,
                            MenuId = m.Id,
                            Name = s.Name,
                            Title = s.Title,
                            Description = s.Description
                        }).ToList()
                }).ToList();

        }

        [HttpPost]
        public async Task<ActionResult> AddUser(ExternalUserViewModel model, FormCollection fc)
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.ExternalUsersCreate;
            ViewBag.txtAccion = "Guardar";

            if (!string.IsNullOrEmpty(model.Id))
                ViewBag.txtAccion = "Actualizar";

            this.LoadViewBags();

            var uCompany = userService.Get(User.Identity.GetUserId());

            if (uCompany == null)
            {
                ModelState.AddModelError("", "El Usuario no tiene una Empresa Asociada!");
                return View();
            }

            if (string.IsNullOrEmpty(uCompany.Code))
            {
                ModelState.AddModelError("", "El Usuario no tiene una Empresa Asociada!");
                return View();
            }

            this.LoadExternalUsersViewBags(uCompany.Code);

            var user = new ApplicationUser
            {
                CreatorNit = uCompany.Code,
                IdentificationTypeId = model.IdentificationTypeId,
                IdentificationId = model.IdentificationId,
                Name = model.Names,
                Email = model.Email,
                UserName = model.Email,
                CreatedBy = User.Identity.GetUserId(),
                CreationDate = DateTime.Now,
                UpdatedBy = User.Identity.Name,
                LastUpdated = DateTime.Now,
                //PasswordHash = UserManager.PasswordHasher.HashPassword(model.Email.Split('@')[0])
            };
            model.Password = UserManager.PasswordHasher.HashPassword(model.Email.Split('@')[0]);

            List<Permission> permissions = null;
            IdentityResult result = null;

            if (string.IsNullOrEmpty(fc["hddPermissions"]) || fc["hddPermissions"].ToString() == "[]")
            {
                ModelState.AddModelError("", "No ha seleccionado los Permisos para el Usuario");
                return View(model);
            }

            permissions = JsonConvert.DeserializeObject<List<Permission>>(fc["hddPermissions"].ToString());

            foreach (var item in permissions)
            {
                item.UserId = string.IsNullOrEmpty(model.Id) ? user.Id : model.Id;
                item.CreatedBy = User.Identity.GetUserId();
                item.UpdatedBy = User.Identity.GetUserId();
            }

            if (string.IsNullOrEmpty(model.Id))//creación de nuevo Usuario
            {
                if (!ModelState.IsValid)
                {
                    foreach (var item in ModelState.Values.SelectMany(v => v.Errors))
                        ModelState.AddModelError("", item.ErrorMessage);
                }

                if (model.IdentificationTypeId <= 0)
                {
                    ModelState.AddModelError("", "Por favor seleccione el Tipo de Documento");

                    return View(model);
                }

                //validar si ya existe un Usuario con el tipo documento y documento suministrados
                var vUserDB = userService.FindUserByIdentificationAndTypeId(model.IdentificationTypeId, model.IdentificationId);

                if (vUserDB != null)
                {
                    ModelState.AddModelError("", "Ya existe un Usuario con el Tipo de Documento y Documento suministrados");
                    return View(model);
                }

                result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    ViewBag.messageAction = "Usuario Registrado exitosamente!";

                    userService.RegisterExternalUserTrazability(JsonConvert.SerializeObject(new ExternalUserViewModel()
                    {
                        Id = user.Id,
                        IdentificationTypeId = model.IdentificationTypeId,
                        IdentificationId = model.IdentificationId,
                        Names = model.Names,
                        Email = model.Email,
                        CreatorNit = user.CreatorNit
                    }), "Creación");

                    var result1 = await UserManager.AddToRoleAsync(user.Id, Roles.UsuarioExterno);

                    //Envio de notificacion por correo
                    _ = SendMailCreate(model);

                    if (!result1.Succeeded)
                    {
                        ModelState.AddModelError("", "El Usario no puedo ser asignado al role 'Usuario Externo'");

                        UserManager.Delete(user);

                        return View(model);
                    }

                    var affected = _permisionService.AddOrUpdate(permissions);

                    userService.RegisterExternalUserTrazability(JsonConvert.SerializeObject(new ExternalUserViewModel()
                    {
                        IdentificationTypeId = model.IdentificationTypeId,
                        IdentificationId = model.IdentificationId,
                        Names = model.Names,
                        Email = model.Email
                    }) + ", permisos: " + JsonConvert.SerializeObject(permissions), "Creación de Permisos");

                    return RedirectToAction("AddUser");
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
            }
            else
            {

                int ru = userService.UpdateExternalUser(new ExternalUserViewModel()
                {
                    Id = model.Id,
                    IdentificationTypeId = model.IdentificationTypeId,
                    IdentificationId = model.IdentificationId,
                    Names = model.Names,
                    Email = model.Email,
                    UpdatedBy = User.Identity.Name,
                    LastUpdated = DateTime.Now
                });

                if (ru > 0)
                {
                    ViewBag.messageAction = "Usuario actualizado exitosamente!";

                    userService.RegisterExternalUserTrazability(JsonConvert.SerializeObject(new ExternalUserViewModel()
                    {
                        IdentificationTypeId = model.IdentificationTypeId,
                        IdentificationId = model.IdentificationId,
                        Names = model.Names,
                        Email = model.Email
                    }), "Actualización");

                    var affected = _permisionService.AddOrUpdate(permissions);

                    userService.RegisterExternalUserTrazability(JsonConvert.SerializeObject(new ExternalUserViewModel()
                    {
                        IdentificationTypeId = model.IdentificationTypeId,
                        IdentificationId = model.IdentificationId,
                        Names = model.Names,
                        Email = model.Email
                    }) + ", permisos: " + JsonConvert.SerializeObject(permissions), "Actualización de Permisos");

                    //Envio de notificacion por correo
                    _ = SendMailUpdate(model);
                    return RedirectToAction("AddUser");
                }
                else
                    ViewBag.messageAction = "No se pudo actualizar el Usuario!";
            }

            return View(model);
        }

        /// <summary>
        /// Activar o Desactivar un Usuario Externo
        /// </summary>
        /// <param name="userId">Id del Usuario Externo</param>
        /// <param name="active">Activar o Desactivar según sea el caso</param>
        /// <returns><see cref="Gosocket.Dian.Web.Models.GeneralResponseModel"/></returns>
        [HttpPost]
        public JsonResult UpdateActive(string userId, string active, string activeDescription, string email)
        {
            byte accion;

            if (active.Equals("Desactivar"))
                accion = 0;
            else
                accion = 1;

            int result = userService.UpdateActive(userId, accion, User.Identity.Name, activeDescription);
            //Envio de notificacion por correo
            _ = SendMailActive(email, User.Identity.Name, active);

            GeneralResponseModel res = new GeneralResponseModel()
            {
                HttpStatusCode = result > 0 ? System.Net.HttpStatusCode.OK.GetHashCode() : System.Net.HttpStatusCode.BadRequest.GetHashCode(),
                StatusCode = result > 0 ? System.Net.HttpStatusCode.OK.ToString() : System.Net.HttpStatusCode.BadRequest.ToString(),
                Message = result > 0 ? "Estado actualizado exitosamente!" : "No se pudo actualizar el estado del Usuario"
            };

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Enviar notificacion email para creacion de usuario externo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SendMailCreate(ExternalUserViewModel model)
        {
            var emailService = new Gosocket.Dian.Application.EmailService();
            StringBuilder message = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            message.Append("<span style='font-size:24px;'><b>Comunicación de servicio</b></span></br>");
            message.Append("</br> <span style='font-size:18px;'><b>Se ha generado una clave de acceso al Catalogo de DIAN</b></span></br>");
            message.AppendFormat("</br> Señor (a) usuario (a): {0}", model.Names);
            message.Append("</br> A continuación, se entrega la clave para realizar tramites y gestión de solicitudes recepción documentos electrónicos.");
            message.AppendFormat("</br> Clave de acceso: {0}", model.Password);

            message.Append("</br> <span style='font-size:10px;'>Te recordamos que esta dirección de correo electrónico es utilizada solamente con fines informativos. Por favor no respondas con consultas, ya que estas no podrán ser atendidas. Así mismo, los trámites y consultas en línea que ofrece la entidad se deben realizar únicamente a través del portal www.dian.gov.co</span>");

            //Nombre del documento, estado, observaciones
            dic.Add("##CONTENT##", message.ToString());

            emailService.SendEmail(model.Email, "DIAN - Creacion de Usuario Registrado", dic);

            return true;
        }

        /// <summary>
        /// Enviar notificacion email para actualizacion de usuario externo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SendMailUpdate(ExternalUserViewModel model)
        {
            var emailService = new Gosocket.Dian.Application.EmailService();
            StringBuilder message = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            message.Append("<span style='font-size:24px;'><b>Comunicación de servicio</b></span></br>");
            message.Append("</br> <span style='font-size:18px;'><b>Se ha actualizado su información de acceso al Catalogo de DIAN</b></span></br>");
            message.AppendFormat("</br> Señor (a) usuario (a): {0}", model.Names);
            message.Append("</br> Su información de registro y acceso al Catalogo de DIAN ha sido actualizada satisfactoriamente.");
            message.AppendFormat("</br> Tipo de documento: {0}", model.IdentificationTypeId);
            message.AppendFormat("</br> Numero  de documento: {0}", model.IdentificationId);
            message.AppendFormat("</br> Correo electronico: {0}", model.Email);
            message.AppendFormat("</br> Clave de acceso: {0}", model.Password);

            message.Append("</br> <span style='font-size:10px;'>Te recordamos que esta dirección de correo electrónico es utilizada solamente con fines informativos. Por favor no respondas con consultas, ya que estas no podrán ser atendidas. Así mismo, los trámites y consultas en línea que ofrece la entidad se deben realizar únicamente a través del portal www.dian.gov.co</span>");

            //Nombre del documento, estado, observaciones
            dic.Add("##CONTENT##", message.ToString());

            emailService.SendEmail(model.Email, "DIAN - Actualización de Usuario Registrado", dic);

            return true;
        }

        /// <summary>
        /// Enviar notificacion email para activar/inactivar usuario externo
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SendMailActive(string email, string name, string state)
        {
            var emailService = new Gosocket.Dian.Application.EmailService();
            StringBuilder message = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            message.Append("<span style='font-size:24px;'><b>Comunicación de servicio</b></span></br>");
            message.Append("</br> <span style='font-size:18px;'><b>Se ha actualizado el estado de acceso al Catalogo de DIAN</b></span></br>");
            message.AppendFormat("</br> Señor (a) usuario (a): {0}", name);
            message.Append("</br> El estado de su acceso al Catalogo de DIAN ha sido actualizado satisfactoriamente.");
            message.AppendFormat("</br> Estado: {0}", state);

            message.Append("</br> <span style='font-size:10px;'>Te recordamos que esta dirección de correo electrónico es utilizada solamente con fines informativos. Por favor no respondas con consultas, ya que estas no podrán ser atendidas. Así mismo, los trámites y consultas en línea que ofrece la entidad se deben realizar únicamente a través del portal www.dian.gov.co</span>");

            //Nombre del documento, estado, observaciones
            dic.Add("##CONTENT##", message.ToString());

            emailService.SendEmail(email, "DIAN - Cambio de Estado de Usuario Registrado", dic);

            return true;
        }

    }
}