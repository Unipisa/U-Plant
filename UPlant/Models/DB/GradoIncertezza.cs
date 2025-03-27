using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class GradoIncertezza
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public Guid organizzazione { get; set; }

    public virtual ICollection<Accessioni> Accessioni { get; set; } = new List<Accessioni>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
