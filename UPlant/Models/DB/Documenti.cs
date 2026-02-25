using System;

namespace UPlant.Models.DB;

public partial class Documenti
{
    public Guid id { get; set; }

    public string tipoEntita { get; set; }

    public Guid? AccessioneId { get; set; }

    public Guid? IndividuoId { get; set; }

    public string nomefile { get; set; }

    public string nomefileFisico { get; set; }

    public string estensione { get; set; }

    public string mimeType { get; set; }

    public long dimensioneBytes { get; set; }

    public string descrizione { get; set; }

    public string utente { get; set; }

    public DateTime dataInserimento { get; set; }

    public virtual Accessioni AccessioneNavigation { get; set; }

    public virtual Individui IndividuoNavigation { get; set; }
}
