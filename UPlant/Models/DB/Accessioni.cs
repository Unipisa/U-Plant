using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Accessioni
{
    public Guid id { get; set; }

    public Guid organizzazione { get; set; }

    public DateTime dataAcquisizione { get; set; }

    public string progressivo { get; set; }

    public string ipen { get; set; }

    public Guid utenteAcquisizione { get; set; }

    public string vecchioprogressivo { get; set; }

    public Guid identificatore { get; set; }

    public Guid tipoAcquisizione { get; set; }

    public Guid? fornitore { get; set; }

    public Guid? raccoglitore { get; set; }

    public Guid? provenienza { get; set; }

    public string nazione { get; set; }

    public string regione { get; set; }

    public string provincia { get; set; }

    public string localita { get; set; }

    public decimal? altitudine { get; set; }

    public string habitat { get; set; }

    public Guid tipoMateriale { get; set; }

    public decimal numeroEsemplari { get; set; }

    public Guid statoMateriale { get; set; }

    public Guid? gradoIncertezza { get; set; }

    public bool associatoErbario { get; set; }

    public string note { get; set; }

    public Guid utenteUltimaModifica { get; set; }

    public DateTime dataUltimaModifica { get; set; }

    public Guid specie { get; set; }

    public string latitudine { get; set; }

    public string longitudine { get; set; }

    public bool validazione { get; set; }

    public DateTime? dataraccolta { get; set; }

    public string ipendiprovenienza { get; set; }

    public virtual ICollection<Individui> Individui { get; set; } = new List<Individui>();

    public virtual Fornitori fornitoreNavigation { get; set; }

    public virtual GradoIncertezza gradoIncertezzaNavigation { get; set; }

    public virtual Identificatori identificatoreNavigation { get; set; }

    public virtual Nazioni nazioneNavigation { get; set; }

    public virtual Organizzazioni organizzazioneNavigation { get; set; }

    public virtual Provenienze provenienzaNavigation { get; set; }

    public virtual Province provinciaNavigation { get; set; }

    public virtual Raccoglitori raccoglitoreNavigation { get; set; }

    public virtual Regioni regioneNavigation { get; set; }

    public virtual Specie specieNavigation { get; set; }

    public virtual StatoMateriale statoMaterialeNavigation { get; set; }

    public virtual TipoAcquisizione tipoAcquisizioneNavigation { get; set; }

    public virtual TipiMateriale tipoMaterialeNavigation { get; set; }

    public virtual Users utenteAcquisizioneNavigation { get; set; }

    public virtual Users utenteUltimaModificaNavigation { get; set; }
}
