using System;
using System.Collections.Generic;
using UplantDiscover.Models.DB;

#nullable disable

namespace UplantDiscover.Models
{
    public partial class Dettaglio
    {
        public Guid Id { get; set; }
        public Guid Individuo { get; set; }
        public string Progressivo { get; set; }
        public string Ipen { get; set; }
        public string Stato { get; set; } 
        public string Indentitatassonomica { get; set; }
        public string Nomecomune { get; set; }
        public string Nomecomuneen { get; set; }
        public string Longitudine { get; set; }
        public string Latitudine { get; set; }

        public string Famiglia { get; set; }
        public Guid Settoreid { get; set; }
        public string Settore { get; set; }
        public string Collezione { get; set; }

        public string Acquisizione { get; set; }

        public string Provenienza { get; set; }
        public string RaccoltaN { get; set; }
        public string RaccoltaNid { get; set; }
        public string RaccoltaR { get; set; }
        public string RaccoltaRid { get; set; }
        public string RaccoltaP { get; set; }
        public string RaccoltaPid { get; set; }
        public string RaccoltaL { get; set; }
        public string Habitat { get; set; }
        public string Propagatodata { get; set; }
        public string Dataraccolta { get; set; }
        public string Raccoglitore { get; set; }
        public int NumeroImmagini { get; set; }
        public string Urlerbario { get; set; }
        public string Percorso { get; set; }
        public string Immagine { get; set; }
        public List<ListaImmagini> ListaImmagini { get; set; }
        public int Ordinamento { get; set; }

    }
}
