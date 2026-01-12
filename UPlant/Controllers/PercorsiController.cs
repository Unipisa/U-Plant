using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UPlant.Controllers;
using UPlant.Models;
using UPlant.Models.DB;


namespace U_Plant.Controllers
{
    [Authorize(Roles = "Administrator,Discover")]
    public class PercorsiController : BaseController
    {
        private readonly Entities _context;
        private readonly IOptions<AppSettings> _opt;
        private readonly IWebHostEnvironment _env;
        private readonly LanguageService _languageService;

        public PercorsiController(Entities context, IOptions<AppSettings> opt, IWebHostEnvironment env, LanguageService languageService)
        {
            _context = context;
            _opt = opt;
            _env = env;
            _languageService = languageService;

        }
        public ActionResult Index()
        {
            List<Guid> perindmorto = new List<Guid>();
            IEnumerable<Percorsi> percorsi = _context.Percorsi.ToList();
            foreach (var i in percorsi)
            {



                if (StatoIndanomalo(i.id) && i.attivo == true)
                {
                    perindmorto.Add(i.id);
                    AddPageAlerts(PageAlertType.Warning, _languageService.Getkey("Message_3").ToString() + ' ' + i.titolo + ' ' + _languageService.Getkey("Message_18").ToString());
                    TempData["message"] = _languageService.Getkey("Message_3").ToString() + ' ' + i.titolo + ' ' + _languageService.Getkey("Message_18").ToString();


                }

            }
            ViewBag.pim = perindmorto;

            return View(percorsi);

        }
        public async Task<IActionResult> Details(Guid? id)
        {
            if (StatoIndanomalo(id))
            {
                AddPageAlerts(PageAlertType.Warning, _languageService.Getkey("Message_4").ToString());
                TempData["message"] = _languageService.Getkey("Message_4").ToString();

            }

            if (id == null)
            {
                return new StatusCodeResult(400);
            }
            var percorsi = await _context.Percorsi
                .Include(i => i.IndividuiPercorso).ThenInclude(i => i.individuoNavigation).ThenInclude(i => i.accessioneNavigation).ThenInclude(i => i.specieNavigation)
                .Include(i => i.IndividuiPercorso).ThenInclude(i => i.individuoNavigation).ThenInclude(i => i.cartellinoNavigation)
                .Include(i => i.IndividuiPercorso).ThenInclude(i => i.individuoNavigation).ThenInclude(i => i.collezioneNavigation)
                .Include(i => i.IndividuiPercorso).ThenInclude(i => i.individuoNavigation).ThenInclude(i => i.propagatoModalitaNavigation)
                .Include(i => i.IndividuiPercorso).ThenInclude(i => i.individuoNavigation).ThenInclude(i => i.sessoNavigation)
                .Include(i => i.IndividuiPercorso).ThenInclude(i => i.individuoNavigation).ThenInclude(i => i.settoreNavigation)
                .Include(i => i.IndividuiPercorso).ThenInclude(i => i.individuoNavigation).ThenInclude(i => i.StoricoIndividuo).ThenInclude(i => i.statoIndividuoNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            //Percorsi percorsi = _context.Percorsi.Find(id);
            ViewBag.idpercorso = percorsi.id;

            // 
            ViewBag.list = percorsi.IndividuiPercorso.OrderBy(x => x.individuoNavigation.accessioneNavigation.specieNavigation.nome_scientifico).ToList();
            percorsi.IndividuiPercorso = ViewBag.list;
            ViewBag.indmorto = StatoIndanomalo(percorsi.id);

            if (percorsi == null)
            {
                return NotFound();
            }
            return View(percorsi);

        }
        
        public ActionResult Create()
        {
            ViewBag.titolo = "";
            ViewBag.descrizione = "";
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "id,titolo,descrizione,ordinamento,attivo")]  Percorsi percorsi)
       
