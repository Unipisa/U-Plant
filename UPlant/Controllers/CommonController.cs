using DocumentFormat.OpenXml.Wordprocessing;
using DymoSDK.Implementations;
using DymoSDK.Interfaces;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UPlant.Models;
using UPlant.Models.DB;
using UPlant.Services;




namespace UPlant.Controllers
{
    public class CommonController : BaseController
    {
        private readonly Entities _context;
        private readonly LanguageService _languageService;
        private readonly ILogger<CommonController> _logger;
        public CommonController(Entities context, LanguageService languageService, ILogger<CommonController> logger)
        {
            _context = context;
            _languageService = languageService;
            _logger = logger;


        }

        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }


        public JsonResult GetNazioni()
        {
            var linguacorrente = _languageService.GetCurrentCulture();

            if (linguacorrente == "en-US")
            {

                return Json(_context.Nazioni.OrderBy(x => x.descrizione).Select(x => new
                {
                    codicenazione = x.codice,
                    descrizionenazione = x.descrizione_en == null ? x.descrizione : x.descrizione_en, // Se la descrizione in inglese è null, usa quella in italiano
                   
                }).ToList(), new System.Text.Json.JsonSerializerOptions());  //, JsonRequestBehavior.AllowGet deprecato

            }
            else
            {

                return Json(_context.Nazioni.OrderBy(x => x.descrizione).Select(x => new
                {
                    codicenazione = x.codice,
                    descrizionenazione = x.descrizione
                }).ToList(), new System.Text.Json.JsonSerializerOptions());  //, JsonRequestBehavior.AllowGet deprecato

            }


          

        }
        public JsonResult GetRegioni()
        {
            var linguacorrente = _languageService.GetCurrentCulture();
            
            if (linguacorrente == "en-US")
            {

                return Json(_context.Regioni.OrderBy(x => x.descrizione).Select(x => new
                {
                    codiceregione = x.codice,
                    descrizioneregione = string.IsNullOrEmpty(x.descrizione_en) ? x.descrizione : x.descrizione_en
                }).ToList(), new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato


            }
            else
            {

                return Json(_context.Regioni.OrderBy(x => x.descrizione).Select(x => new
                {
                    codiceregione = x.codice,
                    descrizioneregione = x.descrizione
                }).ToList(), new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

            }




           

        }

        public JsonResult GetProvince(string codiceregione)
        {

            return Json(_context.Province.OrderBy(x => x.descrizione).Where(x => x.regione == codiceregione).Select(x => new
            {
                codiceprovincia = x.codice,
                descrizioneprovincia = x.descrizione
            }).ToList(), new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }

        public JsonResult GetGenere(Guid codicefamiglia)
        {

            return Json(_context.Generi.Where(x => x.famiglia == codicefamiglia).Select(x => new
            {
                codicegenere = x.id,
                descrizionegenere = x.descrizione,
                fkfamiglia = x.famiglia
            }).OrderBy(x => x.descrizionegenere).ToList(), new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }
        public JsonResult GetSpecie(Guid codicegenere)
        {

            return Json(_context.Specie.Where(x => x.genere == codicegenere).Select(x => new
            {
                codicespecie = x.id,
                nomespecie = x.nome,
                nomescientifico = x.nome_scientifico
            }).OrderBy(x => x.nomescientifico).ToList(), new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }

        public class esitoAddSpecie
        {
            public bool errore { get; set; }
            public string message { get; set; }
            public Specie specie { get; set; }
            public string specie_id { get; set; }
            public string specie_nome { get; set; }
        }

        public class AddSpecieInput
        {
            public Guid genere { get; set; }
            public string nome { get; set; }
            public string nome_comune { get; set; }
            public string nome_comune_en { get; set; }
            public string autori { get; set; }
            public Guid? regno { get; set; }
            public Guid? areale { get; set; }
            public string subspecie { get; set; }
            public string autorisub { get; set; }
            public string varieta { get; set; }
            public string autorivar { get; set; }
            public string cult { get; set; }
            public string autoricult { get; set; }
            public string note { get; set; }
        }
        public JsonResult Cercaspecie(string term)
        {

            //   var prelist = db.Storico.Include(x => x.Individui)
            //     .Where(p => p.Individui.Accessioni.Specie1.nome_scientifico.ToLower().StartsWith(term.ToLower())).Select(g => g.Individui.Accessioni.Specie1.nome_scientifico);//commentata per sostituire con una ricerca votata alle accessioni senza figli
            var prelist = _context.Accessioni.Where(p => p.specieNavigation.nome_scientifico.ToLower().Contains(term.ToLower())).Select(g => g.specieNavigation.nome_scientifico);

            var names = prelist.Distinct().ToList();

            return Json(names, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }
        public JsonResult Cercadest(string term)
        {

            var prelist = _context.Individui
                .Where(p => p.destinazioni.Contains(term) && p.destinazioni != null).Select(g => g.destinazioni);


            var names = prelist.Distinct().ToList();

            return Json(names, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }
        public JsonResult Cercaprog(string term)
        {

            var prelist = _context.Accessioni
                .Where(p => p.progressivo.Contains(term)).Select(g => g.progressivo);


            var names = prelist.Distinct().ToList();

            return Json(names, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }
        public JsonResult Cercaipen(string term)
        {

            var prelist = _context.Accessioni
                .Where(p => p.ipen.Contains(term)).Select(g => g.ipen);


            var names = prelist.Distinct().ToList();

            return Json(names, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }
        public JsonResult Cercavecchioprog(string term)
        {

            var prelist = _context.Accessioni
                .Where(p => p.vecchioprogressivo.Contains(term)).Select(g => g.vecchioprogressivo);


            var names = prelist.Distinct().ToList();

            return Json(names, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }
        [HttpPost]
        public JsonResult AddSpecie([FromBody] AddSpecieInput input)
        {
            esitoAddSpecie esito = new esitoAddSpecie();

            _logger.LogInformation(
                "AddSpecie request received. Genere={GenereId}, Nome={Nome}, NomeComune={NomeComune}, NomeComuneEn={NomeComuneEn}, Note={Note}, Subspecie={Subspecie}, Varieta={Varieta}, Cult={Cult}",
                input?.genere,
                input?.nome,
                input?.nome_comune,
                input?.nome_comune_en,
                input?.note,
                input?.subspecie,
                input?.varieta,
                input?.cult);

            if (input == null || input.genere == Guid.Empty || string.IsNullOrWhiteSpace(input.nome))
            {
                _logger.LogWarning(
                    "AddSpecie rejected for incomplete data. InputNull={InputNull}, Genere={GenereId}, Nome={Nome}",
                    input == null,
                    input?.genere,
                    input?.nome);
                esito.errore = true;
                esito.message = "Dati specie incompleti.";
                esito.specie = null;
                return Json(esito, new System.Text.Json.JsonSerializerOptions());
            }

            var genusName = _context.Generi
                .Where(g => g.id == input.genere)
                .Select(g => g.descrizione)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(genusName))
            {
                _logger.LogWarning(
                    "AddSpecie rejected because genus was not found. Genere={GenereId}, Nome={Nome}",
                    input.genere,
                    input.nome);
                esito.errore = true;
                esito.message = "Genere non valido.";
                esito.specie = null;
                return Json(esito, new System.Text.Json.JsonSerializerOptions());
            }

            var nomeScientifico = SpecieScientificNameHelper.Compose(
                genusName,
                input.nome,
                input.autori,
                input.subspecie,
                input.autorisub,
                input.varieta,
                input.autorivar,
                input.cult,
                input.autoricult);

            var doppio = _context.Specie
                .Where(a => a.nome_scientifico.ToLower() == nomeScientifico.ToLower())
                .ToList();

            if (doppio.Count() > 0)
            {
                _logger.LogWarning(
                    "AddSpecie rejected because species already exists. NomeScientifico={NomeScientifico}, ExistingCount={ExistingCount}",
                    nomeScientifico,
                    doppio.Count());
                esito.errore = true;
                esito.message = "Esiste già una specie definita così controlla";
                esito.specie = null;
                return Json(esito, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato
            }

            if (!ModelState.IsValid)
            {
                var modelErrors = string.Join(" | ", ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(err => $"{x.Key}: {err.ErrorMessage}")));

                _logger.LogWarning(
                    "AddSpecie ModelState invalid. NomeScientifico={NomeScientifico}, Errors={Errors}",
                    nomeScientifico,
                    modelErrors);
            }

            if (ModelState.IsValid)
            {
                var iucnNON = _context.Iucn.Where(a => a.descrizione.ToLower().Contains("definito")).Select(a => a.id).FirstOrDefault();
                var citesNON = _context.Cites.Where(a => a.codice.ToLower().Contains("definito")).Select(a => a.id).FirstOrDefault();
                var validazioneNd = _context.ValidazioneTassonomica
                    .Where(v => v.descrizione == "N.D." || v.descrizione == "Non Definito")
                    .Select(v => v.id)
                    .FirstOrDefault();

                if (validazioneNd == Guid.Empty)
                {
                    validazioneNd = _context.ValidazioneTassonomica
                        .Where(v => v.descrizione.Contains("Definito"))
                        .Select(v => v.id)
                        .FirstOrDefault();
                }

                var addspecie = new Specie
                {
                    id = Guid.NewGuid(),
                    genere = input.genere,
                    validazione_tassonomica = validazioneNd,
                    nome = SpecieScientificNameHelper.NormalizeSpacing(input.nome),
                    nome_scientifico = nomeScientifico,
                    data_inserimento = DateTime.Now,
                    lsid = string.Empty,
                    note = SpecieScientificNameHelper.NormalizeSpacing(input.note),
                    autori = SpecieScientificNameHelper.NormalizeSpacing(input.autori),
                    regno = input.regno,
                    areale = input.areale,
                    subspecie = SpecieScientificNameHelper.NormalizeSpacing(input.subspecie),
                    autorisub = SpecieScientificNameHelper.NormalizeSpacing(input.autorisub),
                    varieta = SpecieScientificNameHelper.NormalizeSpacing(input.varieta),
                    autorivar = SpecieScientificNameHelper.NormalizeSpacing(input.autorivar),
                    cult = SpecieScientificNameHelper.NormalizeSpacing(input.cult),
                    autoricult = SpecieScientificNameHelper.NormalizeSpacing(input.autoricult),
                    nome_comune = SpecieScientificNameHelper.NormalizeSpacing(input.nome_comune),
                    nome_comune_en = SpecieScientificNameHelper.NormalizeSpacing(input.nome_comune_en),
                    iucn_globale = iucnNON,
                    iucn_italia = iucnNON,
                    cites = citesNON
                };

                if (!string.IsNullOrWhiteSpace(addspecie.nome_comune) &&
                    !string.IsNullOrWhiteSpace(addspecie.note) &&
                    string.Equals(addspecie.nome_comune, addspecie.note, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "AddSpecie is saving a species where note matches nome_comune. NomeScientifico={NomeScientifico}, NomeComune={NomeComune}, Note={Note}",
                        addspecie.nome_scientifico,
                        addspecie.nome_comune,
                        addspecie.note);
                }

                _logger.LogInformation(
                    "AddSpecie prepared species. Id={SpecieId}, NomeScientifico={NomeScientifico}, Genere={GenereId}, NomeComune={NomeComune}, Note={Note}, Validazione={ValidazioneId}",
                    addspecie.id,
                    addspecie.nome_scientifico,
                    addspecie.genere,
                    addspecie.nome_comune,
                    addspecie.note,
                    addspecie.validazione_tassonomica);

                _context.Specie.Add(addspecie);
                try
                {
                    _context.SaveChanges();
                    _logger.LogInformation(
                        "AddSpecie saved successfully. Id={SpecieId}, NomeScientifico={NomeScientifico}, NomeComune={NomeComune}, Note={Note}",
                        addspecie.id,
                        addspecie.nome_scientifico,
                        addspecie.nome_comune,
                        addspecie.note);
                    esito.specie = addspecie;
                    esito.specie_id = addspecie.id.ToString();
                    esito.specie_nome = addspecie.nome_scientifico;
                    esito.errore = false;
                    esito.message = "Inserimento andato con successo!";
                }
                catch (Exception e)
                {
                    _logger.LogError(
                        e,
                        "AddSpecie save failed. NomeScientifico={NomeScientifico}, NomeComune={NomeComune}, Note={Note}",
                        addspecie.nome_scientifico,
                        addspecie.nome_comune,
                        addspecie.note);
                    esito.errore = true;
                    esito.message = e.Message;
                    esito.specie = null;

                }
            }

            return Json(esito, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato
        }

        public class esitoAddGenere
        {
            public bool erroreG { get; set; }
            public string messageG { get; set; }
            public string genere_id { get; set; }
            public string genere_descrizione { get; set; }
        }



        public JsonResult AddGenere(Generi addgenere)
        {
            var doppio = _context.Generi.Where(a => a.descrizione.ToLower() == addgenere.descrizione.ToLower().Trim()).ToList();

            esitoAddGenere esito = new esitoAddGenere();

            if (doppio.Count() > 0)
            {
                esito.erroreG = true;
                esito.messageG = "Esiste già un genere definito così controlla";
                esito.genere_id = "";
                esito.genere_descrizione = "";
                return Json(esito, new System.Text.Json.JsonSerializerOptions());
            }



            if (ModelState.IsValid)
            {
                addgenere.id = Guid.NewGuid();
                _context.Generi.Add(addgenere);

                try
                {
                    _context.SaveChanges();
                    esito.genere_id = addgenere.id.ToString();
                    esito.genere_descrizione = addgenere.descrizione;
                    esito.erroreG = false;
                    esito.messageG = "Inserimento andato con successo!";
                }
                catch (Exception e)
                {
                    esito.erroreG = true;
                    esito.messageG = e.Message;
                    esito.genere_id = "";
                    esito.genere_descrizione = "";
                }

            }

            return Json(esito, new System.Text.Json.JsonSerializerOptions());
        }

        public JsonResult GetAccessioniNoAuth()
        {
            var numacc = _context.Accessioni.Where(x => x.validazione == false).Count();
            return Json(numacc);

        }
        public JsonResult GetAccNoAuth()
        {
            int AccResultsCount;
            int IndResultsCount;
            int totalResultsCount;
            int IndResultsCountOk;

            //IQueryable<Accessioni> allEntities = db.Accessioni.Where(x => x.validazione == false);
            //totalResultsCount = allEntities.Count();
            AccResultsCount = _context.Accessioni.Where(x => x.validazione == false).Count();
            IndResultsCount = _context.Individui.Where(x => x.validazione == false).Count();
            IndResultsCountOk = _context.Individui.Where(x => x.validazione == true).Count();
            totalResultsCount = AccResultsCount + IndResultsCount;
            return Json(new
            {
                // this is what datatables wants sending back
                recordAcc = AccResultsCount,
                recordInd = IndResultsCount,
                recordsTotal = totalResultsCount,
                recordIndOk = IndResultsCountOk

            });
        }
        public JsonResult GetSettori()
        {
            var linguacorrente = _languageService.GetCurrentCulture();

            if (linguacorrente == "en-US")
            {
                return Json(_context.Settori.Select(x => new
                {
                    codicesettore = x.id,
                    descrizionesettore = string.IsNullOrEmpty(x.settore_en) ? x.settore : x.settore_en,
                    coloresettore = x.colore
                }).ToList(), new System.Text.Json.JsonSerializerOptions());
            }
            else
            {
                return Json(_context.Settori.Select(x => new
                {
                    codicesettore = x.id,
                    descrizionesettore = x.settore,
                    coloresettore = x.colore
                }).ToList(), new System.Text.Json.JsonSerializerOptions());
            }
        }

        public JsonResult GetCollezioni(Guid? codicesettore)
        {
            var linguacorrente = _languageService.GetCurrentCulture();

            if (linguacorrente == "en-US")
            {

                return Json(_context.Collezioni.OrderBy(a => a.collezione).Where(x => x.settore == codicesettore).Select(x => new
                {
                    codicecollezione = x.id,
                    descrizionecollezione = string.IsNullOrEmpty(x.collezione_en) ? x.collezione : x.collezione_en
                }).ToList(), new System.Text.Json.JsonSerializerOptions());
            }
            else
            {
                return Json(_context.Collezioni.OrderBy(a => a.collezione).Where(x => x.settore == codicesettore).Select(x => new
                {
                    codicecollezione = x.id,
                    descrizionecollezione = x.collezione
                }).ToList(), new System.Text.Json.JsonSerializerOptions());
            }
        }




        
      

        #region Props
        IEnumerable<DymoSDK.Interfaces.IPrinter> _printers;


        public IEnumerable<DymoSDK.Interfaces.IPrinter> Printers
        {
            get
            {
                if (_printers == null)
                    _printers = new List<DymoSDK.Interfaces.IPrinter>();
                return _printers;
            }
            set
            {
                _printers = value;
                // NotifyPropertyChanged("Printers");
            }
        }

        public int PrintersFound
        {
            get { return Printers.Count(); }
        }

        string _fileName;
        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(_fileName))
                    return "No file selected";

                return _fileName;
            }
            set
            {
                _fileName = value;
                //  NotifyPropertyChanged("FileName");
            }
        }
        

        List<DymoSDK.Interfaces.ILabelObject> _labelObjects;
        public List<DymoSDK.Interfaces.ILabelObject> LabelObjects
        {
            get
            {
                if (_labelObjects == null)
                    _labelObjects = new List<DymoSDK.Interfaces.ILabelObject>();
                return _labelObjects;
            }
            set
            {
                _labelObjects = value;
                //  NotifyPropertyChanged("LabelObjects");
            }
        }

        private DymoSDK.Interfaces.ILabelObject _selectedLabelObject;
        public DymoSDK.Interfaces.ILabelObject SelectedLabelObject
        {
            get { return _selectedLabelObject; }
            set
            {
                _selectedLabelObject = value;
                //  NotifyPropertyChanged("SelectedLabelObject");
            }
        }

        private string _objectValue;
        public string ObjectValue
        {
            get { return _objectValue; }
            set
            {
                _objectValue = value;
                //    NotifyPropertyChanged("ObjectValue");
            }
        }


        DymoSDK.Interfaces.IPrinter _selectedPrinter;
        public DymoSDK.Interfaces.IPrinter SelectedPrinter
        {
            get { return _selectedPrinter; }
            set
            {
                _selectedPrinter = value;
                // NotifyPropertyChanged("SelectedPrinter");
                // DisplayConsumableInformation();
            }
        }

        List<string> _twinTurboRolls;
        public List<string> TwinTurboRolls
        {
            get
            {
                if (_twinTurboRolls == null)
                    _twinTurboRolls = new List<string>();
                return _twinTurboRolls;
            }
            set
            {
                _twinTurboRolls = value;
                // NotifyPropertyChanged("TwinTurboRolls");
            }
        }

        private string _selectedRoll;
        public string SelectedRoll
        {
            get { return _selectedRoll; }
            set
            {
                _selectedRoll = value;
                //  NotifyPropertyChanged("SelectedRoll");
            }
        }

        private string _consumableInfoText;
        public string ConsumableInfoText
        {
            get { return _consumableInfoText; }
            set
            {
                _consumableInfoText = value;
                //NotifyPropertyChanged("ConsumableInfoText");
            }
        }

        private bool _showConsumableInfo;
        public bool ShowConsumableInfo
        {
            get { return _showConsumableInfo; }
            set
            {
                _showConsumableInfo = value;
                // NotifyPropertyChanged("ShowConsumableInfo");
            }
        }
        #endregion
       
        public static List<string> GetLabelWriterList(bool tape = true)
        {
            List<string> printers = new List<string>();
            //    foreach (DymoSDK.Interfaces.IPrinter printer in DymoPrinter.Instance.GetPrinters()) //Get list of DYMO printers installed
            //    {
            //    printers.Add(printer.Name);
            // }
            return printers;
        }


        public void PrintLabel(string qrcode, string nomeetichetta, string ipen, string shortprog)
        {
            //devo passargli i valori stampa('@item.id','@item.nomeetichetta','@item.ipen','@item.shortprog')

            //string XmlContent = "<DieCutLabel Version=\"8.0\" Units=\"twips\"><PaperOrientation>Landscape</PaperOrientation><Id>ReturnAddressInt</Id><PaperName>11352 Return Address Int</PaperName><DrawCommands><RoundRectangle X=\"0\" Y=\"0\" Width=\"1440\" Height=\"3060\" Rx=\"180\" Ry=\"180\" /></DrawCommands><ObjectInfo><TextObject><Name>nome</Name><ForeColor Alpha=\"255\" Red=\"0\" Green=\"0\" Blue=\"0\" ></ForeColor><BackColor Alpha=\"0\" Red=\"255\" Green=\"255\" Blue=\"255\" ></BackColor><LinkedObjectName></LinkedObjectName><Rotation>Rotation0</Rotation><IsMirrored>False</IsMirrored><IsVariable>False</IsVariable><HorizontalAlignment>Center</HorizontalAlignment><VerticalAlignment>Top</VerticalAlignment><TextFitMode>AlwaysFit</TextFitMode><UseFullFontHeight>True</UseFullFontHeight><Verticalized>False</Verticalized><StyledText><Element><String></String><Attributes><Font Family=\"Tahoma\" Size=\"9\" Bold=\"False\" Italic=\"False\" Underline=\"False\" Strikeout=\"False\" /><ForeColor Alpha=\"255\" Red=\"0\" Green=\"0\" Blue=\"0\" ></ForeColor></Attributes></Element></StyledText></TextObject><Bounds X=\"1150\" Y=\"188\" Width=\"1756\" Height=\"566.929142372689\" /></ObjectInfo><ObjectInfo><BarcodeObject><Name>qrcode</Name><ForeColor Alpha=\"255\" Red=\"0\" Green=\"0\" Blue=\"0\" ></ForeColor><BackColor Alpha=\"0\" Red=\"255\" Green=\"255\" Blue=\"255\" ></BackColor><LinkedObjectName></LinkedObjectName><Rotation>Rotation0</Rotation><IsMirrored>False</IsMirrored><IsVariable>False</IsVariable><Text></Text><Type>QRCode</Type><Size>Medium</Size><TextPosition>None</TextPosition><TextFont Family=\"Arial\" Size=\"8\" Bold=\"False\" Italic=\"False\" Underline=\"False\" Strikeout=\"False\" /><CheckSumFont Family=\"Arial\" Size=\"8\" Bold=\"False\" Italic=\"False\" Underline=\"False\" Strikeout=\"False\" /><TextEmbedding>None</TextEmbedding><ECLevel>0</ECLevel><HorizontalAlignment>Center</HorizontalAlignment><QuietZonesPadding Left=\"0\" Top=\"0\" Right=\"0\" Bottom=\"0\" /></BarcodeObject><Bounds X=\"130\" Y=\"225\" Width=\"986\" Height=\"960\" /></ObjectInfo><ObjectInfo><TextObject><Name>ipen</Name><ForeColor Alpha=\"255\" Red=\"0\" Green=\"0\" Blue=\"0\" ></ForeColor><BackColor Alpha=\"0\" Red=\"255\" Green=\"255\" Blue=\"255\" ></BackColor><LinkedObjectName></LinkedObjectName><Rotation>Rotation0</Rotation><IsMirrored>False</IsMirrored><IsVariable>False</IsVariable><HorizontalAlignment>Left</HorizontalAlignment><VerticalAlignment>Top</VerticalAlignment><TextFitMode>AlwaysFit</TextFitMode><UseFullFontHeight>True</UseFullFontHeight><Verticalized>False</Verticalized><StyledText><Element><String></String><Attributes><Font Family=\"Tahoma\" Size=\"9\" Bold=\"False\" Italic=\"False\" Underline=\"False\" Strikeout=\"False\" /><ForeColor Alpha=\"255\" Red=\"0\" Green=\"0\" Blue=\"0\" ></ForeColor></Attributes></Element></StyledText></TextObject><Bounds X=\"1150\" Y=\"737.007885084496\" Width=\"1286\" Height=\"240\" /></ObjectInfo><ObjectInfo><TextObject><Name>progressivo</Name><ForeColor Alpha=\"255\" Red=\"0\" Green=\"0\" Blue=\"0\" ></ForeColor><BackColor Alpha=\"0\" Red=\"255\" Green=\"255\" Blue=\"255\" ></BackColor><LinkedObjectName></LinkedObjectName><Rotation>Rotation0</Rotation><IsMirrored>False</IsMirrored><IsVariable>False</IsVariable><HorizontalAlignment>Left</HorizontalAlignment><VerticalAlignment>Top</VerticalAlignment><TextFitMode>AlwaysFit</TextFitMode><UseFullFontHeight>True</UseFullFontHeight><Verticalized>False</Verticalized><StyledText><Element><String></String><Attributes><Font Family=\"Tahoma\" Size=\"8\" Bold=\"False\" Italic=\"False\" Underline=\"False\" Strikeout=\"False\" /><ForeColor Alpha=\"255\" Red=\"0\" Green=\"0\" Blue=\"0\" ></ForeColor></Attributes></Element></StyledText></TextObject><Bounds X=\"1135\" Y=\"963.779542033572\" Width=\"1781\" Height=\"170.078742711807\" /></ObjectInfo></DieCutLabel>";
            string XmlContent = "<DieCutLabel Version='8.0' Units='twips'><PaperOrientation>Landscape</PaperOrientation><Id>LargeShipping</Id><PaperName>30256 Shipping</PaperName><DrawCommands><RoundRectangle X='0' Y='0' Width='1440' Height='3060' Rx='180' Ry='180' /></DrawCommands><ObjectInfo><TextObject><Name>" + nomeetichetta + "</Name><ForeColor Alpha='255' Red='0' Green='0' Blue='0' /><BackColor Alpha='0' Red='255' Green='255' Blue='255' /><LinkedObjectName></LinkedObjectName><Rotation>Rotation0</Rotation><IsMirrored>False</IsMirrored><IsVariable>False</IsVariable><HorizontalAlignment>Center</HorizontalAlignment><VerticalAlignment>Top</VerticalAlignment><TextFitMode>AlwaysFit</TextFitMode><UseFullFontHeight>True</UseFullFontHeight><Verticalized>False</Verticalized><StyledText><Element><String></String><Attributes><Font Family='Tahoma' Size='9' Bold='False' Italic='False' Underline='False' Strikeout='False' /><ForeColor Alpha='255' Red='0' Green='0' Blue='0' /></Attributes></Element></StyledText></TextObject><Bounds X='1150' Y='188' Width='1756' Height='566.929142372689' /></ObjectInfo><ObjectInfo><BarcodeObject><Name>" + qrcode + "</Name><ForeColor Alpha='255' Red='0' Green='0' Blue='0' /><BackColor Alpha='0' Red='255' Green='255' Blue='255' /><LinkedObjectName></LinkedObjectName><Rotation>Rotation0</Rotation><IsMirrored>False</IsMirrored><IsVariable>False</IsVariable><Text></Text><Type>QRCode</Type><Size>Medium</Size><TextPosition>None</TextPosition><TextFont Family='Arial' Size='8' Bold='False' Italic='False' Underline='False' Strikeout='False' /><CheckSumFont Family='Arial' Size='8' Bold='False' Italic='False' Underline='False' Strikeout='False' /><TextEmbedding>None</TextEmbedding><ECLevel>0</ECLevel><HorizontalAlignment>Center</HorizontalAlignment><QuietZonesPadding Left='0' Top='0' Right='0' Bottom='0' /></BarcodeObject><Bounds X='130' Y='225' Width='986' Height='960' /></ObjectInfo><ObjectInfo><TextObject><Name>" + ipen + "</Name><ForeColor Alpha='255' Red='0' Green='0' Blue='0' /><BackColor Alpha='0' Red='255' Green='255' Blue='255' /><LinkedObjectName></LinkedObjectName><Rotation>Rotation0</Rotation><IsMirrored>False</IsMirrored><IsVariable>False</IsVariable><HorizontalAlignment>Left</HorizontalAlignment><VerticalAlignment>Top</VerticalAlignment><TextFitMode>AlwaysFit</TextFitMode><UseFullFontHeight>True</UseFullFontHeight><Verticalized>False</Verticalized><StyledText><Element><String></String><Attributes><Font Family='Tahoma' Size='9' Bold='False' Italic='False' Underline='False' Strikeout='False' /><ForeColor Alpha='255' Red='0' Green='0' Blue='0' /></Attributes></Element></StyledText></TextObject><Bounds X='1150' Y='737.007885084496' Width='1286' Height='240' /></ObjectInfo><ObjectInfo><TextObject><Name>" + shortprog + "</Name><ForeColor Alpha='255' Red='0' Green='0' Blue='0' /><BackColor Alpha='0' Red='255' Green='255' Blue='255' /><LinkedObjectName></LinkedObjectName><Rotation>Rotation0</Rotation><IsMirrored>False</IsMirrored><IsVariable>False</IsVariable><HorizontalAlignment>Left</HorizontalAlignment><VerticalAlignment>Top</VerticalAlignment><TextFitMode>AlwaysFit</TextFitMode><UseFullFontHeight>True</UseFullFontHeight><Verticalized>False</Verticalized><StyledText><Element><String></String><Attributes><Font Family='Tahoma' Size='8' Bold='False' Italic='False' Underline='False' Strikeout='False' /><ForeColor Alpha='255' Red='0' Green='0' Blue='0' /></Attributes></Element></StyledText></TextObject><Bounds X='1135' Y='963.779542033572' Width='1781' Height='170.078742711807' /></ObjectInfo></DieCutLabel>";


            DymoSDK.Interfaces.IDymoLabel dymoSDKLabel;

            DymoSDK.App.Init();

            dymoSDKLabel = DymoLabel.Instance;
            //dymoSDKLabel.LoadLabelFromFilePath("C:\\Users\\a038858\\source\\repos\\U-Plant_AZURE\\U-Plant\\etichetta.label");
            SelectedPrinter.Name = "DYMO LabelWriter 450 Turbo";
            dymoSDKLabel.LoadLabelFromXML(XmlContent);

            List<string> printers = new List<string>();
            //     foreach (DymoSDK.Interfaces.IPrinter printer in DymoPrinter.Instance.GetPrinters()) //Get list of DYMO printers installed
            //      {
            //     printers.Add(printer.Name);
            // }
            //Printers = DymoPrinter.Instance.GetPrinters();
            //TwinTurboRolls = new List<string>() { "Auto", "Left", "Right" };

            int copies = 1;
            if (printers != null)
            {

                if (SelectedPrinter.Name.Contains("Twin Turbo"))
                {
                    int rollSel = SelectedRoll == "Auto" ? 0 : SelectedRoll == "Left" ? 1 : 2;
                    DymoPrinter.Instance.PrintLabel(dymoSDKLabel, SelectedPrinter.Name, copies, rollSelected: rollSel);
                }
                else
                    DymoPrinter.Instance.PrintLabel(dymoSDKLabel, SelectedPrinter.Name, copies);



              
            }

        }


        

       




        private void SetViewBag(Accessioni accessioni, string genere, string famiglia, bool edit = false)
        {
            var linguacorrente = _languageService.GetCurrentCulture();


            if (linguacorrente == "en-US")
            {
                ViewBag.regione = new SelectList(_context.Regioni.OrderBy(a => a.descrizione).Select(a => new { a.codice, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "codice", "Desc", accessioni.regione);
                ViewBag.nazione = new SelectList(_context.Nazioni.OrderBy(a => a.descrizione).Select(a => new { a.codice, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "codice", "Desc", accessioni.nazione);
                ViewBag.gradoIncertezza = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", accessioni.gradoIncertezza);
                ViewBag.provenienza = new SelectList(_context.Provenienze.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", accessioni.provenienza);
                ViewBag.statoMateriale = new SelectList(_context.StatoMateriale.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", accessioni.statoMateriale);
                ViewBag.tipoAcquisizione = new SelectList(_context.TipoAcquisizione.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", accessioni.tipoAcquisizione);
                ViewBag.tipoMateriale = new SelectList(_context.TipiMateriale.OrderBy(a => a.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", accessioni.tipoMateriale);
                ViewBag.regno = new SelectList(_context.Regni.OrderBy(e => e.ordinamento).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc");
            }
            else
            {
                ViewBag.regione = new SelectList(_context.Regioni.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.regione);
                ViewBag.nazione = new SelectList(_context.Nazioni.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.nazione);
                ViewBag.gradoIncertezza = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.gradoIncertezza);
                ViewBag.provenienza = new SelectList(_context.Provenienze.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.provenienza);
                ViewBag.statoMateriale = new SelectList(_context.StatoMateriale.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.statoMateriale);
                ViewBag.tipoAcquisizione = new SelectList(_context.TipoAcquisizione.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.tipoAcquisizione);
                ViewBag.tipoMateriale = new SelectList(_context.TipiMateriale.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.tipoMateriale);
                ViewBag.regno = new SelectList(_context.Regni.OrderBy(r => r.codiceInterno), "id", "descrizione");


            }
            ViewBag.fornitore = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.fornitore);
            ViewBag.raccoglitore = new SelectList(_context.Raccoglitori.OrderBy(a => a.nominativo), "id", "nominativo", accessioni.raccoglitore);
            ViewBag.provincia = new SelectList(_context.Province.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.provincia);
           ViewBag.identificatore = new SelectList(_context.Identificatori.OrderBy(a => a.nominativo), "id", "nominativo", accessioni.identificatore);
            
            ViewBag.areale = new SelectList(_context.Areali.OrderBy(a => a.codiceInterno), "id", "descrizione");

            if (accessioni.specie != new Guid("{00000000-0000-0000-0000-000000000000}"))
            {
                Specie s = _context.Specie.Find(accessioni.specie);
                if (s != null)
                {
                    if (linguacorrente == "en-US")
                    {
                        ViewBag.famiglia = new SelectList(_context.Famiglie.OrderBy(e => e.descrizione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", s.genereNavigation.famigliaNavigation.id);
                    }
                    else
                    {
                        ViewBag.famiglia = new SelectList(_context.Famiglie.OrderBy(e => e.descrizione), "id", "descrizione", s.genereNavigation.famigliaNavigation.id);
                    }
                        ViewBag.genere = new SelectList(_context.Generi.Where(e => e.famiglia == s.genereNavigation.famigliaNavigation.id).OrderBy(e => e.descrizione), "id", "descrizione", s.genereNavigation.id);
                    ViewBag.specie = new SelectList(_context.Specie.Where(e => e.genere == s.genere).OrderBy(e => e.nome), "id", "nome", s.id);
                }
            }
            else
            {
                if (genere != "-1" && genere != null)
                {
                    Generi g = _context.Generi.Find(genere);

                    if (linguacorrente == "en-US")
                    {
                        ViewBag.famiglia = new SelectList(_context.Famiglie.OrderBy(e => e.descrizione).Select(a => new { a.id, Desc = string.IsNullOrEmpty(a.descrizione_en) ? a.descrizione : a.descrizione_en }), "id", "Desc", g.famigliaNavigation.id);
                    }
                    else
                    {
                        ViewBag.famiglia = new SelectList(_context.Famiglie.OrderBy(e => e.descrizione), "id", "descrizione", g.famigliaNavigation.id);
                    }
                        
                        
                        ViewBag.genere = new SelectList(_context.Generi.Where(e => e.famiglia == g.famigliaNavigation.id).OrderBy(e => e.descrizione), "id", "descrizione", g.id);
                }
                else
                {
                    ViewBag.famiglia = new SelectList(_context.Famiglie.OrderBy(e => e.descrizione), "id", "descrizione");
                    List<SelectListItem> t = new List<SelectListItem>();
                    ViewBag.genere = new SelectList(t);
                }
                List<SelectListItem> y = new List<SelectListItem>();
                ViewBag.specie = new SelectList(y);
            }
            if (edit)
                ViewBag.specie = new SelectList(_context.Specie.Where(x => x.genere == accessioni.specieNavigation.genere), "id", "nome_scientifico", accessioni.specie);

            return;
        }

        private void contaFigli(string id)
        {
            int conta = _context.Individui.Where(x => x.validazione == true).Count();
        }
        

    }
}
