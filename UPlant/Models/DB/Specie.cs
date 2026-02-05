using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Specie
{
    public Guid id { get; set; }

    public Guid genere { get; set; }

    public string nome { get; set; }

    public string nome_scientifico { get; set; }

    public string note { get; set; }

    public string autori { get; set; }

    public Guid? regno { get; set; }

    public Guid? areale { get; set; }

    public string subspecie { get; set; }

    public string autorisub { get; set; }

    public string varieta { get; set; }

    public string autorivar { get; set; }

    public string cult { get; set; }

    public string autoricult { get; set; }

    public string nome_comune { get; set; }

    public string nome_comune_en { get; set; }

    public Guid? iucn_globale { get; set; }

    public Guid? iucn_italia { get; set; }

    public Guid? cites { get; set; }

    public virtual ICollection<Accessioni> Accessioni { get; set; } = new List<Accessioni>();

    public virtual Areali arealeNavigation { get; set; }

    public virtual Cites citesNavigation { get; set; }

    public virtual Generi genereNavigation { get; set; }

    public virtual Iucn iucn_globaleNavigation { get; set; }

    public virtual Iucn iucn_italiaNavigation { get; set; }

    public virtual Regni regnoNavigation { get; set; }
}
