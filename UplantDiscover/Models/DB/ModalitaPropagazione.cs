using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class ModalitaPropagazione
{
    public Guid id { get; set; }

    public string propagatoModalita { get; set; }

    public Guid organizzazione { get; set; }

    public int ordinamento { get; set; }

    public virtual ICollection<Individui> Individui { get; set; } = new List<Individui>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
