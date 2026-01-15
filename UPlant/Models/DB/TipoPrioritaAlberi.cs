using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class TipoPrioritaAlberi
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public string descrizione_en { get; set; }

    public Guid organizzazione { get; set; }

    public int ordinamento { get; set; }

    public int livello { get; set; }

    public virtual ICollection<Alberi> Alberi { get; set; } = new List<Alberi>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
