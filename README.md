# UPlant
## Nuova Versione Net Core 8
Il Progetto UPlant nasce per creare un programma di utilità per gestire tutte le attività di un orto botanico, secondo le ultime standardizzazioni.
Mi presento sono Pietro Picconi e sono il creatore e sviluppatore del progetto;con la collaborazione del Coordinatore del Orto Botanico dell'Università di Pisa Marco D'Antraccoli abbiamo fatto che si che Orto avesse a disposizione uno strumento che permettesse la verticalizzazione e ottimizzazione del lavoro all'interno di esso.
Il progetto è realizzato utilizzando come ambiente di sviluppo Microsoft con i prodotti DotNet , SqlServer e IIS.
In fatto di spazi il progetto è molto contenuto 
- Il database è composto da 37 tabelle e per ora dopo 4 anni di utilizzo occupa circa 300 MB
- L'ambiente dove girano le funzionialità occupa poco più che 200 MB
- Con il gestionale è possibile archiviare anche immagini quindi a seconda dell'utilizzo può essere utilizzato un repository per questi tipi di dati.

Il progetto ora in produzione lavora con .NET Framework 4.7 ma questo nuovo repository e stato aperto per aggiornarlo alla versione .Net 8.
Il nostro obiettivo è rendere i sorgenti disponibili anche per altre realtà così da farlo crescere e diffondere per renderlo uno standard nazione.

How to
Per l'installazione ci sono requisiti da garantire
- Database Sql Server
- IIS con supporto .Runtime 8
- Autenticazione -OAuth2 tramite WSO2 o SAML
Opzionale
- Registrazione a Google Map
- Stampa etichette Individui con qrcode tramite stampanti Dymo 450

I file principale da prendere in considerazione all'interno del progetto sono
- Creadb.sql
- Appsettings.json
 

Il primo passo è installare il db sfruttando il file Creadb.sql che è all'interno del progetto , il file ha presente sia la creazione della struttura delle tabelle che l'inserimento di dati già preinseriti per avviare il programma.
Le tabelle sono le seguenti :
- Accessioni : Tabella che contiene tutte le i recorde legate alle accessioni ,cioè la occorrenza astratta che viene fatta di una pianta quando entra in orto, contiene i collegamenti o dipendenze (FK) con diverse tabelle (15).
- Areali : Tabella abbastanza semplice che contiene tutte le occorrenze degli Areali (es : Messico-Honduras) 
- Cartellini : Tabella delle occorrenze che posso essere abbinate ai cartellini dell'individuo (già popolata)
- Cites : Tabella delle occorenze dei CITES (già popolata)
- Collezioni : Tabella delle Collezioni  è la tabella collegata alla tabella Settori che organizza la collocazione delle piante
- Condizioni : Tabella delle Condizione definisce lo stato dell'individuo (es: Buono) (già popolata)
- Famiglie :   Tabella insieme ai Generi che serve a definire la Specie dell'individuo (già popolata)
- Fornitori :  Tabella di semplice per catalogare i Fornitori
- Generi : Tabella come detto in precedenza legata alla Specie dell'individuo (già popolata)
- GradoIncertezza :  Tabella per stabilire il grado d'identificazione della pianta (già popolata)
- ImmaginiIndividuo : Tabella contente tutti i recordo in riferimento alle immagini allegate all'indivduo
- Indivudui : Tabella principale figlia della tabella Accessioni che contiene i dati principali di tutti gli individui presenti in orto
- IndividuiPercorso : Tabella legata alla tabella Percorsi per l'interfaccia UplantDiscover 
- Iucn : Tabella come i CITES per la situazione a livello mondiale della condizione si esistenza della pianta (es:Minacciata) (già popolata)
- Modalità Propagazione : Tabella di come l'individuo sia stato propagato (Es: Talea) (già popolata)
- Nazioni : Tabella contente le Nazioni (già popolata)
- Organizzazioni : Tabella contenente l' Organizzazione che gestisce gli individui per ora contente una singola occorrenza , orto che sta utilizzando il programma,  ma studiata poter contenere più orti in contemporanea in modo che siano ognuno realtà diverse
- Percorsi :Tabella utilizzata per l'interfaccia UplantDiscover per creare per i visitatori dei percorsi a tema definiti dai gestori dell'orto
- Provenienze : Tabella che definisce come l'individuo sia arrivato in orto (es: Selvatica) (già popolata)
- Province : Tabella collegata a quella delle Regioni (già popolata)
- Raccoglitori : Tabella che  contiene i nomi delle persone che hanno raccolto individui esternamente all'orto 
- Regioni : Tabella legata alla tabella Nazioni (già popolata)
- Regni : Tabella dei Regni (es: Indo-Pacifico)  (già popolata)
- Roles : Tabella dei ruoli per l'autenticazione e la profilazione (già popolata) da non modificare i ruoli inseriti 
- Sesso : Tabella che contiene le tipologie di sesso dell'individuo (già popolata)
- Settori : Tabella che definisce la collocazione dell'individuo all'interno dell'orto
- Specie : Tabella di dettaglio dell'individuo riguardo la Specie (già popolata)
- StatoIndividuo :  Tabella che definisce lo stato  dell'individuo (es: Vivo) (già popolata)
- StatoMateriale : Tabella che definisce lo stato del materiale dell'individuo (es: Ottimo) (già popolata)
- StoricoIndividuo : Tabella importante per archiviare l'evoluzione dell'individuo all'interno dell'orto
- TipiMateriale :  Tabella che definisce il tipo del materiale dell'individuo (es: Bulbo) (già popolata)
- TipoAcquisizione : Tabella che definisce come l'individuo è arrivato in orto (es: Donazione privato) (già popolata)
- TipologiaUtente : Tabella per definire che ruolo non legato all'autenticazione ma l'inquandramento all'interno dell orto (es : Staff Orto Botanico) (già popolata)
- TipoVerificatore : Tabella che contiente il tipo di Identificatore 
- UserRole : Tabella di abbinamento Utente - Ruolo, così da assegnare ad un utente specificato un ruolo in definito (es: Amministratore) (già popolata)
- User : Tabella contente gli utenti che posso accedere all'interno del programma la tabella contiene in campo UnipiUserName che verrà cambiato in Username in quale contiene la chiave utente che viene passata dall'autenticatore WSO2 o SAML( messo un utente demo)
- Verificatore : Tabella che da interfaccia è definita come Identificatore in futuro verrà cambiata la definizione della tabella per non creare ambiguità

