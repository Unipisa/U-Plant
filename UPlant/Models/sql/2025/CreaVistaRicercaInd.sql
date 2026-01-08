SELECT        TOP (100) PERCENT s.individuo AS id, MAX(a.progressivo) AS progressivoacc, MAX(s.dataInserimento) AS datainserimento, MAX(i.propagatoData) AS datapropagazione, MAX(i.progressivo) AS progressivo, 
                         MAX(i.vecchioprogressivo) AS vecchioprogressivo, MAX(sp.nome_scientifico) AS nome_scientifico, MAX(st.id) AS idsettore, MAX(st.settore) AS settore, MAX(st.settore_en) AS settore_en, MAX(c.id) AS idcollezione, 
                         MAX(c.collezione) AS collezione, MAX(c.collezione_en) AS collezione_en, MAX(ca.id) AS idcartellino, MAX(ca.descrizione) AS cartellino, MAX(ca.descrizione_en) AS cartellino_en, MAX(s.statoIndividuo) AS idstatoindividuo, 
                         MAX(sta.stato) AS statoindividuo, MAX(sta.descrizione_en) AS statoindividuo_en, MAX(u.Name) + ' ' + MAX(u.LastName) AS nomecognome, MAX(im.id) AS immagine, MAX(ge.descrizione) AS genere, MAX(fa.id) AS idfamiglia, 
                         MAX(fa.descrizione) AS famiglia, MAX(co.id) AS idcondizione, MAX(co.condizione) AS condizione, MAX(i.propagatoData) AS propagatodata, MAX(a.ipen) AS ipen, MAX(sp.nome) AS nome, MAX(sp.subspecie) AS subspecie, 
                         MAX(sp.varieta) AS varieta, MAX(sp.cult) AS cult,
                             (SELECT        COUNT(*) AS Expr1
                               FROM            dbo.ImmaginiIndividuo AS i
                               WHERE        (individuo = s.individuo)) AS numero_immagini
FROM            dbo.StoricoIndividuo AS s LEFT OUTER JOIN
                         dbo.Individui AS i ON s.individuo = i.id LEFT OUTER JOIN
                         dbo.Accessioni AS a ON i.accessione = a.id LEFT OUTER JOIN
                         dbo.Specie AS sp ON a.specie = sp.id LEFT OUTER JOIN
                         dbo.Settori AS st ON i.settore = st.id LEFT OUTER JOIN
                         dbo.Collezioni AS c ON i.collezione = c.id LEFT OUTER JOIN
                         dbo.Cartellini AS ca ON i.cartellino = ca.id LEFT OUTER JOIN
                         dbo.StatoIndividuo AS sta ON sta.id = s.statoIndividuo LEFT OUTER JOIN
                         dbo.Users AS u ON u.Id = s.utente LEFT OUTER JOIN
                         dbo.ImmaginiIndividuo AS im ON im.individuo = i.id LEFT OUTER JOIN
                         dbo.Generi AS ge ON ge.id = sp.genere LEFT OUTER JOIN
                         dbo.Famiglie AS fa ON fa.id = ge.famiglia LEFT OUTER JOIN
                         dbo.Condizioni AS co ON co.id = s.condizione
GROUP BY s.individuo, s.dataInserimento
HAVING        (s.dataInserimento =
                             (SELECT        MAX(dataInserimento) AS Expr1
                               FROM            dbo.StoricoIndividuo
                               WHERE        (individuo = s.individuo)))