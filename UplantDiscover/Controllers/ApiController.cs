using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UplantDiscover.Models.DB;
using UplantDiscover.Models;
//using System.Text.Json;
//using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Net.Http.Headers;


// Progetto UplantDiscover ,rappresentazione dei dati inseriti su UPlant per la fruibilità pubblica ideato e scritto da Pietro Picconi pietro.picconi@unipi.it

namespace UplantDiscover.Controllers
{


    [ApiController]
    [Route("api/orto")]
    public class ApiStoricoController : ControllerBase
    {
        
        private readonly Entities _context;
        private readonly Images _images;
        public ApiStoricoController(Entities context, IOptions<Images> images)
        {
            _context = context;
            _images = images.Value;
        }

        JsonSerializerSettings settings = new()
        {
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };



       [HttpGet("ricerca")]

        public IActionResult Ricerca(string specie, string famiglia, string regno, string settore, string collezione, string collezionedescr, string lingua,string tiporicerca)
        {
            //string referer = Request.Headers["Referer"].ToString();
            /* se uno vuole utilizzare api esterno può avvelersi attivando il token
            Request.Headers.TryGetValue("Authorization", out var headerValue);
           if (headerValue == "Bearer 456-12A4-2288-123F" || Request.Host.Host == "localhost") { */
                try { 
                // var allowedStatus = new[] { "30e70f7c13774994ac9215b3543ebd7b", "3d91514fecb3473783eda3d3f8a63457" }; //Vivo e incerto
              //  var allowedStatus = new[] { "30e70f7c13774994ac9215b3543ebd7b" }; // Vivo
              //  var notallowedsector = new[] { "0ba85efcea3544e485141f7e311d82e2", "0e551835b07642f88540a4ff9d15e84e", "17650e74de9e40c3b1b604531c1d0f6f", "900fdc0ec2de45098ccc5013e796b14f" }; //Nursery , Banca Semi,Portineria e Non Definito
       // 

                IEnumerable<StoricoIndividuo> listaind = _context.StoricoIndividuo
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.collezioneNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.ImmaginiIndividuo)
                    .Include(x => x.statoIndividuoNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.specieNavigation).ThenInclude(x => x.genereNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.specieNavigation)
                    .AsEnumerable()
                        .GroupBy(c => c.individuo)
                        .Select(g => g.OrderByDescending(c => c.dataInserimento).FirstOrDefault())
                        .ToList();

                listaind = listaind.Where(x => x.statoIndividuoNavigation.visualizzazioneweb == true);
                listaind = listaind.Where(x => x.individuoNavigation.settoreNavigation.visualizzazioneweb == true);
                //listaind = listaind.Where(x => allowedStatus.Contains(x.statoindividuoNavigation.stato));
                //listaind = listaind.Where(x => !notallowedsector.Contains(x.individuoNavigation.settoreNavigation.settore));


                if (specie != null && specie.Length > 2)
                {
                        if (tiporicerca == "c") {

                          if  (lingua  == "it") {
                                listaind = listaind.Where(x => x.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune != null ).Where(x => x.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune.ToLower().Contains(specie.ToLower()));
                            } else {
                                listaind = listaind.Where(x => x.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune_en != null).Where(x => x.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune_en.ToLower().Contains(specie.ToLower()));
                            }
                        } else { 
                            listaind = listaind.Where(x => x.individuoNavigation.accessioneNavigation.specieNavigation.nome_scientifico.ToLower().Contains(specie.ToLower()));
                        }
                    }

                if (famiglia != null && famiglia.Length > 2)
                {

                    listaind = listaind.Where(x => x.individuoNavigation.accessioneNavigation.specieNavigation.genereNavigation.famiglia == new Guid(famiglia));

                }
                if (regno != null && regno.Length > 2)
                {

                    listaind = listaind.Where(x => x.individuoNavigation.accessioneNavigation.specieNavigation.regno == new Guid(regno));

                }
                if (settore != null && settore.Length > 2)
                {

                    listaind = listaind.Where(x => x.individuoNavigation.settore == new Guid(settore));

                }
                if (collezione != null && collezione.Length > 2)
                {
                        if (settore != null && settore.Length > 2)
                        {
                            listaind = listaind.Where(x => x.individuoNavigation.collezione == new Guid(collezione));
                        }
                        else
                        {

                            if (lingua == "it")
                            {
                                listaind = listaind.Where(x => x.individuoNavigation.collezioneNavigation.collezione.Contains(collezionedescr));
                            }
                            if (lingua == "en")
                            {
                                listaind = listaind.Where(x => x.individuoNavigation.collezioneNavigation.collezione_en.Contains(collezionedescr));
                            }
                        }

                }



                List<RisultatoRicerca> listarisultatoricerca = listaind.Select(x => new RisultatoRicerca
                {
                    Id = x.id,
                    Individuo = x.individuo,
                    Progressivo = x.individuoNavigation.progressivo,
                    Ipen = x.individuoNavigation.accessioneNavigation.ipen,
                    Indentitatassonomica = x.individuoNavigation.accessioneNavigation.specieNavigation.nome_scientifico,
                    Nomecomune = x.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune,
                    Nomecomuneen = x.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune_en,
                    Longitudine = x.individuoNavigation.longitudine,
                    Latitudine = x.individuoNavigation.latitudine,
                    Settoreid = x.individuoNavigation.settoreNavigation.id,
                    Settore = lingua == "it" ? x.individuoNavigation.settoreNavigation.settore : x.individuoNavigation.settoreNavigation.settore_en,
                    Collezione = lingua == "it" ? x.individuoNavigation.collezioneNavigation.collezione : x.individuoNavigation.collezioneNavigation.collezione_en,
                    Propagatodata = string.Format(Convert.ToDateTime(x.individuoNavigation.propagatoData).ToString(), "{0:yyyy-MM-dd HH:mm:ss}", "yyyy"),
                    Stato = x.statoIndividuoNavigation.stato,
                    NumeroImmagini = x.individuoNavigation.ImmaginiIndividuo.Where(a => a.visibile).Count(),
                    Ordinamento = x.individuoNavigation.settoreNavigation.ordinamento

                }).ToList();

                foreach (var x in listarisultatoricerca)
                {
                    if (x.NumeroImmagini == 0)
                    {
                        x.Immagine = "";
                    }
                    else
                    {
                            ImmaginiIndividuo immagini = _context.ImmaginiIndividuo.Where(i => i.predefinita == true && i.visibile && i.individuo == x.Individuo).FirstOrDefault();
                        if (immagini != null)
                        {
                            //aggiungere estensione file
                            int posizione = immagini.nomefile.IndexOf(".");
                            string estensione = immagini.nomefile.Substring(posizione);
                            x.Immagine = String.Concat(_images.Percorso, x.Individuo, "/thumb/", immagini.id, estensione);
                        }
                        else
                        {
                            x.Immagine = "";
                        }
                    }
                }

                string stringjson = JsonConvert.SerializeObject(listarisultatoricerca, settings);
                return Content(stringjson, "application/json");
                } catch (Exception e) {
                    
                   return BadRequest(e.Message);
                }

