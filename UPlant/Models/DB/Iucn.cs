using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPlant.Models.DB;

public partial class Iucn
{
    public Guid id { get; set; }
    [Required]
    public string codice { get; set; }

    public string descrizione { get; set; }
    [Required]
    public string ordinamento { get; set; }

    public virtual ICollection<Specie> Specieiucn_globaleNavigation { get; set; } = new List<Specie>();

    public virtual ICollection<Specie> Specieiucn_italiaNavigation { get; set; } = new List<Specie>();
}
