using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UplantDiscover.Models;
using UplantDiscover.Models.DB;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Configuration;

// Progetto UplantDiscover ,rappresentazione dei dati inseriti su UPlant per la fruibilità pubblica ideato e scritto da Pietro Picconi pietro.picconi@unipi.it


namespace UplantDiscover.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Entities _context;
        private readonly IOptions<Wso2> _wso2;
        private readonly IConfiguration _conf;


        public HomeController(Entities context, ILogger<HomeController> logger, IOptions<Wso2> wso2, IConfiguration conf)
        {
            _logger = logger;
            _context = context;
            _wso2 = wso2;
            _conf = conf;
        }
        
        public IActionResult Index()
        {


            // svincolato dal api.store CommonController common = new CommonController(_context, _wso2);
            CommonController common = new CommonController(_context);
            var linguacorrente = System.Globalization.CultureInfo.CurrentCulture.Name;
            ViewBag.bandiera = "";
           if (linguacorrente == "it-IT")
            {
                ViewBag.bandiera = "it";
            }
            if (linguacorrente == "en-US")
            {
                ViewBag.bandiera = "en";
            }
            ViewBag.famiglie = common.GetFamiglie(linguacorrente);
            ViewBag.regni = common.GetRegni(linguacorrente);
            ViewBag.settori = common.GetSettori(linguacorrente);
            ViewBag.collezioni = common.GetCollezioni(linguacorrente);

            // ViewBag.token = common.GetToken();
            ViewBag.GoogleMapUrl =_conf.GetValue<string>("GoogleMap:Url");
            ViewBag.GoogleMapKey = _conf.GetValue<string>("GoogleMap:Key");
            ViewBag.GoogleMapOptions = _conf.GetValue<string>("GoogleMap:Options");
            ViewBag.GoogleMapIcons = _conf.GetValue<string>("GoogleMap:Icons");
            
            // string apiUrl = _conf.GetValue<string>("DataApi:Url");
            // ViewBag.apiUrl = apiUrl;






            return View();

        }
       
        public IActionResult Details(Guid? id)
        { 

            if(id==null)
            {
                                return NotFound();
            }
            // svincolato dal api.store CommonController common = new CommonController(_context, _wso2);
            CommonController common = new CommonController(_context);
            var linguacorrente = System.Globalization.CultureInfo.CurrentCulture.Name;
            if (linguacorrente == "it-IT")
            {
                ViewBag.bandiera = "it";
            }
            if (linguacorrente == "en-US")
            {
                ViewBag.bandiera = "en";
            }
            
            Individui individuo = _context.Individui.Find(id);
            
            if (individuo != null) { ///TODO: gestire null
                                     ///
                //string apiUrl = _conf.GetValue<string>("DataApi:Url");
                // ViewBag.apiUrl = apiUrl;
                // ViewBag.token = common.GetToken();
                ViewBag.GoogleMapUrl = _conf.GetValue<string>("GoogleMap:Url");
                ViewBag.GoogleMapKey = _conf.GetValue<string>("GoogleMap:Key");
                ViewBag.GoogleMapOptions = _conf.GetValue<string>("GoogleMap:Options");
                ViewBag.GoogleMapIcons = _conf.GetValue<string>("GoogleMap:Icons");
               
                return View(individuo);
            }
            return RedirectToAction("Index");//aggiungere un pagealert
        }


            public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult ChangeLanguage(string culture)
        {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            return Redirect(Request.Headers["Referer"].ToString());
            //return View();
            //  return Redirect($"{Request.Headers["Referer"]}{Request.QueryString.Value.ToString()}");
        }
       
       
      
    }
}