Successivamente andrò nel dettagli di alcune tabelle per poter dettagliare alcune interazioni


Per proseguire con l'installazione come prima cosa bisogna prendere in considerazione la tabella delle è Organizzazioni.
La tabella Organizzazioni è una tabella che come detto precedentemente  contiene l'organizzazione che dovrà utilizzare il prodotto finchè vi è un unica organizzazione il progetto si può ritenere interno , ma è possibile poter inserire più organizzazioni ( in questo caso stiamo parlando di Orti Botanici) e poter far coesistere più realtà sotto un unico DB.
E' importante dover prima di tutto popolare la tabella Organizzazioni, altrimenti non è possibile inserire un Utente nella tabella Users.
Questa operazione è collegata ad autenticatore oAuth2 affinchè possa essere avviato il progetto.
Quindi senza prima popolare le tabella Organizzazioni e Users il progetto non può funzionare , per quanto riguarda l'autenticazione noi ci avvaliano come Server di Autenticazione WSO2 configurando le varie parametrizzazioni necessarie sia lato server sia lato progetto (file di configurazione) da recentemente abbiamo anche implementato l'autenticazione SAML quindi detto questo per proseguire nell'installazione del prodotto passeremo alla configurazione del progetto tramite la definizione dei parametri elencati nel file appsettings.json

Appsettings in visual studio crea dei sottofile separati per ogni ambiente di sviluppo potresti trovare
- Appsettings.json
 ->  Appsettings_Development.json in locale
 ->  Appsettings_Staging.json in test
 ->  Appsettings_Production.json in produzione
Se ci sono queste tre file i figli vanno a sovrascrivere il settaggio del padre quindi se avessi Appsettings.json e Development quello che è scritto in Appsettings padre verrebbe sovrascritto dal figlioDevelopment.

