using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPlant.Models.DB;

public partial class ModalitaPropagazione
{
    public Guid id { get; set; }
    [Required]
    public string propagatoModalita { get; set; }

    public Guid organizzazione { get; set; }
    [Required]
    public string ordinamento { get; set; }

    public virtual ICollection<Individui> Individui { get; set; } = new List<Individui>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
