/****** Object:  Table [dbo].[TipoInterventiAlberi]    Script Date: 28/01/2026 13:11:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[TipoInterventiAlberi] ADD  CONSTRAINT [DF_InterventiAlberi_id]  DEFAULT (newid()) FOR [id]
GO

ALTER TABLE [dbo].[TipoInterventiAlberi]  WITH CHECK ADD  CONSTRAINT [FK_TipoInterventiAlberi_Organizzazioni] FOREIGN KEY([organizzazione])
REFERENCES [dbo].[Organizzazioni] ([id])
GO
ALTER TABLE [dbo].[TipoInterventiAlberi] CHECK CONSTRAINT [FK_TipoInterventiAlberi_Organizzazioni]
GO

INSERT [dbo].[TipoInterventiAlberi] ([id], [descrizione], [descrizione_en], [organizzazione], [ordinamento]) VALUES (N'6cda01e7-afe8-4140-8731-e758d9876f9d', N'Non Definito', N'Not Defined', N'05c0b59a-65ab-4dc4-be25-8a4e3b734587', 0)
GO
