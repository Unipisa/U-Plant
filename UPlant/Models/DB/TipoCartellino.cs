using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class TipoCartellino
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public string descrizione_en { get; set; }

    public int ordinamento { get; set; }

    public virtual ICollection<Individui> Individui { get; set; } = new List<Individui>();
}
