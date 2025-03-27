using System;
using System.Collections.Generic;

namespace UplantDiscover.Models.DB;

public partial class Identificatori
{
    public Guid id { get; set; }

    public Guid organizzazione { get; set; }

    public string nominativo { get; set; }

    public Guid tipoIdentificatore { get; set; }

    public bool attivo { get; set; }

    public virtual ICollection<Accessioni> Accessioni { get; set; } = new List<Accessioni>();

    public virtual Organizzazioni organizzazioneNavigation { get; set; }

    public virtual TipoIdentificatore tipoIdentificatoreNavigation { get; set; }
}
