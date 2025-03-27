using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using UPlant.Models;

namespace UPlant.ViewComponents
{
    public class MenuUserViewComponent : ViewComponent
    {
        public MenuUserViewComponent() { }

        public IViewComponentResult Invoke(string filter)
        {
            return View();
        }
    }
}