          //  } else {
          //      return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, "Non Sei Autorizzato");
          //  }
        }
        
        [HttpGet("cercaspecie")]
        public IActionResult Cercaspecie(string lingua, string tipo)
        {
          /*Request.Headers.TryGetValue("Authorization", out var headerValue);
           if (headerValue == "Bearer 456-12A4-2288-123F" || Request.Host.Host == "localhost") {*/
            
             try
             {
                string term = HttpContext.Request.Query["term"].ToString();
                   
                     var prelist = _context.StoricoIndividuo.Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation).Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation)
                    .Where(x => x.individuoNavigation.StoricoIndividuo.OrderByDescending(x => x.dataInserimento).Select(x => x.statoIndividuoNavigation.visualizzazioneweb == true).First())
                    .Where(x => x.individuoNavigation.settoreNavigation.visualizzazioneweb == true)
                    .Where(p => p.individuoNavigation.accessioneNavigation.specieNavigation.nome_scientifico.ToLower().StartsWith(term.ToLower())).Select(g => g.individuoNavigation.accessioneNavigation.specieNavigation.nome_scientifico);

                    if (tipo == "c")
                    {
                     if (lingua == "it") {
                            prelist = _context.StoricoIndividuo.Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation).Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation)
                                        .Where(x => x.individuoNavigation.StoricoIndividuo.OrderByDescending(x => x.dataInserimento).Select(x => x.statoIndividuoNavigation.visualizzazioneweb == true).First())
                                        .Where(x => x.individuoNavigation.settoreNavigation.visualizzazioneweb == true)
                                        .Where(p => p.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune != null)
                                        .Where(p => p.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune.ToLower().Contains(term.ToLower())).Select(g => g.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune);
                        } else {
                            prelist = _context.StoricoIndividuo.Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation).Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation)
                                        .Where(x => x.individuoNavigation.StoricoIndividuo.OrderByDescending(x => x.dataInserimento).Select(x => x.statoIndividuoNavigation.visualizzazioneweb == true).First())
                                        .Where(x => x.individuoNavigation.settoreNavigation.visualizzazioneweb == true)
                                        .Where(p => p.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune_en != null)
                                        .Where(p => p.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune_en.ToLower().Contains(term.ToLower())).Select(g => g.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune_en);
                        }
                    }
                    var names = prelist.Distinct().ToList();
               
                return Ok(names);
             }
                 catch (Exception e) {
              
                return BadRequest(e.Message);
             }
         /*  } else {
                return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, "Non Sei Autorizzato");
           }*/
        }



        // nuovo
        [HttpGet("ultimoinserito")]

        public IActionResult Ultimo(string lingua)
        {
            // Request.Headers.TryGetValue("Authorization", out var headerValue);
               // if (headerValue == "Bearer 456-12A4-2288-123F" || Request.Host.Host =="localhost") { 
            try
            {
     
         

                    IEnumerable<StoricoIndividuo> listaind = _context.StoricoIndividuo
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.collezioneNavigation)

                    .Include(x => x.individuoNavigation).ThenInclude(x => x.ImmaginiIndividuo)
                    .Include(x => x.statoIndividuoNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.specieNavigation).ThenInclude(x => x.genereNavigation).ThenInclude(x => x.famigliaNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.specieNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.tipoAcquisizioneNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.provenienzaNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.nazioneNavigation)
                    .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.regioneNavigation)
                    .AsEnumerable()
                    .OrderByDescending(c => c.individuoNavigation.propagatoData)
                    .Where(x => x.individuoNavigation.ImmaginiIndividuo.Count() > 0)
                    .GroupBy(c => c.individuo)
                        .Select(g => g.OrderByDescending(c => c.dataInserimento).FirstOrDefault())
                        .ToList();

                listaind = listaind.Where(x => x.statoIndividuoNavigation.visualizzazioneweb == true);
                listaind = listaind.Where(x => x.individuoNavigation.settoreNavigation.visualizzazioneweb == true);

                    

            List<Ultimo> listaultimo = listaind.Select(x => new Ultimo
            {
                
                Id = x.id,
                Individuo = x.individuo,
                Progressivo = x.individuoNavigation.progressivo,
                Ipen = x.individuoNavigation.accessioneNavigation.ipen,
                Indentitatassonomica = x.individuoNavigation.accessioneNavigation.specieNavigation.nome_scientifico,
                Nomecomune = x.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune,
                Nomecomuneen = x.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune_en,
                Longitudine = x.individuoNavigation.longitudine,
                Latitudine = x.individuoNavigation.latitudine,
                Settoreid = x.individuoNavigation.settoreNavigation.id,
                Settore = lingua == "it" ? x.individuoNavigation.settoreNavigation.settore : x.individuoNavigation.settoreNavigation.settore_en,
                Collezione = lingua == "it" ? x.individuoNavigation.collezioneNavigation.collezione : x.individuoNavigation.collezioneNavigation.collezione_en,
                Stato = x.statoIndividuoNavigation.stato,
                NumeroImmagini = x.individuoNavigation.ImmaginiIndividuo.Where(a => a.visibile).Count()}).Take(10).ToList();

            

            var firstOrDefault = listaultimo.Where(x => x.NumeroImmagini > 0).FirstOrDefault();

             foreach (var x in listaultimo)
             {
                 if (x.NumeroImmagini == 0)
                 {
                     x.Immagine = "";
                 }
                 else
                 {
                     ImmaginiIndividuo immagini = _context.ImmaginiIndividuo.Where(i => i.predefinita == true && i.visibile && i.individuo == x.Individuo).FirstOrDefault();
                     if (immagini != null)
                     {
                        int posizione = immagini.nomefile.IndexOf(".");
                        string estensione = immagini.nomefile.Substring(posizione);
                        //aggiungere estensione file
                        x.Immagine = String.Concat(_images.Percorso, x.Individuo, "/thumb/", immagini.id.ToString(), estensione);
                     }
                     else
                     {
                         x.Immagine = "";
                     }
                 }
             }
           
            string stringjson = JsonConvert.SerializeObject(listaultimo, settings);
            return Content(stringjson, "application/json");
            }
                catch (Exception e)
                {

                    return BadRequest(e.Message);
                }

         //} else {
        //        return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, "Non Sei Autorizzato");
         // }
        }

        [HttpGet("dettaglio")]

        public IActionResult Details(Guid? id, string lingua)
        {
            
       //  Request.Headers.TryGetValue("Authorization", out var headerValue);
       //  if (headerValue == "Bearer 456-12A4-2288-123F") {
           try { 
            IEnumerable<StoricoIndividuo> listaind = _context.StoricoIndividuo
                .Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.collezioneNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.ImmaginiIndividuo)
                .Include(x => x.statoIndividuoNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.specieNavigation).ThenInclude(x => x.genereNavigation).ThenInclude(x => x.famigliaNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.specieNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.tipoAcquisizioneNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.provenienzaNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.nazioneNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.regioneNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.provinciaNavigation)
                .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.raccoglitoreNavigation)
                .AsEnumerable()
                    .GroupBy(c => c.individuo)
                    .Select(g => g.OrderByDescending(c => c.dataInserimento).FirstOrDefault())
                    .ToList();


             if (id != null)
             {

                listaind = listaind.Where(x => x.individuo == id);

             } else {
                    return NotFound();
             }

            List<ListaImmagini> listaimma = new List<ListaImmagini>();
            foreach (var x in listaind)
            {

                IEnumerable<ImmaginiIndividuo> immagini = _context.ImmaginiIndividuo.Where(x => x.individuo == id && x.visibile).ToList();

                //Immagini immagini = _context.Immagini.Where(i =>  i.Visibile && i.Individuo == x.Individuo).toList();
                foreach (var item in immagini)
                {

                    int posizione = item.nomefile.IndexOf(".");
                    string estensione = item.nomefile.Substring(posizione);
                    listaimma.Add(new ListaImmagini { Path = String.Concat(_images.Percorso, item.individuo.ToString(), "/", item.id.ToString(), estensione), Paththumb = String.Concat(_images.Percorso, item.individuo.ToString(), "/thumb/", item.id.ToString(), estensione), Nome = item.nomefile, Estensione = estensione, Credits = item.credits });

                }



            }

            List<Dettaglio> listadettaglio = listaind.Select(x => new Dettaglio
            {
                Id = x.id,
                Individuo = x.individuo,
                Progressivo = x.individuoNavigation.progressivo,
                Indentitatassonomica = x.individuoNavigation.accessioneNavigation.specieNavigation.nome_scientifico,
                Nomecomune = x.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune,
                Nomecomuneen = x.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune_en,
                Longitudine = x.individuoNavigation.longitudine,
                Latitudine = x.individuoNavigation.latitudine,
                Settoreid = x.individuoNavigation.settoreNavigation.id,
                Settore = lingua == "it" ? x.individuoNavigation.settoreNavigation.settore : x.individuoNavigation.settoreNavigation.settore_en,
                Collezione = lingua == "it" ? x.individuoNavigation.collezioneNavigation.collezione : x.individuoNavigation.collezioneNavigation.collezione_en,
                Stato = x.statoIndividuoNavigation.stato,
                Famiglia = x.individuoNavigation.accessioneNavigation.specieNavigation.genereNavigation.famigliaNavigation.descrizione,
                Urlerbario = "https://erbario.unipi.it/it/erbario/index?ErbarioSearch%5Bname%5D="+x.individuoNavigation.accessioneNavigation.specieNavigation.genereNavigation.descrizione+ " "+x.individuoNavigation.accessioneNavigation.specieNavigation.nome,
                Ipen = x.individuoNavigation.accessioneNavigation.ipen,
                Acquisizione = lingua == "it" ? x.individuoNavigation.accessioneNavigation.tipoAcquisizioneNavigation.descrizione : x.individuoNavigation.accessioneNavigation.tipoAcquisizioneNavigation.descrizione_en,
                Provenienza = lingua == "it" ? x.individuoNavigation.accessioneNavigation.provenienzaNavigation.descrizione : x.individuoNavigation.accessioneNavigation.provenienzaNavigation.descrizione_en,
                RaccoltaNid = x.individuoNavigation.accessioneNavigation.nazioneNavigation.codice,
                RaccoltaN = x.individuoNavigation.accessioneNavigation.nazioneNavigation.codice == "NN" ? "" : lingua == "it" ? x.individuoNavigation.accessioneNavigation.nazioneNavigation.descrizione : x.individuoNavigation.accessioneNavigation.nazioneNavigation.descrizione_en,
                RaccoltaRid = x.individuoNavigation.accessioneNavigation.regione,
                RaccoltaR = x.individuoNavigation.accessioneNavigation.regione == null  ? "" : x.individuoNavigation.accessioneNavigation.regione == "99" ? "" : lingua == "it" ? char.ToUpper(x.individuoNavigation.accessioneNavigation.regioneNavigation.descrizione[0]) + x.individuoNavigation.accessioneNavigation.regioneNavigation.descrizione.Substring(1).ToLower() : char.ToUpper(x.individuoNavigation.accessioneNavigation.regioneNavigation.descrizione_en[0]) + x.individuoNavigation.accessioneNavigation.regioneNavigation.descrizione_en.Substring(1).ToLower(),
                RaccoltaPid = x.individuoNavigation.accessioneNavigation.provincia,
                RaccoltaP = x.individuoNavigation.accessioneNavigation.provincia == null ? "" : x.individuoNavigation.accessioneNavigation.provincia =="999" ? "" : x.individuoNavigation.accessioneNavigation.provinciaNavigation.descrizione,
                RaccoltaL = String.IsNullOrEmpty(x.individuoNavigation.accessioneNavigation.localita) ? "" : x.individuoNavigation.accessioneNavigation.localita,
                Habitat = String.IsNullOrEmpty(x.individuoNavigation.accessioneNavigation.habitat) ? "" : x.individuoNavigation.accessioneNavigation.habitat,
                Dataraccolta = x.individuoNavigation.accessioneNavigation.dataraccolta != null ? string.Format(Convert.ToDateTime(x.individuoNavigation.accessioneNavigation.dataraccolta).ToString(),"{0:yyyy-MM-dd HH:mm:ss}", "dd/MM/yyyy"): "",
                Raccoglitore = x.individuoNavigation.accessioneNavigation.raccoglitoreNavigation.id == Guid.Parse("36e4304735d74ea3bc8e8e4caac94291") ?  ""  : x.individuoNavigation.accessioneNavigation.raccoglitoreNavigation.nominativo,
                NumeroImmagini = x.individuoNavigation.ImmaginiIndividuo.Where(a => a.visibile).Count(),
                Propagatodata =  string.Format(Convert.ToDateTime(x.individuoNavigation.propagatoData).ToString(), "{0:yyyy-MM-dd HH:mm:ss}", "yyyy") ,
                Percorso = _images.Percorso,
                ListaImmagini = listaimma,
                Ordinamento = x.individuoNavigation.settoreNavigation.ordinamento

            }).ToList();



            string stringjson = JsonConvert.SerializeObject(listadettaglio, settings);
            return Content(stringjson, "application/json");
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
            // } else {
            //          return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, "Non Sei Autorizzato");
            // }
        }

        [HttpGet("elencopercorsi")]
        public IActionResult Percorsi(string lingua)
        {
            
         /*   Request.Headers.TryGetValue("Authorization", out var headerValue);
          if (headerValue == "Bearer 456-12A4-2288-123F" || Request.Host.Host == "localhost") {*/
                try
            {
                
                var prelist = _context.Percorsi.Where(x => x.attivo).ToList();


                List<RisultatoPercorsi> listapercorsi = prelist.Select(x => new RisultatoPercorsi
                {

                    Id = x.id,
                    Titolo = lingua == "it" ? x.titolo : x.titolo_en,
                    Descrizione = lingua == "it" ? x.descrizione : x.descrizione_en,
                    Titolo_en = x.titolo_en,
                    Descrizione_en = x.descrizione_en,
                    Pathimmagine = String.Concat(_images.Percorso, "Percorsi/" ,x.id.ToString(), "/thumb/", x.nomefile),
                    Credits = x.credits

                }).ToList();


                return Ok(listapercorsi);
            }
                catch (Exception e)
                {

                    return BadRequest(e.Message);
                }
           /* } else {
                return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, "Non Sei Autorizzato");
          }*/
        }

        [HttpGet("percorso")]
        public IActionResult Percorso(Guid? id ,string lingua)
        {
         
         /* Request.Headers.TryGetValue("Authorization", out var headerValue);
            if (headerValue == "Bearer 456-12A4-2288-123F" || Request.Host.Host == "localhost")
            {*/
                try
            {
                if (id == null)
                {
                    return NotFound();
                }
                else
                {
                    Guid? perid = id;
                       // var allowedStatus = new[] { "30e70f7c13774994ac9215b3543ebd7b" }; // Vivo
                        var detpercorso = _context.Percorsi.Where(x => x.id == perid);
                        
                    IEnumerable<IndividuiPercorso> indperc = _context.IndividuiPercorso
                      .Include(x => x.individuoNavigation).ThenInclude(x => x.settoreNavigation)
                      .Include(x => x.individuoNavigation).ThenInclude(x => x.collezioneNavigation)
                      .Include(x => x.individuoNavigation).ThenInclude(x => x.ImmaginiIndividuo)
                      .Include(x => x.individuoNavigation).ThenInclude(x => x.accessioneNavigation).ThenInclude(x => x.specieNavigation)
                      .Include(x => x.individuoNavigation).ThenInclude(x => x.StoricoIndividuo).ThenInclude(x => x.statoIndividuoNavigation);

                        indperc = indperc.Where(x => x.individuoNavigation.StoricoIndividuo.OrderByDescending(x => x.dataInserimento).Select(x => x.statoIndividuoNavigation.visualizzazioneweb == true).First());

                        // indperc = indperc.Where(x => allowedStatus.Contains(x.StatoIndividuo));
                        //            indperc = indperc.Where(y => y.individuoNavigation.Storico.Select(z => z.StatoIndividuo.Contains("30e70f7c13774994ac9215b3543ebd7b")).First()).OrderByDescending(z => z.individuoNavigation.Storico.Select(z => z.DataInserimento));

                        if (perid != null )
                    {
                        indperc = indperc.Where(x => x.percorso == perid);
                    }

                    List<ListaIndividui> listaindi = new List<ListaIndividui>();

                    foreach (var item in indperc)
                    {
                        var nimma = item.individuoNavigation.ImmaginiIndividuo.Where(a => a.visibile).Count();
                        var pathimma = "";

                        if (nimma == 0)
                        {
                            pathimma = "";
                        }
                        else
                        {
                            ImmaginiIndividuo immagini = item.individuoNavigation.ImmaginiIndividuo.Where(i => i.predefinita == true && i.visibile && i.individuo == item.individuoNavigation.id).FirstOrDefault();
                            if (immagini != null)
                            {
                                int posizione = immagini.nomefile.IndexOf(".");
                                string estensione = immagini.nomefile.Substring(posizione);
                                //aggiungere estensione file
                                pathimma = String.Concat(_images.Percorso, item.individuo.ToString(), "/thumb/", immagini.id.ToString(), estensione);
                            }
                            else
                            {
                                pathimma = "";
                            }
                        }


                        listaindi.Add(new ListaIndividui
                        {
                            Id = item.individuo,
                            Ipen = item.individuoNavigation.accessioneNavigation.ipen,
                            Settore = lingua == "it" ? item.individuoNavigation.settoreNavigation.settore : item.individuoNavigation.settoreNavigation.settore_en,
                            Collezione = lingua == "it" ? item.individuoNavigation.collezioneNavigation.collezione : item.individuoNavigation.collezioneNavigation.collezione_en,
                            Propagatodata = string.Format(Convert.ToDateTime(item.individuoNavigation.propagatoData).ToString(), "{0:yyyy-MM-dd HH:mm:ss}", "yyyy"),
                            Longitudine = item.individuoNavigation.longitudine,
                            Latitudine = item.individuoNavigation.latitudine,
                            Settoreid = item.individuoNavigation.settoreNavigation.id,
                            Indentitatassonomica = item.individuoNavigation.accessioneNavigation.specieNavigation.nome_scientifico,
                            Nomecomune = item.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune,
                            Nomecomuneen = item.individuoNavigation.accessioneNavigation.specieNavigation.nome_comune_en,
                            Stato = item.individuoNavigation.StoricoIndividuo.OrderByDescending(x => x.dataInserimento).Select(x => x.statoIndividuoNavigation.stato).First(),
                            Immagine = pathimma,
                            Ordinamento = item.individuoNavigation.settoreNavigation.ordinamento
                        });


                        /*foreach (var item in individuix)
                        {


                            listaindi.Add(new ListaIndividui { Id = item.Id, Ipen = item.individuoNavigation.accessioneNavigation.Ipen, Settore = item.individuoNavigation.settoreNavigation.Settore, Collezione = item.individuoNavigation.collezioneNavigation.Collezione });

                        }*/
                    }
                    List<RisultatoPercorso> listapercorso = detpercorso.Select(x => new RisultatoPercorso
                    {

                        Id = x.id,
                        Titolo = lingua == "it" ? x.titolo : x.titolo_en,
                        Descrizione = lingua == "it" ? x.descrizione : x.descrizione_en,
                        Titolo_en = x.titolo_en,
                        Descrizione_en = x.descrizione_en,
                        Pathimmagine = String.Concat(_images.Percorso, "Percorsi/", x.id.ToString(), "/", x.nomefile.ToString()),
                        Credits = x.credits,
                        ListaIndividui = listaindi

                    }).ToList();

                    return Ok(listapercorso);
                }
            }
                catch (Exception e)
                {

                    return BadRequest(e.Message);
                }
            /*} else {
                return StatusCode((int)System.Net.HttpStatusCode.Unauthorized, "Non Sei Autorizzato");
         }*/
        }
       
    }
}
