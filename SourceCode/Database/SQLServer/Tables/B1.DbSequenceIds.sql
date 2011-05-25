

CREATE TABLE [B1].[TestDbSequenceIds](
	[DbSequenceId] [bigint] IDENTITY(1,1) NOT NULL,
	[Remarks] [varchar](100) NULL,
 CONSTRAINT [PK_SeqTable] PRIMARY KEY CLUSTERED 
(
	[DbSequenceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO