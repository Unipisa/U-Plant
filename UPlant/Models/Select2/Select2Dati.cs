using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Select2
{
    /// <summary>
    /// Prototipo ottimizzato per l'utilizzo con la libreria Select2.    
    /// Per l'utilizzo è sufficiente estendere la classe e aggiungere la proprietà items
    /// </summary>
    public class Select2Dati
    {
        /// <value>Numero dei record trovati</value>
        public int total_count { get; set; }
        /// <value>
        /// Mostra o meno i risultati parziali
        /// </value>
        public bool incomplete_result { get; set; }
    }
    /// <summary>
    /// Prototipo di oggetto generico ottimizzato per l'utilizzo con la libreria Select2.
    /// Una volta istanziata la classe Select2Dati() si aggiunga una lista oggetti Select2Item.
    /// </summary>
    /// Vedi <see cref="Select2Dati" />
    /// <example>
    /// Esempio:
    /// <code>
    /// List&lt;Select2Item&gt; Elenco  = new GetData() // metodo per il recupero dei dati
    /// 
    /// var res = new Select2Users()
    /// {
    ///     incomplete_result = true,
    ///     total_count = Elenco.Count();
    ///     items = Elenco
    /// };
    /// </code>
    /// </example>
    public class Select2Item
    {
        /// <value>Identificativo univoco </value>
        public string id { get; set; }
        /// <value>Label mostrata nella tendina</value>
        public string text { get; set; }
        /// <value> Se true l'elemento è disabilitato, abilitato altrimenti</value>
        public bool disabled { get; set; }
        /// <value> Se true l'elemento è selezionato, non selezionato altrimenti</value>
        public bool selected { get; set; }
    }
}