using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UPlant.Models;
using UPlant.Models.DB;
using Microsoft.AspNetCore.Http;
using Aspose.Drawing;
using Aspose.Drawing.Imaging;
using Aspose.Drawing.Drawing2D;

namespace UPlant.Controllers
{
    [Authorize(Roles = "Administrator,Operator,Reader")]
    public class IndividuiController : BaseController
    {
        private readonly Entities _context;

        private readonly IOptions<AppSettings> _opt;
        private readonly IWebHostEnvironment _env;

       public IndividuiController(Entities context, IOptions<AppSettings> opt,IWebHostEnvironment env)
        {
            _context = context;
            _opt = opt;
            _env = env;


        }
        public async Task<IActionResult> Index(string valida)
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            Guid organizzazione = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@"))).Select(a => a.Organizzazione).FirstOrDefault();
            if (valida == null)
            {
                return View(await _context.Individui.Include(i => i.accessioneNavigation).Include(i => i.cartellinoNavigation).Include(i => i.collezioneNavigation).Include(i => i.propagatoModalitaNavigation).Include(i => i.sessoNavigation).Include(i => i.settoreNavigation).Where(a => a.accessioneNavigation.organizzazione == organizzazione).ToListAsync());

            }
            else
            {

                return View(await _context.Individui.Include(i => i.accessioneNavigation).Include(i => i.cartellinoNavigation).Include(i => i.collezioneNavigation).Include(i => i.propagatoModalitaNavigation).Include(i => i.sessoNavigation).Include(i => i.settoreNavigation).Where(a => a.accessioneNavigation.organizzazione == organizzazione && a.validazione == false).ToListAsync());

            }

        }
        public async Task<IActionResult> Details(Guid? id, string tipo)
        {
            if (id == null || _context.Individui == null)
            {
                return NotFound();
            }
            //Storico storico = await _context.Storico.Where(x => x.individuo == id).OrderByDescending(x => x.dataInserimento).FirstOrDefaultAsync(m => m.individuoNavigation.id == id);
            //Individui individui = await _context.Individui.FindAsync(id);
            var individui = await _context.Individui

                 .Include(i => i.accessioneNavigation).ThenInclude(i => i.specieNavigation)
                 .Include(i => i.cartellinoNavigation)
                 .Include(i => i.collezioneNavigation)

                 .Include(i => i.propagatoModalitaNavigation)
                 .Include(i => i.sessoNavigation)
                 .Include(i => i.settoreNavigation)
                 .FirstOrDefaultAsync(m => m.id == id);


            //   .Include(i => i.individuoNavigation)
            // .FirstOrDefaultAsync(m => m.individuoNavigation.id == id);


            if (individui == null)
            {
                return NotFound();
            }

            ViewBag.list = await _context.StoricoIndividuo.Include(i => i.condizioneNavigation).Include(i => i.statoIndividuoNavigation).Include(i => i.utenteNavigation).Where(x => x.individuo == id).ToListAsync();
            ViewBag.idindividuo = id;
            ViewBag.tipo = tipo;
            ViewBag.list = individui.StoricoIndividuo.OrderByDescending(x => x.dataInserimento).ToList();
            individui.StoricoIndividuo = ViewBag.list;
            //    ViewBag.list = individui.ListaStoricoIndividui.OrderByDescending(x => x.dataInserimento).ToList();
            //            individui.ListaStoricoIndividui = ViewBag.list;
            ViewBag.list2 = await _context.ImmaginiIndividuo.Include(i => i.individuoNavigation).Where(x => x.individuo == id).ToListAsync();
            ViewBag.list2 = individui.ImmaginiIndividuo.OrderByDescending(x => x.dataInserimento).ToList();
            individui.ImmaginiIndividuo = ViewBag.list2;



            return View(individui);
        }


