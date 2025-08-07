using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using UPlant.Models;
using UPlant.Models.DB;

namespace UPlant.Controllers
{
    [Authorize(Roles = "Administrator,Operator,Reader")]
    public class AccessioniController : BaseController
    {
        private readonly Entities _context;
        private readonly IConfiguration _configuration;
        
        private readonly IOptions<Application> _appOpt;
        private readonly IOptions<MapSettings> _googlemap;
        private readonly LanguageService _languageService;


        public AccessioniController(Entities context, IConfiguration Configuration,  IOptions<Application> appOpt, IOptions<MapSettings> googlemap, LanguageService languageService)
        {
            _context = context;
            _configuration = Configuration;
            _languageService = languageService;


            _appOpt = appOpt;
            _googlemap = googlemap;

           
        }

        // GET: Accessioni
        public async Task<IActionResult> Index(string valida)
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            Guid organizzazione = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@"))).Select(a => a.Organizzazione).FirstOrDefault();
            if (valida == null)
            {
                return View(await _context.Accessioni.Include(a => a.Individui)
                    .Include(a => a.fornitoreNavigation)
                    .Include(a => a.gradoIncertezzaNavigation)
                    .Include(a => a.nazioneNavigation)
                    .Include(a => a.organizzazioneNavigation)
                    .Include(a => a.provenienzaNavigation)
                    .Include(a => a.provinciaNavigation)
                    .Include(a => a.raccoglitoreNavigation)
                    .Include(a => a.regioneNavigation)
                    .Include(a => a.statoMaterialeNavigation)
                    .Include(a => a.tipoAcquisizioneNavigation)
                    .Include(a => a.tipoMaterialeNavigation)
                    .Include(a => a.utenteAcquisizioneNavigation)
                    .Include(a => a.utenteUltimaModificaNavigation)
                    .Include(a => a.identificatoreNavigation)
                    .Include(a => a.specieNavigation).ThenInclude(a => a.genereNavigation).ThenInclude(a => a.famigliaNavigation)
                    .OrderByDescending(x => x.progressivo)
                    .ThenByDescending(a => a.dataAcquisizione).Where(a => a.organizzazione == organizzazione).Take(100).ToListAsync());  
               
            }
            else
            {

                return View(await _context.Accessioni
                    .Include(a => a.Individui)
                    .Include(a => a.fornitoreNavigation)
                    .Include(a => a.gradoIncertezzaNavigation)
                    .Include(a => a.nazioneNavigation)
                    .Include(a => a.organizzazioneNavigation)
                    .Include(a => a.provenienzaNavigation)
                    .Include(a => a.provinciaNavigation)
                    .Include(a => a.raccoglitoreNavigation)
                    .Include(a => a.regioneNavigation)
                    .Include(a => a.statoMaterialeNavigation)
                    .Include(a => a.tipoAcquisizioneNavigation)
                    .Include(a => a.tipoMaterialeNavigation)
                    .Include(a => a.utenteAcquisizioneNavigation)
                    .Include(a => a.utenteUltimaModificaNavigation)
                    .Include(a => a.identificatoreNavigation)
                    .Include(a => a.specieNavigation).ThenInclude(a => a.genereNavigation).ThenInclude(a => a.famigliaNavigation)
                    .OrderByDescending(x => x.progressivo).ThenByDescending(a => a.dataAcquisizione).Where(a => a.organizzazione == organizzazione && a.validazione == false).Take(100).ToListAsync());

            }

            
        }