Il file è composto così

 {
  "AppSettings": {
    "Application": {
    "NomeAppShort": "", // nome applicazione 
    "NomeAppLong": "", //nome che si visualizza sulla testata del header
    "Universita": "Università di X",
    "UrlLogoU": "img/cherubino-white.svg", // path logo a destra nel header
    "AltUrlLogoU": "cherugino", //testo immagine in rilievo
    "ClassUrlLogoU": "logocherubino", // definizione classe css per il logo a destra
    "Struttura": "Orto Botanico di X", //definizione struttura
    "UrlLogoSsmall": "img/uplant.png", // logo a sinistra quando il menù è aperto
    "UrlLogoSlarge": "img/uplantlarge.png", // logo a sinistra quando il menù è chiuso
    "NomeUrlLogoS": "Orto Botanico di Pisa", //nome logo a sinistra
    "Demo": true, //utilizzato da template ma escluso dal progetto
      "MessaggioDemo": "Versione dimostrativa di sviluppo locale", //messaggio se vuoi includere il layer dal template originario
      "UrlPrefix": "https://localhost:7038/", //link locahlost con porta
      "UrlBaseCode": "https://localhost:7038/",
      "CustomClaimsIdentityName": "UplantClaims", //variable di test non sfruttata
      "TypeAuth": "WSO2" ////SAML2 WSO2 può avere sia gestione WSO2 rispettando i claim che gli arrivano altrimenti bisogna ritoccare il codice
    },
    "GoogleMap": {
      "Url": "https://maps.google.com/maps/api/js?key=",// url di google maps
      "Key": "AIzaSyBs-7Eb1R1-Napenc2NEUy9kcFBHYhLWC8"// chiave google maps
    },
    "Pathfile": {
      "Basepath": "C:\\immagini", // il path fisico dove mettere le immagini es in windows "C:\\immagini"
      "LimitMaxUpload": "4194304", // limite delle immagini es "4194304"
      "Docs": "C:\\xlsx" // il path fisico dove vengono messi i documenti di estrazione xlsx es path in windows "C:\\xlsx"
    }
  },

  "ConnectionStrings": {
    "UPlant": "" // stringa di connessione del db
  },

  "WSo2Auth": {
    "Name": "", //nome da assegnare al identity per wso2
    "ClientID": "",
    "ClientSecret": "",
    "AuthorizationEndpoint": "", //path endpoint di wso2 es "https://sitoweb.com/oauth2/authorize"
    "TokenEndpoint": "", //path endpoint di wso2 es "https://sitoweb.com/oauth2/token"
    "UserinfoEndpoint": "", //path endpoint di wso2 es "https://sitoweb.com/oauth2/userinfo"
    "CallbackPath": "/signin-oidc",
    "Scope": "openid",
    "Claims": [
      "principal",
      "sub",
      "credential",
      "given_name",
      "family_name",
      "fiscalNumber",
      "email"
    ]
  },
  "Saml2Auth": { // nel caso si utilizzi Saml2 questo è creato basandomi sul mio idp sarà da modificare il program.cs per adattare i claim del IDP
    "Name": "NewUPlant", //nome da assegnare se si vuole al context
    "EntityId": "https://sito.it:7038/Saml2", // entityid che corrisponde al url base del sito qui ci lascio un esempio
    "IdpEntity": "https://sitoidp.it/idp/shibboleth", // url dove trovare il metadata del IPD esempio https://sitoidp.it/idp/shibboleth o  https://sitoidp.it/idp/shibboleth/medata.xml
    "typeclaim_codicefiscale": "", // ho tirato fuori il type che shibboleth ritorna che nel mio caso si base solo codice fiscale e uid come sono questi due variabili ma potrebbero essere di più
    "typeclaim_udi": "", //
    "SigningCertificateFile": "Sutests.pfx",// file messo nella stessa posizione del appsetting ma può essere spostato del certificato di autenticazione con IDP per avere la validazione la configurazione del IPD deve essere forzata ad un algoritmo di cryptazione vecchio perchè windows non supporta AES-CGM
    "SigningCertificatePassword": ""// password del certificato quando si crea pfx si può proteggere con password che può essere settata qui
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  
  "AllowedHosts": "*"
}

Una volta settato il file dobbiamo testare l'autenticazione all'interno della tabella Users utilizzando autenticatore bisogna inserire nella tabella User nel campo UserName il valore identificativo che passa l'autenticatore

Ci sono dei passaggi per l'installazione su IIS uno di questi è creare Webdeploy che se non è installato bisogna scaricarlo e installarlo per uplodare il progetto, ora è alla versione 4. https://www.microsoft.com/en-us/download/details.aspx?id=106070
In più il  runtime del dotnet8 https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-8.0.11-windows-hosting-bundle-installer 

Creiamo il sito dentro IIS
lo chiamamo come più ci aggrada nell'esempio lo chiameremo 
-->uplant
Dobbiamo creare un application Pools da abbinare al sito sono importanti due configurazioni la prima in generale per tutti è settare l'attributo .NET CLR Version = NO Managed Code, in più nel caso dovessimo utilizzare l'autenticazione SAML selezioneremo per quel pool in advanced setting il Load User Profile  = True
Nel Binding del sito andiamo a settare i vari campi ip address port host, il require server name indication checked e il certificato ssl abbiato che abbiamo creato precedentemente
Se dobbiamo autenticare il programma con SAML2 andremo in appsetting a settare la variaile typeauth ="SAML2"
Fatto questo setteremo in appsetting
"EntityId": "https://uplant.sito.it/Saml2", // entityid che corrisponde al url base del sito  quello che useremo pre creare il metadata
"IdpEntity": "https://shibidp.sito.it/idp/shibboleth // url dove trovare il metadata del IPD esempio https://sitoidp.it/idp/shibboleth
"typeclaim_codicefiscale": "", // ho tirato fuori il type che shibboleth ritorna che nel mio caso si base solo codice fiscale e uid come sono questi due variabili ma potrebbero essere di più
"typeclaim_udi": "", //
"SigningCertificateFile": "esempio.pfx",// file messo nella stessa posizione del appsetting ma può essere spostato del certificato di autenticazione con IDP per avere la validazione la configurazione del IPD deve essere forzata ad un algoritmo di cryptazione vecchio perchè windows non supporta AES-CGM
    "SigningCertificatePassword": "password"/
Per creare il certificato che poi verrà passato nel metadata possiamo avvalerci del server certificates di IIS

possiamo seguire questa guida

To generate a certificate using Windows Internet Information Services (IIS):

In the IIS Manager, navigate to the Features view and double-click Server Certificates.
In the Actions pane, click Create Self-Signed Certificate
On the Create Self-Signed Certificate page, specify a name for the certificate, and then click OK.
The certificate will now be listed on the Server Certificates page. Select the new certificate and click Export in the Actions pane.
Select a directory to export the certificate to and enter a password for the certificate.

Esportato il file nella directory principale del sito andiamo sul appsetting e settiamo il nome del certificato pfx e la password

