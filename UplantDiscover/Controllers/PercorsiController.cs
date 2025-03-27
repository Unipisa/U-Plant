using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using UplantDiscover.Models;
using UplantDiscover.Models.DB;

// Progetto UplantDiscover ,rappresentazione dei dati inseriti su UPlant per la fruibilità pubblica ideato e scritto da Pietro Picconi pietro.picconi@unipi.it


namespace UplantDiscover.Controllers
{
    public class PercorsiController : Controller
    {
        private readonly Entities _context;
        private readonly IOptions<Wso2> _wso2; 
        private readonly IConfiguration _conf;
        public PercorsiController(Entities context, IOptions<Wso2> wso2, IConfiguration conf)
        {
            _context = context;
            _wso2 = wso2;
            _conf = conf;
        }

        // GET: Pippo
        public async Task<IActionResult> Index()
        {
            var dBPlant = _context.Percorsi.Include(i => i.IndividuiPercorso);
            var linguacorrente = System.Globalization.CultureInfo.CurrentCulture.Name;
            // svincolato dal api.store CommonController common = new CommonController(_context, _wso2);
            CommonController common = new CommonController(_context);
            ViewBag.bandiera = "";
            if (linguacorrente == "it-IT")
            {
                ViewBag.bandiera = "it";
            }
            if (linguacorrente == "en-US")
            {
                ViewBag.bandiera = "en";
            }
            ViewBag.GoogleMapUrl = _conf.GetValue<string>("GoogleMap:Url");
            ViewBag.GoogleMapKey = _conf.GetValue<string>("GoogleMap:Key");
            ViewBag.GoogleMapOptions = _conf.GetValue<string>("GoogleMap:Options");
            ViewBag.GoogleMapIcons = _conf.GetValue<string>("GoogleMap:Icons");
            //ViewBag.token = common.GetToken();
            //ViewBag.ApiUrl = _conf.GetValue<string>("DataApi:Url");
            return View(await dBPlant.ToListAsync());
        }
        public IActionResult Details(Guid? id)
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
            if (id==null)
            {
                return NotFound();
            }
            Percorsi percorsi = _context.Percorsi.Find(id);
            if (percorsi == null)
            {
                return NotFound();
            }
            IEnumerable<IndividuiPercorso> indperc = _context.IndividuiPercorso.ToList();

            if (id != null )
            {
                indperc = indperc.Where(x => x.percorso == id);
            }
           // ViewBag.token = common.GetToken();
            ViewBag.idpercorso = id;
            ViewBag.GoogleMapUrl = _conf.GetValue<string>("GoogleMap:Url");
            ViewBag.GoogleMapKey = _conf.GetValue<string>("GoogleMap:Key");
            ViewBag.GoogleMapOptions = _conf.GetValue<string>("GoogleMap:Options");
            ViewBag.GoogleMapIcons = _conf.GetValue<string>("GoogleMap:Icons");
            // ViewBag.ApiUrl = _conf.GetValue<string>("DataApi:Url");
            return View(percorsi);
        }
    }

}