        //creo vista parziale per il dettaglio
        public ActionResult UploadImg(Guid? id, string tipo)
        {

            if (id == null)
            {
                return NotFound();
            }

            ViewBag.listaimg = _context.ImmaginiIndividuo.Where(a => a.individuo == id);
            ViewBag.individuo = id;
            ViewBag.tipo = tipo;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImg(IEnumerable<IFormFile> files, Guid idindividuo, string descrizione, string credits, string tipo)

        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            string autore = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@"))).Select(a => a.Name + " " + a.LastName).FirstOrDefault();

           // var cognome = @User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "family_name").FirstOrDefault()?.Value;
           // var nome = @User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "given_name").FirstOrDefault()?.Value;
            // Do something with your files

            var t = _opt.Value;

            foreach (var file in files)
            {
             

                if (file.Length > 0 && file.Length <= Convert.ToDecimal(t.Pathfile.LimitMaxUpload))


                    try
                    {
                        int posizione1 = file.FileName.LastIndexOf(".");
                        string estensione1 = file.FileName.Substring(posizione1);
                        if (estensione1.ToLower() == ".heic" || estensione1.ToLower() == ".heif" || estensione1 == ".hevc" || estensione1.ToLower() == ".png") //ho modificato il file .heic in jpeg forzatamente
                        {
                            estensione1 = ".jpg";
                        }

                        var containdimma = _context.ImmaginiIndividuo.Where(x => x.individuo == idindividuo).Count();
                        ImmaginiIndividuo immagini = new ImmaginiIndividuo();
                        
                        //immagini.id = Guid.NewGuid().ToString("N");
                        immagini.individuo = idindividuo;
                        immagini.descrizione = descrizione;
                        immagini.dataInserimento = DateTime.Now;
                        immagini.autore = autore;
                        immagini.nomefile = idindividuo.ToString() + '_' + DateTime.Now.ToString("yyyyMMddHHmm") + estensione1; //file.FileName.ToLower(); //se il nome ha estensione maiuscola crea problemi
                        immagini.visibile = false;
                        if (containdimma == 0)
                        {
                            immagini.predefinita = true;
                        }
                        else
                        {
                            immagini.predefinita = false;
                        }
                        immagini.credits = credits;

                        if (ModelState.IsValid)
                        {
                          
                            _context.ImmaginiIndividuo.Add(immagini);
                            await _context.SaveChangesAsync();
                        }



                        int posizione = immagini.nomefile.LastIndexOf(".");
                        string estensione = immagini.nomefile.Substring(posizione);
                        if (estensione.ToLower() == ".heic" || estensione.ToLower() == ".heif" || estensione == ".hevc" || estensione.ToLower() == ".png") //ho modificato il file .heic in jpeg forzatamente
                        {
                            estensione = ".jpg";
                        }
                        string filename = StaticUtils.SetImgPath(immagini.individuo.ToString(), immagini.id + estensione,t.Pathfile.Basepath);

                        if (file.Length > 0)
                        {
                            string filePath = Path.Combine(t.Pathfile.Basepath, filename);
                            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }
                        }


                        //file.SaveAs(filename);
                        //creo thumb
                        string path = StaticUtils.GetImgPath(immagini.individuo.ToString(), immagini.id.ToString(), immagini.nomefile, t.Pathfile.Basepath);
                        if (System.IO.File.Exists(path))
                        {
                            string filenamethumb = StaticUtils.SetThumbImgPath(immagini.individuo.ToString(), immagini.id + estensione, t.Pathfile.Basepath);
                            StaticUtils.ResizeAndSave(filename, filenamethumb, 400, true);

                        }

                        TempData["MsgSuc"] = "Immagine inserita con successo";

                    }
                    catch (Exception ex)
                    {
                        TempData["MsgErr"] = "Errore nel salvataggio del file o nel DB, dettagli :" + ex.Message.ToString();
                        return RedirectToAction("Details", "Individui", new { id = idindividuo, tipo = tipo });
                    }
                else
                {
                    TempData["MsgErr"] = "File vuoto o troppo grande";
                    return RedirectToAction("Details", "Individui", new { id = idindividuo, tipo = tipo });
                }


            }

