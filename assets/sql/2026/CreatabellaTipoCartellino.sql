/****** Object:  Table [dbo].[TipoCartellino]    Script Date: 28/01/2026 13:00:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TipoCartellino](
	[id] [uniqueidentifier] NOT NULL,
	[descrizione] [varchar](500) NOT NULL,
	[descrizione_en] [varchar](500) NULL,
	[ordinamento] [int] NOT NULL,
 CONSTRAINT [PK_TipoCartellino] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT [dbo].[TipoCartellino] ([id], [descrizione], [descrizione_en], [ordinamento]) VALUES (N'61428fdf-2774-4916-9d9e-3746b4a2f23c', N'Non Definito', N'Not Defined', 0)
GO