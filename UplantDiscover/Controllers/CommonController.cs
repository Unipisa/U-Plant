using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UplantDiscover.Models;
using UplantDiscover.Models.DB;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net;
using System.IO;


// Progetto UplantDiscover ,rappresentazione dei dati inseriti su UPlant per la fruibilità pubblica ideato e scritto da Pietro Picconi pietro.picconi@unipi.it


namespace UplantDiscover.Controllers
{
    public class CommonController : Controller
    {
        private readonly Entities _context;
       // private readonly IOptions<Wso2> _wso2;
        public CommonController(Entities context)
        {
            _context = context;
          //  _wso2 = wso2;
        }
        public Object GetFamiglie(string lingua)
        {
            List<SelectListItem> listafamiglie = new List<SelectListItem>();
            if (lingua == "it-IT")
            {
                listafamiglie = (from product in _context.Famiglie.OrderBy(a => a.descrizione)

                                 select new SelectListItem()
                                 {
                                     Text = product.descrizione != null ? product.descrizione : product.descrizione_en,
                                     Value = product.id.ToString(),
                                 }).ToList();

                listafamiglie.Insert(0, new SelectListItem()
                {
                    Text = "----Tutte----",
                    Value = string.Empty
                });
            }
                if (lingua == "en-US")
                {
                    listafamiglie = (from product in _context.Famiglie.OrderBy(a => a.descrizione)
                                     select new SelectListItem()
                                     {
                                         Text = product.descrizione_en != null ? product.descrizione_en : product.descrizione,
                                         Value = product.id.ToString(),
                                     }).ToList();

               
                listafamiglie.Insert(0, new SelectListItem()
                {
                    Text = "----All----",
                    Value = string.Empty
                });
            } 
            return listafamiglie;
        }
        public Object GetRegni(string lingua)
        {
            var notallowedreign = new[] { "343b8a465dfa41279ab429eb0d1d1ad5" };//non Definito
            List<SelectListItem> listaregni = new List<SelectListItem>();
            if (lingua == "it-IT")
            {


                listaregni = (from product in _context.Regni.OrderBy(a => a.descrizione).Where(a => !notallowedreign.Contains(a.descrizione))
                              select new SelectListItem()
                              {
                                  Text = product.descrizione != null ? product.descrizione : product.descrizione_en,
                                  Value = product.id.ToString(),
                              }).ToList();

                listaregni.Insert(0, new SelectListItem()
                {
                    Text = "----Tutti----",
                    Value = string.Empty
                });
            }
            if (lingua == "en-US")
            {
                listaregni = (from product in _context.Regni.OrderBy(a => a.descrizione_en).Where(a => !notallowedreign.Contains(a.descrizione_en))
                              select new SelectListItem()
                              {
                                  Text = product.descrizione_en != null ? product.descrizione_en : product.descrizione,
                                  Value = product.id.ToString(),
                              }).ToList();

                listaregni.Insert(0, new SelectListItem()
                {
                    Text = "----All----",
                    Value = string.Empty
                });
            }
                return listaregni;
        }

