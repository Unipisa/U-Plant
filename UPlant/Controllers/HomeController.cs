using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UPlant.Models.DB;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Path = System.IO.Path;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UPlant.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using DocumentFormat.OpenXml.Bibliography;
using X14 = DocumentFormat.OpenXml.Office2010.Excel;
using X15 = DocumentFormat.OpenXml.Office2013.Excel;

using Newtonsoft.Json;
using System.Collections;
using System.Collections.Frozen;



namespace UPlant.Controllers
{
    public class HomeController : BaseController
    {
        
        private readonly Entities _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private IWebHostEnvironment _environment;
        private readonly LanguageService _languageService;
        private readonly IOptions<AppSettings> _opt;
        public HomeController(Entities context, IWebHostEnvironment environment, IHttpContextAccessor contextAccessor , IOptions<AppSettings> opt, LanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
            _contextAccessor = contextAccessor;
            _environment = environment;
            _opt = opt;
        }
        //iniziovecchio codice
        public ActionResult PreviewAndPrintLabel()
        {
            return View();
        }


        public ActionResult Index()
        {
            var linguacorrente = System.Globalization.CultureInfo.CurrentCulture.Name;
            /*var articleQuery = from a in _context.Individui
                               select new { a.propagatoData, a.settore };
            // codice utilizzato insieme al modello Datapoint contenuto dentro DataPoint.cs per la creazione dei Chart
            List<DataPoint> dataPoints = new List<DataPoint>();

            foreach (var item in articleQuery)
            {
                double stockPriceClose = double.Parse(item.settore.ToString());
                DateTime date =item.propagatoData;
                dataPoints.Add(new DataPoint(item.propagatoData, stockPriceClose));
                new DataPoint(, stockPriceClose);
            }

            dataPoints.Add(new DataPoint("1", _context.Accessioni.Count()));
            dataPoints.Add(new DataPoint("Individui", _context.Individui.Count()));
            dataPoints.Add(new DataPoint("Index Seminum", _context.Individui.Where(x => x.indexSeminum == true ).Count()));
           

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
           */
            ViewBag.thumb = "Genera Thumb";




            /*
                        ViewBag.fam = famiglie;
                        ViewBag.gen = generi;
                        ViewBag.spe = specie;
                        ViewBag.nur = nursery;
                        ViewBag.ban = banca;*/
            if(_context.Contafamiglie.Count() > 0 || _context.Contafamiglie != null)
            {
                ViewBag.fam = _context.Contafamiglie.Count();
            } else
            {
                ViewBag.fam = 0;
            }
            if (_context.Contageneri.Count() > 0 || _context.Contageneri != null)
            {
                ViewBag.gen = _context.Contageneri.Count();
            }
            else
            {
                ViewBag.gen = 0;
            }
            if (_context.Contaspecie.Count() > 0 || _context.Contaspecie != null)
            {
                ViewBag.spe = _context.Contaspecie.Count();
            }
            else
            {
                ViewBag.spe = 0;
            }
           
            if (_context.Contaspecienursery.Count() > 0 || _context.Contaspecienursery != null)
            {
                ViewBag.nur = _context.Contaspecienursery.Count();
            }
            else
            {
                ViewBag.nur = 0;
            }
            if (_context.Contaspecieindexseminum.Count() > 0 || _context.Contaspecieindexseminum != null)
            {
                ViewBag.ban = _context.Contaspecieindexseminum.Count();
            }
            else
            {
                ViewBag.ban = 0;
            }
            if (_context.Specie.Count() > 0 || _context.Specie != null)
            {
                ViewBag.cens = _context.Specie.Count();
            }
            else
            {
                ViewBag.cens  = 0;
            }
            /* ViewBag.gen = _context.contageneri.Count();
             ViewBag.spe = _context.contaspecie.Count();
             ViewBag.nur = _context.contaspecienursery.Count();
             ViewBag.ban = _context.contaspecieindexseminum.Count(); 
             ViewBag.cens = _context.Specie.Count();*/
            return View();
        }




        public ActionResult Grafico1()
        {
            ViewBag.fam = _context.Contafamiglie.Count();
            ViewBag.gen = _context.Contageneri.Count();
            ViewBag.spe = _context.Contaspecie.Count();
            ViewBag.nur = _context.Contaspecienursery.Count();
            ViewBag.ban = _context.Contaspecieindexseminum.Count();
            ViewBag.cens = _context.Specie.Count();
            
            return null;
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }




