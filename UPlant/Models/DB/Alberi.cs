using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Alberi
{
    public Guid id { get; set; }

    public Guid individuo { get; set; }

    public DateTime dataapertura { get; set; }

    public Guid priorita { get; set; }

    public Guid intervento { get; set; }

    public Guid fornitore { get; set; }

    public string motivo { get; set; }

    public string esitointervento { get; set; }

    public bool stato { get; set; }

    public DateTime dataultimamodifica { get; set; }

    public Guid utenteapertura { get; set; }

    public Guid utenteultimamodifica { get; set; }

    public virtual Fornitori fornitoreNavigation { get; set; }

    public virtual Individui individuoNavigation { get; set; }

    public virtual TipoInterventiAlberi interventoNavigation { get; set; }

    public virtual TipoPrioritaAlberi prioritaNavigation { get; set; }

    public virtual Users utenteaperturaNavigation { get; set; }

    public virtual Users utenteultimamodificaNavigation { get; set; }
}
