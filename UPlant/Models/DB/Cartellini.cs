using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPlant.Models.DB;

public partial class Cartellini
{
    public Guid id { get; set; }
    [Required]
    public string descrizione { get; set; }

    public Guid organizzazione { get; set; }
    [Required]
    public string ordinamento { get; set; }

    public virtual ICollection<Individui> Individui { get; set; } = new List<Individui>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
