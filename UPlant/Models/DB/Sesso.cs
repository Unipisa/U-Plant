using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Sesso
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public Guid organizzazione { get; set; }

    public string descrizione_en { get; set; }

    public virtual ICollection<Individui> Individui { get; set; } = new List<Individui>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
