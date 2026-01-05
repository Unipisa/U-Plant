using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Fornitori
{
    public Guid id { get; set; }

    public Guid organizzazione { get; set; }

    public string descrizione { get; set; }

    public string note { get; set; }

    public bool attivo { get; set; }

    public string descrizione_en { get; set; }

    public virtual ICollection<Accessioni> Accessioni { get; set; } = new List<Accessioni>();

    public virtual ICollection<Alberi> Alberi { get; set; } = new List<Alberi>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
