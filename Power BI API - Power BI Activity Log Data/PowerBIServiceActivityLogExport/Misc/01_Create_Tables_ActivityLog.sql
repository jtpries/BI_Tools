USE BITools

IF NOT EXISTS (SELECT name FROM sys.schemas WHERE [name] = N'etl')
BEGIN
	EXEC('CREATE SCHEMA [etl]')
END
GO

IF NOT EXISTS (SELECT name FROM sys.schemas WHERE [name] = N'it')
BEGIN
	EXEC('CREATE SCHEMA [it]')
END
GO

IF NOT EXISTS (SELECT name FROM sys.schemas WHERE [name] = N'stage')
BEGIN
	EXEC('CREATE SCHEMA [stage]')
END
GO

IF NOT EXISTS (SELECT name FROM sys.schemas WHERE [name] = N'tmp')
BEGIN
	EXEC('CREATE SCHEMA [tmp]')
END
GO

--------------------------------------------------------------------------------------------------

DROP TABLE IF EXISTS tmp.PBIActivityLogExtract

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [tmp].[PBIActivityLogExtract](
	[ActivityLogKey] [int] IDENTITY(1,1) NOT NULL,
	[ActivityLogInternalID] [varchar](50) NULL,
	[RecordType] [int] NULL, 
	[CreationTime] [datetime] NULL,
	[Operation] [varchar](100) NULL,
	[OrganizationID] [varchar](50) NULL,
	[UserType] [int] NULL, 
	[UserKey] [varchar](200) NULL,
	[Workload] [varchar](100) NULL,
	[UserID] [varchar](500) NULL,
	[ClientIP] [varchar](50) NULL,
	[UserAgent] [varchar](1000) NULL, 
	[Activity] [varchar](100) NULL,
	[ItemName] [nvarchar](500) NULL, 
	[ObjectID] [varchar](500) NULL, 
	[RequestID] [varchar](50) NULL, 
	[ActivityID] [varchar](50) NULL,
	[IsSuccess] [bit] NULL, 

	[WorkspaceName] [nvarchar](500) NULL,
	[WorkspaceID] [varchar](50) NULL,
	[ImportID] [varchar](50) NULL,
	[ImportSource] [varchar](50) NULL,
	[ImportType] [varchar](50) NULL,
	[ImportDisplayName] [nvarchar](500) NULL,
	[DatasetName] [nvarchar](500) NULL,
	[DatasetID] [varchar](50) NULL,
	[DataConnectivityMode] [varchar](200) NULL,
	[GatewayID] [varchar](50) NULL,
	[GatewayName] [nvarchar](500) NULL,
	[GatewayType] [varchar](100) NULL,
	[ReportName] [nvarchar](500) NULL,
	[ReportID] [varchar](50) NULL,
	[ReportType] [varchar](100) NULL,
	[FolderObjectID] [varchar](50) NULL, 
	[FolderDisplayName] [nvarchar](500) NULL, 
	[ArtifactName] [nvarchar](500) NULL,
	[ArtifactID] [varchar](50) NULL,
	[CapacityName] [varchar](200) NULL, 
	[CapacityUsers] [nvarchar](4000) NULL, 
	[CapacityState] [varchar](100) NULL,
	[DistributionMethod] [varchar](100) NULL,
	[ConsumptionMethod] [varchar](100) NULL,
	[RefreshType] [varchar](100) NULL,
	[ExportEventStartDateTimeParameter] [varchar](50) NULL, 
	[ExportEventEndDateTimeParameter] [varchar](50) NULL,

	[ExportedArtifactExportType] [varchar](50) NULL, 
	[ExportedArtifactType] [varchar](50) NULL, 
	[AuditedArtifactName] [nvarchar](500) NULL, 
	[AuditedArtifactObjectID] [varchar](50) NULL, 
	[AuditedArtifactItemType] [varchar](50) NULL, 
	[OtherDatasetIDs] [varchar](4000) NULL, 
	[OtherDatasetNames] [nvarchar](4000) NULL, 
	[OtherDatasourceTypes] [varchar](4000) NULL, 
	[OtherDatasourceConnectionDetails] [varchar](4000) NULL, 
	[SharingRecipientEmails] [nvarchar](4000) NULL, 
	[SharingResharePermissions] [varchar](4000) NULL, 
	[SubscribeeRecipientEmails] [nvarchar](4000) NULL, 
	[SubscribeeRecipientNames] [nvarchar](4000) NULL, 
	[SubscribeeObjectIDs] [varchar](4000) NULL, 

	[AddedDateTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ActivityLogKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


--------------------------------------------------------------------------------------------------

DROP TABLE IF EXISTS tmp.PBIActivityLog

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [tmp].[PBIActivityLog](
	[ActivityLogKey] [int] IDENTITY(1,1) NOT NULL,
	[ActivityLogInternalID] [varchar](50) NULL,
	[RecordType] [int] NULL, 
	[CreationTime] [datetime] NULL,
	[Operation] [varchar](100) NULL,
	[OrganizationID] [varchar](50) NULL,
	[UserType] [int] NULL, 
	[UserKey] [varchar](200) NULL,
	[Workload] [varchar](100) NULL,
	[UserID] [varchar](500) NULL,
	[ClientIP] [varchar](50) NULL,
	[UserAgent] [varchar](1000) NULL, 
	[Activity] [varchar](100) NULL,
	[ItemName] [nvarchar](500) NULL, 
	[ObjectID] [varchar](500) NULL, 
	[RequestID] [varchar](50) NULL, 
	[ActivityID] [varchar](50) NULL,
	[IsSuccess] [bit] NULL, 

	[WorkspaceName] [nvarchar](500) NULL,
	[WorkspaceID] [varchar](50) NULL,
	[ImportID] [varchar](50) NULL,
	[ImportSource] [varchar](50) NULL,
	[ImportType] [varchar](50) NULL,
	[ImportDisplayName] [nvarchar](500) NULL,
	[DatasetName] [nvarchar](500) NULL,
	[DatasetID] [varchar](50) NULL,
	[DataConnectivityMode] [varchar](200) NULL,
	[GatewayID] [varchar](50) NULL,
	[GatewayName] [nvarchar](500) NULL,
	[GatewayType] [varchar](100) NULL,
	[ReportName] [nvarchar](500) NULL,
	[ReportID] [varchar](50) NULL,
	[ReportType] [varchar](100) NULL,
	[FolderObjectID] [varchar](50) NULL, 
	[FolderDisplayName] [nvarchar](500) NULL, 
	[ArtifactName] [nvarchar](500) NULL,
	[ArtifactID] [varchar](50) NULL,
	[CapacityName] [varchar](200) NULL, 
	[CapacityUsers] [nvarchar](4000) NULL, 
	[CapacityState] [varchar](100) NULL,
	[DistributionMethod] [varchar](100) NULL,
	[ConsumptionMethod] [varchar](100) NULL,
	[RefreshType] [varchar](100) NULL,
	[ExportEventStartDateTimeParameter] [varchar](50) NULL, 
	[ExportEventEndDateTimeParameter] [varchar](50) NULL,

	[ExportedArtifactExportType] [varchar](50) NULL, 
	[ExportedArtifactType] [varchar](50) NULL, 
	[AuditedArtifactName] [nvarchar](500) NULL, 
	[AuditedArtifactObjectID] [varchar](50) NULL, 
	[AuditedArtifactItemType] [varchar](50) NULL, 
	[OtherDatasetIDs] [varchar](4000) NULL, 
	[OtherDatasetNames] [nvarchar](4000) NULL, 
	[OtherDatasourceTypes] [varchar](4000) NULL, 
	[OtherDatasourceConnectionDetails] [varchar](4000) NULL, 
	[SharingRecipientEmails] [nvarchar](4000) NULL, 
	[SharingResharePermissions] [varchar](4000) NULL, 
	[SubscribeeRecipientEmails] [nvarchar](4000) NULL, 
	[SubscribeeRecipientNames] [nvarchar](4000) NULL, 
	[SubscribeeObjectIDs] [varchar](4000) NULL, 

	[AddedDateTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ActivityLogKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


--------------------------------------------------------------------------------------------------


DROP TABLE IF EXISTS stage.PBIActivityLog

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [stage].[PBIActivityLog](
	[ActivityLogKey] [int] IDENTITY(1,1) NOT NULL,
	[BKActivityLogInternalID] [varchar](50) NULL,
	[CreateDateKey] [int] NULL, 
	[CreateTimeKey] [int] NULL, 
	[CreateDateTime] [datetime] NULL,
	[ActivityTypeKey] [int] NULL,
	[Activity] [varchar](100) NULL,
	[IsSuccess] [bit] NULL,
	[ItemName] [nvarchar](500) NULL,
	[ItemType] [varchar](50) NULL,
	[UserType] [int] NULL,
	[UserID] [varchar](500) NULL,
	[ClientIP] [varchar](50) NULL,
	[UserAgent] [varchar](1000) NULL,
	[DistributionMethod] [varchar](100) NULL,
	[ConsumptionMethod] [varchar](100) NULL,
	[BKWorkspaceID] [varchar](50) NULL,
	[WorkspaceKey] [int] NULL, 
	[WorkspaceName] [nvarchar](500) NULL,
	[BKReportID] [varchar](50) NULL,
	[ReportKey] [int] NULL,
	[ReportName] [nvarchar](500) NULL,
	[FolderDisplayName] [nvarchar](500) NULL,
	[ImportSource] [varchar](50) NULL,
	[ImportType] [varchar](50) NULL,
	[ImportDisplayName] [nvarchar](500) NULL,
	[DataConnectivityMode] [varchar](200) NULL,
	[BKDatasetID] [varchar](50) NULL,
	[DatasetKey] [int] NULL,
	[DatasetName] [nvarchar](500) NULL,
	[RefreshType] [varchar](100) NULL,
	[BKGatewayClusterID] [varchar](50) NULL,
	[GatewayClusterKey] [int] NULL,
	[GatewayClusterName] [nvarchar](500) NULL,
	[GatewayClusterType] [varchar](100) NULL,	
	[BKArtifactID] [varchar](50) NULL,
	[ArtifactName] [nvarchar](500) NULL,
	[CapacityName] [varchar](200) NULL, 
	[CapacityUsers] [nvarchar](4000) NULL, 
	[CapacityState] [varchar](100) NULL,
	[ExportedArtifactExportType] [varchar](50) NULL,
	[ExportedArtifactType] [varchar](50) NULL,
	[AuditedArtifactName] [nvarchar](500) NULL,
	[AuditedArtifactItemType] [varchar](50) NULL,
	[OtherDatasetNames] [nvarchar](4000) NULL,
	[OtherDatasourceTypes] [varchar](4000) NULL,
	[OtherDatasourceConnectionDetails] [varchar](4000) NULL,
	[SharingRecipientEmails] [nvarchar](4000) NULL,
	[SharingResharePermissions] [varchar](4000) NULL,
	[SubscribeeRecipientEmails] [nvarchar](4000) NULL,
	[SubscribeeRecipientNames] [nvarchar](4000) NULL,
PRIMARY KEY CLUSTERED 
(
	[ActivityLogKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

--------------------------------------------------------------------------------------------------

DROP TABLE IF EXISTS it.PBIActivityLog

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [it].[PBIActivityLog](
	[ActivityLogKey] [int] IDENTITY(1,1) NOT NULL,
	[BKActivityLogInternalID] [varchar](50) NULL,
	[CreateDateKey] [int] NULL, 
	[CreateTimeKey] [int] NULL, 
	[CreateDateTime] [datetime] NULL,
	[ActivityTypeKey] [int] NULL,
	[Activity] [varchar](100) NULL,
	[IsSuccess] [bit] NULL,
	[ItemName] [nvarchar](500) NULL,
	[ItemType] [varchar](50) NULL,
	[UserType] [int] NULL,
	[UserID] [varchar](500) NULL,
	[ClientIP] [varchar](50) NULL,
	[UserAgent] [varchar](1000) NULL,
	[DistributionMethod] [varchar](100) NULL,
	[ConsumptionMethod] [varchar](100) NULL,
	[BKWorkspaceID] [varchar](50) NULL,
	[WorkspaceKey] [int] NULL, 
	[WorkspaceName] [nvarchar](500) NULL,
	[BKReportID] [varchar](50) NULL,
	[ReportKey] [int] NULL,
	[ReportName] [nvarchar](500) NULL,
	[FolderDisplayName] [nvarchar](500) NULL,
	[ImportSource] [varchar](50) NULL,
	[ImportType] [varchar](50) NULL,
	[ImportDisplayName] [nvarchar](500) NULL,
	[DataConnectivityMode] [varchar](200) NULL,
	[BKDatasetID] [varchar](50) NULL,
	[DatasetKey] [int] NULL,
	[DatasetName] [nvarchar](500) NULL,
	[RefreshType] [varchar](100) NULL,
	[BKGatewayClusterID] [varchar](50) NULL,
	[GatewayClusterKey] [int] NULL,
	[GatewayClusterName] [nvarchar](500) NULL,
	[GatewayClusterType] [varchar](100) NULL,	
	[BKArtifactID] [varchar](50) NULL,
	[ArtifactName] [nvarchar](500) NULL,
	[CapacityName] [varchar](200) NULL, 
	[CapacityUsers] [nvarchar](4000) NULL, 
	[CapacityState] [varchar](100) NULL,
	[ExportedArtifactExportType] [varchar](50) NULL,
	[ExportedArtifactType] [varchar](50) NULL,
	[AuditedArtifactName] [nvarchar](500) NULL,
	[AuditedArtifactItemType] [varchar](50) NULL,
	[OtherDatasetNames] [nvarchar](4000) NULL,
	[OtherDatasourceTypes] [varchar](4000) NULL,
	[OtherDatasourceConnectionDetails] [varchar](4000) NULL,
	[SharingRecipientEmails] [nvarchar](4000) NULL,
	[SharingResharePermissions] [varchar](4000) NULL,
	[SubscribeeRecipientEmails] [nvarchar](4000) NULL,
	[SubscribeeRecipientNames] [nvarchar](4000) NULL,
PRIMARY KEY CLUSTERED 
(
	[ActivityLogKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
