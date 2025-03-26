USE [ServiceCenter1]
GO

/****** Object:  Table [dbo].[Requests]    Script Date: 26.03.2025 7:47:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Requests](
	[RequestID] [int] IDENTITY(1,1) NOT NULL,
	[DeviceName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[RequestDate] [datetime] NOT NULL,
	[Phone] [nvarchar](50) NOT NULL,
	[ClientID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RequestID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Requests]  WITH CHECK ADD  CONSTRAINT [FK_Requests_Clients1] FOREIGN KEY([ClientID])
REFERENCES [dbo].[Clients] ([ClientID])
GO

ALTER TABLE [dbo].[Requests] CHECK CONSTRAINT [FK_Requests_Clients1]
GO

