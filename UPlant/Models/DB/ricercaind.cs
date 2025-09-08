using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Ricercaind
{
    public Guid id { get; set; }

    public string progressivoacc { get; set; }

    public DateTime? datainserimento { get; set; }

    public DateTime? datapropagazione { get; set; }

    public string progressivo { get; set; }

    public string vecchioprogressivo { get; set; }

    public string nome_scientifico { get; set; }

    public Guid? idsettore { get; set; }

    public string settore { get; set; }

    public Guid? idcollezione { get; set; }

    public string collezione { get; set; }

    public Guid? idcartellino { get; set; }

    public string cartellino { get; set; }

    public Guid? idstatoindividuo { get; set; }

    public string statoindividuo { get; set; }

    public string nomecognome { get; set; }

    public Guid? immagine { get; set; }

    public string genere { get; set; }

    public Guid? idfamiglia { get; set; }

    public string famiglia { get; set; }

    public Guid? idcondizione { get; set; }

    public string condizione { get; set; }

    public DateTime? propagatodata { get; set; }

    public string ipen { get; set; }

    public string nome { get; set; }

    public string subspecie { get; set; }

    public string varieta { get; set; }

    public string cult { get; set; }
}
