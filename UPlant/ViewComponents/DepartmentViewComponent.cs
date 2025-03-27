using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.IO;

namespace UPlant.ViewComponents
{
    public class DepartmentViewComponent : ViewComponent
    {

        public DepartmentViewComponent() { }

        public IViewComponentResult Invoke(string filter)
        {
            if (string.IsNullOrEmpty(ViewBag.PageHelpFileName))
            {
                return View(string.Empty);
            }

            ViewBag.PageHelpContainer = LoadData(ViewBag.PageHelpFileName);
            return View();
        }

        private string LoadData(string filepath)
        {
            string result = string.Empty;
            return result;
        }
    }
}