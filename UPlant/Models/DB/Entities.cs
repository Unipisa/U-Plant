using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace UPlant.Models.DB;

public partial class Entities : DbContext
{
    public Entities(DbContextOptions<Entities> options)
        : base(options)
    {
    }

    public virtual DbSet<Accessioni> Accessioni { get; set; }

    public virtual DbSet<Areali> Areali { get; set; }

    public virtual DbSet<Cartellini> Cartellini { get; set; }

    public virtual DbSet<Cites> Cites { get; set; }

    public virtual DbSet<Collezioni> Collezioni { get; set; }

    public virtual DbSet<Condizioni> Condizioni { get; set; }

    public virtual DbSet<Contafamiglie> Contafamiglie { get; set; }

    public virtual DbSet<Contageneri> Contageneri { get; set; }

    public virtual DbSet<Contaspecie> Contaspecie { get; set; }

    public virtual DbSet<Contaspecieindexseminum> Contaspecieindexseminum { get; set; }

    public virtual DbSet<Contaspecienursery> Contaspecienursery { get; set; }

    public virtual DbSet<Famiglie> Famiglie { get; set; }

    public virtual DbSet<Fornitori> Fornitori { get; set; }

    public virtual DbSet<Generi> Generi { get; set; }

    public virtual DbSet<GradoIncertezza> GradoIncertezza { get; set; }

    public virtual DbSet<Identificatori> Identificatori { get; set; }

    public virtual DbSet<ImmaginiIndividuo> ImmaginiIndividuo { get; set; }

    public virtual DbSet<Individui> Individui { get; set; }

    public virtual DbSet<IndividuiPercorso> IndividuiPercorso { get; set; }

    public virtual DbSet<Iucn> Iucn { get; set; }

    public virtual DbSet<ModalitaPropagazione> ModalitaPropagazione { get; set; }

    public virtual DbSet<Nazioni> Nazioni { get; set; }

    public virtual DbSet<Organizzazioni> Organizzazioni { get; set; }

    public virtual DbSet<Percorsi> Percorsi { get; set; }

    public virtual DbSet<Provenienze> Provenienze { get; set; }

    public virtual DbSet<Province> Province { get; set; }

    public virtual DbSet<Raccoglitori> Raccoglitori { get; set; }

    public virtual DbSet<Regioni> Regioni { get; set; }

    public virtual DbSet<Regni> Regni { get; set; }

    public virtual DbSet<Ricercaacc> Ricercaacc { get; set; }

