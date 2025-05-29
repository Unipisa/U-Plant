using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class Cites
{
    public Guid id { get; set; }

    public string codice { get; set; }

    public string descrizione { get; set; }

    public int ordinamento { get; set; }

    public virtual ICollection<Specie> Specie { get; set; } = new List<Specie>();
}
