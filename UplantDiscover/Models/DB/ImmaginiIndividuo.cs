using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class ImmaginiIndividuo
{
    public Guid id { get; set; }

    public Guid individuo { get; set; }

    public string descrizione { get; set; }

    public DateTime dataInserimento { get; set; }

    public string autore { get; set; }

    public string nomefile { get; set; }

    public bool visibile { get; set; }

    public bool? predefinita { get; set; }

    public string credits { get; set; }

    public virtual Individui individuoNavigation { get; set; }
}
