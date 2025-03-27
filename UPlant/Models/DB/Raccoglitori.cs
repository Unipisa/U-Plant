using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Raccoglitori
{
    public Guid id { get; set; }

    public Guid organizzazione { get; set; }

    public string nominativo { get; set; }

    public bool attivo { get; set; }

    public virtual ICollection<Accessioni> Accessioni { get; set; } = new List<Accessioni>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
