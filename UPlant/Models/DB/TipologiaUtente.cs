using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class TipologiaUtente
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public Guid organizzazione { get; set; }

    public int ordinamento { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
