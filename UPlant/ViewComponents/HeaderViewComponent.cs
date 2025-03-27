using Microsoft.AspNetCore.Mvc;

namespace UPlant.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        public HeaderViewComponent() { }

        public IViewComponentResult Invoke(string filter)
        {
            return View();
        }
    }
}