using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class IndividuiPercorso
{
    public Guid id { get; set; }

    public Guid percorso { get; set; }

    public Guid individuo { get; set; }

    public virtual Individui individuoNavigation { get; set; }

    public virtual Percorsi percorsoNavigation { get; set; }
}
