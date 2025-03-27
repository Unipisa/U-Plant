using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class Areali
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public string codiceInterno { get; set; }

    public virtual ICollection<Specie> Specie { get; set; } = new List<Specie>();
}
