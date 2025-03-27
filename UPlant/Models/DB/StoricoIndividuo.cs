using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class StoricoIndividuo
{
    public Guid id { get; set; }

    public Guid statoIndividuo { get; set; }

    public Guid condizione { get; set; }

    public string operazioniColturali { get; set; }

    public DateTime dataInserimento { get; set; }

    public Guid individuo { get; set; }

    public Guid utente { get; set; }

    public virtual Condizioni condizioneNavigation { get; set; }

    public virtual Individui individuoNavigation { get; set; }

    public virtual StatoIndividuo statoIndividuoNavigation { get; set; }

    public virtual Users utenteNavigation { get; set; }
}
