/****** Object:  Table [dbo].[TipoPrioritaAlberi]    Script Date: 28/01/2026 13:11:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoPrioritaAlberi](
	[id] [uniqueidentifier] NOT NULL,
	[descrizione] [varchar](max) NOT NULL,
	[descrizione_en] [varchar](max) NULL,
	[organizzazione] [uniqueidentifier] NOT NULL,
	[ordinamento] [int] NOT NULL,
	[livello] [int] NOT NULL,
 CONSTRAINT [PK_PrioritaAlberi] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[TipoPrioritaAlberi] ADD  CONSTRAINT [DF_PrioritaAlberi_id]  DEFAULT (newid()) FOR [id]
GO
ALTER TABLE [dbo].[TipoPrioritaAlberi]  WITH CHECK ADD  CONSTRAINT [FK_TipoPrioritaAlberi_Organizzazioni] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[TipoPrioritaAlberi] CHECK CONSTRAINT [FK_TipoPrioritaAlberi_Organizzazioni]
GO
INSERT [dbo].[TipoPrioritaAlberi] ([id], [descrizione], [descrizione_en], [organizzazione], [ordinamento], [livello]) VALUES (N'fa5a2095-052f-4f2c-ae5c-2343605d1a34', N'Non Definito', N'Not Defined', N'05c0b59a-65ab-4dc4-be25-8a4e3b734587', 0, 0)
GO
