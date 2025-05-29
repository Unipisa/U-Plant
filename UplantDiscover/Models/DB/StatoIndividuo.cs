using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class StatoIndividuo
{
    public Guid id { get; set; }

    public string stato { get; set; }

    public Guid organizzazione { get; set; }

    public bool visualizzazioneweb { get; set; }

    public int ordinamento { get; set; }

    public virtual ICollection<StoricoIndividuo> StoricoIndividuo { get; set; } = new List<StoricoIndividuo>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
