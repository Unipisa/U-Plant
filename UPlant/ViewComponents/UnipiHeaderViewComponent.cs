using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UPlant.Models;

namespace UPlant.ViewComponents
{
    public class UnipiHeaderViewComponent : ViewComponent
    {
        public UnipiHeaderViewComponent() { }
        public IViewComponentResult Invoke(string filter)
        {
            return View();
        }
    }
}