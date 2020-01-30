--------------------------------------------------------------------------------------------------------------------------

DROP VIEW IF EXISTS it.vPBIActivityLog

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [it].[vPBIActivityLog] AS 
(

	SELECT
		al.ActivityLogKey
		,al.BKActivityLogInternalID
		,al.CreateDateKey
		,al.CreateTimeKey
		,al.CreateDateTime
		,al.ActivityTypeKey
		,al.Activity
		,al.IsSuccess
		,al.ItemName
		,al.ItemType
		,al.ItemName + CASE WHEN ISNULL(al.ItemName, '') <> '' THEN ' (' + al.ItemType + ')' ELSE '' END AS ItemNameLong
		,al.UserType
		,al.UserID
		,al.ClientIP
		,al.UserAgent
		,al.DistributionMethod
		,al.ConsumptionMethod
		,al.BKWorkspaceID
		,al.WorkspaceName
		,CASE 
			WHEN WorkspaceName LIKE 'PersonalWorkspace%' THEN 'Personal'
			WHEN WorkspaceName LIKE 'PowerBIAdminGroupDisplayName' THEN 'Admin'
			WHEN WorkspaceName <> '' THEN 'Workspace'
			ELSE ''
			END AS WorkspaceType
		,al.BKReportID
		,al.ReportName
		,al.FolderDisplayName
		,al.ImportSource
		,al.ImportType
		,al.ImportDisplayName
		,al.DataConnectivityMode
		,al.BKDatasetID
		,al.DatasetName
		,al.RefreshType
		,al.BKGatewayClusterID
		,al.GatewayClusterName
		,al.GatewayClusterType
		,al.BKArtifactID
		,al.ArtifactName
		,al.CapacityName
		,al.CapacityUsers
		,al.CapacityState
		,al.ExportedArtifactExportType
		,al.ExportedArtifactType
		,al.AuditedArtifactName
		,al.AuditedArtifactItemType
		,al.OtherDatasetNames
		,al.OtherDatasourceTypes
		,al.OtherDatasourceConnectionDetails
		,al.SharingRecipientEmails
		,al.SharingResharePermissions
		,al.SubscribeeRecipientEmails
		,al.SubscribeeRecipientNames
	FROM it.PBIActivityLog al


)

GO

--------------------------------------------------------------------------------------------------------------------------