using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Province
{
    public string codice { get; set; }

    public string regione { get; set; }

    public string descrizione { get; set; }

    public string descrizione_en { get; set; }

    public virtual ICollection<Accessioni> Accessioni { get; set; } = new List<Accessioni>();

    public virtual Regioni regioneNavigation { get; set; }
}
