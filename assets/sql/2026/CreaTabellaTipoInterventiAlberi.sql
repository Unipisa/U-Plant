GO

CREATE TABLE [dbo].[TipoInterventiAlberi](
	[id] [uniqueidentifier] NOT NULL,
	[descrizione] [varchar](max) NOT NULL,
	[descrizione_en] [varchar](max) NULL,
	[organizzazione] [uniqueidentifier] NOT NULL,
	[ordinamento] [int] NOT NULL,
 CONSTRAINT [PK_InterventiAlberi] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[TipoInterventiAlberi] ADD  CONSTRAINT [DF_InterventiAlberi_id]  DEFAULT (newid()) FOR [id]
GO

ALTER TABLE [dbo].[TipoInterventiAlberi]  WITH CHECK ADD  CONSTRAINT [FK_TipoInterventiAlberi_Organizzazioni] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO

ALTER TABLE [dbo].[TipoInterventiAlberi] CHECK CONSTRAINT [FK_TipoInterventiAlberi_Organizzazioni]
GO


