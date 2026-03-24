using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class StatusNomenclaturale
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public string descrizione_en { get; set; }

    public int ordinamento { get; set; }

    public Guid organizzazione { get; set; }

    public virtual Organizzazioni organizzazioneNavigation { get; set; }

    public virtual ICollection<Specie> Specie { get; set; } = new List<Specie>();
}
