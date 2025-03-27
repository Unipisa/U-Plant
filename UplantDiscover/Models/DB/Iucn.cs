using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class Iucn
{
    public Guid id { get; set; }

    public string codice { get; set; }

    public string descrizione { get; set; }

    public string ordinamento { get; set; }

    public virtual ICollection<Specie> Specieiucn_globaleNavigation { get; set; } = new List<Specie>();

    public virtual ICollection<Specie> Specieiucn_italiaNavigation { get; set; } = new List<Specie>();
}
