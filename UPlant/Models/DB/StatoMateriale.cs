using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPlant.Models.DB;

public partial class StatoMateriale
{
    public Guid id { get; set; }

    public Guid organizzazione { get; set; }
    [Required]
    public string descrizione { get; set; }
    [Required]
    public string ordinamento { get; set; }

    public virtual ICollection<Accessioni> Accessioni { get; set; } = new List<Accessioni>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
