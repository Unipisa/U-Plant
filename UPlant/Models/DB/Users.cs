using System;
using System.Collections.Generic;

namespace UPlant.Models.DB;

public partial class Users
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string UnipiUserName { get; set; }

    public string CF { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; }

    public string CreatedFrom { get; set; }

    public Guid Organizzazione { get; set; }

    public Guid TipologiaUtente { get; set; }

    public virtual ICollection<Accessioni> AccessioniutenteAcquisizioneNavigation { get; set; } = new List<Accessioni>();

    public virtual ICollection<Accessioni> AccessioniutenteUltimaModificaNavigation { get; set; } = new List<Accessioni>();

    public virtual Organizzazioni OrganizzazioneNavigation { get; set; }

    public virtual ICollection<StoricoIndividuo> StoricoIndividuo { get; set; } = new List<StoricoIndividuo>();

    public virtual TipologiaUtente TipologiaUtenteNavigation { get; set; }

    public virtual ICollection<UserRole> UserRole { get; set; } = new List<UserRole>();
}