    public virtual DbSet<Ricercaind> Ricercaind { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<Sesso> Sesso { get; set; }

    public virtual DbSet<Settori> Settori { get; set; }

    public virtual DbSet<Specie> Specie { get; set; }

    public virtual DbSet<StatoIndividuo> StatoIndividuo { get; set; }

    public virtual DbSet<StatoMateriale> StatoMateriale { get; set; }

    public virtual DbSet<StoricoIndividuo> StoricoIndividuo { get; set; }

    public virtual DbSet<TipiMateriale> TipiMateriale { get; set; }

    public virtual DbSet<TipoAcquisizione> TipoAcquisizione { get; set; }

    public virtual DbSet<TipoIdentificatore> TipoIdentificatore { get; set; }

    public virtual DbSet<TipologiaUtente> TipologiaUtente { get; set; }

    public virtual DbSet<UserRole> UserRole { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accessioni>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.altitudine).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.dataAcquisizione).HasColumnType("datetime");
            entity.Property(e => e.dataUltimaModifica).HasColumnType("datetime");
            entity.Property(e => e.dataraccolta).HasColumnType("datetime");
            entity.Property(e => e.habitat).IsUnicode(false);
            entity.Property(e => e.ipen)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ipendiprovenienza)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.latitudine).IsUnicode(false);
            entity.Property(e => e.localita).IsUnicode(false);
            entity.Property(e => e.longitudine).IsUnicode(false);
            entity.Property(e => e.nazione)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.note).HasColumnType("text");
            entity.Property(e => e.numeroEsemplari).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.progressivo)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.provincia)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.regione)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.vecchioprogressivo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.fornitoreNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.fornitore)
                .HasConstraintName("FK_Accessioni_fornitore");

            entity.HasOne(d => d.gradoIncertezzaNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.gradoIncertezza)
                .HasConstraintName("FK_Accessioni_GradoIncertezza");

            entity.HasOne(d => d.identificatoreNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.identificatore)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accessioni_Identificatore");

            entity.HasOne(d => d.nazioneNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.nazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accessioni_Nazione");

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accessioni_Organizzazione");

            entity.HasOne(d => d.provenienzaNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.provenienza)
                .HasConstraintName("FK_Accessioni_Provenienza");

            entity.HasOne(d => d.provinciaNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.provincia)
                .HasConstraintName("FK_Accessioni_Provincia");

            entity.HasOne(d => d.raccoglitoreNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.raccoglitore)
                .HasConstraintName("FK_Accessioni_Raccoglitore");

            entity.HasOne(d => d.regioneNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.regione)
                .HasConstraintName("FK_Accessioni_Regione");

            entity.HasOne(d => d.specieNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.specie)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accessioni_Specie");

            entity.HasOne(d => d.statoMaterialeNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.statoMateriale)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accessioni_StatoMateriale");

            entity.HasOne(d => d.tipoAcquisizioneNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.tipoAcquisizione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accessioni_TipoAcquisizione");

            entity.HasOne(d => d.tipoMaterialeNavigation).WithMany(p => p.Accessioni)
                .HasForeignKey(d => d.tipoMateriale)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accessioni_TipoMateriale");

            entity.HasOne(d => d.utenteAcquisizioneNavigation).WithMany(p => p.AccessioniutenteAcquisizioneNavigation)
                .HasForeignKey(d => d.utenteAcquisizione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accessioni_UtentiInserimento");

            entity.HasOne(d => d.utenteUltimaModificaNavigation).WithMany(p => p.AccessioniutenteUltimaModificaNavigation)
                .HasForeignKey(d => d.utenteUltimaModifica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accessioni_UtentiModifica");
        });

        modelBuilder.Entity<Areali>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.codiceInterno)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<Cartellini>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.Cartellini)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cartellini_Organizzazione");
        });

        modelBuilder.Entity<Cites>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.codice)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.descrizione).IsUnicode(false);
        });

        modelBuilder.Entity<Collezioni>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.collezione)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.collezione_en)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.Collezioni)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Collezioni_Organizzazioni");

            entity.HasOne(d => d.settoreNavigation).WithMany(p => p.Collezioni)
                .HasForeignKey(d => d.settore)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Collezioni_Settori");
        });

        modelBuilder.Entity<Condizioni>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.condizione)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.Condizioni)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Condizioni_Organizzazioni");
        });

        modelBuilder.Entity<Contafamiglie>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Contafamiglie");

            entity.Property(e => e.descrizione)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Contageneri>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Contageneri");
        });

        modelBuilder.Entity<Contaspecie>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Contaspecie");
        });

        modelBuilder.Entity<Contaspecieindexseminum>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Contaspecieindexseminum");
        });

        modelBuilder.Entity<Contaspecienursery>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Contaspecienursery");
        });

        modelBuilder.Entity<Famiglie>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.descrizione_en)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Fornitori>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.note).HasColumnType("text");

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.Fornitori)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Fornitori_Organizzazione");
        });

        modelBuilder.Entity<Generi>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.famigliaNavigation).WithMany(p => p.Generi)
                .HasForeignKey(d => d.famiglia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Genere_Famiglia");
        });

        modelBuilder.Entity<GradoIncertezza>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.GradoIncertezza)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GradoIncertezza_Organizzazione");
        });

        modelBuilder.Entity<Identificatori>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.nominativo)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.Identificatori)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Identificatori_Organizzazione");

            entity.HasOne(d => d.tipoIdentificatoreNavigation).WithMany(p => p.Identificatori)
                .HasForeignKey(d => d.tipoIdentificatore)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Identificatori_Tipo");
        });

        modelBuilder.Entity<ImmaginiIndividuo>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.autore)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.credits)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.dataInserimento).HasColumnType("datetime");
            entity.Property(e => e.descrizione).IsUnicode(false);
            entity.Property(e => e.nomefile)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.individuoNavigation).WithMany(p => p.ImmaginiIndividuo)
                .HasForeignKey(d => d.individuo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ImmaginiIndividuo_Individui");
        });

        modelBuilder.Entity<Individui>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.destinazioni).HasColumnType("text");
            entity.Property(e => e.latitudine).IsUnicode(false);
            entity.Property(e => e.longitudine).IsUnicode(false);
            entity.Property(e => e.note).HasColumnType("text");
            entity.Property(e => e.progressivo)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.propagatoData).HasColumnType("datetime");
            entity.Property(e => e.vecchioprogressivo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.accessioneNavigation).WithMany(p => p.Individui)
                .HasForeignKey(d => d.accessione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Individui_Accessioni");

            entity.HasOne(d => d.cartellinoNavigation).WithMany(p => p.Individui)
                .HasForeignKey(d => d.cartellino)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Individui_Cartellini");

            entity.HasOne(d => d.collezioneNavigation).WithMany(p => p.Individui)
                .HasForeignKey(d => d.collezione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Individui_Collezioni");

            entity.HasOne(d => d.propagatoModalitaNavigation).WithMany(p => p.Individui)
                .HasForeignKey(d => d.propagatoModalita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Individui_ModalitaPropagazione");

            entity.HasOne(d => d.sessoNavigation).WithMany(p => p.Individui)
                .HasForeignKey(d => d.sesso)
                .HasConstraintName("FK_Individui_Sesso");

            entity.HasOne(d => d.settoreNavigation).WithMany(p => p.Individui)
                .HasForeignKey(d => d.settore)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Individui_Settori");
        });

        modelBuilder.Entity<IndividuiPercorso>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.individuoNavigation).WithMany(p => p.IndividuiPercorso)
                .HasForeignKey(d => d.individuo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndividuiPercorso_Individui");

            entity.HasOne(d => d.percorsoNavigation).WithMany(p => p.IndividuiPercorso)
                .HasForeignKey(d => d.percorso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndividuiPercorso_Percorsi");
        });

        modelBuilder.Entity<Iucn>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.codice)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.descrizione).IsUnicode(false);
        });

        modelBuilder.Entity<ModalitaPropagazione>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.propagatoModalita)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.ModalitaPropagazione)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ModalitaPropagazione_Organizzazione");
        });

        modelBuilder.Entity<Nazioni>(entity =>
        {
            entity.HasKey(e => e.codice);

            entity.Property(e => e.codice)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.descrizione_en).IsUnicode(false);
        });

        modelBuilder.Entity<Organizzazioni>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.prefissoIpen)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Percorsi>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.autore)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.credits)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.datacreazione).HasColumnType("datetime");
            entity.Property(e => e.descrizione).IsUnicode(false);
            entity.Property(e => e.descrizione_en).IsUnicode(false);
            entity.Property(e => e.nomefile)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ordinamento).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.titolo)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.titolo_en)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Provenienze>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.descrizione_en)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.Provenienze)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Provenienze_Organizzazione");
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasKey(e => e.codice);

            entity.Property(e => e.codice)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.regione)
                .IsRequired()
                .HasMaxLength(2)
                .IsUnicode(false);

            entity.HasOne(d => d.regioneNavigation).WithMany(p => p.Province)
                .HasForeignKey(d => d.regione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Province_Regione");
        });

        modelBuilder.Entity<Raccoglitori>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.nominativo)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.Raccoglitori)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Raccoglitori_Organizzazione");
        });

        modelBuilder.Entity<Regioni>(entity =>
        {
            entity.HasKey(e => e.codice);

            entity.Property(e => e.codice)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.descrizione_en).IsUnicode(false);
        });

        modelBuilder.Entity<Regni>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.codiceInterno)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.descrizione_en).IsUnicode(false);
        });

        modelBuilder.Entity<Ricercaacc>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Ricercaacc");

            entity.Property(e => e.dataAcquisizione).HasColumnType("datetime");
            entity.Property(e => e.famiglia)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.fornitore).IsUnicode(false);
            entity.Property(e => e.genere)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.gradoincertezza).IsUnicode(false);
            entity.Property(e => e.inseritoda)
                .HasMaxLength(301)
                .IsUnicode(false);
            entity.Property(e => e.modificatoda)
                .HasMaxLength(301)
                .IsUnicode(false);
            entity.Property(e => e.nome_scientifico)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.progressivo)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.raccoglitore).IsUnicode(false);
            entity.Property(e => e.tipoacquisizione).IsUnicode(false);
            entity.Property(e => e.tipomateriale).IsUnicode(false);
            entity.Property(e => e.vecchioprogressivo)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ricercaind>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Ricercaind");

            entity.Property(e => e.cartellino)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.collezione)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.condizione)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.cult)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.datainserimento).HasColumnType("datetime");
            entity.Property(e => e.datapropagazione).HasColumnType("datetime");
            entity.Property(e => e.famiglia)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.genere)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ipen)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.nome)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.nome_scientifico)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.nomecognome)
                .HasMaxLength(301)
                .IsUnicode(false);
            entity.Property(e => e.progressivo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.progressivoacc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.propagatodata).HasColumnType("datetime");
            entity.Property(e => e.settore)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.statoindividuo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.subspecie)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.varieta)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.vecchioprogressivo)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07D331C006");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Descr)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Sesso>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.Sesso)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sesso_Organizzazioni");
        });

        modelBuilder.Entity<Settori>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.settore)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.settore_en)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.Settori)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Settori_Organizzazioni");
        });

        modelBuilder.Entity<Specie>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.autori)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.autoricult)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.autorisub)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.autorivar)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.cult)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.nome)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.nome_comune)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.nome_comune_en)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.nome_scientifico)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.nome_volgare)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.subspecie)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.varieta)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.arealeNavigation).WithMany(p => p.Specie)
                .HasForeignKey(d => d.areale)
                .HasConstraintName("FK_Specie_Areale");

            entity.HasOne(d => d.citesNavigation).WithMany(p => p.Specie)
                .HasForeignKey(d => d.cites)
                .HasConstraintName("FK_Specie_Cites");

            entity.HasOne(d => d.genereNavigation).WithMany(p => p.Specie)
                .HasForeignKey(d => d.genere)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Specie_Genere");

            entity.HasOne(d => d.iucn_globaleNavigation).WithMany(p => p.Specieiucn_globaleNavigation)
                .HasForeignKey(d => d.iucn_globale)
                .HasConstraintName("FK_Specie_IucnGlobale");

            entity.HasOne(d => d.iucn_italiaNavigation).WithMany(p => p.Specieiucn_italiaNavigation)
                .HasForeignKey(d => d.iucn_italia)
                .HasConstraintName("FK_Specie_IucnItalia");

            entity.HasOne(d => d.regnoNavigation).WithMany(p => p.Specie)
                .HasForeignKey(d => d.regno)
                .HasConstraintName("FK_Specie_Regno");
        });

        modelBuilder.Entity<StatoIndividuo>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.stato)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.StatoIndividuo)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StatoIndividuo_Organizzazione");
        });

        modelBuilder.Entity<StatoMateriale>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.StatoMateriale)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StatoMateriale_Organizzazione");
        });

        modelBuilder.Entity<StoricoIndividuo>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.dataInserimento).HasColumnType("datetime");
            entity.Property(e => e.operazioniColturali).IsUnicode(false);

            entity.HasOne(d => d.condizioneNavigation).WithMany(p => p.StoricoIndividuo)
                .HasForeignKey(d => d.condizione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoricoIndividuo_Condizioni");

            entity.HasOne(d => d.individuoNavigation).WithMany(p => p.StoricoIndividuo)
                .HasForeignKey(d => d.individuo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoricoIndividuo_Individui");

            entity.HasOne(d => d.statoIndividuoNavigation).WithMany(p => p.StoricoIndividuo)
                .HasForeignKey(d => d.statoIndividuo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoricoIndividuo_StatoIndividuo");

            entity.HasOne(d => d.utenteNavigation).WithMany(p => p.StoricoIndividuo)
                .HasForeignKey(d => d.utente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoricoIndividuo_Users");
        });

        modelBuilder.Entity<TipiMateriale>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.TipiMateriale)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TipiMateriale_Organizzazione");
        });

        modelBuilder.Entity<TipoAcquisizione>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.descrizione_en).IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.TipoAcquisizione)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TipoAcquisizione_Organizzazione");
        });

        modelBuilder.Entity<TipoIdentificatore>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK_TipoVerificatore");

            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.TipoIdentificatore)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TipoVerificatore_Organizzazione");
        });

        modelBuilder.Entity<TipologiaUtente>(entity =>
        {
            entity.Property(e => e.id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.descrizione)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.organizzazioneNavigation).WithMany(p => p.TipologiaUtente)
                .HasForeignKey(d => d.organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TipologiaUtente_Organizzazioni");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRole__3214EC07F208CA42");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.RoleFKNavigation).WithMany(p => p.UserRole)
                .HasForeignKey(d => d.RoleFK)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRole__RoleFK__45BE5BA9");

            entity.HasOne(d => d.UserFKNavigation).WithMany(p => p.UserRole)
                .HasForeignKey(d => d.UserFK)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRole__UserFK__2739D489");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0759D0EB04");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CF)
                .IsRequired()
                .HasMaxLength(16)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedFrom)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.UnipiUserName)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.OrganizzazioneNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Organizzazione)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Organizzazioni");

            entity.HasOne(d => d.TipologiaUtenteNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.TipologiaUtente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_TipologiaUtente");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
