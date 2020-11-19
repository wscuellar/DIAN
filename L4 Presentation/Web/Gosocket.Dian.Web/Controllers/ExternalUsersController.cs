using Gosocket.Dian.Application;
using Gosocket.Dian.Domain.Sql;
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

        private IdentificationTypeService identificationTypeService = new IdentificationTypeService();

        public ExternalUsersController(IPermissionService permisionService)
        {
            _context = new ApplicationDbContext();
            _permisionService = permisionService;
        }

        // GET: ExternalUsers
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult AddUser(string id = "")
        {
            ViewBag.CurrentPage = Navigation.NavigationEnum.ExternalUsersCreate;

            var identificationTypeList = identificationTypeService.List();

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

            ViewBag.ExternalUsersList = _context.Users.Where(u => u.Roles.Any(r => r.RoleId == role.Id)).ToList()
                .Select(u =>
                new ExternalUserViewModel
                {
                    Id = u.Id,
                    IdentificationTypeId =  u.IdentificationTypeId,
                    IdentificationId = u.IdentificationId,
                    Names = u.Name,
                    Email = u.Email,
                    //Roles = u.Roles.ToList(),
                    Active = u.Active,
                    IdentificationTypes = identificationTypeService.List()
                        .Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList()
                }).ToList();

            ExternalUserViewModel model = null;

            if (!string.IsNullOrEmpty(id))
            {
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

            return View(model);
        }

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

        [HttpPost]
        public async Task<ActionResult> AddUser(ExternalUserViewModel model, FormCollection fc)
        {
            ViewBag.Menu = this.MenuApp();

            var user = new ApplicationUser
            {
                Code = Guid.NewGuid().ToString().Substring(0, 6),
                Name = model.Names,
                Email = model.Email,
                PasswordHash = model.Password,// UserManager.PasswordHasher.HashPassword(model.Email.Split('@')[0]),
                UserName = model.Email,
                CreatedBy = User.Identity.Name
            };

            var result = await UserManager.CreateAsync(user);
            if (result.Succeeded)
            {
                var result1 = await UserManager.AddToRoleAsync(user.Id, Roles.UsuarioExterno);

                if (!result1.Succeeded)
                {
                    ModelState.AddModelError("", "El Usario no puedo ser asignado al role 'Usuario Externo'");
                    return View(model);
                }

                List<Permission> permissions = null;

                if (fc["hddPermissions"] == null)
                {
                    ModelState.AddModelError("no_permissions", "No ha seleccionado los Permisos para el Usuario");
                    return View(model);
                }

                permissions = JsonConvert.DeserializeObject<List<Permission>>(fc["hddPermissions"].ToString());

                foreach (var item in permissions)
                {
                    item.UserId = user.Id;
                    item.CreatedBy = User.Identity.GetUserId();
                    item.UpdatedBy = User.Identity.GetUserId();
                }

                var affected = _permisionService.AddOrUpdate(permissions);



                return View(model);
            }

            foreach (var item in result.Errors)
                ModelState.AddModelError(string.Empty, item);

            foreach (var item in ModelState)
                if (item.Key.Contains("Code"))
                    item.Value.Errors.Clear();

            return View(model);
        }

    }
}