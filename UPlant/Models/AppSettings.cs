using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UPlant.Models;


namespace UPlant.Models
{
    /// <summary>
    /// Classe dei parametri generali dell'applicazioni i valori verranno mappati 
    /// con i corrispondenti campi del corrispondente file appsettings.{Environment}.json
    /// </summary>
    public class Application
    {
        ///<value>Nome esteso dell'applicazione.</value>
        public string NomeAppLong { get; set; }
        ///<value>Nome breve dell'applicazione.</value>
        public string NomeAppShort { get; set; }
        ///<value>Nome Ente che gestisce l'applicazione.</value>
        public string Universita { get; set; }
        ///<value>Nome struttura che gestisce l'applicazione.</value>
        public string Struttura { get; set; }
        ///<value>Flag che indica se caricare o meno il layout Demo.</value>
        public bool Demo { get; set; }
        ///<value>Messaggio visibile, come watermark, solo se Demo è true.</value>
        public string MessaggioDemo { get; set; }
        ///<value>UrlPrefix dell'applicazioni </value>
        public string UrlPrefix { get; set; }
        ///<value>UrlPrefix dell'applicazioni </value>
        public string UrlBaseCode { get; set; }
        ///<value>Nome ClaimBased Identity </value>
        public string CustomClaimsIdentityName { get; set; }
        ///<value> parametri chiave mappa </value>
        public string UrlLogoU { get; set; }
        ///<value>UrlPrefix dell'applicazioni </value
        public string AltUrlLogoU { get; set; }
        ///<value>UrlPrefix dell'applicazioni </value>
        public string ClassUrlLogoU { get; set; }
        ///<value>UrlPrefix dell'applicazioni </value>
        public string UrlLogoSsmall { get; set; }
        ///<value>UrlPrefix dell'applicazioni </value>
        public string UrlLogoSlarge { get; set; }
        public string NomeUrlLogoS { get; set; }
        ///<value>UrlPrefix dell'applicazioni </value>
        ///
        public string TypeAuth { get; set; }
    }
    public class MapSettings
    {
        public string Url { get; set; } 
        public string Key { get; set; }
    }
    
    public class Pathfile
    {
        ///<value>Basepath dell'applicazioni </value>
        public string Basepath { get; set; }
        ///<value>LimitMaxUpload dell'applicazioni </value>
        public string LimitMaxUpload { get; set; }

        public string Docs { get; set; }
    }

    public class AppSettings
    {
        public Application Application { get; set; }
        public Pathfile Pathfile { get; set; }

        public MapSettings GoogleMap { get; set; }

       
    }

   
        
    

    #region Pdf
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class PdfConf
    {
        public Page Page { get; set; }

    }
    public class Page
    {
        public Margins Margins { get; set; }
        public SignBox SignBox { get; set; }
    }
    public class Margins
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
    }
    public class SignBox
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int Larghezza { get; set; }
        public int Altezza { get; set; }
        public int SpaceX { get; set; }
        public int SpaceY { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public TopString TopString { get; set; }
        public BottomString BottomString { get; set; }
        public Image Image { get; set; }
    }
    public class TopString
    {
        public string Align { get; set; }
        public int FontSize { get; set; }
        public bool Bold { get; set; }
    }
    public class BottomString
    {
        public string Align { get; set; }
        public int FontSize { get; set; }
        public bool Bold { get; set; }
    }
    public class Image
    {
        public string HAlign { get; set; }
        public string Border { get; set; }
        public bool Bold { get; set; }
        public int Width { get; set; }
    }
    #endregion
}