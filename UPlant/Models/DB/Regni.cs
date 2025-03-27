using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPlant.Models.DB;

public partial class Regni
{
    public Guid id { get; set; }
    [Required]
    public string descrizione { get; set; }

    public string codiceInterno { get; set; }

    public string descrizione_en { get; set; }
    [Required]
    public string ordinamento { get; set; }

    public virtual ICollection<Specie> Specie { get; set; } = new List<Specie>();
}
