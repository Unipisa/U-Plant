using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Collezioni
{
    public Guid id { get; set; }

    public string collezione { get; set; }

    public Guid organizzazione { get; set; }

    public Guid settore { get; set; }

    public string collezione_en { get; set; }

    public virtual ICollection<Individui> Individui { get; set; } = new List<Individui>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }

    public virtual Settori settoreNavigation { get; set; }
}
