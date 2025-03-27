using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class TipiMateriale
{
    public Guid id { get; set; }

    public Guid organizzazione { get; set; }

    public string descrizione { get; set; }

    public string ordinamento { get; set; }

    public virtual ICollection<Accessioni> Accessioni { get; set; } = new List<Accessioni>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
