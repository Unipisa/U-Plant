SELECT        a.id AS idacc, a.progressivo, a.vecchioprogressivo, sp.nome_scientifico, fa.id AS idfamiglia, fa.descrizione AS famiglia, ge.descrizione AS genere, a.dataAcquisizione, a.tipoMateriale AS idtipomateriale, 
                         tm.descrizione AS tipomateriale, tm.descrizione_en AS tipomateriale_en, a.tipoAcquisizione AS idtipoacquisizione, ta.descrizione AS tipoacquisizione, ta.descrizione_en AS tipoacquisizione_en, 
                         a.gradoIncertezza AS idgradoincertezza, gi.descrizione AS gradoincertezza, gi.descrizione_en AS gradoincertezza_en, a.utenteAcquisizione AS idinseritoda, u.Name + ' ' + u.LastName AS inseritoda, 
                         a.utenteUltimaModifica AS idmodificatoda, uu.Name + ' ' + uu.LastName AS modificatoda, a.fornitore AS idfornitore, f.descrizione AS fornitore, a.raccoglitore AS idraccoglitore, r.nominativo AS raccoglitore, a.validazione,
                             (SELECT        COUNT(*) AS Expr1
                               FROM            dbo.Individui AS i
                               WHERE        (accessione = a.id)) AS numero_individui
FROM            dbo.Accessioni AS a LEFT OUTER JOIN
                         dbo.Specie AS sp ON a.specie = sp.id LEFT OUTER JOIN
                         dbo.Generi AS ge ON ge.id = sp.genere LEFT OUTER JOIN
                         dbo.Famiglie AS fa ON fa.id = ge.famiglia LEFT OUTER JOIN
                         dbo.TipiMateriale AS tm ON a.tipoMateriale = tm.id LEFT OUTER JOIN
                         dbo.TipoAcquisizione AS ta ON a.tipoAcquisizione = ta.id LEFT OUTER JOIN
                         dbo.Users AS u ON u.Id = a.utenteAcquisizione LEFT OUTER JOIN
                         dbo.Users AS uu ON uu.Id = a.utenteUltimaModifica LEFT OUTER JOIN
                         dbo.Fornitori AS f ON f.id = a.fornitore LEFT OUTER JOIN
                         dbo.Raccoglitori AS r ON r.id = a.raccoglitore LEFT OUTER JOIN
                         dbo.GradoIncertezza AS gi ON gi.id = a.gradoIncertezza