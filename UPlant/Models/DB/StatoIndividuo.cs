using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPlant.Models.DB;

public partial class StatoIndividuo
{
    public Guid id { get; set; }
    [Required]
    public string stato { get; set; }

    public Guid organizzazione { get; set; }
    [Required]
    public bool visualizzazioneweb { get; set; }
    [Required]
    public string ordinamento { get; set; }

    public virtual ICollection<StoricoIndividuo> StoricoIndividuo { get; set; } = new List<StoricoIndividuo>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }
}
