using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class TipoIdentificatore
{
    public Guid id { get; set; }

    public Guid organizzazione { get; set; }

    public string descrizione { get; set; }

    public virtual ICollection<Identificatori> Identificatori { get; set; } = new List<Identificatori>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