        public ActionResult Create(string titolo, string descrizione, string titolo_en, string descrizione_en)
        {
            Percorsi percorsi = new Percorsi();
            if (!String.IsNullOrEmpty(titolo))
            {
                percorsi.titolo = titolo.Trim();
            }

            if (!String.IsNullOrEmpty(descrizione))
            {
                percorsi.descrizione = descrizione.Trim();
            }
            if (!String.IsNullOrEmpty(titolo_en))
            {
                percorsi.titolo_en = titolo_en.Trim();
            }

            if (!String.IsNullOrEmpty(descrizione_en))
            {
                percorsi.descrizione_en = descrizione_en.Trim();
            }

            percorsi.datacreazione = DateTime.Now;
            percorsi.attivo = false;
            percorsi.ordinamento = 1;
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            string autore = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@"))).Select(a => a.Name + " " + a.LastName).FirstOrDefault();


            percorsi.autore = autore;
            if (ModelState.IsValid)
            {

                _context.Percorsi.Add(percorsi);
                _context.SaveChanges();
                return RedirectToAction("Index");

            }

            return View(percorsi);
        }
        
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new StatusCodeResult(400);
            }
            Percorsi percorsi = _context.Percorsi.Find(id);
            if (percorsi == null)
            {
                return NotFound();
            }
            return View(percorsi);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "id,titolo,descrizione,datacreazione,ordinamento,attivo")] Percorsi percorsi)
        public ActionResult Edit(Guid id, string titolo, string descrizione, string titolo_en, string descrizione_en)
        {
            Percorsi percorsi = _context.Percorsi.Find(id);
            percorsi.titolo = titolo.Trim();
            if (descrizione.Length > 1)
            {
                percorsi.descrizione = descrizione.Trim();
            }
            if (!string.IsNullOrEmpty(titolo_en))
            {
                percorsi.titolo_en = titolo_en.Trim();
            }

            if (!string.IsNullOrEmpty(descrizione_en))
            {
                percorsi.descrizione_en = descrizione_en.Trim();
            }

            if (ModelState.IsValid)
            {

                _context.Entry(percorsi).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(percorsi);
        }
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                AddPageAlerts(PageAlertType.Error, _languageService.Getkey("Message_12").ToString());
                TempData["message"] = _languageService.Getkey("Message_12").ToString();
                return new StatusCodeResult(400);
            }
            Percorsi percorsi = _context.Percorsi.Find(id);
            if (percorsi == null)
            {
                return NotFound();
            }
            return View(percorsi);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Percorsi percorsi = _context.Percorsi.Find(id);
            IEnumerable<IndividuiPercorso> individuipercorso = _context.IndividuiPercorso.Where(s => s.percorso == id);
            if (individuipercorso != null)
            {
                _context.IndividuiPercorso.RemoveRange(individuipercorso);
            }
            if (!String.IsNullOrEmpty(percorsi.nomefile))
            {

                //  string path = StaticUtils.SetImgPath("Percorsi\\" + percorsi.id, percorsi.nomefile, _opt.Value.Pathfile.Basepath);
                //  string paththumb = StaticUtils.SetThumbImgPath("Percorsi\\" + percorsi.id, percorsi.nomefile, _opt.Value.Pathfile.Basepath);
                string path = StaticUtils.SetImgPath(percorsi.id.ToString(), percorsi.nomefile, _opt.Value.Pathfile.Basepath + "\\Percorsi\\");
                string paththumb = StaticUtils.SetThumbImgPath(percorsi.id.ToString(), percorsi.nomefile, _opt.Value.Pathfile.Basepath + "\\Percorsi\\");
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        System.IO.File.Delete(path);
                        System.IO.File.Delete(paththumb);
                        ViewBag.deleteSuccess = "true";
                    }
                    catch (System.IO.IOException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            _context.Percorsi.Remove(percorsi);
            _context.SaveChanges();

            return RedirectToAction("Index");

        }
        
        public ActionResult DeleteIndividuoPercorso(Guid? id, Guid percorso)
        {
            if (id == null)
            {
                return new StatusCodeResult(400);
            }
            ViewBag.id = id;
            ViewBag.percorso = percorso;
            return PartialView();
        }
        [HttpPost, ActionName("DeleteIndividuoPercorsoConfirmed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteIndividuoPercorsoConfirmed(Guid id, Guid percorso)
        {
            IndividuiPercorso individuopercorso = _context.IndividuiPercorso.Find(id);
            _context.IndividuiPercorso.Remove(individuopercorso);
            _context.SaveChanges();
            AddPageAlerts(PageAlertType.Success, _languageService.Getkey("Message_6").ToString());
            TempData["message"] = _languageService.Getkey("Message_6").ToString();
            return RedirectToAction("Details", "Percorsi", new { id = percorso });

        }

        public JsonResult AutoComplete()
        {
            //  var allowedStatus = new[] { "30e70f7c13774994ac9215b3543ebd7b", "3d91514fecb3473783eda3d3f8a63457", "429773f8ba564e2b87a0b775935c3ff7" }; //Vivo e incerto, malato
            var notallowedsector = new[] { "0ba85efcea3544e485141f7e311d82e2", "0e551835b07642f88540a4ff9d15e84e" }; //Nursery e Banca Semi
            string term = HttpContext.Request.Query["term"].ToString();

            IEnumerable<StoricoIndividuo> prog =
                _context.StoricoIndividuo
                .Include(x => x.individuoNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation)
                .Include(x => x.statoIndividuoNavigation)
                .AsEnumerable()
                .OrderByDescending(c => c.individuoNavigation.propagatoData)
                .GroupBy(c => c.individuo)
                        .Select(g => g.OrderByDescending(c => c.dataInserimento).FirstOrDefault())
                        .Where(x => x.individuoNavigation.progressivo.StartsWith(term))
            .Where(x => x.statoIndividuoNavigation.visualizzazioneweb == true)
            .Where(x => x.individuoNavigation.settoreNavigation.visualizzazioneweb == true).ToList();

            var result = prog.Take(10).Select(x => x.individuoNavigation.progressivo);



            // var prog = _context.Storico.GroupBy(c => c.individuo).Select(g => g.OrderByDescending(c => c.dataInserimento).FirstOrDefault()).Where(x => allowedStatus.Contains(x.statoIndividuo)).Where(x => !notallowedsector.Contains(x.Individui.settore)).Where(x => x.Individui.progressivo.StartsWith(term)).Take(10).Select(x => x.Individui.progressivo).ToList()
            //var prelist = _context.StoricoIndividuo.Include(x => x.statoIndividuoNavigation)
            //   .Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation)
            //    .GroupBy(c => c.individuo)
            //   .Select(g => g.OrderByDescending(c => c.dataInserimento).FirstOrDefault()).Where(x => x.statoIndividuoNavigation.visualizzazioneweb == true).Where(x => x.individuoNavigation.settoreNavigation.visualizzazioneweb == true).Where(x => x.individuoNavigation.progressivo.StartsWith(term)).Take(10).Select(x => x.individuoNavigation.progressivo);

            // var prog = _context.StoricoIndividuo.GroupBy(c => c.individuo).Select(g => g.OrderByDescending(c => c.dataInserimento).FirstOrDefault()).Where(x => x.statoIndividuoNavigation.visualizzazioneweb == true).Where(x => x.individuoNavigation.settoreNavigation.visualizzazioneweb == true).Where(x => x.individuoNavigation.progressivo.StartsWith(term)).Take(10).Select(x => x.individuoNavigation.progressivo);

            return Json(result, new System.Text.Json.JsonSerializerOptions());
        }
        
        //public ActionResult Create([Bind(Include = "id,titolo,descrizione,ordinamento,attivo")]  Percorsi percorsi)
        public ActionResult Inserisciindividuopercorso(Guid percorso, Guid individuo)
        {


            var indicerca = _context.IndividuiPercorso.Where(x => x.individuo == individuo && x.percorso == percorso);
            if (indicerca.Count() == 0)
            {
                IndividuiPercorso indiper = new IndividuiPercorso();

                indiper.percorso = percorso;
                indiper.individuo = individuo;

                if (ModelState.IsValid)
                {


                    _context.IndividuiPercorso.Add(indiper);
                    _context.SaveChanges();
                    AddPageAlerts(PageAlertType.Success, _languageService.Getkey("Message_5").ToString());
                    TempData["message"] = _languageService.Getkey("Message_5").ToString();

                    return RedirectToAction("Details", "Percorsi", new { id = percorso });

                }
            }
            AddPageAlerts(PageAlertType.Warning, _languageService.Getkey("Message_2").ToString());
            TempData["message"] = _languageService.Getkey("Message_2").ToString();






            return RedirectToAction("Details", "Percorsi", new { id = percorso });
        }

        public JsonResult Ricerca(string idpercorso, string progressivo)
        {

            return Json(_context.Individui.Where(a => a.progressivo.Contains(progressivo)).OrderByDescending(c => c.progressivo).Select(x => new
            {
                idindividuo = x.id,
                progressivo = x.progressivo,
                vecchioprogressivo = x.vecchioprogressivo,
                nomescientifico = x.accessioneNavigation.specieNavigation.nome_scientifico,
                settore = x.settoreNavigation.settore,
                collezione = x.collezioneNavigation.collezione,
                cartellino = x.cartellinoNavigation.descrizione,
                immagini = x.ImmaginiIndividuo.Count,
                idpercorso = idpercorso
            }).ToList(), new System.Text.Json.JsonSerializerOptions());


        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult UploadImg(Guid id)
        {

            ViewBag.idpercorso = id;

            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImg(IEnumerable<IFormFile> files, Guid id, string credits)

        {

            // Do something with your files
            var t = _opt.Value;


            foreach (var file in files)
            {
                //   System.Drawing.Image sourceimage =
                //   System.Drawing.Image.FromStream(file.InputStream);

                if (file.Length > 0 && file.Length <= Convert.ToDecimal(t.Pathfile.LimitMaxUpload))

                    try
                    {

                        Percorsi percorsi = _context.Percorsi.Find(id);


                        percorsi.nomefile = file.FileName.ToLower(); //se il nome ha estensione maiuscola crea problemi

                        percorsi.credits = credits;
                        _context.Entry(percorsi).State = EntityState.Modified;
                        _context.SaveChanges();


                        int posizione = percorsi.nomefile.LastIndexOf(".");
                        string estensione = percorsi.nomefile.Substring(posizione);
                        if (estensione.ToLower() == ".heic" || estensione.ToLower() == ".heif" || estensione == ".hevc" || estensione.ToLower() == ".png" || estensione.ToLower() == ".jpg" || estensione.ToLower() == ".jpeg") //ho modificato il file .heic in jpeg forzatamente
                        {
                            estensione = ".jpg";
                        }
                        //  string filename = StaticUtils.SetImgPath("Percorsi\\", percorsi.id.ToString() + estensione,t.Pathfile.Basepath);
                        string filename = StaticUtils.SetImgPath(percorsi.id.ToString(), percorsi.nomefile, _opt.Value.Pathfile.Basepath + "\\Percorsi\\");

                        {
                            string filePath = Path.Combine(t.Pathfile.Basepath, filename);
                            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }
                        }


                        if (System.IO.File.Exists(filename))
                        {
                            string filenamethumb = StaticUtils.SetThumbImgPath(percorsi.id.ToString(), percorsi.nomefile, _opt.Value.Pathfile.Basepath + "\\Percorsi\\");
                            //string filenamethumb = StaticUtils.SetThumbImgPath("Percorsi\\", percorsi.id + estensione, t.Pathfile.Basepath);
                            StaticUtils.ResizeAndSave(filename, filenamethumb, 400, true);

                        }



                        AddPageAlerts(PageAlertType.Success, _languageService.Getkey("Message_9").ToString());
                        TempData["message"] = _languageService.Getkey("Message_9").ToString();


                    }
                    catch (Exception ex)
                    {
                        AddPageAlerts(PageAlertType.Error, _languageService.Getkey("Message_13").ToString() + ex.Message.ToString());
                        TempData["message"] = _languageService.Getkey("Message_13").ToString() + ex.Message.ToString();

                        return RedirectToAction("Details", "Percorsi", new { id = id });
                    }
                else
                {
                    AddPageAlerts(PageAlertType.Error, _languageService.Getkey("Message_15").ToString());
                    TempData["message"] = _languageService.Getkey("Message_15").ToString();

                    return RedirectToAction("Details", "Percorsi", new { id = id });
                }


            }

            return RedirectToAction("Details", "Percorsi", new { id = id });






        }
        public ActionResult ViewImg(Guid percorso, string nomefile)
        {
            //string path = StaticUtils.GetThumbImgPath("Percorsi\\", percorso.ToString(), nomefile, _opt.Value.Pathfile.Basepath);
            // string path = StaticUtils.SetThumbImgPath("Percorsi\\" + percorso.ToString(), nomefile);
            string path = StaticUtils.SetThumbImgPath(percorso.ToString(), nomefile, _opt.Value.Pathfile.Basepath + "\\Percorsi\\");
            //completare con case se immagini hanno nome jpg
            //return base.File(path, "image/jpeg");
            var fileExists = System.IO.File.Exists(path);
            if (fileExists)
            {
                var fs = System.IO.File.OpenRead(path);
                return File(fs, "image/jpeg", path);
            }
            else
            {
                AddPageAlerts(PageAlertType.Warning, _languageService.Getkey("Message_10").ToString());
                TempData["message"] = _languageService.Getkey("Message_10").ToString();

                return RedirectToAction("Details", "Percorsi", new { id = percorso });
            }


            //string path = StaticUtils.SetThumbImgPath("Percorsi\\" + percorso, nomefile, t.Pathfile.Basepath);

            //completare con case se immagini hanno nome jpg
            // return base.File(path, "image/jpeg");

        }
        public ActionResult ViewBigImg(Guid percorso, string nomefile)
        {
            //string path = StaticUtils.GetImgPath("Percorsi\\", percorso.ToString(), nomefile, _opt.Value.Pathfile.Basepath);
            string path = StaticUtils.SetImgPath(percorso.ToString(), nomefile, _opt.Value.Pathfile.Basepath + "\\Percorsi\\");
            var fileExists = System.IO.File.Exists(path);
            if (fileExists)
            {
                var fs = System.IO.File.OpenRead(path);
                return File(fs, "image/jpeg", path);
            }
            else
            {
                AddPageAlerts(PageAlertType.Warning, _languageService.Getkey("Message_12").ToString());
                TempData["message"] = _languageService.Getkey("Message_12").ToString();

                return RedirectToAction("Details", "Percorsi", new { id = percorso });
            }


        }
        public ActionResult DeleteImg(Guid? id, string nomefile)
        {
            if (id == null)
            {
                return new StatusCodeResult(400);
            }
            ViewBag.percorso = id;
            ViewBag.nomefile = nomefile;
            return PartialView();
        }

        // POST
        [HttpPost, ActionName("DeleteImg")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteImgConfirmed(Guid id)
        {

            Percorsi percorso = _context.Percorsi.Find(id);
            string path = StaticUtils.GetImgPath("Percorsi\\", percorso.id.ToString(), percorso.nomefile, _opt.Value.Pathfile.Basepath);
            string paththumb = StaticUtils.GetThumbImgPath("Percorsi\\", percorso.id.ToString(), percorso.nomefile, _opt.Value.Pathfile.Basepath);


            if (System.IO.File.Exists(path))
            {
                try
                {
                    System.IO.File.Delete(path);
                    System.IO.File.Delete(paththumb);
                    ViewBag.deleteSuccess = "true";
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                }


            }

            percorso.nomefile = null;

            percorso.credits = null;
            _context.Entry(percorso).State = EntityState.Modified;
            _context.SaveChanges();

            return RedirectToAction("Edit", "Percorsi", new { id = id });


        }
        
        public ActionResult ShowHidden(Guid percorso)
        {

            Percorsi percorsi = _context.Percorsi.Find(percorso);

            percorsi.attivo = !percorsi.attivo;
            _context.Entry(percorsi).State = EntityState.Modified;
            _context.SaveChanges();

            return RedirectToAction("Index", "Percorsi");

        }

        public bool StatoIndanomalo(Guid? percorso)
        {
            bool allarme = false;
            IEnumerable<IndividuiPercorso> listaindividui = _context.IndividuiPercorso.Where(x => x.percorso == percorso).ToList();

            foreach (var i in listaindividui)
            {
                StoricoIndividuo storico = _context.StoricoIndividuo.Include(x => x.statoIndividuoNavigation).Where(x => x.individuo == i.individuo).OrderByDescending(x => x.dataInserimento).FirstOrDefault();
                if (storico != null)
                {

                    if (storico.statoIndividuoNavigation.stato.Trim().ToLower() != "vivo")
                    {
                        allarme = true;

                    }


                }
            }
            return allarme;

        }


    }
}