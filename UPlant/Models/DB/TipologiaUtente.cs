using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPlant.Models.DB;

public partial class TipologiaUtente
{
    public Guid id { get; set; }
    [Required]
    public string descrizione { get; set; }

    public Guid organizzazione { get; set; }
    [Required]
    public string ordinamento { get; set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
