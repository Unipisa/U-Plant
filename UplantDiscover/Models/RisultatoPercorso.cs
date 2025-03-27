using System.Collections.Generic;
using System;

namespace UplantDiscover.Models
{
    public partial class RisultatoPercorso    {
        public Guid Id { get; set; }
        public string Titolo { get; set; }
        public string Descrizione { get; set; }
        public string Titolo_en { get; set; }
        public string Descrizione_en { get; set; } 
        public string Pathimmagine { get; set; }
        public string Credits { get; set; }

        public List<ListaIndividui> ListaIndividui { get; set; }


    }
}
