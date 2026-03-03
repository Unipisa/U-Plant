# UPlant

## Panoramica
UPlant è un gestionale per orti botanici nato per digitalizzare e semplificare le attività operative quotidiane (anagrafica piante, tracciamento storico, collocazione, immagini, percorsi per visitatori e stampa etichette).

Il progetto è stato ideato e sviluppato da **Pietro Picconi**, con la collaborazione di **Marco D'Antraccoli** (Coordinatore dell'Orto Botanico dell'Università di Pisa), per offrire uno strumento concreto di verticalizzazione e ottimizzazione del lavoro in orto botanico.

Questa repository contiene la nuova versione in **.NET 8**; la versione oggi in produzione è su .NET Framework 4.7.

---

## Obiettivi del progetto
- Rendere disponibile una base applicativa riusabile anche da altre realtà botaniche.
- Favorire la standardizzazione dei processi gestionali degli orti botanici.
- Mantenere dimensioni infrastrutturali contenute:
  - database (37 tabelle) con occupazione ~300MB dopo anni di utilizzo;
  - applicazione con occupazione ~200MB;
  - supporto ad archiviazione immagini su repository filesystem.

---

## Stack tecnologico
- **Backend:** ASP.NET Core (.NET 8)
- **Database:** SQL Server
- **Hosting:** IIS (Windows)
- **Autenticazione:** OAuth2/OIDC (WSO2 o Azure) oppure SAML2
- **Opzionale:** OpenStreetMap, stampa etichette QRCode con DYMO 450

---

## Prerequisiti
### Obbligatori
- SQL Server disponibile
- IIS configurato
- .NET 8 Hosting Bundle installato su server IIS
- Sistema di autenticazione esterno configurato (WSO2, Azure o SAML2)

### Opzionali
- Stampante DYMO 450 per etichette con QRCode

---

## File principali da conoscere
- `Creadb.sql`
- `appsettings.json`

---

## Quick start (installazione)

### 1) Creazione database
Eseguire `Creadb.sql` su SQL Server.

Lo script include:
- creazione struttura tabelle;
- dati iniziali necessari per avvio applicazione.

### 2) Tabelle principali (panoramica funzionale)
> Nota: alcune tabelle sono già pre-popolate da script iniziale.

- **Accessioni**: entità astratta di ingresso pianta in orto; collega molte FK.
- **Individui**: tabella principale degli esemplari, figlia di Accessioni.
- **StoricoIndividuo**: evoluzione dello stato dell'individuo nel tempo.
- **Collezioni / Settori**: collocazione fisica e organizzazione botanica.
- **Specie / Generi / Famiglie**: classificazione tassonomica.
- **Condizioni / StatoIndividuo / StatoMateriale**: stato biologico e di conservazione.
- **ImmaginiIndividuo**: archivio immagini associate agli individui.
- **Percorsi / IndividuiPercorso**: percorsi tematici per interfaccia visitatori (UPlantDiscover).
- **Organizzazioni**: multi-ente (pensata per più orti botanici su stesso DB).
- **Users / Roles / UserRole**: autenticazione e autorizzazione utenti.

### 3) Popolamento minimo indispensabile
Prima del primo avvio:
1. valorizzare **Organizzazioni**;
2. creare almeno un record in **Users** con identificativo coerente con claim ricevuto da autenticatore (WSO2/Azure/SAML).

Senza questi dati iniziali, il login non va a buon fine.

---

## Configurazione applicativa (`appsettings.json`)

In molti ambienti troverai file specifici:
- `appsettings.Development.json`
- `appsettings.Staging.json`
- `appsettings.Production.json`

Questi file **sovrascrivono** i valori del file base `appsettings.json`.

### Sezioni chiave
- `AppSettings:Application`
  - nome applicazione, branding (logo/testi), URL base, tipo autenticazione (`TypeAuth`: `WSO2`, `AZURE` o `SAML2`).
- `AppSettings:Pathfile`
  - path filesystem per immagini/documenti e limite upload.
