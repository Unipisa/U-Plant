using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class Percorsi
{
    public Guid id { get; set; }

    public string titolo { get; set; }

    public string descrizione { get; set; }

    public DateTime datacreazione { get; set; }

    public decimal ordinamento { get; set; }

    public bool attivo { get; set; }

    public string titolo_en { get; set; }

    public string descrizione_en { get; set; }

    public string nomefile { get; set; }

    public string credits { get; set; }

    public string autore { get; set; }

    public virtual ICollection<IndividuiPercorso> IndividuiPercorso { get; set; } = new List<IndividuiPercorso>();
}
