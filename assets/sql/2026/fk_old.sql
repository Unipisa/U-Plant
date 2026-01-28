ALTER TABLE [dbo].[Accessioni] ADD  CONSTRAINT [DF_Accessioni_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Areali] ADD  CONSTRAINT [DF_Areali_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Cartellini] ADD  CONSTRAINT [DF_Cartellini_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Cites] ADD  CONSTRAINT [DF_Cites_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Collezioni] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Condizioni] ADD  CONSTRAINT [DF_Condizioni_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Famiglie] ADD  CONSTRAINT [DF_Famiglie_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Fornitori] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Generi] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[GradoIncertezza] ADD  CONSTRAINT [DF_GradoIncertezza_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Identificatori] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[ImmaginiIndividuo] ADD  CONSTRAINT [DF_ImmaginiIndividuo_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Individui] ADD  CONSTRAINT [DF_Individui_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[IndividuiPercorso] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Iucn] ADD  CONSTRAINT [DF_Iucn_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[ModalitaPropagazione] ADD  CONSTRAINT [DF__ModalitaProp__id__0C85DE4D]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Organizzazioni] ADD  CONSTRAINT [DF_Organizzazioni_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Percorsi] ADD  CONSTRAINT [DF_Percorsi_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Provenienze] ADD  CONSTRAINT [DF__Provenienze__id__0E6E26BF]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Raccoglitori] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Regni] ADD  CONSTRAINT [DF_Regni_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Roles] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[Sesso] ADD  CONSTRAINT [DF__Sesso__id__1332DBDC]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Settori] ADD  CONSTRAINT [DF__Settori__id__151B244E]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[Specie] ADD  CONSTRAINT [DF_Specie_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[StatoIndividuo] ADD  CONSTRAINT [DF__StatoIndivid__id__1CBC4616]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[StatoMateriale] ADD  CONSTRAINT [DF_StatoMateriale_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[StoricoIndividuo] ADD  CONSTRAINT [DF_StoricoIndividuo_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[TipiMateriale] ADD  CONSTRAINT [DF__TipiMaterial__id__236943A5]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[TipoAcquisizione] ADD  CONSTRAINT [DF_TipoAcquisizione_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[TipoIdentificatore] ADD  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[TipologiaUtente] ADD  CONSTRAINT [DF_TipologiaUtente_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[UserRole] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF__Users__Id__6C190EBB]  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF__Users__CreatedAt__6D0D32F4]  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_fornitore] FOREIGN KEY([fornitore])
REFERENCES [dbo].[Fornitori] ([id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_fornitore]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_GradoIncertezza] FOREIGN KEY([gradoIncertezza])
REFERENCES [dbo].[GradoIncertezza] ([id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_GradoIncertezza]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_Identificatore] FOREIGN KEY([identificatore])
REFERENCES [dbo].[Identificatori] ([id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_Identificatore]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_Nazione] FOREIGN KEY([nazione])
REFERENCES [dbo].[Nazioni] ([codice])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_Nazione]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_Organizzazione]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_Provenienza] FOREIGN KEY([provenienza])
REFERENCES [dbo].[Provenienze] ([id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_Provenienza]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_Provincia] FOREIGN KEY([provincia])
REFERENCES [dbo].[Province] ([codice])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_Provincia]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_Raccoglitore] FOREIGN KEY([raccoglitore])
REFERENCES [dbo].[Raccoglitori] ([id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_Raccoglitore]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_Regione] FOREIGN KEY([regione])
REFERENCES [dbo].[Regioni] ([codice])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_Regione]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_Specie] FOREIGN KEY([specie])
REFERENCES [dbo].[Specie] ([id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_Specie]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_StatoMateriale] FOREIGN KEY([statoMateriale])
REFERENCES [dbo].[StatoMateriale] ([id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_StatoMateriale]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_TipoAcquisizione] FOREIGN KEY([tipoAcquisizione])
REFERENCES [dbo].[TipoAcquisizione] ([id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_TipoAcquisizione]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_TipoMateriale] FOREIGN KEY([tipoMateriale])
REFERENCES [dbo].[TipiMateriale] ([id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_TipoMateriale]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_UtentiInserimento] FOREIGN KEY([utenteAcquisizione])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_UtentiInserimento]
GO
ALTER TABLE [dbo].[Accessioni]  WITH CHECK ADD  CONSTRAINT [FK_Accessioni_UtentiModifica] FOREIGN KEY([utenteUltimaModifica])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Accessioni] CHECK CONSTRAINT [FK_Accessioni_UtentiModifica]
GO
ALTER TABLE [dbo].[Cartellini]  WITH CHECK ADD  CONSTRAINT [FK_Cartellini_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Cartellini] CHECK CONSTRAINT [FK_Cartellini_Organizzazione]
GO
ALTER TABLE [dbo].[Collezioni]  WITH CHECK ADD  CONSTRAINT [FK_Collezioni_Organizzazioni] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Collezioni] CHECK CONSTRAINT [FK_Collezioni_Organizzazioni]
GO
ALTER TABLE [dbo].[Collezioni]  WITH CHECK ADD  CONSTRAINT [FK_Collezioni_Settori] FOREIGN KEY([settore])
REFERENCES [dbo].[Settori] ([id])
GO
ALTER TABLE [dbo].[Collezioni] CHECK CONSTRAINT [FK_Collezioni_Settori]
GO
ALTER TABLE [dbo].[Condizioni]  WITH CHECK ADD  CONSTRAINT [FK_Condizioni_Organizzazioni] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Condizioni] CHECK CONSTRAINT [FK_Condizioni_Organizzazioni]
GO
ALTER TABLE [dbo].[Fornitori]  WITH CHECK ADD  CONSTRAINT [FK_Fornitori_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Fornitori] CHECK CONSTRAINT [FK_Fornitori_Organizzazione]
GO
ALTER TABLE [dbo].[Generi]  WITH CHECK ADD  CONSTRAINT [FK_Genere_Famiglia] FOREIGN KEY([famiglia])
REFERENCES [dbo].[Famiglie] ([id])
GO
ALTER TABLE [dbo].[Generi] CHECK CONSTRAINT [FK_Genere_Famiglia]
GO
ALTER TABLE [dbo].[GradoIncertezza]  WITH CHECK ADD  CONSTRAINT [FK_GradoIncertezza_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[GradoIncertezza] CHECK CONSTRAINT [FK_GradoIncertezza_Organizzazione]
GO
ALTER TABLE [dbo].[Identificatori]  WITH CHECK ADD  CONSTRAINT [FK_Identificatori_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Identificatori] CHECK CONSTRAINT [FK_Identificatori_Organizzazione]
GO
ALTER TABLE [dbo].[Identificatori]  WITH CHECK ADD  CONSTRAINT [FK_Identificatori_Tipo] FOREIGN KEY([tipoIdentificatore])
REFERENCES [dbo].[TipoIdentificatore] ([id])
GO
ALTER TABLE [dbo].[Identificatori] CHECK CONSTRAINT [FK_Identificatori_Tipo]
GO
ALTER TABLE [dbo].[ImmaginiIndividuo]  WITH CHECK ADD  CONSTRAINT [FK_ImmaginiIndividuo_Individui] FOREIGN KEY([individuo])
REFERENCES [dbo].[Individui] ([id])
GO
ALTER TABLE [dbo].[ImmaginiIndividuo] CHECK CONSTRAINT [FK_ImmaginiIndividuo_Individui]
GO
ALTER TABLE [dbo].[Individui]  WITH CHECK ADD  CONSTRAINT [FK_Individui_Accessioni] FOREIGN KEY([accessione])
REFERENCES [dbo].[Accessioni] ([id])
GO
ALTER TABLE [dbo].[Individui] CHECK CONSTRAINT [FK_Individui_Accessioni]
GO
ALTER TABLE [dbo].[Individui]  WITH CHECK ADD  CONSTRAINT [FK_Individui_Cartellini] FOREIGN KEY([cartellino])
REFERENCES [dbo].[Cartellini] ([id])
GO
ALTER TABLE [dbo].[Individui] CHECK CONSTRAINT [FK_Individui_Cartellini]
GO
ALTER TABLE [dbo].[Individui]  WITH CHECK ADD  CONSTRAINT [FK_Individui_Collezioni] FOREIGN KEY([collezione])
REFERENCES [dbo].[Collezioni] ([id])
GO
ALTER TABLE [dbo].[Individui] CHECK CONSTRAINT [FK_Individui_Collezioni]
GO
ALTER TABLE [dbo].[Individui]  WITH CHECK ADD  CONSTRAINT [FK_Individui_ModalitaPropagazione] FOREIGN KEY([propagatoModalita])
REFERENCES [dbo].[ModalitaPropagazione] ([id])
GO
ALTER TABLE [dbo].[Individui] CHECK CONSTRAINT [FK_Individui_ModalitaPropagazione]
GO
ALTER TABLE [dbo].[Individui]  WITH CHECK ADD  CONSTRAINT [FK_Individui_Sesso] FOREIGN KEY([sesso])
REFERENCES [dbo].[Sesso] ([id])
GO
ALTER TABLE [dbo].[Individui] CHECK CONSTRAINT [FK_Individui_Sesso]
GO
ALTER TABLE [dbo].[Individui]  WITH CHECK ADD  CONSTRAINT [FK_Individui_Settori] FOREIGN KEY([settore])
REFERENCES [dbo].[Settori] ([id])
GO
ALTER TABLE [dbo].[Individui] CHECK CONSTRAINT [FK_Individui_Settori]
GO
ALTER TABLE [dbo].[IndividuiPercorso]  WITH CHECK ADD  CONSTRAINT [FK_IndividuiPercorso_Individui] FOREIGN KEY([individuo])
REFERENCES [dbo].[Individui] ([id])
GO
ALTER TABLE [dbo].[IndividuiPercorso] CHECK CONSTRAINT [FK_IndividuiPercorso_Individui]
GO
ALTER TABLE [dbo].[IndividuiPercorso]  WITH CHECK ADD  CONSTRAINT [FK_IndividuiPercorso_Percorsi] FOREIGN KEY([percorso])
REFERENCES [dbo].[Percorsi] ([id])
GO
ALTER TABLE [dbo].[IndividuiPercorso] CHECK CONSTRAINT [FK_IndividuiPercorso_Percorsi]
GO
ALTER TABLE [dbo].[ModalitaPropagazione]  WITH CHECK ADD  CONSTRAINT [FK_ModalitaPropagazione_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[ModalitaPropagazione] CHECK CONSTRAINT [FK_ModalitaPropagazione_Organizzazione]
GO
ALTER TABLE [dbo].[Provenienze]  WITH CHECK ADD  CONSTRAINT [FK_Provenienze_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Provenienze] CHECK CONSTRAINT [FK_Provenienze_Organizzazione]
GO
ALTER TABLE [dbo].[Province]  WITH CHECK ADD  CONSTRAINT [FK_Province_Regione] FOREIGN KEY([regione])
REFERENCES [dbo].[Regioni] ([codice])
GO
ALTER TABLE [dbo].[Province] CHECK CONSTRAINT [FK_Province_Regione]
GO
ALTER TABLE [dbo].[Raccoglitori]  WITH CHECK ADD  CONSTRAINT [FK_Raccoglitori_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Raccoglitori] CHECK CONSTRAINT [FK_Raccoglitori_Organizzazione]
GO
ALTER TABLE [dbo].[Sesso]  WITH CHECK ADD  CONSTRAINT [FK_Sesso_Organizzazioni] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Sesso] CHECK CONSTRAINT [FK_Sesso_Organizzazioni]
GO
ALTER TABLE [dbo].[Settori]  WITH CHECK ADD  CONSTRAINT [FK_Settori_Organizzazioni] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Settori] CHECK CONSTRAINT [FK_Settori_Organizzazioni]
GO
ALTER TABLE [dbo].[Specie]  WITH CHECK ADD  CONSTRAINT [FK_Specie_Areale] FOREIGN KEY([areale])
REFERENCES [dbo].[Areali] ([id])
GO
ALTER TABLE [dbo].[Specie] CHECK CONSTRAINT [FK_Specie_Areale]
GO
ALTER TABLE [dbo].[Specie]  WITH CHECK ADD  CONSTRAINT [FK_Specie_Cites] FOREIGN KEY([cites])
REFERENCES [dbo].[Cites] ([id])
GO
ALTER TABLE [dbo].[Specie] CHECK CONSTRAINT [FK_Specie_Cites]
GO
ALTER TABLE [dbo].[Specie]  WITH CHECK ADD  CONSTRAINT [FK_Specie_Genere] FOREIGN KEY([genere])
REFERENCES [dbo].[Generi] ([id])
GO
ALTER TABLE [dbo].[Specie] CHECK CONSTRAINT [FK_Specie_Genere]
GO
ALTER TABLE [dbo].[Specie]  WITH CHECK ADD  CONSTRAINT [FK_Specie_IucnGlobale] FOREIGN KEY([iucn_globale])
REFERENCES [dbo].[Iucn] ([id])
GO
ALTER TABLE [dbo].[Specie] CHECK CONSTRAINT [FK_Specie_IucnGlobale]
GO
ALTER TABLE [dbo].[Specie]  WITH CHECK ADD  CONSTRAINT [FK_Specie_IucnItalia] FOREIGN KEY([iucn_italia])
REFERENCES [dbo].[Iucn] ([id])
GO
ALTER TABLE [dbo].[Specie] CHECK CONSTRAINT [FK_Specie_IucnItalia]
GO
ALTER TABLE [dbo].[Specie]  WITH CHECK ADD  CONSTRAINT [FK_Specie_Regno] FOREIGN KEY([regno])
REFERENCES [dbo].[Regni] ([id])
GO
ALTER TABLE [dbo].[Specie] CHECK CONSTRAINT [FK_Specie_Regno]
GO
ALTER TABLE [dbo].[StatoIndividuo]  WITH CHECK ADD  CONSTRAINT [FK_StatoIndividuo_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[StatoIndividuo] CHECK CONSTRAINT [FK_StatoIndividuo_Organizzazione]
GO
ALTER TABLE [dbo].[StatoMateriale]  WITH CHECK ADD  CONSTRAINT [FK_StatoMateriale_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[StatoMateriale] CHECK CONSTRAINT [FK_StatoMateriale_Organizzazione]
GO
ALTER TABLE [dbo].[StoricoIndividuo]  WITH CHECK ADD  CONSTRAINT [FK_StoricoIndividuo_Condizioni] FOREIGN KEY([condizione])
REFERENCES [dbo].[Condizioni] ([id])
GO
ALTER TABLE [dbo].[StoricoIndividuo] CHECK CONSTRAINT [FK_StoricoIndividuo_Condizioni]
GO
ALTER TABLE [dbo].[StoricoIndividuo]  WITH CHECK ADD  CONSTRAINT [FK_StoricoIndividuo_Individui] FOREIGN KEY([individuo])
REFERENCES [dbo].[Individui] ([id])
GO
ALTER TABLE [dbo].[StoricoIndividuo] CHECK CONSTRAINT [FK_StoricoIndividuo_Individui]
GO
ALTER TABLE [dbo].[StoricoIndividuo]  WITH CHECK ADD  CONSTRAINT [FK_StoricoIndividuo_StatoIndividuo] FOREIGN KEY([statoIndividuo])
REFERENCES [dbo].[StatoIndividuo] ([id])
GO
ALTER TABLE [dbo].[StoricoIndividuo] CHECK CONSTRAINT [FK_StoricoIndividuo_StatoIndividuo]
GO
ALTER TABLE [dbo].[StoricoIndividuo]  WITH CHECK ADD  CONSTRAINT [FK_StoricoIndividuo_Users] FOREIGN KEY([utente])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[StoricoIndividuo] CHECK CONSTRAINT [FK_StoricoIndividuo_Users]
GO
ALTER TABLE [dbo].[TipiMateriale]  WITH CHECK ADD  CONSTRAINT [FK_TipiMateriale_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[TipiMateriale] CHECK CONSTRAINT [FK_TipiMateriale_Organizzazione]
GO
ALTER TABLE [dbo].[TipoAcquisizione]  WITH CHECK ADD  CONSTRAINT [FK_TipoAcquisizione_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[TipoAcquisizione] CHECK CONSTRAINT [FK_TipoAcquisizione_Organizzazione]
GO
ALTER TABLE [dbo].[TipoIdentificatore]  WITH CHECK ADD  CONSTRAINT [FK_TipoVerificatore_Organizzazione] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[TipoIdentificatore] CHECK CONSTRAINT [FK_TipoVerificatore_Organizzazione]
GO
ALTER TABLE [dbo].[TipologiaUtente]  WITH CHECK ADD  CONSTRAINT [FK_TipologiaUtente_Organizzazioni] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[TipologiaUtente] CHECK CONSTRAINT [FK_TipologiaUtente_Organizzazioni]
GO
ALTER TABLE [dbo].[UserRole]  WITH CHECK ADD FOREIGN KEY([RoleFK])
REFERENCES [dbo].[Roles] ([Id])
GO
ALTER TABLE [dbo].[UserRole]  WITH CHECK ADD  CONSTRAINT [FK__UserRole__UserFK__2739D489] FOREIGN KEY([UserFK])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[UserRole] CHECK CONSTRAINT [FK__UserRole__UserFK__2739D489]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Organizzazioni] FOREIGN KEY([Organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_Organizzazioni]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_TipologiaUtente] FOREIGN KEY([TipologiaUtente])
REFERENCES [dbo].[TipologiaUtente] ([id])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_TipologiaUtente]