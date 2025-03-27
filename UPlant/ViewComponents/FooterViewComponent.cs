using Microsoft.AspNetCore.Mvc;

namespace UPlant.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {

        public FooterViewComponent() { }

        public IViewComponentResult Invoke(string filter)
        {
            return View();
        }
    }
}