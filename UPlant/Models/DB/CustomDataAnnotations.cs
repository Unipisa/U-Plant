using System.ComponentModel.DataAnnotations;

namespace UPlant.Models.DB
{

    public partial class Accessioni
    {
       

        public int contaFigli { get { return Individui.Count(); } }
        //aggiunte successive

    //   public ICollection<Storico> ListaStoricoAccessioni { get; set; }  //DA TOGLIERE FA CASINO
     
    }
    public partial class RisultatoRicercaInd
    {
        public Guid id { get; set; }
        public string progressivo { get; set; }
        public string shortprog { get; set; }
        public string ipen { get; set; }
        public string vecchioprogressivo { get; set; }
        public string nome_scientifico { get; set; }
        public Guid? idsettore { get; set; }
        public string settore { get; set; }
        public string settore_en { get; set; }
        public Guid? idcollezione { get; set; }
        public string collezione { get; set; }
        public string collezione_en { get; set; }
        public Guid? idcartellino { get; set; }
        public string cartellino { get; set; }

        public string cartellino_en { get; set; }
        public Guid? idstatoindividuo { get; set; }
        public string stato { get; set; }
        public string stato_en { get; set; }
        public string nomecognome { get; set; }
        public string datainserimento { get; set; }
        public string countimg { get; set; }

        public string nomeetichetta { get; set; }
    }
    public partial class RisultatoRicercaAcc
    {
        public Guid id { get; set; }
        public string progressivo { get; set; }
        public string vecchioprogressivo { get; set; }
        public string nome_scientifico { get; set; }
        public string famiglia { get; set; }
        public string genere { get; set; }
        public string dataacquisizione { get; set; }
        public string tipomateriale { get; set; }
        public string tipomateriale_en { get; set; }
        public string countind { get; set; }
        public string inseritoda { get; set; }
        public string modificatoda { get; set; }
        public bool validazione { get; set; }
    }
    public partial class Users
    {
        public string NomeCognome { get { return Name + " " + LastName; } }
    }
    public partial class Individui
    {
     //  public ICollection<Storico> ListaStoricoIndividui { get; set; }
      //  public ICollection<ImmaginiIndividuo> ListaImmagini { get; set; } 
    }

    


}