- `ConnectionStrings:UPlant`
  - stringa connessione SQL Server.
- `Wso2Auth`
  - endpoint OAuth2/OIDC e claim utilizzati.
- `AzureAd`
  - tenant, client id/secret e callback per autenticazione Azure (OIDC).
- `Saml2Auth`
  - entity ID SP, metadata IdP, mapping claim, certificato di firma (`.pfx`).

### Esempio (semplificato)
```json
{
  "AppSettings": {
    "Application": {
      "NomeAppShort": "UPlant",
      "NomeAppLong": "UPlant - Orto Botanico",
      "UrlPrefix": "https://localhost:7038/",
      "UrlBaseCode": "https://localhost:7038/",
      "TypeAuth": "WSO2" // possibili valori: WSO2, AZURE, SAML2
    },
    "Pathfile": {
      "Basepath": "C:\\immagini",
      "Docs": "C:\\xlsx",
      "LimitMaxUpload": "4194304"
    }
  },
  "ConnectionStrings": {
    "UPlant": "<connection-string-sql-server>"
  }
}
```

---

## Deploy su IIS (Windows)

### Prerequisiti server
- Installare **Web Deploy 4** (se necessario per pubblicazione).
- Installare **ASP.NET Core Runtime / Hosting Bundle .NET 8**.

### Configurazione sito IIS
1. Creare nuovo sito (es. `uplant`).
2. Creare e associare Application Pool con:
   - `.NET CLR Version = No Managed Code`
   - `Load User Profile = True` (consigliato/necessario in scenari SAML).
3. Configurare binding HTTPS:
   - IP/porta/host name
   - SNI se richiesto
   - certificato SSL associato.

---

## Configurazione autenticazione

### WSO2 (OAuth2/OIDC)
Impostare in `appsettings.json`:
- endpoint `AuthorizationEndpoint`, `TokenEndpoint`, `UserinfoEndpoint`;
- `ClientID`, `ClientSecret`;
- lista claim previsti in ingresso.

### Azure (OpenID Connect)
Impostare in `appsettings.json` la sezione `AzureAd` (TenantId, ClientId, ClientSecret, callback) e usare `TypeAuth = "AZURE"`.

### SAML2
Impostare in `appsettings.json`:
- `TypeAuth = "SAML2"`
- `EntityId` (SP)
- `IdpEntity` (metadata IdP)
- mapping claim (es. codice fiscale / uid)
- `SigningCertificateFile` e `SigningCertificatePassword`.

> Nota tecnica: il mapping claim dipende dall'IdP; in alcuni casi va adattato anche il codice in `Program.cs`.

---

## Generazione certificato `.pfx` da IIS (self-signed)
1. IIS Manager → **Server Certificates**.
2. Click **Create Self-Signed Certificate**.
3. Assegnare nome certificato e confermare.
4. Selezionare certificato creato → **Export**.
5. Esportare in `.pfx` con password.
6. Copiare file nella directory applicativa e configurare nome/password in `appsettings.json`.

---

## Note operative importanti
- La tabella `Roles` iniziale non andrebbe alterata senza analisi impatti.
- In caso di errori login, verificare in ordine:
  1. record in `Organizzazioni`;
  2. record utente in `Users` con `UserName` coerente ai claim ricevuti;
  3. allineamento claim tra IdP e configurazione app.

---

## Roadmap proposta (prossimi passi)
1. Aggiungere una sezione **"Installazione in 15 minuti"** con checklist passo-passo.
2. Fornire un file `appsettings.example.json` senza segreti.
3. Documentare troubleshooting (errori più comuni OAuth2/Azure/SAML/IIS).
4. Inserire screenshot dell'interfaccia e diagramma base entità.

---

## Contatti e contributi
Se vuoi adottare UPlant nel tuo orto botanico o contribuire al progetto, apri una issue o una pull request con il tuo caso d'uso: aiuterà a rendere la piattaforma più robusta e condivisibile.