        // GET: Accessioni/Details/5
        public async Task<IActionResult> Details(Guid? id, string tipo)
         {

            
            

            if (id == null || _context.Accessioni == null)
            {
                return NotFound();
            }
            ViewBag.idaccessione = id;
            ViewBag.tipo = tipo;
            var accessioni = await _context.Accessioni
                .Include(a => a.Individui).ThenInclude(a => a.collezioneNavigation)
                .Include(a => a.Individui).ThenInclude(a => a.settoreNavigation)
                .Include(a => a.Individui).ThenInclude(a => a.cartellinoNavigation)
              .Include(a => a.specieNavigation).ThenInclude(a => a.genereNavigation)
              
               .Include(a => a.Individui).ThenInclude(a => a.StoricoIndividuo).ThenInclude(a => a.statoIndividuoNavigation)
               .Include(a => a.Individui).ThenInclude(a => a.StoricoIndividuo).ThenInclude(a => a.utenteNavigation)
                
               .Include(a => a.fornitoreNavigation)
                .Include(a => a.gradoIncertezzaNavigation)
                .Include(a => a.nazioneNavigation)
                .Include(a => a.organizzazioneNavigation)
                .Include(a => a.provenienzaNavigation)
                .Include(a => a.provinciaNavigation)
                .Include(a => a.raccoglitoreNavigation)
                .Include(a => a.regioneNavigation)
                .Include(a => a.statoMaterialeNavigation)
                .Include(a => a.tipoAcquisizioneNavigation)
                .Include(a => a.tipoMaterialeNavigation)
                .Include(a => a.utenteAcquisizioneNavigation)
                .Include(a => a.utenteUltimaModificaNavigation)
                .Include(a => a.identificatoreNavigation)
                .Include(a => a.specieNavigation).ThenInclude(a => a.genereNavigation).ThenInclude(a => a.famigliaNavigation)
                .FirstOrDefaultAsync(m => m.id == id);

         


            ViewBag.list = accessioni.Individui.ToList();
            // ViewBag.list2 = accessioni.ListaStorico.OrderByDescending(x => x.dataInserimento).ToList();
           // ViewBag.list2 = await _context.StoricoIndividuo.Include(i => i.individuoNavigation).ThenInclude(i => i.accessioneNavigation).Where(x => x.individuoNavigation.accessioneNavigation.id == id).ToListAsync();
            accessioni.Individui = ViewBag.list;
            
            




            if (accessioni == null)
            {
                return NotFound();
            }
            
            return View(accessioni);
        }
        [Authorize(Roles = "Administrator,Operator")]
        // GET: Accessioni/Create
        public IActionResult Create()
        {
            var linguacorrente = _languageService.GetCurrentCulture();


            if (linguacorrente == "en-US")
            {
                ViewData["regione"] = new SelectList(_context.Regioni.OrderBy(a => a.descrizione), "codice", "descrizione_en");

            } else
            {
                ViewData["regione"] = new SelectList(_context.Regioni.OrderBy(a => a.descrizione), "codice", "descrizione");
            }


            ViewData["provincia"] = new SelectList(_context.Province.OrderBy(a => a.descrizione), "codice", "descrizione");
            ViewData["famiglia"] = new SelectList(_context.Famiglie.OrderBy(e => e.descrizione), "id", "descrizione");
            ViewData["areale"] = new SelectList(_context.Areali.OrderBy(e => e.descrizione), "id", "descrizione");
            ViewData["regno"] = new SelectList(_context.Regni.OrderBy(e => e.ordinamento), "id", "descrizione");
            ViewData["fornitore"] = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione), "id", "descrizione");//da controllare il fornitore principale "9189177a860442bea1f8f82e4e878d1e"
            ViewData["gradoIncertezza"] = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione), "id", "descrizione");
            ViewData["nazione"] = new SelectList(_context.Nazioni.OrderBy(a => a.descrizione), "codice", "descrizione");
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni, "id", "descrizione");
            ViewData["provenienza"] = new SelectList(_context.Provenienze.OrderBy(a => a.ordinamento), "id", "descrizione");
            ViewData["provincia"] = new SelectList(_context.Province.OrderBy(a => a.descrizione), "codice", "descrizione");
            ViewData["raccoglitore"] = new SelectList(_context.Raccoglitori.OrderBy(a => a.nominativo), "id", "nominativo");
            ViewData["regione"] = new SelectList(_context.Regioni.OrderBy(a => a.descrizione), "codice", "descrizione");
            ViewData["specie"] = new SelectList(_context.Specie.OrderBy(a => a.nome_scientifico), "id", "nome_scientifico");
            ViewData["statoMateriale"] = new SelectList(_context.StatoMateriale.OrderBy(a => a.ordinamento), "id", "descrizione");
            ViewData["tipoAcquisizione"] = new SelectList(_context.TipoAcquisizione.OrderBy(a => a.ordinamento), "id", "descrizione");
            ViewData["tipoMateriale"] = new SelectList(_context.TipiMateriale.OrderBy(a => a.ordinamento), "id", "descrizione");//da controllare il fornitore principale"28902C38-3BE2-4AC4-B0B8-AE41776CF7B4"
            ViewData["utenteAcquisizione"] = new SelectList(_context.Users, "Id", "CreatedBy");
            ViewData["utenteUltimaModifica"] = new SelectList(_context.Users, "Id", "CreatedBy");
            ViewData["identificatore"] = new SelectList(_context.Identificatori.OrderBy(a => a.nominativo), "id", "nominativo");
            List<SelectListItem> x = new List<SelectListItem>();
            ViewBag.genere = new SelectList(x);
            ViewBag.specie = new SelectList(x);
            
            return View();
        }

        // POST: Accessioni/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string dataAcquisizione, Guid identificatore, Guid tipoAcquisizione,
                                   Guid fornitore, Guid raccoglitore, Guid provenienza, Guid famiglia, Guid genere,// non mi serve la famiglia e il genere
                                   Guid specie,
                                   bool associatoErbario,
                                   string nazione, string regione, string provincia,
                                   string localita,
                                   string altitudine,
                                   string habitat, Guid tipoMateriale,
                                   int numeroEsemplari, Guid statoMateriale, Guid gradoIncertezza, string note, string vecchioprogressivo, string longitudine, string latitudine, string dataraccolta, string ipendiprovenienza)
        {
            // string msgerr ;
            string risultato = "si";
            Accessioni accessioni = new Accessioni();
            
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
            
            Guid organizzazione = oggettoutente.Select(a => a.Organizzazione).FirstOrDefault();
            Guid utente = oggettoutente.Select(a => a.Id).FirstOrDefault();
            string ruolo = _context.UserRole.Where(a => a.UserFK == utente).Select(a => a.RoleFKNavigation.Descr).FirstOrDefault();
            accessioni.utenteAcquisizione = utente;
            accessioni.organizzazione = organizzazione;
            accessioni.utenteUltimaModifica = utente;
            accessioni.dataUltimaModifica = DateTime.Now;
            accessioni.dataAcquisizione = DateTime.Parse(dataAcquisizione);
            accessioni.progressivo = nextProg(accessioni);
            accessioni.longitudine = longitudine;
            accessioni.latitudine = latitudine;
            if (associatoErbario == true)
            {
                accessioni.associatoErbario = true;
            }
            else
            {
                accessioni.associatoErbario = false;
            }
            if (ruolo == "Administrator")
            {
                accessioni.validazione = true;
            }
            else
            {
                accessioni.validazione = false;
            }

            // TODO: da ricavare da dataAcquisizione...
            accessioni.vecchioprogressivo = vecchioprogressivo; // TODO: da inserire in form
            accessioni.identificatore = identificatore; //Not Null
            accessioni.fornitore = fornitore;
            accessioni.raccoglitore = raccoglitore;
            accessioni.provenienza = provenienza;
            // NOT NULL
            if (regione == null || regione == "-1")
            {
                accessioni.regione = "99";
            }
            else
            {
                accessioni.regione = regione;
            }
            if (provincia == null || provincia == "-1")
            {
                accessioni.provincia = "999";
            }
            else
            {
                accessioni.provincia = provincia;
            }

            accessioni.localita = localita;
            accessioni.habitat = habitat;

            if (!String.IsNullOrEmpty(altitudine))
            {
                accessioni.altitudine = Convert.ToInt32(altitudine);
            }
            else
            {
                accessioni.altitudine = null;
            }


            accessioni.statoMateriale = statoMateriale;
            accessioni.note = note;
            accessioni.nazione = nazione;
            accessioni.tipoMateriale = tipoMateriale; // NOT NULL
            accessioni.tipoAcquisizione = tipoAcquisizione;
            accessioni.numeroEsemplari = numeroEsemplari;
            if (String.IsNullOrEmpty(dataraccolta))
            {
                accessioni.dataraccolta = null;
            }
            else
            {
                accessioni.dataraccolta = DateTime.Parse(dataraccolta);
            }
            accessioni.gradoIncertezza = gradoIncertezza;
            // || String.IsNullOrEmpty(nazione) || String.IsNullOrEmpty(tipoMateriale)  || String.IsNullOrEmpty(tipoAcquisizione)  || numeroEsemplari <= 0 || String.IsNullOrEmpty(gradoIncertezza)
            accessioni.specie = specie;


            //if singoli
            if (!String.IsNullOrEmpty(ipendiprovenienza))
            {
                accessioni.ipen = ipendiprovenienza;
            }
            else
            {
                accessioni.ipen = nextIpen(accessioni);
            }

            

                if (ModelState.IsValid)
                {
                    try
                    {
                        //  _context.Accessioni.Add(accessioni);
                        //  _context.SaveChanges();
                        _context.Add(accessioni);
                        await _context.SaveChangesAsync();
                        AddPageAlerts(PageAlertType.Success, "Accessione inserita correttamente , puoi inserire l'individuo figlio");
                        TempData["message"] = "Accessione inserita correttamente , puoi inserire l'individuo figlio";
                        risultato = "si";

                    } catch (Exception ) {
                        AddPageAlerts(PageAlertType.Error, "Qualcosa nell'inserimento è andato storto");
                        TempData["message"] = "Qualcosa nell'inserimento è andato storto";
                        risultato = "no";
                    }
                } else { 

            

                AddPageAlerts(PageAlertType.Warning, "controlla i campi inseriti");
                TempData["message"] = "controlla i campi inseriti";
                risultato = "no";
                // SetViewBag(accessioni, genere, famiglia);
                ViewData["fornitore"] = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.fornitore);
                ViewData["gradoIncertezza"] = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.gradoIncertezza);
                ViewData["nazione"] = new SelectList(_context.Nazioni.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.nazione);
                ViewData["organizzazione"] = new SelectList(_context.Organizzazioni, "id", "descrizione", accessioni.organizzazione);
                ViewData["provenienza"] = new SelectList(_context.Provenienze.OrderBy(a => a.ordinamento), "id", "descrizione", accessioni.provenienza);
                ViewData["provincia"] = new SelectList(_context.Province.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.provincia);
                ViewData["raccoglitore"] = new SelectList(_context.Raccoglitori.OrderBy(a => a.nominativo), "id", "nominativo", accessioni.raccoglitore);
                ViewData["regione"] = new SelectList(_context.Regioni.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.regione);
                ViewData["famiglia"] = new SelectList(_context.Famiglie.OrderBy(a => a.descrizione), "id", "descrizione", famiglia);
                ViewData["genere"] = new SelectList(_context.Generi.OrderBy(a => a.descrizione), "id", "descrizione",genere);
                ViewData["specie"] = new SelectList(_context.Specie.OrderBy(a => a.nome_scientifico), "id", "nome_scientifico", accessioni.specie);
                ViewData["statoMateriale"] = new SelectList(_context.StatoMateriale.OrderBy(a => a.ordinamento), "id", "descrizione", accessioni.statoMateriale);
                ViewData["tipoAcquisizione"] = new SelectList(_context.TipoAcquisizione.OrderBy(a => a.ordinamento), "id", "descrizione", accessioni.tipoAcquisizione);
                ViewData["tipoMateriale"] = new SelectList(_context.TipiMateriale.OrderBy(a => a.ordinamento), "id", "descrizione", accessioni.tipoMateriale);
                ViewData["utenteAcquisizione"] = new SelectList(_context.Users, "Id", "CreatedBy", accessioni.utenteAcquisizione);
                ViewData["utenteUltimaModifica"] = new SelectList(_context.Users, "Id", "CreatedBy", accessioni.utenteUltimaModifica);
                ViewData["identificatore"] = new SelectList(_context.Identificatori.OrderBy(a => a.nominativo), "id", "nominativo", accessioni.identificatore);
                ViewData["areale"] = new SelectList(_context.Areali.OrderBy(e => e.descrizione), "id", "descrizione");
                ViewData["regno"] = new SelectList(_context.Regni.OrderBy(e => e.ordinamento), "id", "descrizione");


            }
            if (risultato == "si")
            {
                return RedirectToAction("Index", new { idaccessione = accessioni.id });

            }
            else { return View(accessioni); }



            
        }
        [Authorize(Roles = "Administrator,Operator")]
        // GET: Accessioni/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Accessioni == null)
            {
                return NotFound();
            }

            var accessioni = await _context.Accessioni
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.gradoIncertezzaNavigation)
                .Include(a => a.nazioneNavigation)
                .Include(a => a.organizzazioneNavigation)
                .Include(a => a.provenienzaNavigation)
                .Include(a => a.provinciaNavigation)
                .Include(a => a.raccoglitoreNavigation)
                .Include(a => a.regioneNavigation)
                .Include(a => a.statoMaterialeNavigation)
                .Include(a => a.tipoAcquisizioneNavigation)
                .Include(a => a.tipoMaterialeNavigation)
                .Include(a => a.utenteAcquisizioneNavigation)
                .Include(a => a.utenteUltimaModificaNavigation)
                .Include(a => a.identificatoreNavigation)
                .Include(a => a.specieNavigation).ThenInclude(a => a.genereNavigation).ThenInclude(a => a.famigliaNavigation).FirstOrDefaultAsync(i => i.id == id);
            if (accessioni == null)
            {
                return NotFound();
            }
            ViewData["fornitore"] = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.fornitore);
            ViewData["gradoIncertezza"] = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.gradoIncertezza);
            ViewData["nazione"] = new SelectList(_context.Nazioni.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.nazione);
            ViewData["organizzazione"] = new SelectList(_context.Organizzazioni, "id", "descrizione", accessioni.organizzazione);
            ViewData["provenienza"] = new SelectList(_context.Provenienze.OrderBy(a => a.ordinamento), "id", "descrizione", accessioni.provenienza);
            ViewData["provincia"] = new SelectList(_context.Province.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.provincia);
            ViewData["raccoglitore"] = new SelectList(_context.Raccoglitori.OrderBy(a => a.nominativo), "id", "nominativo", accessioni.raccoglitore);
            ViewData["regione"] = new SelectList(_context.Regioni.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.regione);
            ViewData["specie"] = new SelectList(_context.Specie.OrderBy(a => a.nome_scientifico), "id", "nome_scientifico", accessioni.specie);
            ViewData["statoMateriale"] = new SelectList(_context.StatoMateriale.OrderBy(a => a.ordinamento), "id", "descrizione", accessioni.statoMateriale);
            ViewData["tipoAcquisizione"] = new SelectList(_context.TipoAcquisizione.OrderBy(a => a.ordinamento), "id", "descrizione", accessioni.tipoAcquisizione);
            ViewData["tipoMateriale"] = new SelectList(_context.TipiMateriale.OrderBy(a => a.ordinamento), "id", "descrizione", accessioni.tipoMateriale);
            ViewData["utenteAcquisizione"] = new SelectList(_context.Users, "Id", "CreatedBy", accessioni.utenteAcquisizione);
            ViewData["utenteUltimaModifica"] = new SelectList(_context.Users, "Id", "CreatedBy", accessioni.utenteUltimaModifica);
            ViewData["identificatore"] = new SelectList(_context.Identificatori.OrderBy(a => a.nominativo), "id", "nominativo", accessioni.identificatore);


            return View(accessioni);
        }

        // POST: Accessioni/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator,Operator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public ActionResult Edit(Guid id, string ipen, Guid identificatore, Guid tipoAcquisizione,
                                   Guid fornitore, Guid raccoglitore, Guid provenienza, // non mi serve la famiglia e il genere
                                   Guid specie, string nazione, string regione, string provincia,
                                   string localita, string altitudine, string habitat, Guid tipoMateriale,
                                   int numeroEsemplari, Guid statoMateriale, Guid gradoIncertezza, bool associatoErbario, string note, string vecchioprogressivo, string longitudine, string latitudine, string dataraccolta)//,string ipendiprovenienza
        {
            Accessioni accessioni = _context.Accessioni.Find(id);
            string msgerr = "";
            if (ModelState.IsValid)
            {
                string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
                var oggettoutente = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@")));
                Guid organizzazione = oggettoutente.Select(a => a.Organizzazione).FirstOrDefault();
                Guid utente = oggettoutente.Select(a => a.Id).FirstOrDefault();
                try
                {
                    accessioni.identificatore = identificatore;
                    accessioni.tipoAcquisizione = tipoAcquisizione;
                    accessioni.fornitore = fornitore;
                    accessioni.raccoglitore = raccoglitore;
                    accessioni.provenienza = provenienza;
                    accessioni.nazione = nazione;
                    if (regione == "")
                    {
                        accessioni.regione = null;
                        accessioni.provincia = null;
                    }
                    else
                    {
                        accessioni.regione = regione;
                        accessioni.provincia = provincia;
                    }
                    accessioni.localita = localita;
                    if (ipen.Length > 2 && ipen.Substring(5, 3) == "PI-")///da rivedere e sostituire PI con la substring dell'organizzazione
                    {
                        if (nazione == "NN")
                        {
                            accessioni.ipen = "XX" + ipen.Substring(2);
                        }
                        else
                        {
                            accessioni.ipen = nazione + ipen.Substring(2);
                        }

                    }
                    else
                    {
                        accessioni.ipen = ipen;
                    }


                    accessioni.habitat = habitat;
                    accessioni.tipoMateriale = tipoMateriale;
                    accessioni.numeroEsemplari = numeroEsemplari;
                    accessioni.statoMateriale = statoMateriale;
                    accessioni.gradoIncertezza = gradoIncertezza;
                    accessioni.specie = specie;
                    if (associatoErbario == true)
                    {
                        accessioni.associatoErbario = true;
                    }
                    else
                    {
                        accessioni.associatoErbario = false;
                    }


                    if (!String.IsNullOrEmpty(altitudine))
                    {
                        accessioni.altitudine = Convert.ToInt32(altitudine);
                    } else {
                        accessioni.altitudine = null;
                    }


                        accessioni.note = note;
                    accessioni.longitudine = longitudine;
                    accessioni.latitudine = latitudine;
                    accessioni.vecchioprogressivo = vecchioprogressivo;
                    // accessioni.ipendiprovenienza = ipendiprovenienza;
                    accessioni.utenteUltimaModifica = utente;
                    accessioni.dataUltimaModifica = DateTime.Now;
                    if (dataraccolta == "" || dataraccolta == null)
                    {
                        accessioni.dataraccolta = null;
                    }
                    else
                    {
                        accessioni.dataraccolta = DateTime.Parse(dataraccolta);
                    }

                    _context.Entry(accessioni).State = EntityState.Modified;

                    _context.SaveChanges();
                    AddPageAlerts(PageAlertType.Success, "Accessione modificata correttamente");
                    TempData["message"] = "Accessione modificata correttamente";
                  
              

                }
                catch (Exception e)
                {
                    msgerr = e.Message;
                    TempData["MsgErr"] = msgerr + "Ricontrolla i campi modificati identifica che siano valorizzati";
                    ViewBag.fornitore = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.fornitore);
                    ViewBag.nazione = new SelectList(_context.Nazioni.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.nazione);
                    ViewBag.organizzazione = new SelectList(_context.Organizzazioni.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.organizzazione);
                    ViewBag.provenienza = new SelectList(_context.Provenienze.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.provenienza);
                    ViewBag.provincia = new SelectList(_context.Province.OrderBy(a => a.descrizione), "codice", "regione", accessioni.provincia);
                    ViewBag.raccoglitore = new SelectList(_context.Raccoglitori.OrderBy(a => a.nominativo), "id", "nominativo", accessioni.raccoglitore);
                    ViewBag.regione = new SelectList(_context.Regioni.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.regione);
                    ViewBag.specie = new SelectList(_context.Specie.OrderBy(a => a.nome_scientifico), "id", "nome_scientifico", accessioni.specie);
                    ViewBag.tipoAcquisizione = new SelectList(_context.TipoAcquisizione.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.tipoAcquisizione);
                    ViewBag.tipoMateriale = new SelectList(_context.TipiMateriale.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.tipoMateriale);
                    ViewBag.statoMateriale = new SelectList(_context.StatoMateriale.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.statoMateriale);
                    ViewBag.gradoIncertezza = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.gradoIncertezza);
                    ViewBag.utenteAcquisizione = new SelectList(_context.Users, "Id", "CreatedBy", accessioni.utenteAcquisizione);
                    ViewBag.utenteUltimaModifica = new SelectList(_context.Users, "Id", "CreatedBy", accessioni.utenteUltimaModifica);
                    ViewBag.identificatore = new SelectList(_context.Identificatori.OrderBy(a => a.nominativo), "id", "nominativo", accessioni.identificatore);

                    AddPageAlerts(PageAlertType.Warning, "controlla i campi modificati");
                    TempData["message"] = "controlla i campi modificati";


                    return View(accessioni);
                }

            }
             return RedirectToAction(nameof(Details),  new { id = accessioni.id });
        }
        [Authorize(Roles = "Administrator,Operator")]
        // GET: Accessioni/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            
            if (id == null || _context.Accessioni == null)
            {
                return NotFound();
            }

            var accessioni = await _context.Accessioni
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.gradoIncertezzaNavigation)
                .Include(a => a.nazioneNavigation)
                .Include(a => a.organizzazioneNavigation)
                .Include(a => a.provenienzaNavigation)
                .Include(a => a.provinciaNavigation)
                .Include(a => a.raccoglitoreNavigation)
                .Include(a => a.regioneNavigation)
                .Include(a => a.specieNavigation)
                .Include(a => a.statoMaterialeNavigation)
                .Include(a => a.tipoAcquisizioneNavigation)
                .Include(a => a.tipoMaterialeNavigation)
                .Include(a => a.utenteAcquisizioneNavigation)
                .Include(a => a.utenteUltimaModificaNavigation)
                .Include(a => a.identificatoreNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
            if (accessioni == null)
            {
                return NotFound();
            }

            return View(accessioni);
        }
        [Authorize(Roles = "Administrator,Operator")]
        // POST: Accessioni/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Accessioni == null)
            {
                return Problem("Entity set 'Entities.Accessioni'  is null.");
            }
            var accessioni = await _context.Accessioni.FindAsync(id);
            if (accessioni != null)
            {
                _context.Accessioni.Remove(accessioni);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccessioniExists(Guid id)
        {
          return _context.Accessioni.Any(e => e.id == id);
        }
        public string nextProg(Accessioni a)
        {
            Accessioni accessultima = ultima(a);
            string retnextprog;

            if (accessultima != null)
            {
                int ult;
                string ultimoprogressivo = accessultima.progressivo;
                Int32.TryParse(ultimoprogressivo.Substring(5, 4), out ult);
                ult++;

                retnextprog = accessultima.dataAcquisizione.Year.ToString() + "-" + ult.ToString().PadLeft(4, '0');
            }
            else
            {

                retnextprog = a.dataAcquisizione.Year.ToString() + "-0001";
            }
            return retnextprog;
        }
        public string nextIpen(Accessioni a)
        {
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            Guid organizzazione = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@"))).Select(a => a.Organizzazione).FirstOrDefault();
            string prefissoipen = _context.Organizzazioni.Where(a => a.id == organizzazione).Select(a => a.prefissoIpen).FirstOrDefault();
            Accessioni accessultima = ultima(a);
            string retnextipen;
            if (a.nazione == "NN")
            {
                retnextipen = "XX-0-"+ prefissoipen + "-";// anche qui rendere universale non mettendo PI un domani potrebbe essere un altro orto
            }
            else
            {
                retnextipen = a.nazione + "-0-"+ prefissoipen + "-";// anche qui rendere universale non mettendo PI un domani potrebbe essere un altro orto
            }


            if (accessultima != null)
            {
                string ultimoprogressivo = accessultima.progressivo;
                int ult;

                Int32.TryParse(ultimoprogressivo.Substring(5, 4), out ult);

                ult++;

                retnextipen = retnextipen + accessultima.dataAcquisizione.Year.ToString() + "-" + ult.ToString().PadLeft(4, '0');
            }

            else
            {
                retnextipen += a.dataAcquisizione.Year.ToString() + "-" + "0001";
            }
            return retnextipen;

        }
        public Accessioni ultima(Accessioni a)
        {
            int anno = a.dataAcquisizione.Year;
            string username = User.Identities.FirstOrDefault()?.Claims?.Where(c => c.Type == "UnipiUserID").FirstOrDefault()?.Value;
            Guid organizzazione = _context.Users.Where(a => a.UnipiUserName == (username).Substring(0, username.IndexOf("@"))).Select(a => a.Organizzazione).FirstOrDefault();
            return _context.Accessioni.Where(c => c.dataAcquisizione.Year == anno && c.organizzazione == organizzazione).OrderByDescending(c => c.progressivo).FirstOrDefault();
        }

        public async Task<IActionResult> ValidaAccessione(Guid? id)
        {
            if (id == null || _context.Accessioni == null)
            {
                return NotFound();
            }

            var accessioni = await _context.Accessioni
                .Include(a => a.fornitoreNavigation)
                .Include(a => a.gradoIncertezzaNavigation)
                .Include(a => a.nazioneNavigation)
                .Include(a => a.organizzazioneNavigation)
                .Include(a => a.provenienzaNavigation)
                .Include(a => a.provinciaNavigation)
                .Include(a => a.raccoglitoreNavigation)
                .Include(a => a.regioneNavigation)
                .Include(a => a.specieNavigation)
                .Include(a => a.statoMaterialeNavigation)
                .Include(a => a.tipoAcquisizioneNavigation)
                .Include(a => a.tipoMaterialeNavigation)
                .Include(a => a.utenteAcquisizioneNavigation)
                .Include(a => a.utenteUltimaModificaNavigation)
                .Include(a => a.identificatoreNavigation)
                .FirstOrDefaultAsync(m => m.id == id);
                 
            return View(accessioni);
        }


        [HttpPost, ActionName("ValidaAccessione")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidaAccessioneConfirmed(Guid id)
        {

            Accessioni accessioni = _context.Accessioni.Find(id);
            accessioni.validazione = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}
