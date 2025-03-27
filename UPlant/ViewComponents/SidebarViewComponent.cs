
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using UPlant.Models;
using UPlant.Models.DB;

namespace UPlant.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IStringLocalizer _localizer;
        public SidebarViewComponent(IStringLocalizer Localizer)
        {
            _localizer = Localizer;
        }
        public IViewComponentResult Invoke(string filter)
        {
            var sidebars = new List<SidebarMenu>();
            return View(sidebars);
          /*  var u = User;
            var sidebars = new List<SidebarMenu>();
            
            sidebars.Add(ModuleHelper.AddModule(ModuleHelper.Module.Home, _localizer["Home"], Url.Action("Index", "Home", null), "fa fa-home"));
            
            if (User.IsInRole(UserRoles.Administrator))
            {
                
                //riga tabelle di amministrazione
                sidebars.Add(ModuleHelper.AddTree(_localizer["Tabelle di Amministrazione"], "fa fa-table style='color: #FFD43B;'"));
                sidebars.Last().TreeChild = new List<SidebarMenu>()
                {
                    ModuleHelper.AddModule(ModuleHelper.Module.Tabellediamministrazione, _localizer["Organizzazione"], Url.Action("Index", "Organizzazioni", null),"fa fa-pencil"),
                    ModuleHelper.AddModule(ModuleHelper.Module.Tabellediamministrazione, _localizer["Utenti"], Url.Action("Index", "Users", null),"fa fa-pencil"),
                    
                };

              
               

                sidebars.Add(ModuleHelper.AddTree(_localizer["Moduli di inserimento"], "fa fa-table "));
                sidebars.Last().TreeChild = new List<SidebarMenu>()
                {
                    ModuleHelper.AddModule(ModuleHelper.Module.Modulidiinserimento, _localizer["Nuova Accessione"], Url.Action("Create", "Accessioni", null),"fa fa-calendar-days")
                };
                sidebars.Add(ModuleHelper.AddTree(_localizer["Moduli di Ricerca"], "fa fa-table "));
                sidebars.Last().TreeChild = new List<SidebarMenu>()
                {
                    ModuleHelper.AddModule(ModuleHelper.Module.Modulidiinserimento, _localizer["Accessioni Recenti"], Url.Action("Index", "Accessioni", null),"fa fa-calendar-days")
                };
            }
            return View(sidebars);*/
        }
    }
}