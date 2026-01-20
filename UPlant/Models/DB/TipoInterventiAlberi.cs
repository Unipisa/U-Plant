using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class TipoInterventiAlberi
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public string descrizione_en { get; set; }

    public Guid organizzazione { get; set; }

    public int ordinamento { get; set; }

    public virtual ICollection<InterventiAlberi> InterventiAlberi { get; set; } = new List<InterventiAlberi>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
