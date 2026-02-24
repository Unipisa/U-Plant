using System;

namespace UPlant.Models.DB;

public partial class Documenti
{
    public Guid id { get; set; }

    public string tipoEntita { get; set; }

    public Guid entitaId { get; set; }

    public string nomefile { get; set; }

    public string nomefileFisico { get; set; }

    public string estensione { get; set; }

    public string mimeType { get; set; }

    public long dimensioneBytes { get; set; }

    public string descrizione { get; set; }

    public string credits { get; set; }

    public string autore { get; set; }

    public DateTime dataInserimento { get; set; }

    public bool visibile { get; set; }
}
