CREATE TABLE [dbo].[TipoPrioritaAlberi](
	[id] [uniqueidentifier] NOT NULL,
	[descrizione] [varchar](max) NOT NULL,
	[descrizione_en] [varchar](max) NULL,
	[organizzazione] [uniqueidentifier] NOT NULL,
	[ordinamento] [int] NOT NULL,
 CONSTRAINT [PK_PrioritaAlberi] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[TipoPrioritaAlberi] ADD  CONSTRAINT [DF_PrioritaAlberi_id]  DEFAULT (newid()) FOR [id]
GO

ALTER TABLE [dbo].[TipoPrioritaAlberi]  WITH CHECK ADD  CONSTRAINT [FK_TipoPrioritaAlberi_Organizzazioni] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO

ALTER TABLE [dbo].[TipoPrioritaAlberi] CHECK CONSTRAINT [FK_TipoPrioritaAlberi_Organizzazioni]
GO