            return RedirectToAction("Details", "Individui", new { id = idindividuo, tipo = tipo });






        }
        public ActionResult ViewImg(Guid individuo, string img, string filename)
        {
            
            string filePath = StaticUtils.GetThumbImgPath(individuo.ToString(), img, filename, _opt.Value.Pathfile.Basepath);
            var fileExists = System.IO.File.Exists(filePath);
            
            if (fileExists)
            {
                var fs = System.IO.File.OpenRead(filePath);
                return File(fs, "image/jpeg", filename);
            }
            else
            {
                AddPageAlerts(PageAlertType.Warning, "Immagine dell'individuo non presente");
                TempData["message"] = "Immagine dell'individuo non presente";

                return RedirectToAction("Details", "Individui", new { id = individuo });
            }
           



        }
        public ActionResult Download(Guid individuo, string img, string filename)
        {



            string filePath = StaticUtils.GetImgPath(individuo.ToString(), img, filename, _opt.Value.Pathfile.Basepath);
            
            var fileExists = System.IO.File.Exists(filePath);
            var fs = System.IO.File.OpenRead(filePath);
            return File(fs, "image/jpeg", filename);


        }
        
        public async Task<IActionResult> ShowHidden(Guid individuo, Guid img, string tipo)
        {
            var immagini = await _context.ImmaginiIndividuo
                .Include(i => i.individuoNavigation)
                .FirstOrDefaultAsync(m => m.id == img);

          //  ImmaginiIndividuo immagini = _context.ImmaginiIndividuo.Find(img);

            immagini.visibile = !immagini.visibile;
            _context.Entry(immagini).State = EntityState.Modified;
            _context.SaveChanges();

            return RedirectToAction("Details", "Individui", new { id = individuo, tipo = tipo });

        }
        public ActionResult FlipThumb(Guid individuo, string img, string filename, string tipo)
        {
            try
            {
                string path = StaticUtils.GetThumbImgPath(individuo.ToString(), img, filename, _opt.Value.Pathfile.Basepath);
                Aspose.Drawing.Image imgPhoto = Aspose.Drawing.Image.FromFile(path);
                if (imgPhoto != null)
                {
                    imgPhoto.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    imgPhoto.Save(path);
                }


            }
            catch (Exception ex)
            {
                TempData["MsgErr"] = "Errore FlitThumb :" + ex.Message.ToString();
                return RedirectToAction("Details", "Individui", new { id = individuo, tipo = tipo });
            }
            return RedirectToAction("Details", "Individui", new { id = individuo, tipo = tipo });
        }

        //public async Task<IActionResult> Details(Guid? id)
        public async Task<IActionResult> Default(Guid individuo, Guid img, string tipo)
        {

            var gruppoimmagini = _context.ImmaginiIndividuo.Where(x => x.individuo == individuo).ToList();

            foreach (ImmaginiIndividuo imma in gruppoimmagini)
            {
                imma.predefinita = false;
            }
            _context.SaveChanges();


            var immagini = await _context.ImmaginiIndividuo
                .Include(i => i.individuoNavigation)
                .FirstOrDefaultAsync(m => m.id == img);
           
            immagini.predefinita = true;
            _context.Entry(immagini).State = EntityState.Modified;
            _context.SaveChanges();

            return RedirectToAction("Details", "Individui", new { id = individuo, tipo = tipo });

        }



        public ActionResult DeleteImg(Guid? id, string tipo)
        {
            if (id == null)
            {
                AddPageAlerts(PageAlertType.Error, "Errore nella cancellazione");
                TempData["message"] = "Errore nella cancellazione";
                return PartialView();
            }
            ViewBag.individuo = id;
            ViewBag.tipo = tipo;

            return PartialView();
        }

        // POST
        [HttpPost, ActionName("DeleteImg")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteImgConfirmed(Guid id, string tipo)
        {
            
            ImmaginiIndividuo immagini = _context.ImmaginiIndividuo.Find(id);
            Guid numeroindividuo = immagini.individuo;

            string path = StaticUtils.GetImgPath(immagini.individuo.ToString(), immagini.id.ToString(), immagini.nomefile, _opt.Value.Pathfile.Basepath);
            string paththumb = StaticUtils.GetThumbImgPath(immagini.individuo.ToString(), immagini.id.ToString(), immagini.nomefile, _opt.Value.Pathfile.Basepath);
            if (System.IO.File.Exists(path))
            {
                try
                {

                    System.IO.File.SetAttributes(path, FileAttributes.Normal);
                    System.IO.File.Delete(path);
                    System.IO.File.SetAttributes(paththumb, FileAttributes.Normal);
                    System.IO.File.Delete(paththumb);
                    ViewBag.deleteSuccess = "true";
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                }


            }

            if (immagini.predefinita == true)
            {
                IEnumerable<ImmaginiIndividuo> gruppoimmagini = _context.ImmaginiIndividuo.Where(x => x.individuo == numeroindividuo && x.predefinita == false).ToList();
                //sostituisco predefinita
                int numerorighe = 0;
                if (gruppoimmagini.Count() > 0)
                {
                    foreach (var x in gruppoimmagini)
                    {

                        numerorighe = numerorighe + 1;

                        if (numerorighe == 1)
                        {
                            x.predefinita = true;
                            _context.Entry(x).State = EntityState.Modified;

                            _context.SaveChanges();
                        }
                    }
                }
            }
            _context.ImmaginiIndividuo.Remove(immagini);
            _context.SaveChanges();

            return RedirectToAction("Details", "Individui", new { id = numeroindividuo, tipo = tipo });


        }







        // GET: Individui/Create

        public ActionResult Create(Guid idaccessione,string tipo)
        {
            var accessione =  _context.Accessioni
                .Include(s => s.specieNavigation)
                .FirstOrDefault(m => m.id == idaccessione);

            //Accessioni accessione = _context.Accessioni.specieNavigation.Find(idaccessione);
            //ViewBag.descr_acc = accessione.denominazione;
            ViewBag.accessione = accessione.id;
            ViewBag.nome_scientifico = accessione.specieNavigation.nome_scientifico;
            //ViewBag.individuo = idindividuo;
            ViewBag.progressivo = accessione.progressivo;
            ViewBag.sesso = new SelectList(_context.Sesso.OrderByDescending(a => a.descrizione), "id", "descrizione");
            ViewBag.collezione = new SelectList(_context.Collezioni.OrderBy(a => a.collezione), "id", "collezione");
            ViewBag.settore = new SelectList(_context.Settori.OrderBy(a => a.ordinamento), "id", "settore");

            ViewBag.cartellino = new SelectList(_context.Cartellini.OrderBy(a => a.ordinamento), "id", "descrizione");
            ViewBag.propagatoModalita = new SelectList(_context.ModalitaPropagazione.OrderBy(a => a.ordinamento), "id", "propagatoModalita");
            ViewBag.statoindividuo = new SelectList(_context.StatoIndividuo.OrderBy(a => a.ordinamento), "id", "stato");
            // ViewBag.statoindividuo = new SelectList(_context.StatoIndividuo.Where(x => x.stato.ToLower().Contains("non definito")).Concat(source2: _context.StatoIndividuo.OrderByDescending(a => a.stato).Where(x => !x.stato.ToLower().Contains("non definito"))), "id", "stato");
            ViewBag.condizione = new SelectList(_context.Condizioni.OrderBy(p => p.ordinamento), "id", "condizione");
            ViewBag.accvecprog = accessione.vecchioprogressivo;
            return View();
        }

        // POST: Individui/Create
        // Per proteggere da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per ulteriori dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "id,accessione,individuo,progressivo,sesso,propagatoData,propagatoModalita,settore,collezione,indexSeminum,destinazioni,note")] Individui individui)
        public async Task<IActionResult> Create(Guid accessione, Guid idindividuo, string progressivo,
                                   Guid sesso, string propagatoData, Guid propagatoModalita, Guid settore, Guid collezione,// non mi serve la famiglia e il genere
                                   bool indexSeminum, string destinazioni, string note, Guid statoindividuo,
                                   Guid condizione, string operazioniColturali, Guid cartellino, string vecchioprogressivo, string accvecprog, string latitudine, string longitudine,string tipo)  // solo campi 


        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            Guid utente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@"))).Select(a => a.Id).FirstOrDefault();


            Individui individuo = new Individui();
            //individuo.id = Guid.NewGuid().ToString("N");
            individuo.progressivo = nextProg(accessione);
            individuo.accessione = accessione;
            individuo.individuo = null;
            individuo.sesso = sesso;
            individuo.cartellino = cartellino;
            individuo.propagatoData = DateTime.Parse(propagatoData);
            individuo.propagatoModalita = propagatoModalita;
            individuo.settore = settore;

            if (collezione != Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                individuo.collezione = collezione;
            } else
            {
                var collezionenon = await _context.Collezioni.Where(x => x.settore == settore && x.collezione.Contains("Non ")).Select(x => x.id).FirstOrDefaultAsync();
                individuo.collezione = collezionenon;
            }
            
            individuo.indexSeminum = indexSeminum;
            individuo.destinazioni = destinazioni;
            individuo.note = note;
            individuo.latitudine = latitudine;
            individuo.longitudine = longitudine;
            
            if (!String.IsNullOrEmpty(accvecprog))
            {
                if (!String.IsNullOrEmpty(vecchioprogressivo))
                {
                    individuo.vecchioprogressivo = accvecprog + "/" +vecchioprogressivo;
                }
                else
                {
                    individuo.vecchioprogressivo = accvecprog;
                }
               
            }

            individuo.validazione = true;
            /*  if (Session["ruolo"].ToString() == "Amministratore")
              {
                  individuo.validazione = true;
              }
              else
              {
                  individuo.validazione = false;
              }*/
            StoricoIndividuo storico = new StoricoIndividuo();
            //storico.id = Guid.NewGuid().ToString("N");
            storico.individuo = individuo.id;
            storico.statoIndividuo = statoindividuo;
            storico.condizione = condizione;
            storico.operazioniColturali = operazioniColturali;
            storico.dataInserimento = DateTime.Now;
            storico.utente = utente;

            if (ModelState.IsValid)
            {
                _context.Individui.Add(individuo);
                await _context.SaveChangesAsync();

                Individui individuotrovato = _context.Individui.Find(individuo.id);
                if (individuotrovato != null)
                {
                    storico.individuo = individuotrovato.id;
                    _context.StoricoIndividuo.Add(storico);
                    await _context.SaveChangesAsync();
                    // return RedirectToAction(nameof(Details), nameof(Accessioni), individuo.accessione);
                    return RedirectToAction(nameof(Details), nameof(Accessioni), new { id = individuo.accessione, tipo = tipo });
                  
                }


            }



            ViewBag.propagatoModalita = new SelectList(_context.ModalitaPropagazione, "id", "propagatoModalita", propagatoModalita);
            ViewBag.sesso = new SelectList(_context.Sesso, "id", "descrizione", individuo.sesso);
            ViewBag.collezione = new SelectList(_context.Collezioni.OrderBy(a => a.collezione), "id", "collezione", collezione);
            ViewBag.settore = new SelectList(_context.Settori.OrderBy(a => a.ordinamento), "id", "settore", settore);
            ViewBag.cartellino = new SelectList(_context.Cartellini, "id", "descrizione", individuo.cartellino);
            ViewBag.statoindividuo = new SelectList(_context.StatoIndividuo.OrderByDescending(p => p.ordinamento), "id", "stato", statoindividuo);
            ViewBag.condizione = new SelectList(_context.Condizioni, "id", "condizione", condizione);
            ViewBag.latitudine = latitudine;
            ViewBag.longitudine = longitudine;
            return View(individuo);
        }

        // GET: Individui/Edit/5
        public async Task<IActionResult> Edit(Guid? id, string tipo)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }
            Individui individui = await _context.Individui.Include(i => i.accessioneNavigation).ThenInclude(i => i.specieNavigation)
                .Include(i => i.cartellinoNavigation)
                .Include(i => i.collezioneNavigation)
                
                .Include(i => i.propagatoModalitaNavigation)
                .Include(i => i.sessoNavigation)
                .Include(i => i.settoreNavigation)
                .FirstOrDefaultAsync(m => m.id == id);

            if (individui == null)
            {
                return NotFound();
            }
            ViewBag.tipo = tipo;
            ViewBag.propagatoModalita = new SelectList(_context.ModalitaPropagazione.OrderBy(a => a.ordinamento), "id", "propagatoModalita", individui.propagatoModalita);
            ViewBag.sesso = new SelectList(_context.Sesso, "id", "descrizione", individui.sesso);
            ViewBag.settore = new SelectList(_context.Settori.OrderBy(a => a.ordinamento), "id", "settore", individui.settore);
            ViewBag.collezione = new SelectList(_context.Collezioni.Where(x => x.settore == individui.settore).OrderBy(a => a.collezione), "id", "collezione", individui.collezione);
            ViewBag.cartellino = new SelectList(_context.Cartellini.OrderBy(a => a.ordinamento), "id", "descrizione", individui.cartellino);

            var storicoesite = _context.StoricoIndividuo.Where(x => x.individuo == individui.id).FirstOrDefault();
            if (storicoesite != null)
            {
                StoricoIndividuo storico = _context.StoricoIndividuo.Where(x => x.individuo == individui.id).OrderByDescending(x => x.dataInserimento).First();
                ViewBag.statoindividuo = _context.StatoIndividuo.Where(x => x.id == storico.statoIndividuo).Single().stato;
                ViewBag.condizione = _context.Condizioni.Where(x => x.id == storico.condizione).Single().condizione;
                ViewBag.operazioniColturali = storico.operazioniColturali;
                ViewBag.esiste = "ok";
            }

            return View(individui);
        }

        // POST: Individui/Edit/5
        // Per proteggere da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per ulteriori dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //  public ActionResult Edit([Bind(Include = "id,accessione,individuo,progressivo,sesso,propagatoData,propagatoModalita,settore,collezione,cartellino,statoindividuo,condizione,indexSeminum,destinazioni,note")] Individui individui)
        public async Task<IActionResult> Edit(Guid id, Guid sesso, string propagatoData, Guid propagatoModalita, Guid settore, Guid collezione,
                                   bool indexSeminum, string destinazioni, string note, string statoindividuo,
                                   Guid condizione, string operazioniColturali, Guid cartellino, string vecchioprogressivo, string longitudine, string latitudine,string tipo)



        {
            Individui individui = await _context.Individui.Include(i => i.accessioneNavigation)
                .Include(i => i.propagatoModalitaNavigation)
                .Include(i => i.cartellinoNavigation)
                .Include(i => i.collezioneNavigation)
                .Include(i => i.propagatoModalitaNavigation)
                .Include(i => i.sessoNavigation)
                .Include(i => i.settoreNavigation)
                .FirstOrDefaultAsync(m => m.id == id);



            // Storico storico = db.Storico.Where(x => x.individuo == id).Single(); 
            StoricoIndividuo storico = _context.StoricoIndividuo.Where(x => x.individuo == individui.id).OrderByDescending(x => x.dataInserimento).First();
            //var storicoesite = _context.Storico.Where(x => x.individuo == individui.id).FirstOrDefault();
            if (ModelState.IsValid)
            {

                individui.sesso = sesso;
                if (propagatoData != "")
                {
                    individui.propagatoData = DateTime.Parse(propagatoData);
                }
                individui.propagatoModalita = propagatoModalita;
                individui.settore = settore;
                individui.collezione = collezione;
                individui.cartellino = cartellino;
                individui.indexSeminum = indexSeminum;
                individui.destinazioni = destinazioni;
                individui.note = note;
                individui.vecchioprogressivo = vecchioprogressivo;
                individui.longitudine = longitudine;
                individui.latitudine = latitudine;

                _context.Entry(individui).State = EntityState.Modified;
                //db.Entry(storico).State = EntityState.Modified;

                //storico.operazioniColturali = operazioniColturali;
                //storico.condizione = condizione;
                //storico.statoIndividuo = statoindividuo;



                _context.SaveChanges();


               
                // passare ultimi valori stato individuo
                return RedirectToAction(nameof(Create), nameof(StoricoIndividuo), new { idindividuo = individui.id, tipo= tipo ,damodifica = "ok" });
              //  return RedirectToAction("../StoricoIndivi/Create", new { idindividuo = individui.id, damodifica = "ok" });
            }
            ViewBag.propagatoModalita = new SelectList(_context.ModalitaPropagazione.OrderBy(a => a.ordinamento), "id", "propagatoModalita", individui.propagatoModalita);
            ViewBag.sesso = new SelectList(_context.Sesso, "id", "descrizione", individui.sesso);
            ViewBag.settore = new SelectList(_context.Settori.OrderBy(a => a.ordinamento), "id", "settore", individui.settore);
            ViewBag.collezione = new SelectList(_context.Collezioni.Where(x => x.settore == individui.settore).OrderBy(a => a.collezione), "id", "collezione", individui.collezione);
            ViewBag.cartellino = new SelectList(_context.Cartellini.OrderBy(a => a.ordinamento), "id", "descrizione", individui.cartellino);
            //ViewBag.statoindividuo = new SelectList(_context.StatoIndividuo.OrderByDescending(p => p.stato), "id", "stato", individui.statoindividuo);
            // ViewBag.condizione = new SelectList(_context.Condizioni, "id", "condizione", individui.condizione);
            if (storico != null)
            {
                // Storico storico = _context.Storico.Where(x => x.individuo == individui.id).OrderByDescending(x => x.dataInserimento).First();
                ViewBag.statoindividuo = _context.StatoIndividuo.Where(x => x.id == storico.statoIndividuo).Single().stato;
                ViewBag.condizione = _context.Condizioni.Where(x => x.id == storico.condizione).Single().condizione;
                ViewBag.operazioniColturali = storico.operazioniColturali;
                ViewBag.esiste = "ok";
            }


            if (TempData["MsgAle"] != null)
                ViewBag.MsgAle = TempData["MsgAle"];
            if (TempData["MsgErr"] != null)
                ViewBag.MsgErr = TempData["MsgErr"];
            if (TempData["MsgAck"] != null)
                ViewBag.MsgAck = TempData["MsgAck"];
            return View(individui);
        }

        // GET: Individui/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new StatusCodeResult(400);
            }
            Individui individui = _context.Individui.Find(id);
            if (individui == null)
            {
                return NotFound();
            }
            return View(individui);
        }

        // POST: Individui/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid? id)
        {

            Individui individui = _context.Individui.Find(id);
            Guid numeroaccessione = individui.accessione;
            IEnumerable<StoricoIndividuo> storici = _context.StoricoIndividuo.Where(s => s.individuo == id);
            IEnumerable<ImmaginiIndividuo> immagini = _context.ImmaginiIndividuo.Where(s => s.individuo == id);
            if (storici != null)
            {
                _context.StoricoIndividuo.RemoveRange(storici);
            }

            if (immagini != null)
            {
                _context.ImmaginiIndividuo.RemoveRange(immagini);
            }

            _context.Individui.Remove(individui);
            _context.SaveChanges();
            return RedirectToAction(nameof(Details), nameof(Accessioni), new { id = numeroaccessione });
           

        }
        private string nextProg(Guid accessione)
        {
            Accessioni accpadre = _context.Accessioni.Find(accessione);
            string acceprogressivo = accpadre.progressivo;
            Individui indivultima = ultima(accessione);
            string retnextprog;

            if (indivultima != null)
            {


                int ult;
                string ultimoprogressivo = indivultima.progressivo;
                string prova = ultimoprogressivo.Substring(10, 4);
                Int32.TryParse(ultimoprogressivo.Substring(10, 4), out ult);
                ult++;
                retnextprog = acceprogressivo + "/" + ult.ToString().PadLeft(4, '0');
            }
            else
            {
                retnextprog = acceprogressivo + "/0001";
            }
            return retnextprog;
        }

        private Individui ultima(Guid accpadre)
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            Guid organizzazione = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@"))).Select(a => a.Organizzazione).FirstOrDefault();
            return _context.Individui.Where(c => c.accessione == accpadre).OrderByDescending(c => c.progressivo).FirstOrDefault();
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult ValidaIndividuo(string id)
        {
            if (id == null)
            {
                return new StatusCodeResult(400);
            }
            Individui individui = _context.Individui.Find(id);
            if (individui == null)
            {
                return NotFound();
            }
            return View(individui);
        }

        // POST: Accessioni/Delete/5
        [HttpPost, ActionName("ValidaIndividuo")]
        [ValidateAntiForgeryToken]
        public ActionResult ValidaIndividuoConfirmed(string id)
        {

            Individui individui = _context.Individui.Find(id);
            individui.validazione = true;
            _context.SaveChanges();
            return RedirectToAction("../Accessioni/Details/" + individui.accessione);


        }

        public ActionResult ValidaTutto(string all)
        {
            if (all == null)
            {
                return new StatusCodeResult(400);
            }
            return View();
        }




        // POST: Accessioni/Delete/5
        [HttpPost, ActionName("ValidaTutto")]
        [ValidateAntiForgeryToken]
        public ActionResult ValidaTuttoConfirmed(string all)
        {
            if (all == "yes")
            {

                var gruppo = _context.Individui.Where(i => i.validazione == false).ToList();
                foreach (Individui ind in gruppo)
                {
                    ind.validazione = true;
                }
                _context.SaveChanges();
                return RedirectToAction("Index", "Home");
                //Redirect("~")
            }
            else
            {
                return new StatusCodeResult(400);
            }


        }
    }
}
