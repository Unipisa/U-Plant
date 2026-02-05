using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Individui
{
    public Guid id { get; set; }

    public Guid accessione { get; set; }

    public Guid? individuo { get; set; }

    public string progressivo { get; set; }

    public Guid? sesso { get; set; }

    public DateTime propagatoData { get; set; }

    public Guid propagatoModalita { get; set; }

    public Guid settore { get; set; }

    public Guid collezione { get; set; }

    public bool indexSeminum { get; set; }

    public string destinazioni { get; set; }

    public string note { get; set; }

    public string longitudine { get; set; }

    public string latitudine { get; set; }

    public Guid cartellino { get; set; }

    public bool validazione { get; set; }

    public string vecchioprogressivo { get; set; }

    public Guid tipocartellino { get; set; }

    public virtual ICollection<ImmaginiIndividuo> ImmaginiIndividuo { get; set; } = new List<ImmaginiIndividuo>();

    public virtual ICollection<IndividuiPercorso> IndividuiPercorso { get; set; } = new List<IndividuiPercorso>();

    public virtual ICollection<InterventiAlberi> InterventiAlberi { get; set; } = new List<InterventiAlberi>();

    public virtual ICollection<StoricoIndividuo> StoricoIndividuo { get; set; } = new List<StoricoIndividuo>();

    public virtual Accessioni accessioneNavigation { get; set; }

    public virtual Cartellini cartellinoNavigation { get; set; }

    public virtual Collezioni collezioneNavigation { get; set; }

    public virtual ModalitaPropagazione propagatoModalitaNavigation { get; set; }

    public virtual Sesso sessoNavigation { get; set; }

    public virtual Settori settoreNavigation { get; set; }

    public virtual TipoCartellino tipocartellinoNavigation { get; set; }
}
