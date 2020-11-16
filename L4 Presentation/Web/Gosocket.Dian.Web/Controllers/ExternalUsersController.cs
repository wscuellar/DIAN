using Gosocket.Dian.Application;
using Gosocket.Dian.Web.Filters;
using Gosocket.Dian.Web.Models;
using Gosocket.Dian.Web.Models.Role;
using Gosocket.Dian.Web.Utils;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    [CustomRoleAuthorization(CustomRoles = "Administrador, Super")]
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
        private IdentificationTypeService identificationTypeService = new IdentificationTypeService();

        public ExternalUsersController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: ExternalUsers
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddUser()
        {
            ExternalUserViewModel model = new ExternalUserViewModel()
            {
                IdentificationTypes = identificationTypeService.List()
                .Select(x => new IdentificationTypeListViewModel { Id = x.Id, Description = x.Description }).ToList()
                //.Insert(0, new IdentificationTypeListViewModel { Id = 0, Description = "Seleccione..." })
                
            };
            ViewBag.CurrentPage = Navigation.NavigationEnum.ExternalUsers;
            ViewBag.Roles = new SelectList(_context.Roles.Where(u => !u.Name.Contains("Super"))
                                            .ToList(), "Id", "Name");

            ViewBag.Menu = this.MenuApp();

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
                        new SubMenuViewModel() { MenuId = 2, Id=3, Title="Configuraciones", Name = "Configuraciones" }
                        }
                    },
                new MenuViewModel() { Id=3, Name= "Documentos", Title="Document", Options = new List<SubMenuViewModel>()
                    {
                        new SubMenuViewModel() { MenuId = 3, Id=1, Title="Consultar", Name = "Consultar" },
                        new SubMenuViewModel() { MenuId = 3, Id=2, Title="Rango de Númeración", Name = "Rango de Númeración" },
                        new SubMenuViewModel() { MenuId = 3, Id=3, Title="Exportar", Name = "Exportar" }
                    }
                },
                new MenuViewModel() { Id=4, Name= "Participantes", Title="Participants", Options = new List<SubMenuViewModel>(){
                        new SubMenuViewModel() { MenuId = 4, Id=1, Name="Proveedores Tecnológicos", Title="Proveedores Tecnológicos" },
                        new SubMenuViewModel() { MenuId = 4, Id=2, Name="Proveedores Autorizados", Title="Proveedores Autorizados" },
                        new SubMenuViewModel() { MenuId = 4, Id=3, Name="Facturadores Electrónicos", Title="Facturadores Electrónicos" },
                        new SubMenuViewModel() { MenuId = 4, Id=4, Name="Representantes Legales", Title="Representantes Legales" },
                        new SubMenuViewModel() { MenuId = 4, Id=5, Name="Softwares", Title="Softwares" },
                        new SubMenuViewModel() { MenuId = 4, Id=6, Name="Contribuyentes", Title="Contribuyentes" },
                        new SubMenuViewModel() { MenuId = 4, Id=7, Name="Gestionar Solicitudes Recepción por Lote - Asíncrono", Title="Gestionar Solicitudes Recepción por Lote - Asíncrono" }
                    }
                },
                new MenuViewModel() { Id=5, Name= "Configuración", Title="Settings", Options = new List<SubMenuViewModel>(){
                        new SubMenuViewModel() { MenuId = 5, Id=1, Name="Tipos de Ficheros" },
                        new SubMenuViewModel() { MenuId = 5, Id=2, Name="Set de Pruebas" },
                        new SubMenuViewModel() { MenuId = 5, Id=3, Name="Administrar Contingencias" },
                        new SubMenuViewModel() { MenuId = 5, Id=4, Name="Usuarios" }
                    }
                },
                new MenuViewModel() { Id=6, Name= "Business Intelligence", Title="Business Intelligence"},
                new MenuViewModel() { Id=7, Name= "Facturador Gratuito", Title="Facturador Gratuito"},
            };
        }

        [HttpPost]
        public async Task<ActionResult> AddUser(ExternalUserViewModel model, FormCollection fc)
        {
            var user = new ApplicationUser
            {
                Code = Guid.NewGuid().ToString().Substring(0, 6),
                Name = model.Names,
                Email = model.Email,
                PasswordHash = UserManager.PasswordHasher.HashPassword(model.Email.Split('@')[0]),
                UserName = model.Email,
                CreatedBy = User.Identity.Name
            };

            var result = await UserManager.CreateAsync(user);
            //if (result.Succeeded)
            //result = await UserManager.AddToRoleAsync(user.Id, Roles.Administrator);

            MenuViewModel permissions = null;

            if (result.Succeeded)
            {
                if(fc["hddPermissions"] == null)
                {
                    ModelState.AddModelError("no_permissions", "No ha seleccionado los Permisos para el Usuario");
                    return View(model);
                }

                permissions = JsonConvert.DeserializeObject<MenuViewModel>(fc["hddPermissions"].ToString());


                return RedirectToAction(nameof(Index));
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