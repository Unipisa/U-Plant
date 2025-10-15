using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace UPlant.Models.DB
{
    
    public partial class CartelliniCustom
    {
        [Required]
        public string descrizione { get; set; }

        [Required]
        public int ordinamento { get; set; }
    }
    [ModelMetadataType(typeof(CartelliniCustom))]
    public partial class Cartellini
    {   
    }


    public partial class UsersCustom
    {
        [Required]
        public string UnipiUserName { get; set; }
        [Required]
        public string CF { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }


    }
    [ModelMetadataType(typeof(UsersCustom))]
    public partial class Users
    {
    }



    public partial class PercorsiCustom
    {

        [Required(ErrorMessage = "Titolo richiesto.")]
        public string titolo { get; set; }

    }

    [ModelMetadataType(typeof(PercorsiCustom))]
    public partial class Percorsi{}
   


    public partial class CitesCustom
    {
       
        [Required]
        public string codice { get; set; }

        [Required]
        public string ordinamento { get; set; }
    }
    [ModelMetadataType(typeof(CitesCustom))]
    public partial class Cites
    {
    }
    
    public partial class CondizioniCustom
    {
      
        [Required]
        public string condizione { get; set; }

        [Required]
        public string ordinamento { get; set; }

    }
    [ModelMetadataType(typeof(CondizioniCustom))]
    public partial class Condizioni
    { }

    public partial class IucnCustom
    {
        public Guid id { get; set; }
        [Required]
        public string codice { get; set; }

        public string descrizione { get; set; }
        [Required]
        public string ordinamento { get; set; }

    }
    [ModelMetadataType(typeof(IucnCustom))]
    public partial class Iucn
    { }


        public partial class ModalitaPropagazioneCustom
    {
        public Guid id { get; set; }
        [Required]
        public string propagatoModalita { get; set; }

        public Guid organizzazione { get; set; }
        [Required]
        public string ordinamento { get; set; }

    }
    [ModelMetadataType(typeof(ModalitaPropagazioneCustom))]
    public partial class ModalitaPropagazione
    { }

    public partial class ProvenienzeCustom
    {
        public Guid id { get; set; }

        public Guid organizzazione { get; set; }
        [Required]
        public string descrizione { get; set; }

        public string descrizione_en { get; set; }
        [Required]
        public string ordinamento { get; set; }

    }
    [ModelMetadataType(typeof(ProvenienzeCustom))]
    public partial class Provenienze
    { }


    public partial class RegniCustom
    {
        public Guid id { get; set; }
        [Required]
        public string descrizione { get; set; }

        public string codiceInterno { get; set; }

        public string descrizione_en { get; set; }
        [Required]
        public string ordinamento { get; set; }

    }
    [ModelMetadataType(typeof(RegniCustom))]
    public partial class Regni
    { }




    public partial class SettoriCustom
    {
        public Guid id { get; set; }
        [Required]
        public string settore { get; set; }

        public Guid organizzazione { get; set; }

        public string settore_en { get; set; }
        [Required]
        public bool visualizzazioneweb { get; set; }
        [Required]
        public string ordinamento { get; set; }

    }
    [ModelMetadataType(typeof(SettoriCustom))]
    public partial class Settori
    { }

    public partial class StatoIndividuoCustom
    {
        public Guid id { get; set; }
        [Required]
        public string stato { get; set; }

        public Guid organizzazione { get; set; }
        [Required]
        public bool visualizzazioneweb { get; set; }
        [Required]
        public string ordinamento { get; set; }

    }
    [ModelMetadataType(typeof(StatoIndividuoCustom))]
    public partial class StatoIndividuo { }


    public partial class StatoMaterialeCustom
    {
        public Guid id { get; set; }

        public Guid organizzazione { get; set; }
        [Required]
        public string descrizione { get; set; }
        [Required]
        public string ordinamento { get; set; }


    }
    [ModelMetadataType(typeof(StatoMaterialeCustom))]
    public partial class StatoMateriale { }


    public partial class TipiMaterialeCustom
    {
        public Guid id { get; set; }

        public Guid organizzazione { get; set; }
        [Required]
        public string descrizione { get; set; }
        [Required]
        public string ordinamento { get; set; }


    }
    [ModelMetadataType(typeof(TipiMaterialeCustom))]
    public partial class TipiMateriale { }


    public partial class TipoAcquisizioneCustom
    {
        public Guid id { get; set; }
        [Required]
        public string descrizione { get; set; }

        public Guid organizzazione { get; set; }

        public string descrizione_en { get; set; }
        [Required]
        public string ordinamento { get; set; }

    }
    [ModelMetadataType(typeof(TipoAcquisizioneCustom))]
    public partial class TipoAcquisizione { }


    public partial class TipologiaUtenteCustom
        {
        public Guid id { get; set; }
        [Required]
        public string descrizione { get; set; }

        public Guid organizzazione { get; set; }
        [Required]
        public string ordinamento { get; set; }


    }
    [ModelMetadataType(typeof(TipologiaUtenteCustom))]
    public partial class TipologiaUtente { }



    public partial class AccessioniCustom
    {
        [Editable(false)]
        [Display(Name = "Data Acquisizione")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime dataAcquisizione { get; set; }


        [DisplayFormat(DataFormatString = "{0:0.###}", ApplyFormatInEditMode = true)]
        public decimal? altitudine { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.###}", ApplyFormatInEditMode = true)]
        public decimal numeroEsemplari { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? dataraccolta { get; set; }
    }
    [ModelMetadataType(typeof(AccessioniCustom))]
    public partial class Accessioni { }










}
