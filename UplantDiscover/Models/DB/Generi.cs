using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class Generi
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public Guid famiglia { get; set; }

    public virtual ICollection<Specie> Specie { get; set; } = new List<Specie>();

    public virtual Famiglie famigliaNavigation { get; set; }
}
