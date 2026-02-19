/****** Object:  Table [dbo].[InterventiAlberi]    Script Date: 28/01/2026 13:10:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InterventiAlberi](
	[id] [uniqueidentifier] NOT NULL,
	[individuo] [uniqueidentifier] NOT NULL,
	[dataapertura] [datetime] NOT NULL,
	[priorita] [uniqueidentifier] NOT NULL,
	[intervento] [uniqueidentifier] NOT NULL,
	[fornitore] [uniqueidentifier] NOT NULL,
	[motivo] [varchar](max) NOT NULL,
	[esitointervento] [varchar](max) NULL,
	[statoIntervento] [bit] NOT NULL,
	[dataultimamodifica] [datetime] NULL,
	[utenteapertura] [uniqueidentifier] NOT NULL,
	[utenteultimamodifica] [uniqueidentifier] NOT NULL,
	[statoIndividuo] [uniqueidentifier] NOT NULL,
	[condizione] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Alberi] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[InterventiAlberi] ADD  CONSTRAINT [DF_Alberi_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[InterventiAlberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_Fornitori] FOREIGN KEY([fornitore])
REFERENCES [dbo].[Fornitori] ([id])
GO
ALTER TABLE [dbo].[InterventiAlberi] CHECK CONSTRAINT [FK_Alberi_Fornitori]
GO
ALTER TABLE [dbo].[InterventiAlberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_Individui] FOREIGN KEY([individuo])
REFERENCES [dbo].[Individui] ([id])
GO
ALTER TABLE [dbo].[InterventiAlberi] CHECK CONSTRAINT [FK_Alberi_Individui]
GO
ALTER TABLE [dbo].[InterventiAlberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_TipoInterventiAlberi] FOREIGN KEY([intervento])
REFERENCES [dbo].[TipoInterventiAlberi] ([id])
GO
ALTER TABLE [dbo].[InterventiAlberi] CHECK CONSTRAINT [FK_Alberi_TipoInterventiAlberi]
GO
ALTER TABLE [dbo].[InterventiAlberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_TipoPrioritaAlberi] FOREIGN KEY([priorita])
REFERENCES [dbo].[TipoPrioritaAlberi] ([id])
GO
ALTER TABLE [dbo].[InterventiAlberi] CHECK CONSTRAINT [FK_Alberi_TipoPrioritaAlberi]
GO
ALTER TABLE [dbo].[InterventiAlberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_UsersApertura] FOREIGN KEY([utenteapertura])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[InterventiAlberi] CHECK CONSTRAINT [FK_Alberi_UsersApertura]
GO
ALTER TABLE [dbo].[InterventiAlberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_UsersUltima] FOREIGN KEY([utenteultimamodifica])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[InterventiAlberi] CHECK CONSTRAINT [FK_Alberi_UsersUltima]
GO
ALTER TABLE [dbo].[InterventiAlberi]  WITH CHECK ADD  CONSTRAINT [FK_InterventiAlberi_Condizioni] FOREIGN KEY([condizione])
REFERENCES [dbo].[Condizioni] ([id])
GO
ALTER TABLE [dbo].[InterventiAlberi] CHECK CONSTRAINT [FK_InterventiAlberi_Condizioni]
GO
ALTER TABLE [dbo].[InterventiAlberi]  WITH CHECK ADD  CONSTRAINT [FK_InterventiAlberi_StatoIndividuo] FOREIGN KEY([statoIndividuo])
REFERENCES [dbo].[StatoIndividuo] ([id])
GO
ALTER TABLE [dbo].[InterventiAlberi] CHECK CONSTRAINT [FK_InterventiAlberi_StatoIndividuo]
GO

INSERT [dbo].[TipoInterventiAlberi] ([id], [descrizione], [descrizione_en], [organizzazione], [ordinamento]) VALUES (N'6cda01e7-afe8-4140-8731-e758d9876f9d', N'Non Definito', N'Not Defined', N'05c0b59a-65ab-4dc4-be25-8a4e3b734587', 0)
GO