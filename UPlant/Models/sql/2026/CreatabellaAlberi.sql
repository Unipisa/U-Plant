SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Alberi](
	[id] [uniqueidentifier] NOT NULL,
	[individuo] [uniqueidentifier] NOT NULL,
	[dataapertura] [datetime] NOT NULL,
	[priorita] [uniqueidentifier] NOT NULL,
	[intervento] [uniqueidentifier] NOT NULL,
	[fornitore] [uniqueidentifier] NOT NULL,
	[motivo] [varchar](max) NOT NULL,
	[esitointervento] [varchar](max) NULL,
	[stato] [bit] NOT NULL,
	[datachiusura] [datetime] NULL,
	[utenteapertura] [uniqueidentifier] NOT NULL,
	[utenteultimamodifica] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Alberi] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Alberi] ADD  CONSTRAINT [DF_Alberi_id]  DEFAULT (newid()) FOR [id]
GO

ALTER TABLE [dbo].[Alberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_Fornitori] FOREIGN KEY([fornitore])
REFERENCES [dbo].[Fornitori] ([id])
GO

ALTER TABLE [dbo].[Alberi] CHECK CONSTRAINT [FK_Alberi_Fornitori]
GO

ALTER TABLE [dbo].[Alberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_Individui] FOREIGN KEY([individuo])
REFERENCES [dbo].[Individui] ([id])
GO

ALTER TABLE [dbo].[Alberi] CHECK CONSTRAINT [FK_Alberi_Individui]
GO

ALTER TABLE [dbo].[Alberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_TipoInterventiAlberi] FOREIGN KEY([intervento])
REFERENCES [dbo].[TipoInterventiAlberi] ([id])
GO

ALTER TABLE [dbo].[Alberi] CHECK CONSTRAINT [FK_Alberi_TipoInterventiAlberi]
GO

ALTER TABLE [dbo].[Alberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_TipoPrioritaAlberi] FOREIGN KEY([priorita])
REFERENCES [dbo].[TipoPrioritaAlberi] ([id])
GO

ALTER TABLE [dbo].[Alberi] CHECK CONSTRAINT [FK_Alberi_TipoPrioritaAlberi]
GO

ALTER TABLE [dbo].[Alberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_UsersApertura] FOREIGN KEY([utenteapertura])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Alberi] CHECK CONSTRAINT [FK_Alberi_UsersApertura]
GO

ALTER TABLE [dbo].[Alberi]  WITH CHECK ADD  CONSTRAINT [FK_Alberi_UsersUltima] FOREIGN KEY([utenteultimamodifica])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[Alberi] CHECK CONSTRAINT [FK_Alberi_UsersUltima]
GO