using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Ricercaacc
{
    public Guid idacc { get; set; }

    public string progressivo { get; set; }

    public string vecchioprogressivo { get; set; }

    public string nome_scientifico { get; set; }

    public Guid? idfamiglia { get; set; }

    public string famiglia { get; set; }

    public string genere { get; set; }

    public DateTime dataAcquisizione { get; set; }

    public Guid idtipomateriale { get; set; }

    public string tipomateriale { get; set; }

    public Guid idtipoacquisizione { get; set; }

    public string tipoacquisizione { get; set; }

    public Guid? idgradoincertezza { get; set; }

    public string gradoincertezza { get; set; }

    public Guid idinseritoda { get; set; }

    public string inseritoda { get; set; }

    public Guid idmodificatoda { get; set; }

    public string modificatoda { get; set; }

    public Guid? idfornitore { get; set; }

    public string fornitore { get; set; }

    public Guid? idraccoglitore { get; set; }

    public string raccoglitore { get; set; }

    public bool validazione { get; set; }
}
