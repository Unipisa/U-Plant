# Proposta tecnica: gestione documenti per Accessioni e Individui

## Obiettivo
Aggiungere una gestione **Documenti** analoga a quella già presente per le immagini, con:
- card Documenti nel dettaglio **Accessione** (sotto elenco individui);
- card Documenti nel dettaglio **Individuo** (sotto card immagini);
- upload, download, eliminazione, metadati (dimensione, formato, descrizione, crediti);
- configurazione path dedicato in `AppSettings` (già presente `Pathfile.Docs`).

## Decisione consigliata (prima di implementare)

### 1) Unificare la tabella documenti (consigliato)
Usare **una tabella unica** per i documenti con un campo tipologia/contesto, invece di due tabelle separate.

**Schema logico suggerito:**
- `Documenti`:
  - `id` (Guid)
  - `tipoEntita` (enum/stringa: `Accessione`, `Individuo`)
  - `entitaId` (Guid dell'accessione o individuo)
  - `nomeFileOriginale`
  - `nomeFileFisico`
  - `estensione`
  - `mimeType`
  - `dimensioneBytes`
  - `descrizione`
  - `credits`
  - `autore`
  - `dataInserimento`
  - `visibile`

**Vantaggi:**
- una sola logica CRUD e upload;
- riuso massimo del codice;
- estendibile in futuro (es. documenti per Specie/Percorsi).

### 2) Unificare anche le funzioni applicative
Creare un servizio condiviso, ad esempio `DocumentStorageService`, per:
- validazione estensioni consentite;
- validazione dimensione massima (`LimitMaxUpload` o nuova chiave dedicata documenti);
- salvataggio fisico su disco (`Pathfile.Docs`);
- costruzione path/filename sicuro;
- recupero stream per download;
- cancellazione file fisico e record DB.

### 3) Validazioni consigliate upload
- **Whitelist estensioni** (esempio): `.pdf, .doc, .docx, .xls, .xlsx, .csv, .txt`.
- **Controllo MIME** lato server (non solo estensione).
- **Sanificazione nome file** (mai usare direttamente il nome client come path).
- **Dimensione massima** (in bytes), con messaggio utente coerente con quello già usato per immagini.
- Possibile evoluzione: antivirus/scansione in pipeline.

### 4) Organizzazione file system
Usare una struttura leggibile e stabile dentro `Pathfile.Docs`, ad esempio:
- `Docs/Accessioni/{accessioneId}/{documentoId}{estensione}`
- `Docs/Individui/{individuoId}/{documentoId}{estensione}`

In questo modo:
- facile cleanup per entità;
- separazione naturale tra i due domini;
- naming sicuro senza collisioni.

### 5) UI proposta
- **Dettaglio Accessione**: card "Documenti" sotto lista individui, tabella con:
  - nome file,
  - formato,
  - dimensione (KB/MB),
  - descrizione,
  - autore/data,
  - azioni (download, elimina).
- **Dettaglio Individuo**: card analoga sotto la card immagini.
- Modal upload simile a quella immagini, ma con testo/formati documentali.

## Alternative valutate

### A) Due tabelle separate (`DocumentiAccessione`, `DocumentiIndividuo`)
**Pro:** mapping DB più esplicito.
**Contro:** duplicazione controller/view/logica e maggiore manutenzione.

### B) Tabella unica (raccomandata)
**Pro:** riduce duplicazione, facilita estensioni future.
**Contro:** richiede un minimo di attenzione in query/constraint applicativi.

## Piano di implementazione proposto
1. Definizione modello DB (tabella unica `Documenti` con `tipoEntita` + `entitaId`).
2. Creazione servizio comune di storage/validazione documenti.
3. Endpoint controller per:
   - lista per entità,
   - upload,
   - download,
   - delete.
4. Inserimento card Documenti nelle view Details di Accessioni e Individui.
5. Messaggistica localizzata + controlli lato client coerenti con server.
6. Test manuale:
   - upload valido,
   - blocco estensione non valida,
   - blocco file troppo grande,
   - download,
   - delete e verifica rimozione fisica.

## Nota su configurazione
Nel progetto è già presente `Pathfile.Docs`; si può riusare direttamente per i documenti funzionali (non solo export xlsx), eventualmente distinguendo sotto-cartelle (`Exports`, `EntityDocs`) per separare i casi d'uso.


## Risposte ai dubbi emersi (decisioni pratiche)

### Cartelle: progressivo o ID tecnico?
Per i documenti conviene usare una strategia **ibrida**:

- chiave tecnica stabile per il path: `id` (Guid) dell'entità;
- progressivo solo come metadato leggibile (nel DB o eventualmente nel nome visualizzato).

**Perché non solo progressivo?**
- il progressivo è più leggibile, ma può avere casi di normalizzazione/formattazione diversi nel tempo;
- l'`id` è univoco e immutabile, quindi più sicuro per riferimenti file-system e API.

**Struttura consigliata aggiornata:**
- `EntityDocs/Accessioni/{accessioneIdGuid}/{documentoId}{estensione}`
- `EntityDocs/Individui/{individuoIdGuid}/{documentoId}{estensione}`

Volendo, per leggibilità operativa, si può aggiungere un livello informativo non vincolante:
- `EntityDocs/Accessioni/{progressivo}_{accessioneIdGuid}/{documentoId}{estensione}`
- `EntityDocs/Individui/{progressivo}_{individuoIdGuid}/{documentoId}{estensione}`

### Riutilizzo di `Pathfile.Docs` per export ricerca + documenti entità
Sì, è una buona idea, ma con sotto-cartelle separate:

- `Pathfile.Docs/Exports` → file temporanei di ricerca/estrazione;
- `Pathfile.Docs/EntityDocs` → documenti permanenti di Accessioni/Individui.

Così eviti collisioni tra file temporanei e documenti funzionali.

### Pulizia file temporanei delle ricerche (fondamentale)
Per evitare file inutili sul server, introdurre una retention semplice:

1. quando crei l'export, salva in `Exports` e registra timestamp;
2. alla fine del download, prova a cancellare subito (best effort);
3. aggiungi un job schedulato (HostedService) che ogni N minuti elimina file in `Exports` più vecchi di una soglia (es. 24 ore).

**Regola pratica consigliata:**
- TTL export: 24h (configurabile);
- cleanup scheduler: ogni 30/60 minuti;
- loggare numero file rimossi + eventuali errori.

Questo mantiene il server pulito anche se l'utente non scarica o interrompe la sessione.
