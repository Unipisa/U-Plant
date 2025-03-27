using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class Settori
{
    public Guid id { get; set; }

    public string settore { get; set; }

    public Guid organizzazione { get; set; }

    public string settore_en { get; set; }

    public bool visualizzazioneweb { get; set; }

    public string ordinamento { get; set; }

    public virtual ICollection<Collezioni> Collezioni { get; set; } = new List<Collezioni>();

    public virtual ICollection<Individui> Individui { get; set; } = new List<Individui>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
