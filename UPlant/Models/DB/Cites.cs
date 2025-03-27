using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPlant.Models.DB;

public partial class Cites
{
    public Guid id { get; set; }
    [Required]
    public string codice { get; set; }

    public string descrizione { get; set; }
    [Required]
    public string ordinamento { get; set; }

    public virtual ICollection<Specie> Specie { get; set; } = new List<Specie>();
}