        public Object GetSettori(string lingua)
        {
            var notallowedsector = new[] { "0ba85efcea3544e485141f7e311d82e2", "0e551835b07642f88540a4ff9d15e84e", "17650e74de9e40c3b1b604531c1d0f6f", "900fdc0ec2de45098ccc5013e796b14f" }; //Nursery , Banca Semi,Portineria e Non Definito

            List<SelectListItem> listasettori = new List<SelectListItem>();
            if (lingua == "it-IT")
            {
                listasettori = (from product in _context.Settori.OrderBy(a => a.settore).Where(a => !notallowedsector.Contains(a.settore))
                                select new SelectListItem()
                                {

                                    Text = product.settore != null ? product.settore : product.settore_en,
                                    Value = product.id.ToString(),
                                }).ToList();


                listasettori.Insert(0, new SelectListItem()
                {
                    Text = "----Tutti----",
                    Value = string.Empty
                });
            }
            if (lingua == "en-US")
            {
                listasettori = (from product in _context.Settori.OrderBy(a => a.settore_en).Where(a => !notallowedsector.Contains(a.settore))
                                select new SelectListItem()
                                {

                                    Text = product.settore_en != null ? product.settore_en : product.settore,
                                    Value = product.id.ToString(),
                                }).ToList();


                listasettori.Insert(0, new SelectListItem()
                {
                    Text = "----All----",
                    Value = string.Empty
                });
            }
            return listasettori;

        }
        public Object GetCollezioni(string lingua)
        {
            var notallowedsector = new[] { "0ba85efcea3544e485141f7e311d82e2", "0e551835b07642f88540a4ff9d15e84e", "17650e74de9e40c3b1b604531c1d0f6f", "900fdc0ec2de45098ccc5013e796b14f" }; //Nursery , Banca Semi,Portineria e Non Definito
            List<SelectListItem> listacollezioni = new List<SelectListItem>();
            if (lingua == "it-IT")
            {
               
                /* listacollezioni = _context.Collezioni.Select(c => new SelectListItem
                 {
                     Value = c.Id.ToString(),
                     Text = c.Collezione,
                 }).Distinct().ToList();*/
                

                listacollezioni = (from product in _context.Collezioni.OrderBy(a => a.collezione).Where(a => !notallowedsector.Contains(a.settoreNavigation.settore)).Where(a =>!a.collezione.Contains("Non Def")).Where(a => !a.collezione.ToUpper().Contains("NESSUNA"))
                                   select  new SelectListItem()
                                     {
                                         Text = product.collezione != null ? product.collezione : product.collezione_en,
                                         Value = product.id.ToString(),
                                     }).ToList();
                
                listacollezioni = listacollezioni.GroupBy(x => x.Text).Select(x => x.First()).ToList();
                listacollezioni.Insert(0, new SelectListItem()
                {
                    Text = "----Tutti----",
                    Value = string.Empty
                });
                
            }
            if (lingua == "en-US")
            {
                listacollezioni = (from product in _context.Collezioni.OrderBy(a => a.collezione_en).Where(a => !notallowedsector.Contains(a.settoreNavigation.settore_en)).Where(a => !a.collezione_en.Contains("Undefined")).Where(a => !a.collezione_en.ToUpper().Contains("NONE"))
                                   select new SelectListItem()
                                   {
                                       Text = product.collezione_en != null ? product.collezione_en : product.collezione_en,
                                       Value = product.id.ToString(),
                                   }).ToList();
                listacollezioni = listacollezioni.GroupBy(x => x.Text).Select(x => x.First()).ToList();
                listacollezioni.Insert(0, new SelectListItem()
                {
                    Text = "----All----",
                    Value = string.Empty
                });
            }


            return listacollezioni;

        }
        public Object GetSettoreCollezioni(Guid? codicesettore,string lingua)
        {
            var notallowedsector = new[] { "0ba85efcea3544e485141f7e311d82e2", "0e551835b07642f88540a4ff9d15e84e", "17650e74de9e40c3b1b604531c1d0f6f", "900fdc0ec2de45098ccc5013e796b14f" }; //Nursery , Banca Semi,Portineria e Non Definito
            IEnumerable<Collezioni> col = null;
            if (lingua == "it")
            {
                col =  _context.Collezioni.OrderBy(a => a.collezione).Where(a => !notallowedsector.Contains(a.settoreNavigation.settore)).Where(a => !a.collezione.Contains("Non Def"));
                if (codicesettore != null)
                {
                    col = col.Where(x => x.settore == codicesettore);

                }
                else
                {
                     col = col.GroupBy(x => x.collezione).Select(x => x.First()).ToList();

                }
            }
            if (lingua == "en")
            {
                col = _context.Collezioni.OrderBy(a => a.collezione_en).Where(a => !notallowedsector.Contains(a.settoreNavigation.settore)).Where(a => !a.collezione_en.Contains("Undefined"));
                if (codicesettore != null)
                {
                    col = col.Where(x => x.settore == codicesettore);
                }
                else
                {
                    col = col.GroupBy(x => x.collezione_en).Select(x => x.First()).ToList();
                }
            }
            return col;
           
        }
       
        public ActionResult ViewImg(Guid individuo, string img, string filename, Images images)
        {
            
           
            string path = StaticUtils.GetImgPath(individuo.ToString(), img, filename, images);
            int posizione = filename.IndexOf(".");
            string estensione = filename.Substring(posizione + 1);

            return base.File(path, "image/" + estensione);

        }

        public string GetToken()
        {

            string wClientId = "";//non vengono più passati perchè l'ho svincolato da un api.store _wso2.Value.ClientId;
            string wClientSecretKey = "";//non vengono più passati perchè l'ho svincolato da un api.store _wso2.Value.ClientSecret; 
            string wAccessToken;

            //--------------------------- Approch-1 to get token using HttpClient -------------------------------------------------------------------------------------
            HttpResponseMessage responseMessage;
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage tokenRequest = new HttpRequestMessage(HttpMethod.Post, "");// non vengono più passati perchè l'ho svincolato_wso2.Value.Wso2Domain);
                HttpContent httpContent = new FormUrlEncodedContent(
                        new[]
                        {
                                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        });
                tokenRequest.Content = httpContent;
                tokenRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(wClientId + ":" + wClientSecretKey)));
                responseMessage = client.SendAsync(tokenRequest).Result;
            }
            string ResponseJSON = responseMessage.Content.ReadAsStringAsync().Result;


            //--------------------------- Approch-2 to get token using HttpWebRequest and deserialize json object into ResponseModel class -------------------------------------------------------------------------------------


            byte[] byte1 = Encoding.ASCII.GetBytes("grant_type=client_credentials&validity_period=10");
            //da sostituire
            HttpWebRequest oRequest = WebRequest.Create(""/*non viene più passato perchè l'ho svincolato da un api.store_wso2.Value.Wso2Domain */) as HttpWebRequest;
            oRequest.Accept = "application/json";
            oRequest.Method = "POST";
            oRequest.ContentType = "application/x-www-form-urlencoded";
            oRequest.ContentLength = byte1.Length;
            oRequest.KeepAlive = false;
            oRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(wClientId + ":" + wClientSecretKey)));
            Stream newStream = oRequest.GetRequestStream();
            newStream.Write(byte1, 0, byte1.Length);

            WebResponse oResponse = oRequest.GetResponse();

            using (var reader = new StreamReader(oResponse.GetResponseStream(), Encoding.UTF8))
            {
                var oJsonReponse = reader.ReadToEnd();
                ResponseModel oModel = JsonConvert.DeserializeObject<ResponseModel>(oJsonReponse);
                wAccessToken = oModel.TokenType + " " + oModel.AccessToken;
            }

            return wAccessToken;
        }

        internal class ResponseModel
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }
        }

    }
}
