using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UPlant.Models.DB;
using DymoSDK.Implementations;
using DymoSDK.Interfaces;

using System.Configuration;




namespace UPlant.Controllers
{
    public class CommonController : BaseController
    {
        private readonly Entities _context;
        
        public CommonController(Entities context)
        {
            _context = context;
          


        }

        public JsonResult GetNazioni()
        {

            return Json(_context.Nazioni.OrderBy(x => x.descrizione).Select(x => new
            {
                codicenazione = x.codice,
                descrizionenazione = x.descrizione
            }).ToList(), new System.Text.Json.JsonSerializerOptions());  //, JsonRequestBehavior.AllowGet deprecato

        }
        public JsonResult GetRegioni()
        {

            return Json(_context.Regioni.OrderBy(x => x.descrizione).Select(x => new
            {
                codiceregione = x.codice,
                descrizioneregione = x.descrizione
            }).ToList(), new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

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
                nomescientifico = x.nome_scientifico,
                nomevolgare = x.nome_volgare
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
        public JsonResult Cercaspecie(string term)
        {

            //   var prelist = db.Storico.Include(x => x.Individui)
            //     .Where(p => p.Individui.Accessioni.Specie1.nome_scientifico.ToLower().StartsWith(term.ToLower())).Select(g => g.Individui.Accessioni.Specie1.nome_scientifico);//commentata per sostituire con una ricerca votata alle accessioni senza figli
            var prelist = _context.Accessioni.Where(p => p.specieNavigation.nome_scientifico.ToLower().StartsWith(term.ToLower())).Select(g => g.specieNavigation.nome_scientifico);

            var names = prelist.Distinct().ToList();

            return Json(names, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }
        public JsonResult Cercaprog(string term)
        {

            var prelist = _context.Accessioni
                .Where(p => p.progressivo.StartsWith(term)).Select(g => g.progressivo);


            var names = prelist.Distinct().ToList();

            return Json(names, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }
        public JsonResult Cercavecchioprog(string term)
        {

            var prelist = _context.Accessioni
                .Where(p => p.vecchioprogressivo.StartsWith(term)).Select(g => g.vecchioprogressivo);


            var names = prelist.Distinct().ToList();

            return Json(names, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato

        }
        public JsonResult AddSpecie(Specie addspecie)
        {
            var doppio = _context.Specie.Where(a => a.nome_scientifico.ToLower() == addspecie.nome_scientifico.ToLower()).ToList();

            esitoAddSpecie esito = new esitoAddSpecie();
            if (doppio.Count() > 0)
            {
                esito.errore = true;
                esito.message = "Esiste già una specie definita così controlla";
                esito.specie = null;
                return Json(esito, new System.Text.Json.JsonSerializerOptions());//, JsonRequestBehavior.AllowGet deprecato
            }



            if (ModelState.IsValid)
            {

                var iucnNON = _context.Iucn.Where(a => a.descrizione.ToLower().Contains("definito")).Select(a => a.id).FirstOrDefault();
                var citesNON = _context.Cites.Where(a => a.codice.ToLower().Contains("definito")).Select(a => a.id).FirstOrDefault();
                addspecie.id = Guid.NewGuid();
                addspecie.iucn_globale = iucnNON;
                addspecie.iucn_italia = iucnNON;
                addspecie.cites = citesNON;
                if (String.IsNullOrEmpty(addspecie.nome_comune))
                {
                    addspecie.nome_comune = "";
                }
                if (String.IsNullOrEmpty(addspecie.nome_comune_en))
                {
                    addspecie.nome_comune_en = "";
                }
                _context.Specie.Add(addspecie);
                try
                {
                    _context.SaveChanges();
                    esito.specie = addspecie;
                    esito.specie_id = addspecie.id.ToString();
                    esito.specie_nome = addspecie.nome_scientifico;
                    esito.errore = false;
                    esito.message = "Inserimento andato con successo!";
                }
                catch (Exception e)
                {
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

            return Json(_context.Settori.Select(x => new
            {
                codicesettore = x.id,
                descrizionesettore = x.settore
            }).ToList(), new System.Text.Json.JsonSerializerOptions());

        }

        public JsonResult GetCollezioni(Guid? codicesettore)
        {

            return Json(_context.Collezioni.OrderBy(a => a.collezione).Where(x => x.settore == codicesettore).Select(x => new
            {
                codicecollezione = x.id,
                descrizionecollezione = x.collezione
            }).ToList(), new System.Text.Json.JsonSerializerOptions());

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
            ViewBag.fornitore = new SelectList(_context.Fornitori.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.fornitore);

            ViewBag.provenienza = new SelectList(_context.Provenienze.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.provenienza);

            ViewBag.raccoglitore = new SelectList(_context.Raccoglitori.OrderBy(a => a.nominativo), "id", "nominativo", accessioni.raccoglitore);
            ViewBag.nazione = new SelectList(_context.Nazioni.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.nazione);
            ViewBag.provincia = new SelectList(_context.Province.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.provincia);
            ViewBag.regione = new SelectList(_context.Regioni.OrderBy(a => a.descrizione), "codice", "descrizione", accessioni.regione);
            //ViewBag.specie = new List<SelectListItem>();

            /* ViewBag.specie = db.Specie.OrderBy(n => n.nome_scientifico).Select(n=> new SelectListItem
             {

                 Value = n.id.ToString(),
                 Text = n.nome_scientifico + n.nome

             }).ToList();*/
            ViewBag.tipoAcquisizione = new SelectList(_context.TipoAcquisizione.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.tipoAcquisizione);
            ViewBag.tipoMateriale = new SelectList(_context.TipiMateriale.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.tipoMateriale);
            ViewBag.statoMateriale = new SelectList(_context.StatoMateriale.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.statoMateriale);
            ViewBag.gradoIncertezza = new SelectList(_context.GradoIncertezza.OrderBy(a => a.descrizione), "id", "descrizione", accessioni.gradoIncertezza);
            ViewBag.identificatore = new SelectList(_context.Identificatori.OrderBy(a => a.nominativo), "id", "nominativo", accessioni.identificatore);
            ViewBag.regno = new SelectList(_context.Regni.OrderBy(r => r.codiceInterno), "id", "descrizione");
            ViewBag.areale = new SelectList(_context.Areali.OrderBy(a => a.codiceInterno), "id", "descrizione");

            if (accessioni.specie != new Guid("{00000000-0000-0000-0000-000000000000}"))
            {
                Specie s = _context.Specie.Find(accessioni.specie);
                if (s != null)
                {
                    ViewBag.famiglia = new SelectList(_context.Famiglie.OrderBy(e => e.descrizione), "id", "descrizione", s.genereNavigation.famigliaNavigation.id);
                    ViewBag.genere = new SelectList(_context.Generi.Where(e => e.famiglia == s.genereNavigation.famigliaNavigation.id).OrderBy(e => e.descrizione), "id", "descrizione", s.genereNavigation.id);
                    ViewBag.specie = new SelectList(_context.Specie.Where(e => e.genere == s.genere).OrderBy(e => e.nome), "id", "nome", s.id);
                }
            }
            else
            {
                if (genere != "-1" && genere != null)
                {
                    Generi g = _context.Generi.Find(genere);
                    ViewBag.famiglia = new SelectList(_context.Famiglie.OrderBy(e => e.descrizione), "id", "descrizione", g.famigliaNavigation.id);
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
