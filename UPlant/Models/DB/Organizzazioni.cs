using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Organizzazioni
{
    public Guid id { get; set; }

    public string descrizione { get; set; }

    public bool attivo { get; set; }

    public string prefissoIpen { get; set; }

    public virtual ICollection<Accessioni> Accessioni { get; set; } = new List<Accessioni>();

    public virtual ICollection<Cartellini> Cartellini { get; set; } = new List<Cartellini>();

    public virtual ICollection<Collezioni> Collezioni { get; set; } = new List<Collezioni>();

    public virtual ICollection<Condizioni> Condizioni { get; set; } = new List<Condizioni>();

    public virtual ICollection<Fornitori> Fornitori { get; set; } = new List<Fornitori>();

    public virtual ICollection<GradoIncertezza> GradoIncertezza { get; set; } = new List<GradoIncertezza>();

    public virtual ICollection<Identificatori> Identificatori { get; set; } = new List<Identificatori>();

    public virtual ICollection<ModalitaPropagazione> ModalitaPropagazione { get; set; } = new List<ModalitaPropagazione>();

    public virtual ICollection<Provenienze> Provenienze { get; set; } = new List<Provenienze>();

    public virtual ICollection<Raccoglitori> Raccoglitori { get; set; } = new List<Raccoglitori>();

    public virtual ICollection<Sesso> Sesso { get; set; } = new List<Sesso>();

    public virtual ICollection<Settori> Settori { get; set; } = new List<Settori>();

    public virtual ICollection<StatoIndividuo> StatoIndividuo { get; set; } = new List<StatoIndividuo>();

    public virtual ICollection<StatoMateriale> StatoMateriale { get; set; } = new List<StatoMateriale>();

    public virtual ICollection<TipiMateriale> TipiMateriale { get; set; } = new List<TipiMateriale>();

    public virtual ICollection<TipoAcquisizione> TipoAcquisizione { get; set; } = new List<TipoAcquisizione>();

    public virtual ICollection<TipoIdentificatore> TipoIdentificatore { get; set; } = new List<TipoIdentificatore>();

    public virtual ICollection<TipoInterventiAlberi> TipoInterventiAlberi { get; set; } = new List<TipoInterventiAlberi>();

    public virtual ICollection<TipoPrioritaAlberi> TipoPrioritaAlberi { get; set; } = new List<TipoPrioritaAlberi>();

    public virtual ICollection<TipologiaUtente> TipologiaUtente { get; set; } = new List<TipologiaUtente>();

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
}
