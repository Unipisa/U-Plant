using System;
using System.Collections.Generic;
using UplantDiscover.Models.DB;

#nullable disable

namespace UplantDiscover.Models
{
    public partial class Ultimo
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
        public Guid Settoreid { get; set; }
        public string Settore { get; set; }
        public string Collezione { get; set; }

        public int NumeroImmagini { get; set; }

        public string Immagine { get; set; }
    }
}