        public ActionResult RicercaIndividui()
        {
            var linguacorrente = _languageService.GetCurrentCulture();


            if (linguacorrente == "en-US")
            {
                ViewData["listasettori"] = new SelectList(_context.Settori.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.settore_en) ? a.settore : a.settore_en }), "id", "Desc");
                ViewData["listacollezioni"] = new SelectList(_context.Collezioni.OrderBy(a => a.collezione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.collezione_en) ? a.collezione : a.collezione_en }), "id", "Desc");
                ViewData["listastatoindividui"] = new SelectList(_context.StatoIndividuo.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.stato : a.descrizione_en }), "id", "Desc");
                ViewData["listacondizioni"] = new SelectList(_context.Condizioni.OrderBy(p => p.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.condizione : a.descrizione_en }), "id", "Desc");
                ViewData["listacartellini"] = new SelectList(_context.Cartellini.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc");

            }
            else
            {

                ViewData["listasettori"] = new SelectList(_context.Settori.OrderBy(a => a.settore), "id", "settore");
                ViewData["listacollezioni"] = new SelectList(_context.Collezioni.OrderBy(a => a.collezione), "id", "collezione");
                ViewData["listastatoindividui"] = new SelectList(_context.StatoIndividuo.OrderBy(a => a.stato), "id", "stato");
                ViewData["listacondizioni"] = new SelectList(_context.Condizioni.OrderBy(a => a.condizione), "id", "condizione");
                ViewData["listacartellini"] = new SelectList(_context.Cartellini.OrderBy(a => a.descrizione), "id", "descrizione");
            }




            ViewData["listafamiglie"] = new SelectList(_context.Famiglie.OrderBy(e => e.descrizione), "id", "descrizione");
            ViewData["famiglia"]= "";
            ViewBag.datapropagazioneinizio = new DateTime(1543, 1, 1, 0, 0, 0);
            ViewBag.datapropagazionefine = DateTime.Now;
            ViewData["settore"] = "";
            ViewData["collezione"] = "";
            ViewData["statoindividuo"] = "";
            ViewData["condizione"] = "";
            ViewData["cartellino"] = "";
          

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RicercaIndividui(string famiglia, string specie, string progacc, Guid? settore, Guid? collezione, DateTime datapropagazioneinizio, DateTime datapropagazionefine, Guid? statoindividuo, Guid? condizione, Guid? cartellino)
        {

            var linguacorrente = _languageService.GetCurrentCulture();
            IEnumerable<Ricercaind> listaind = (from m in _context.Ricercaind select m).ToList();

            if (!String.IsNullOrEmpty(famiglia))
            {
                listaind = listaind.Where(a => a.idfamiglia == new Guid(famiglia));
            }
            if (!String.IsNullOrEmpty(specie))
            {
                listaind = listaind.Where(a => a.nome_scientifico.ToLower().Contains(specie.ToLower()));

            }
            if (!String.IsNullOrEmpty(progacc))
            {
                listaind = listaind.Where(a => a.progressivoacc.Contains(progacc));

            }
            if (settore != null)
            {
                listaind = listaind.Where(a => a.idsettore == settore);
            }
            if (collezione != null)
            {
                listaind = listaind.Where(a => a.idcollezione == collezione);
            }

            if (datapropagazioneinizio == DateTime.MinValue && datapropagazionefine == DateTime.MinValue)
            {
                datapropagazionefine = datapropagazionefine.AddSeconds(86399);// servirà se si utilizzarà un dataaquisizione al secondo
                listaind = listaind.Where(a => a.propagatodata >= datapropagazioneinizio && a.propagatodata < datapropagazionefine);
            }

            if (statoindividuo != null)
            {
                listaind = listaind.Where(a => a.idstatoindividuo == statoindividuo);
            }
            if (condizione != null)
            {
                listaind = listaind.Where(a => a.idcondizione == condizione);
            }
            if (cartellino != null)
            {
                listaind = listaind.Where(a => a.idcartellino == cartellino);
            }
            //listaind = listaind.GroupBy(x => x.individuo).Select(g => g.OrderByDescending(p => p.dataInserimento).FirstOrDefault());
            listaind = listaind.OrderByDescending(f => f.progressivo);

            ViewBag.listafamiglie = new SelectList(_context.Famiglie.OrderBy(x => x.descrizione), "id", "descrizione").ToList();
            ViewBag.famiglia = famiglia;
            ViewBag.specie = specie;
            ViewBag.progacc = progacc;
            ViewBag.datapropagazioneinizio = datapropagazioneinizio;
            ViewBag.datapropagazionefine = datapropagazionefine;
            ViewBag.settore = settore;
            ViewBag.collezione = collezione;
            ViewBag.statoindividuo = statoindividuo;
            ViewBag.condizione = condizione;
            ViewBag.cartellino = cartellino;


            if (linguacorrente == "en-US")
            {
                ViewBag.listasettori = new SelectList(_context.Settori.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.settore_en) ? a.settore : a.settore_en }), "id", "Desc").ToList(); 
                ViewBag.listacollezioni = new SelectList(_context.Collezioni.OrderBy(a => a.collezione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.collezione_en) ? a.collezione : a.collezione_en }), "id", "Desc").ToList();
                ViewBag.listastatoindividui = new SelectList(_context.StatoIndividuo.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.stato : a.descrizione_en }), "id", "Desc").ToList();
                ViewBag.listacondizioni = new SelectList(_context.Condizioni.OrderBy(p => p.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.condizione : a.descrizione_en }), "id", "Desc").ToList();
                ViewBag.listacartellini = new SelectList(_context.Cartellini.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc").ToList();

            }
            else
            {

                ViewBag.listasettori = new SelectList(_context.Settori.OrderBy(a => a.settore), "id", "settore").ToList();
                ViewBag.listacollezioni = new SelectList(_context.Collezioni.OrderBy(a => a.collezione), "id", "collezione").ToList();
                ViewBag.listastatoindividui = new SelectList(_context.StatoIndividuo.OrderBy(a => a.stato), "id", "stato").ToList();
                ViewBag.listacondizioni = new SelectList(_context.Condizioni.OrderBy(a => a.condizione), "id", "condizione").ToList();
                ViewBag.listacartellini = new SelectList(_context.Cartellini.OrderBy(a => a.descrizione), "id", "descrizione").ToList();
            }




            IEnumerable<RisultatoRicercaInd> listarisultatoricerca = listaind.Select(r => new RisultatoRicercaInd
            {
                id = r.id,
                progressivo = r.progressivo,
                shortprog = r.progressivo.Substring(10, 4),
                ipen = r.ipen,
                vecchioprogressivo = r.vecchioprogressivo,
                nome_scientifico = r.nome_scientifico,
                settore = r.settore,
                settore_en = r.settore_en,
                collezione = r.collezione,
                collezione_en = r.collezione_en,
                cartellino = r.cartellino,
                cartellino_en = r.cartellino_en,
                stato = r.statoindividuo,
                stato_en = r.statoindividuo_en,
                nomecognome = r.nomecognome,
                datainserimento = string.Format(Convert.ToDateTime(r.datainserimento).ToString(), "{0:yyyy-MM-dd HH:mm:ss}", "yyyy"),
                countimg = _context.ImmaginiIndividuo.Where(a => a.individuo == r.id).Count().ToString(),
                //nomeetichetta = StaticUtils.CleanInput(r.nome_scientifico).Replace("  "," ")
                nomeetichetta = StaticUtils.CleanInput(r.genere).Replace("  ", " ") + " " + StaticUtils.CleanInput(r.nome).Replace("  ", " ")
            });
            ViewData["filename"] = CreaExcelRicerca(null,listaind, "individuo"); 
            
            ViewData["famiglia"] = famiglia;
            

            return View(listarisultatoricerca);
        }

       



        public ActionResult RicercaAccessioni()
        {
            var linguacorrente = _languageService.GetCurrentCulture();
            ViewBag.listafamiglie = new SelectList(_context.Famiglie.OrderBy(x => x.descrizione), "id", "descrizione").ToList();
            ViewBag.listafornitore = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione), "id", "descrizione").ToList();
            ViewBag.listaraccoglitore = new SelectList(_context.Raccoglitori.OrderBy(a => a.nominativo), "id", "nominativo").ToList();
            if (linguacorrente == "en-US")
            {
               
                ViewBag.listatipoacquisizione = new SelectList(_context.TipoAcquisizione.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc").ToList();
                ViewBag.listatipomateriale = new SelectList(_context.TipiMateriale.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc").ToList();
                ViewBag.listagradoincertezza = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc").ToList();
            }
            else
            {
                ViewBag.listatipomateriale = new SelectList(_context.TipiMateriale.OrderBy(a => a.descrizione), "id", "descrizione").ToList();
                ViewBag.listatipoacquisizione = new SelectList(_context.TipoAcquisizione.OrderBy(a => a.descrizione), "id", "descrizione").ToList();
                ViewBag.listagradoincertezza = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione), "id", "descrizione").ToList();
              
            }
            
            
            
            ViewBag.famiglia = "";
            ViewBag.datainserimentoinizio = new DateTime(1543, 1, 1, 0, 0, 0);
            ViewBag.datainserimentofine = DateTime.Now;
            ViewBag.tipomateriale = "";
            ViewBag.tipoacquisizione = "";
            ViewBag.fornitore = "";
            ViewBag.gradoincertezza = "";
            ViewBag.raccoglitore = "";

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RicercaAccessioni(string famiglia, string specie, string progressivo, string vecchioprogressivo, DateTime datainserimentoinizio, DateTime datainserimentofine, Guid tipomateriale, Guid tipoacquisizione, Guid fornitore, Guid gradoincertezza, Guid raccoglitore)
        {
            var linguacorrente = _languageService.GetCurrentCulture();
            IEnumerable<Ricercaacc> listaacces = (from m in _context.Ricercaacc select m).AsQueryable();
            // IEnumerable <Accessioni> listaacces = (from m in _context.Accessioni select m).ToList();
            if (!String.IsNullOrEmpty(famiglia))
            {
                listaacces = listaacces.Where(a => a.idfamiglia == new Guid(famiglia));
            }
            if (!String.IsNullOrEmpty(specie))
            {
                listaacces = listaacces.Where(a => (a.nome_scientifico ?? "").ToLower().Contains(specie.ToLower()));
            }
            if (!String.IsNullOrEmpty(progressivo))
            {
               
                listaacces = listaacces.Where(a => (a.progressivo ?? "").Contains(progressivo));
            }
            if (!String.IsNullOrEmpty(vecchioprogressivo))
            {
                listaacces = listaacces.Where(a => (a.vecchioprogressivo ?? "").Contains(vecchioprogressivo));
            }

            if (datainserimentoinizio == DateTime.MinValue && datainserimentofine == DateTime.MinValue)
            {
                datainserimentofine = datainserimentofine.AddSeconds(86399);// servirà se si utilizzarà un dataaquisizione al secondo
                listaacces = listaacces.Where(a => a.dataAcquisizione >= datainserimentoinizio && a.dataAcquisizione < datainserimentofine);
            }

            if (tipomateriale != Guid.Empty)
            {
                listaacces = listaacces.Where(a => a.idtipomateriale == tipomateriale);
            }
            if (tipoacquisizione != Guid.Empty)
            {
                listaacces = listaacces.Where(a => a.idtipoacquisizione == tipoacquisizione);
            }
            if (fornitore != Guid.Empty)
            {
                listaacces = listaacces.Where(a => a.idfornitore == fornitore);
            }
            if (gradoincertezza != Guid.Empty)
            {
                listaacces = listaacces.Where(a => a.idgradoincertezza == gradoincertezza);
            }
            if (raccoglitore != Guid.Empty)
            {
                listaacces = listaacces.Where(a => a.idraccoglitore == raccoglitore);
            }

            listaacces = listaacces.OrderByDescending(x => x.progressivo);

            IEnumerable<RisultatoRicercaAcc> listarisultatoricerca = listaacces.Select(r => new RisultatoRicercaAcc
            {
                id = r.idacc,
                progressivo = r.progressivo,
                vecchioprogressivo = r.vecchioprogressivo,
                nome_scientifico = r.nome_scientifico,
                famiglia = r.famiglia,
                genere = r.genere,
                dataacquisizione = string.Format(Convert.ToDateTime(r.dataAcquisizione).ToString(), "{0:yyyy-MM-dd HH:mm:ss}", "yyyy"),
                tipomateriale = r.tipomateriale,
                tipomateriale_en = r.tipomateriale_en,
                countind = _context.Individui.Where(a => a.accessione == r.idacc).Count().ToString(),
                inseritoda = r.inseritoda,
                modificatoda = r.modificatoda,
                validazione = r.validazione
            });



            ViewBag.listafamiglie = new SelectList(_context.Famiglie.OrderBy(x => x.descrizione), "id", "descrizione").ToList();


            ViewBag.famiglia = famiglia;
            ViewBag.specie = specie;
            ViewBag.progressivo = progressivo;
            ViewBag.vecchioprogressivo = vecchioprogressivo;
            ViewBag.datainserimentoinizio = datainserimentoinizio;
            ViewBag.datainserimentofine = datainserimentofine;
            ViewBag.tipomateriale = tipomateriale;
            ViewBag.tipoacquisizione = tipoacquisizione;
            ViewBag.fornitore = fornitore;
            ViewBag.gradoincertezza = gradoincertezza;
            ViewBag.raccoglitore = raccoglitore;

            if (linguacorrente == "en-US")
            {
                ViewBag.listatipoacquisizione = new SelectList(_context.TipoAcquisizione.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc").ToList();
                ViewBag.listatipomateriale = new SelectList(_context.TipiMateriale.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc").ToList();
                ViewBag.listagradoincertezza = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc").ToList();

            }
            else
            {
                ViewBag.listatipomateriale = new SelectList(_context.TipiMateriale.OrderBy(x => x.descrizione), "id", "descrizione").ToList();
                ViewBag.listatipoacquisizione = new SelectList(_context.TipoAcquisizione.OrderBy(x => x.descrizione), "id", "descrizione").ToList();
                ViewBag.listagradoincertezza = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione), "id", "descrizione").ToList();
            }

               
            
            ViewBag.listafornitore = new SelectList(_context.Fornitori.OrderBy(x => x.descrizione), "id", "descrizione").ToList();
           
            
            
            ViewBag.listaraccoglitore = new SelectList(_context.Raccoglitori.OrderBy(a => a.nominativo), "id", "nominativo").ToList();
            

            //ViewBag.filename = EstraiAccessioni("xlsx", listaacces);
            ViewBag.filename = CreaExcelRicerca(listaacces,null,"accessione");
            
            return View(listarisultatoricerca);
        }




        [HttpGet]
        //Action Filter, it will auto delete the file after download, 
        //I will explain it later
        public ActionResult Download(string file)
        {
            var t = _opt.Value;
            string basepath = t.Pathfile.Docs;
            string fullPath = Path.Combine(basepath, file);
            var mimeType = "application/vnd.ms-excel";
            FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            return File(fileStream, mimeType, file);

        }
       
        public void EstraiAccessioniTotale()
        {

            var listaaccessioni = _context.Accessioni
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.gradoIncertezzaNavigation)
                .Include(a => a.nazioneNavigation)
                .Include(a => a.regioneNavigation)
                .Include(a => a.provinciaNavigation)
                .Include(a => a.organizzazioneNavigation)
                .Include(a => a.provenienzaNavigation)
                .Include(a => a.raccoglitoreNavigation)
                .Include(a => a.specieNavigation)
                .ThenInclude(a => a.genereNavigation)
                .ThenInclude(a => a.famigliaNavigation)
                .Include(a => a.statoMaterialeNavigation)
                .Include(a => a.tipoAcquisizioneNavigation)
                .Include(a => a.tipoMaterialeNavigation)
                .Include(a => a.utenteAcquisizioneNavigation)
                .Include(a => a.utenteUltimaModificaNavigation)
                .Include(a => a.identificatoreNavigation).ToList();
            StringBuilder sb = new StringBuilder();
            //var list = new List<string> {"\"Progressivo Individuo\"", ada("VecchioProgressivo"), ada("Accessione"), ada("Individuo Padre"), ada("Sesso"), ada("PropagatoData"), ada("PropagatoModalità"), ada("Identità Tassonomica"), ada("Settore"), ada("Collezione"), ada("IndexSeminum"), ada("Destinazione"), ada("Note"), ada("Longitudine"), ada("Latitudine"), ada("Cartellino"), ada("Validazione") };
            //var data = string.Join(",", list);
            
            sb.AppendFormat("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23};{24};{25};{26};{27};{28};{29};",
            //0
            ada("Data Acquisizione"),
            //1
            ada("Progressivo"),
            //2
            ada("Vecchio Progressivo"),
            //3
            ada("Ipen"),
            //4
            ada("Ipen di Provenienza"),
            //5
            ada("Utente Acquisizione"),
            //6
            ada("Modalità Acquisizione"),
            //7
            ada("Fornitore/Donatore"),
            //8
            ada("Raccoglitore"),
            //9
            ada("Identificato da"),
            //10
            ada("Provenienza"),
            //11
            ada("Nazione"),
            //12
            ada("Regione"),
            //13
            ada("Provincia"),
            //14
            ada("Località"),
            //15
            ada("Longitudine"),
            //16
            ada("Latitudine"),
            //17
            ada("Altitudine"),
            //18
            ada("Habitat"),
            //19
            ada("Data Raccolta"),
            //20
            ada("Famiglia"),
            //21
            ada("Nome Scientifico"),
            //22
            ada("Tipo Materiale"),
            //23
            ada("Numero Esemplari"),
            //24
            ada("Stato Materiale"),
            //25
            ada("Grado di Incertezza"),
            //26
            ada("Associato Erbario"),
            //27
            ada("Note"),
            //28
            ada("Utente Ultima Modifica"),
            //29
            ada("Data Ultima Modifica"),
            //30
            ada("Validato")
            );
            sb.AppendFormat(Environment.NewLine);
            foreach (var item in listaaccessioni)
            {
                var associatoErbario = "";
                if (item.associatoErbario == false) { associatoErbario = "NO"; } else { associatoErbario = "SI"; }
                var validazione = "";
                if (item.validazione == false) { validazione = "NO"; } else { validazione = "SI"; }
                var altitudine = "";
                if (item.altitudine == null) { altitudine = "Non Definita"; } else { altitudine = item.altitudine.ToString(); }
                var regione = "";
                if (item.regione == null) { regione = "Non Definita"; } else { regione = item.regioneNavigation.descrizione; }
                var provincia = "";
                if (item.provincia == null) { provincia = "Non Definita"; } else { provincia = item.provinciaNavigation.descrizione; }
                var specie= "";
                if (item.specie == Guid.Empty) { specie = "Non Definita"; } else { specie = item.specieNavigation.nome_scientifico; }
                var tipomateriale = "";
                if (item.tipoMateriale == Guid.Empty) { tipomateriale = "Non Definita"; } else { tipomateriale = item.tipoMaterialeNavigation.descrizione; }
                var statomateriale = "";
                if (item.statoMateriale == Guid.Empty) { statomateriale = "Non Definita"; } else { statomateriale = item.statoMaterialeNavigation.descrizione; }
                var gradoincertezza = "";
                if (item.gradoIncertezza == Guid.Empty) { gradoincertezza = "Non Definita"; } else { gradoincertezza = item.gradoIncertezzaNavigation.descrizione; }
                var localita = "";
                if (item.localita == null) { localita = ""; } else { localita = item.localita; }
                var note = "";
                if (item.note == null) { note = ""; } else { note = item.note; }
                var ipendiprovenienza = "";
                if (item.ipendiprovenienza == null) { ipendiprovenienza = ""; } else { ipendiprovenienza = item.ipendiprovenienza; }
                var dataraccolta = "";
                if (item.dataraccolta == null) { dataraccolta = ""; } else { dataraccolta = string.Format("{0:dd/MM/yyyy}", item.dataraccolta); }
                


                sb.AppendFormat("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23};{24};{25};{26};{27};{28};{29};",
                //0
                ada(item.dataAcquisizione.ToString("dd/MM/yyyy")),
                //1
                ada(item.progressivo),
                //2
                ada(item.vecchioprogressivo),
                //3
                ada(item.ipen),
                //4
                ada(ipendiprovenienza),
                //5
                ada(item.utenteAcquisizioneNavigation.NomeCognome),
                //6
                ada(item.tipoAcquisizioneNavigation.descrizione),
                //7
                ada(item.fornitoreNavigation.descrizione),
                //8
                ada(item.raccoglitoreNavigation.nominativo),
                //9
                ada(item.identificatoreNavigation.nominativo),
                //10
                ada(item.provenienzaNavigation.descrizione),
                //11
                ada(item.nazioneNavigation.descrizione),
                //12
                ada(regione),
                //13
                ada(provincia),
                //14
                ada(localita.Replace("\r\n", " ").Replace("\n", " ")),
                //15
                ada(item.longitudine),
                //16
                ada(item.latitudine),
                //17
                ada(altitudine),
                //18
                ada(item.habitat),
                //19
                string.Format("{0:dd/MM/yyyy}", item.dataraccolta),
                //20
                ada(item.specieNavigation.genereNavigation.famigliaNavigation.descrizione),
                //21
                ada(specie),
                //22
                ada(tipomateriale),
                //23
                ada(item.numeroEsemplari.ToString()),
                //24
                ada(statomateriale),
                //25
                ada(gradoincertezza),
                //26
                ada(associatoErbario),
                //27
                ada(note.Replace("\r\n", " ").Replace("\n", " ")),
                //28
                ada(item.utenteUltimaModificaNavigation.NomeCognome),
                //29
                ada(item.dataUltimaModifica.ToString("dd/MM/yyyy")),
                //30
                ada(validazione));
                sb.AppendFormat(Environment.NewLine);
            }
           // sb.Replace("\"", "");
            //
            //
            //Get Current Response  
            var response = _contextAccessor.HttpContext.Response;
            var headers = new Microsoft.AspNetCore.Http.HeaderDictionary();
            response.Clear();
            response.Headers.Clear();
            response.ContentType = "application/CSV";

            headers.Add("content-disposition", "attachment;filename=Accessioni.CSV");
            response.Headers.ContentEncoding = Encoding.UTF8.ToString();
            response.ContentType = "text/csv";
            
            response.WriteAsync(sb.ToString());
            response.CompleteAsync();
        }
        public void EstraiSpecieTotale()
        {

            var listaspecie = _context.Specie.Include(s => s.arealeNavigation)
                .Include(s => s.citesNavigation)
                .Include(s => s.genereNavigation)
                .ThenInclude(a => a.famigliaNavigation)
                .Include(s => s.iucn_globaleNavigation)
                .Include(s => s.iucn_italiaNavigation)
                .Include(s => s.regnoNavigation).ToList();
            StringBuilder sb = new StringBuilder();
            var Famiglia = "";
            var Genere = "";
            var NomeScientifico = "";
            var NomeComune = "";
            var NomeComuneEN = "";
            var Regno = "";
            var Areale = "";
            var IUCNGlobale = "";
            var IUCNItalia = "";
            var CITES = "";
            sb.AppendFormat("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};", ada("Famiglia"), ada("Genere"), ada("Nome Scientifico"), ada("Nome Comune"), ada("Nome Comune Inglese"), ada("Regno"), ada("Areale"), ada("IUCN Globale"), ada("IUCN Italia"), ada("CITES"));
            sb.AppendFormat(Environment.NewLine);
            foreach (var item in listaspecie)
            {
               
                if (item.iucn_globale == Guid.Empty ) { IUCNGlobale = "Campo Null o Vuoto Verificare"; } else { IUCNGlobale = item.iucn_globaleNavigation.codice; }
               
                if (item.iucn_italia == Guid.Empty ) { IUCNItalia = "Campo Null o Vuoto Verificare"; } else { IUCNItalia = item.iucn_italiaNavigation.codice; }
               
                if (item.cites == Guid.Empty ) { CITES = "Campo Null o Vuoto Verificare"; } else { CITES = item.citesNavigation.codice; }

                if (String.IsNullOrEmpty(item.genereNavigation.famigliaNavigation.descrizione)) { Famiglia = "Campo Null o Vuoto Verificare"; } else { CITES = item.genereNavigation.famigliaNavigation.descrizione; }
                if (String.IsNullOrEmpty(item.genereNavigation.descrizione)) { Genere = "Campo Null o Vuoto Verificare"; } else { Genere = item.genereNavigation.descrizione; }
                if (String.IsNullOrEmpty(item.nome_scientifico)) { NomeScientifico = "Campo Null o Vuoto Verificare"; } else { NomeScientifico = item.nome_scientifico; }
                if (String.IsNullOrEmpty(item.nome_comune)) { NomeComune = "Campo Null o Vuoto Verificare"; } else { NomeComune = item.nome_comune; }
                if (String.IsNullOrEmpty(item.nome_comune_en)) { NomeComuneEN = "Campo Null o Vuoto Verificare"; } else { NomeComuneEN = item.nome_comune_en; }
                if (String.IsNullOrEmpty(item.regnoNavigation.descrizione)) { Regno = "Campo Null o Vuoto Verificare"; } else { Regno = item.regnoNavigation.descrizione; }
                if (String.IsNullOrEmpty(item.arealeNavigation.descrizione)) { Areale = "Campo Null o Vuoto Verificare"; } else { Areale = item.arealeNavigation.descrizione; }


                sb.AppendFormat("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};", ada(Famiglia), ada(Genere), ada(NomeScientifico), ada(NomeComune), ada(NomeComuneEN), ada(Regno), ada(Areale), ada(IUCNGlobale), ada(IUCNItalia), ada(CITES));
                sb.AppendFormat(Environment.NewLine);

            }
            var response = _contextAccessor.HttpContext.Response;
            var headers = new Microsoft.AspNetCore.Http.HeaderDictionary();
            response.Clear();
            response.Headers.Clear();
            response.ContentType = "application/CSV";

            headers.Add("content-disposition", "attachment;filename=Specie.CSV ");
            response.Headers.ContentEncoding = Encoding.UTF8.ToString();
            response.ContentType = "text/csv";
            response.WriteAsync(sb.ToString());
            response.CompleteAsync();


        }


        public void EstraiIndividuiTotale()
        {

            var listaindividui = _context.Individui
                .Include(i => i.accessioneNavigation)
                .ThenInclude(i => i.specieNavigation)
                .Include(i => i.accessioneNavigation)
                .Include(i => i.cartellinoNavigation)
                .Include(i => i.collezioneNavigation)
                
                .Include(i => i.propagatoModalitaNavigation)
                .Include(i => i.sessoNavigation)
                .Include(i => i.settoreNavigation).ToList();
            StringBuilder sb = new StringBuilder();
           
            sb.AppendFormat("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};", ada("Progressivo Individuo"), ada("VecchioProgressivo"), ada("Accessione"), ada("Ipen"), ada("Individuo Padre"), ada("Sesso"), ada("PropagatoData"), ada("PropagatoModalità"), ada("Nome Scientifico"), ada("Settore"), ada("Collezione"), ada("IndexSeminum"), ada("Destinazione"), ada("Longitudine"), ada("Latitudine"), ada("Cartellino"), ada("Stato"), ada("Numero Immagini"), ada("Validazione"), ada("Note"));
            sb.AppendFormat(Environment.NewLine);
            foreach (var item in listaindividui)
            {
                var indexSeminum = "";
                if (item.indexSeminum == false) { indexSeminum = "NO"; } else { indexSeminum = "SI"; }
                var validazione = "";
                if (item.validazione == false) { validazione = "NO"; } else { validazione = "SI"; }


                var recordstorico = _context.StoricoIndividuo.Include(x => x.statoIndividuoNavigation).Where(x => x.individuo == item.id).OrderByDescending(a => a.dataInserimento).FirstOrDefault();
                var stato = "";
                if (recordstorico == null)
                {
                    stato = "Non c'è uno storico per individuo";
                }
                else
                {
                    stato = recordstorico.statoIndividuoNavigation.stato;
                }

                sb.AppendFormat("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};", ada(item.progressivo), ada(item.vecchioprogressivo), ada(item.accessioneNavigation.progressivo), ada(item.accessioneNavigation.ipen), ada(item.individuo?.ToString()), ada(item.sessoNavigation.descrizione), ada(item.propagatoData.ToString("dd/MM/yyyy")), ada(item.propagatoModalitaNavigation.propagatoModalita), ada(item.accessioneNavigation.specieNavigation.nome_scientifico), ada(item.settoreNavigation.settore), ada(item.collezioneNavigation.collezione), ada(indexSeminum), ada(item.destinazioni), ada(item.longitudine), ada(item.latitudine), ada(item.cartellinoNavigation.descrizione), ada(stato), ada(item.ImmaginiIndividuo.Count.ToString()), ada(validazione), ada(item.note));
                sb.AppendFormat(Environment.NewLine);

            }
            var response =  _contextAccessor.HttpContext.Response;
            var headers = new Microsoft.AspNetCore.Http.HeaderDictionary();
            response.Clear();
            response.Headers.Clear();
            response.ContentType = "application/CSV";

            headers.Add("content-disposition", "attachment;filename=Individui.CSV ");
            response.Headers.ContentEncoding = Encoding.UTF8.ToString();
            response.ContentType = "text/csv";
            response.WriteAsync(sb.ToString());
            response.CompleteAsync();


          
        }
        public String ada(String pulita)
        {
            if (String.IsNullOrEmpty(pulita))
            {
                return pulita = "\"\"";
            }
            else
            {
                return pulita = "\"" + pulita.Trim() + "\"";
            }
        }
        
        public ActionResult CreaThumb()
        {
            var t = _opt.Value;
            var immagini = _context.ImmaginiIndividuo.ToList();
            int contathumbgenerate = 0;
            foreach (var item in immagini)
            {

                string basepath = t.Pathfile.Basepath;
                string pathdirindividuo = Path.Combine(basepath, item.individuo.ToString());
                if (Directory.Exists(pathdirindividuo))
                {
                    string pathdirthumb = Path.Combine(pathdirindividuo, "thumb");
                    bool exists = Directory.Exists(pathdirthumb);
                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(pathdirthumb);
                    }
                    string pathimmagine = Path.Combine(pathdirindividuo, item.id + ".jpg");
                    string paththumnimmagine = Path.Combine(pathdirthumb, item.id + ".jpg");
                    if (System.IO.File.Exists(pathimmagine))
                    {
                        if (!System.IO.File.Exists(paththumnimmagine))
                        {
                            StaticUtils.ResizeAndSave(pathimmagine, paththumnimmagine, 400, true);
                            contathumbgenerate = contathumbgenerate + 1;
                        }
                    }
                }

            }
            ViewBag.thumb = "Genera Thumb - > " + contathumbgenerate;

            return View("Index");

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





       
        private string CreaExcelRicerca(IEnumerable<Ricercaacc> accessionixls, IEnumerable<Ricercaind> individuixls, string tipo)
        {
            var datetime = DateTime.Now.ToString().Replace("/", "_").Replace(":", "_");
            var t = _opt.Value;
            var fileName = "";
            if (tipo == "accessione")
            {
                 fileName = "Accessioni" + Guid.NewGuid().ToString() + ".xlsx";
            }
            if (tipo == "individuo")
            {
                 fileName = "Individui" + Guid.NewGuid().ToString() + ".xlsx";
            }
                string basepath = t.Pathfile.Docs;

            string fileFullname = Path.Combine(basepath, fileName);
          

            if (Path.Exists(fileFullname))
            {
                fileFullname = Path.Combine(basepath, fileName);
            }

            using (SpreadsheetDocument package = SpreadsheetDocument.Create(fileFullname, SpreadsheetDocumentType.Workbook))
            {
               if (tipo == "accessione") {
                    CreatePartsForExcelAccessioni(package, accessionixls,tipo);
                }
                if (tipo == "individuo")
                {
                    CreatePartsForExcelIndividui(package, individuixls, tipo);
                }
            }
            return fileName;
        }

        private void CreatePartsForExcelAccessioni(SpreadsheetDocument document, IEnumerable<Ricercaacc> data,string tipo)
        {
            SheetData partSheetData = GenerateSheetdataForDetailsAccessioni(data,tipo);
           
            WorkbookPart workbookPart1 = document.AddWorkbookPart();
            GenerateWorkbookPartContent(workbookPart1);

            WorkbookStylesPart workbookStylesPart1 = workbookPart1.AddNewPart<WorkbookStylesPart>("rId3");
            GenerateWorkbookStylesPartContent(workbookStylesPart1);

            WorksheetPart worksheetPart1 = workbookPart1.AddNewPart<WorksheetPart>("rId1");
            GenerateWorksheetPartContent(worksheetPart1, partSheetData);
        }
        private void CreatePartsForExcelIndividui(SpreadsheetDocument document, IEnumerable<Ricercaind> data, string tipo)
        {
            SheetData partSheetData = GenerateSheetdataForDetailsIndividui(data, tipo);

            WorkbookPart workbookPart1 = document.AddWorkbookPart();
            GenerateWorkbookPartContent(workbookPart1);

            WorkbookStylesPart workbookStylesPart1 = workbookPart1.AddNewPart<WorkbookStylesPart>("rId3");
            GenerateWorkbookStylesPartContent(workbookStylesPart1);

            WorksheetPart worksheetPart1 = workbookPart1.AddNewPart<WorksheetPart>("rId1");
            GenerateWorksheetPartContent(worksheetPart1, partSheetData);
        }
        private void GenerateWorkbookPartContent(WorkbookPart workbookPart1)
        {
            Workbook workbook1 = new Workbook();
            Sheets sheets1 = new Sheets();
            Sheet sheet1 = new Sheet() { Name = "Orto", SheetId = (UInt32Value)1U, Id = "rId1" };
            sheets1.Append(sheet1);
            workbook1.Append(sheets1);
            workbookPart1.Workbook = workbook1;
        }

        private void GenerateWorksheetPartContent(WorksheetPart worksheetPart1, SheetData sheetData1)
        {
            Worksheet worksheet1 = new Worksheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac" } };
            worksheet1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            worksheet1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            worksheet1.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            SheetDimension sheetDimension1 = new SheetDimension() { Reference = "A1" };

            SheetViews sheetViews1 = new SheetViews();

            SheetView sheetView1 = new SheetView() { TabSelected = true, WorkbookViewId = (UInt32Value)0U };
            Selection selection1 = new Selection() { ActiveCell = "A1", SequenceOfReferences = new ListValue<StringValue>() { InnerText = "A1" } };

            sheetView1.Append(selection1);

            sheetViews1.Append(sheetView1);
            SheetFormatProperties sheetFormatProperties1 = new SheetFormatProperties() { DefaultRowHeight = 15D, DyDescent = 0.25D };

            PageMargins pageMargins1 = new PageMargins() { Left = 0.7D, Right = 0.7D, Top = 0.75D, Bottom = 0.75D, Header = 0.3D, Footer = 0.3D };
            worksheet1.Append(sheetDimension1);
            worksheet1.Append(sheetViews1);
            worksheet1.Append(sheetFormatProperties1);
            worksheet1.Append(sheetData1);
            worksheet1.Append(pageMargins1);
            worksheetPart1.Worksheet = worksheet1;
        }
        private void GenerateWorkbookStylesPartContent(WorkbookStylesPart workbookStylesPart1)
        {
            Stylesheet stylesheet1 = new Stylesheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac" } };
            stylesheet1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            stylesheet1.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");

            Fonts fonts1 = new Fonts() { Count = (UInt32Value)2U, KnownFonts = true };

            Font font1 = new Font();
            FontSize fontSize1 = new FontSize() { Val = 11D };
            Color color1 = new Color() { Theme = (UInt32Value)1U };
            FontName fontName1 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering1 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme1 = new FontScheme() { Val = FontSchemeValues.Minor };

            font1.Append(fontSize1);
            font1.Append(color1);
            font1.Append(fontName1);
            font1.Append(fontFamilyNumbering1);
            font1.Append(fontScheme1);

            Font font2 = new Font();
            Bold bold1 = new Bold();
            FontSize fontSize2 = new FontSize() { Val = 11D };
            Color color2 = new Color() { Theme = (UInt32Value)1U };
            FontName fontName2 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering2 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme2 = new FontScheme() { Val = FontSchemeValues.Minor };

            font2.Append(bold1);
            font2.Append(fontSize2);
            font2.Append(color2);
            font2.Append(fontName2);
            font2.Append(fontFamilyNumbering2);
            font2.Append(fontScheme2);

            fonts1.Append(font1);
            fonts1.Append(font2);

            Fills fills1 = new Fills() { Count = (UInt32Value)2U };

            Fill fill1 = new Fill();
            PatternFill patternFill1 = new PatternFill() { PatternType = PatternValues.None };

            fill1.Append(patternFill1);

            Fill fill2 = new Fill();
            PatternFill patternFill2 = new PatternFill() { PatternType = PatternValues.Gray125 };

            fill2.Append(patternFill2);

            fills1.Append(fill1);
            fills1.Append(fill2);

            Borders borders1 = new Borders() { Count = (UInt32Value)2U };

            Border border1 = new Border();
            LeftBorder leftBorder1 = new LeftBorder();
            RightBorder rightBorder1 = new RightBorder();
            TopBorder topBorder1 = new TopBorder();
            BottomBorder bottomBorder1 = new BottomBorder();
            DiagonalBorder diagonalBorder1 = new DiagonalBorder();

            border1.Append(leftBorder1);
            border1.Append(rightBorder1);
            border1.Append(topBorder1);
            border1.Append(bottomBorder1);
            border1.Append(diagonalBorder1);

            Border border2 = new Border();

            LeftBorder leftBorder2 = new LeftBorder() { Style = BorderStyleValues.Thin };
            Color color3 = new Color() { Indexed = (UInt32Value)64U };

            leftBorder2.Append(color3);

            RightBorder rightBorder2 = new RightBorder() { Style = BorderStyleValues.Thin };
            Color color4 = new Color() { Indexed = (UInt32Value)64U };

            rightBorder2.Append(color4);

            TopBorder topBorder2 = new TopBorder() { Style = BorderStyleValues.Thin };
            Color color5 = new Color() { Indexed = (UInt32Value)64U };

            topBorder2.Append(color5);

            BottomBorder bottomBorder2 = new BottomBorder() { Style = BorderStyleValues.Thin };
            Color color6 = new Color() { Indexed = (UInt32Value)64U };

            bottomBorder2.Append(color6);
            DiagonalBorder diagonalBorder2 = new DiagonalBorder();

            border2.Append(leftBorder2);
            border2.Append(rightBorder2);
            border2.Append(topBorder2);
            border2.Append(bottomBorder2);
            border2.Append(diagonalBorder2);

            borders1.Append(border1);
            borders1.Append(border2);

            CellStyleFormats cellStyleFormats1 = new CellStyleFormats() { Count = (UInt32Value)1U };
            CellFormat cellFormat1 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U };

            cellStyleFormats1.Append(cellFormat1);

            CellFormats cellFormats1 = new CellFormats() { Count = (UInt32Value)3U };
            CellFormat cellFormat2 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U };
            CellFormat cellFormat3 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyBorder = true };
            CellFormat cellFormat4 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)1U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyFont = true, ApplyBorder = true };

            cellFormats1.Append(cellFormat2);
            cellFormats1.Append(cellFormat3);
            cellFormats1.Append(cellFormat4);

            CellStyles cellStyles1 = new CellStyles() { Count = (UInt32Value)1U };
            CellStyle cellStyle1 = new CellStyle() { Name = "Normal", FormatId = (UInt32Value)0U, BuiltinId = (UInt32Value)0U };

            cellStyles1.Append(cellStyle1);
            DifferentialFormats differentialFormats1 = new DifferentialFormats() { Count = (UInt32Value)0U };
            TableStyles tableStyles1 = new TableStyles() { Count = (UInt32Value)0U, DefaultTableStyle = "TableStyleMedium2", DefaultPivotStyle = "PivotStyleLight16" };

            StylesheetExtensionList stylesheetExtensionList1 = new StylesheetExtensionList();

            StylesheetExtension stylesheetExtension1 = new StylesheetExtension() { Uri = "{EB79DEF2-80B8-43e5-95BD-54CBDDF9020C}" };
            stylesheetExtension1.AddNamespaceDeclaration("x14", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main");
            X14.SlicerStyles slicerStyles1 = new X14.SlicerStyles() { DefaultSlicerStyle = "SlicerStyleLight1" };

            stylesheetExtension1.Append(slicerStyles1);

            StylesheetExtension stylesheetExtension2 = new StylesheetExtension() { Uri = "{9260A510-F301-46a8-8635-F512D64BE5F5}" };
            stylesheetExtension2.AddNamespaceDeclaration("x15", "http://schemas.microsoft.com/office/spreadsheetml/2010/11/main");
            X15.TimelineStyles timelineStyles1 = new X15.TimelineStyles() { DefaultTimelineStyle = "TimeSlicerStyleLight1" };

            stylesheetExtension2.Append(timelineStyles1);

            stylesheetExtensionList1.Append(stylesheetExtension1);
            stylesheetExtensionList1.Append(stylesheetExtension2);

            stylesheet1.Append(fonts1);
            stylesheet1.Append(fills1);
            stylesheet1.Append(borders1);
            stylesheet1.Append(cellStyleFormats1);
            stylesheet1.Append(cellFormats1);
            stylesheet1.Append(cellStyles1);
            stylesheet1.Append(differentialFormats1);
            stylesheet1.Append(tableStyles1);
            stylesheet1.Append(stylesheetExtensionList1);

            workbookStylesPart1.Stylesheet = stylesheet1;
        }
        private SheetData GenerateSheetdataForDetailsAccessioni(IEnumerable<Ricercaacc> data, string tipo)
        {
            SheetData sheetData1 = new SheetData();
            sheetData1.Append(CreateHeaderRowForExcelAccessione());

            foreach (var t in data)
            {
                Row partsRows = GenerateRowForChildPartDetailAccessione(t);
                sheetData1.Append(partsRows);
            }
            return sheetData1;
        }
        private SheetData GenerateSheetdataForDetailsIndividui(IEnumerable<Ricercaind> data,string tipo)
        {
            SheetData sheetData1 = new SheetData();
            sheetData1.Append(CreateHeaderRowForExcelIndividuo());

            foreach (var t in data)
            {
                Row partsRows = GenerateRowForChildPartDetailIndividuo(t);
                sheetData1.Append(partsRows);
            }
            return sheetData1;
        }
        
        private Row CreateHeaderRowForExcelAccessione()
        {
          
            Row workRow = new Row();
            workRow.Append(CreateCell("Progressivo", 2U));
            workRow.Append(CreateCell("Vecchio Progressivo", 2U));
            workRow.Append(CreateCell("Nome Scientifico", 2U));
            workRow.Append(CreateCell("Famiglia", 2U));
            workRow.Append(CreateCell("Genere", 2U));
            workRow.Append(CreateCell("Data Acquisizione", 2U));
            workRow.Append(CreateCell("Tipo Materiale", 2U));
            workRow.Append(CreateCell("Numero Individui Figli", 2U));
            workRow.Append(CreateCell("Inserito da", 2U));
            workRow.Append(CreateCell("Modificato da", 2U));
            return workRow;
        }
        private Row CreateHeaderRowForExcelIndividuo()
        {
                Row workRow = new Row();
                workRow.Append(CreateCell("Progressivo", 2U));
                workRow.Append(CreateCell("Vecchio Progressivo", 2U));
                workRow.Append(CreateCell("Nome Scientifico", 2U));
                workRow.Append(CreateCell("Settore", 2U));
                workRow.Append(CreateCell("Collezione", 2U));
                workRow.Append(CreateCell("Cartellino", 2U));
                workRow.Append(CreateCell("Stato Individuo", 2U));
                workRow.Append(CreateCell("Nominativo ultima modifica", 2U));
                workRow.Append(CreateCell("Data Ultima Modifica", 2U));
                return workRow;
            
        }
        private Row GenerateRowForChildPartDetailAccessione(Ricercaacc v)
        {
            
                var contafigli = _context.Individui.Where(a => a.accessione == v.idacc).Count().ToString();

                Row tRow = new Row();
                tRow.Append(CreateCell(v.progressivo));
                tRow.Append(CreateCell(v.vecchioprogressivo));
                tRow.Append(CreateCell(v.nome_scientifico));
                tRow.Append(CreateCell(v.famiglia));
                tRow.Append(CreateCell(v.genere));
                tRow.Append(CreateCell(v.dataAcquisizione.ToShortDateString()));
                tRow.Append(CreateCell(v.tipomateriale));
                tRow.Append(CreateCell(contafigli));
                tRow.Append(CreateCell(v.inseritoda));
                tRow.Append(CreateCell(v.modificatoda));

                return tRow;
        }
        private Row GenerateRowForChildPartDetailIndividuo(Ricercaind v)
        {


                Row tRow = new Row();
                tRow.Append(CreateCell(v.progressivo));
                tRow.Append(CreateCell(v.vecchioprogressivo));
                tRow.Append(CreateCell(v.nome_scientifico));
                tRow.Append(CreateCell(v.settore));
                tRow.Append(CreateCell(v.collezione));
                tRow.Append(CreateCell(v.cartellino));
                tRow.Append(CreateCell(v.statoindividuo));
                tRow.Append(CreateCell(v.nomecognome));
                tRow.Append(CreateCell(v.datainserimento?.ToShortDateString()));
                
                return tRow;
            

        }
        private Cell CreateCell(string text)
        {
            Cell cell = new Cell();
            cell.StyleIndex = 1U;
            cell.DataType = ResolveCellDataTypeOnValue(text);
            cell.CellValue = new CellValue(text);
            return cell;
        }
        private Cell CreateCell(string text, uint styleIndex)
        {
            Cell cell = new Cell();
            cell.StyleIndex = styleIndex;
            cell.DataType = ResolveCellDataTypeOnValue(text);
            cell.CellValue = new CellValue(text);
            return cell;
        }
        private EnumValue<CellValues> ResolveCellDataTypeOnValue(string text)
        {
            int intVal;
            double doubleVal;
            if (int.TryParse(text, out intVal) || double.TryParse(text, out doubleVal))
            {
                return CellValues.Number;
            }
            else
            {
                return CellValues.String;
            }
        }
    }
}