using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Famiglie
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public string descrizione_en { get; set; }

    public virtual ICollection<Generi> Generi { get; set; } = new List<Generi>();
}
