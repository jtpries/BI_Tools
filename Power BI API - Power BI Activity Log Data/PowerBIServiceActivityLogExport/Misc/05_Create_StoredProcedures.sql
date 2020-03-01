USE BITools

-------------------------------------------------------------------------------------------------------

DROP PROCEDURE IF EXISTS etl.usp_PBI_LoadActivityLog

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		jeff@jpries.com
-- Create date: 10/14/2019
-- Description:	Load for Power BI ActivityLog
-- =============================================
CREATE PROCEDURE [etl].[usp_PBI_LoadActivityLog] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	SET XACT_ABORT ON

	BEGIN TRAN
		SET IDENTITY_INSERT it.PBIActivityLog ON

		MERGE INTO it.PBIActivityLog AS TARGET
		USING
		(
			SELECT
				ActivityLogKey
				,BKActivityLogInternalID
				,CreateDateKey
				,CreateTimeKey
				,CreateDateTime
				,ActivityTypeKey
				,Activity
				,IsSuccess
				,ItemName
				,ItemType
				,UserType
				,UserID
				,ClientIP
				,UserAgent
				,DistributionMethod
				,ConsumptionMethod
				,BKWorkspaceID
				,-1 AS WorkspaceKey
				,WorkspaceName
				,BKReportID
				,-1 AS ReportKey
				,ReportName
				,FolderDisplayName
				,ImportSource
				,ImportType
				,ImportDisplayName
				,DataConnectivityMode
				,BKDatasetID
				,-1 AS DatasetKey
				,DatasetName
				,RefreshType
				,BKGatewayClusterID
				,-1 AS GatewayClusterKey
				,GatewayClusterName
				,GatewayClusterType
				,BKArtifactID
				,ArtifactName
				,CapacityName
				,CapacityUsers
				,CapacityState
				,ExportedArtifactExportType
				,ExportedArtifactType
				,AuditedArtifactName
				,AuditedArtifactItemType
				,OtherDatasetNames
				,OtherDatasourceTypes
				,OtherDatasourceConnectionDetails
				,SharingRecipientEmails
				,SharingResharePermissions
				,SubscribeeRecipientEmails
				,SubscribeeRecipientNames
			FROM stage.PBIActivityLog
		) AS SOURCE ON (SOURCE.ActivityLogKey = TARGET.ActivityLogKey)

		-- Matched but old, update
		WHEN MATCHED AND (
			SOURCE.BKActivityLogInternalID <> TARGET.BKActivityLogInternalID
			OR SOURCE.CreateDateTime <> TARGET.CreateDateTime
			OR SOURCE.ActivityTypeKey <> TARGET.ActivityTypeKey
			OR SOURCE.Activity <> TARGET.Activity
			OR SOURCE.IsSuccess <> TARGET.IsSuccess
			OR SOURCE.ItemName <> TARGET.ItemName
			OR SOURCE.ItemType <> TARGET.ItemType
			OR SOURCE.UserType <> TARGET.UserType
			OR SOURCE.UserID <> TARGET.UserID
			OR SOURCE.BKWorkspaceID <> TARGET.BKWorkspaceID
			OR SOURCE.WorkspaceKey <> TARGET.WorkspaceKey
			OR SOURCE.BKReportID <> TARGET.BKReportID
			OR SOURCE.ReportKey <> TARGET.ReportKey
			OR SOURCE.BKDatasetID <> TARGET.BKDatasetID
			OR SOURCE.DatasetKey <> TARGET.DatasetKey
			OR SOURCE.BKGatewayClusterID <> TARGET.BKGatewayClusterID
			OR SOURCE.GatewayClusterKey <> TARGET.GatewayClusterKey
		) THEN

		UPDATE
			SET
				TARGET.BKActivityLogInternalID = SOURCE.BKActivityLogInternalID
				,TARGET.CreateDateKey = SOURCE.CreateDateKey
				,TARGET.CreateTimeKey = SOURCE.CreateTimeKey
				,TARGET.CreateDateTime = SOURCE.CreateDateTime
				,TARGET.ActivityTypeKey = SOURCE.ActivityTypeKey
				,TARGET.Activity = SOURCE.Activity
				,TARGET.IsSuccess = SOURCE.IsSuccess
				,TARGET.ItemName = SOURCE.ItemName
				,TARGET.ItemType = SOURCE.ItemType
				,TARGET.UserType = SOURCE.UserType
				,TARGET.UserID = SOURCE.UserID
				,TARGET.ClientIP = SOURCE.ClientIP
				,TARGET.UserAgent = SOURCE.UserAgent
				,TARGET.DistributionMethod = SOURCE.DistributionMethod
				,TARGET.ConsumptionMethod = SOURCE.ConsumptionMethod
				,TARGET.BKWorkspaceID = SOURCE.BKWorkspaceID
				,TARGET.WorkspaceKey = SOURCE.WorkspaceKey
				,TARGET.WorkspaceName = SOURCE.WorkspaceName
				,TARGET.BKReportID = SOURCE.BKReportID
				,TARGET.ReportKey = SOURCE.ReportKey
				,TARGET.ReportName = SOURCE.ReportName
				,TARGET.FolderDisplayName = SOURCE.FolderDisplayName
				,TARGET.ImportSource = SOURCE.ImportSource
				,TARGET.ImportType = SOURCE.ImportType
				,TARGET.ImportDisplayName = SOURCE.ImportDisplayName
				,TARGET.DataConnectivityMode = SOURCE.DataConnectivityMode
				,TARGET.BKDatasetID = SOURCE.BKDatasetID
				,TARGET.DatasetKey = SOURCE.DatasetKey
				,TARGET.DatasetName = SOURCE.DatasetName
				,TARGET.RefreshType = SOURCE.RefreshType
				,TARGET.BKGatewayClusterID = SOURCE.BKGatewayClusterID
				,TARGET.GatewayClusterKey = SOURCE.GatewayClusterKey
				,TARGET.GatewayClusterName = SOURCE.GatewayClusterName
				,TARGET.GatewayClusterType = SOURCE.GatewayClusterType
				,TARGET.BKArtifactID = SOURCE.BKArtifactID
				,TARGET.ArtifactName = SOURCE.ArtifactName
				,TARGET.CapacityName = SOURCE.CapacityName
				,TARGET.CapacityUsers = SOURCE.CapacityUsers
				,TARGET.CapacityState = SOURCE.CapacityState
				,TARGET.ExportedArtifactExportType = SOURCE.ExportedArtifactExportType
				,TARGET.ExportedArtifactType = SOURCE.ExportedArtifactType
				,TARGET.AuditedArtifactName = SOURCE.AuditedArtifactName
				,TARGET.AuditedArtifactItemType = SOURCE.AuditedArtifactItemType
				,TARGET.OtherDatasetNames = SOURCE.OtherDatasetNames
				,TARGET.OtherDatasourceTypes = SOURCE.OtherDatasourceTypes
				,TARGET.OtherDatasourceConnectionDetails = SOURCE.OtherDatasourceConnectionDetails
				,TARGET.SharingRecipientEmails = SOURCE.SharingRecipientEmails
				,TARGET.SharingResharePermissions = SOURCE.SharingResharePermissions
				,TARGET.SubscribeeRecipientEmails = SOURCE.SubscribeeRecipientEmails
				,TARGET.SubscribeeRecipientNames = SOURCE.SubscribeeRecipientNames

		-- No target match, insert new record
		WHEN NOT MATCHED BY TARGET THEN
			INSERT
			(
				ActivityLogKey
				,BKActivityLogInternalID
				,CreateDateKey
				,CreateTimeKey
				,CreateDateTime
				,ActivityTypeKey
				,Activity
				,IsSuccess
				,ItemName
				,ItemType
				,UserType
				,UserID
				,ClientIP
				,UserAgent
				,DistributionMethod
				,ConsumptionMethod
				,BKWorkspaceID
				,WorkspaceKey
				,WorkspaceName
				,BKReportID
				,ReportKey
				,ReportName
				,FolderDisplayName
				,ImportSource
				,ImportType
				,ImportDisplayName
				,DataConnectivityMode
				,BKDatasetID
				,DatasetKey
				,DatasetName
				,RefreshType
				,BKGatewayClusterID
				,GatewayClusterKey
				,GatewayClusterName
				,GatewayClusterType
				,BKArtifactID
				,ArtifactName
				,CapacityName
				,CapacityUsers
				,CapacityState
				,ExportedArtifactExportType
				,ExportedArtifactType
				,AuditedArtifactName
				,AuditedArtifactItemType
				,OtherDatasetNames
				,OtherDatasourceTypes
				,OtherDatasourceConnectionDetails
				,SharingRecipientEmails
				,SharingResharePermissions
				,SubscribeeRecipientEmails
				,SubscribeeRecipientNames
			)

			VALUES
			(
				SOURCE.ActivityLogKey
				,SOURCE.BKActivityLogInternalID
				,SOURCE.CreateDateKey
				,SOURCE.CreateTimeKey
				,SOURCE.CreateDateTime
				,SOURCE.ActivityTypeKey
				,SOURCE.Activity
				,SOURCE.IsSuccess
				,SOURCE.ItemName
				,SOURCE.ItemType
				,SOURCE.UserType
				,SOURCE.UserID
				,SOURCE.ClientIP
				,SOURCE.UserAgent
				,SOURCE.DistributionMethod
				,SOURCE.ConsumptionMethod
				,SOURCE.BKWorkspaceID
				,SOURCE.WorkspaceKey
				,SOURCE.WorkspaceName
				,SOURCE.BKReportID
				,SOURCE.ReportKey
				,SOURCE.ReportName
				,SOURCE.FolderDisplayName
				,SOURCE.ImportSource
				,SOURCE.ImportType
				,SOURCE.ImportDisplayName
				,SOURCE.DataConnectivityMode
				,SOURCE.BKDatasetID
				,SOURCE.DatasetKey
				,SOURCE.DatasetName
				,SOURCE.RefreshType
				,SOURCE.BKGatewayClusterID
				,SOURCE.GatewayClusterKey
				,SOURCE.GatewayClusterName
				,SOURCE.GatewayClusterType
				,SOURCE.BKArtifactID
				,SOURCE.ArtifactName
				,SOURCE.CapacityName
				,SOURCE.CapacityUsers
				,SOURCE.CapacityState
				,SOURCE.ExportedArtifactExportType
				,SOURCE.ExportedArtifactType
				,SOURCE.AuditedArtifactName
				,SOURCE.AuditedArtifactItemType
				,SOURCE.OtherDatasetNames
				,SOURCE.OtherDatasourceTypes
				,SOURCE.OtherDatasourceConnectionDetails
				,SOURCE.SharingRecipientEmails
				,SOURCE.SharingResharePermissions
				,SOURCE.SubscribeeRecipientEmails
				,SOURCE.SubscribeeRecipientNames
			)

		-- No source match, delete
		WHEN NOT MATCHED BY SOURCE THEN DELETE

		OUTPUT
			$action AS MergeAction
			,inserted.ActivityLogKey AS Inserted
			,deleted.ActivityLogKey AS Deleted
		;

		SET IDENTITY_INSERT it.PBIActivityLog OFF

	COMMIT TRAN

END
GO

-------------------------------------------------------------------------------------------------------

DROP PROCEDURE IF EXISTS etl.usp_PBI_TransformActivityLog

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		jeff@jpries.com
-- Create date: 10/14/2019
-- Description:	Transform for Power BI ActivityLog
-- =============================================
CREATE PROCEDURE [etl].[usp_PBI_TransformActivityLog] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- ActivityLog
	TRUNCATE TABLE stage.PBIActivityLog

	INSERT INTO stage.PBIActivityLog 
	(
		BKActivityLogInternalID
		,CreateDateKey
		,CreateTimeKey
		,CreateDateTime
		,ActivityTypeKey
		,Activity
		,IsSuccess
		,ItemName
		,ItemType
		,UserType
		,UserID
		,ClientIP
		,UserAgent
		,DistributionMethod
		,ConsumptionMethod
		,BKWorkspaceID
		,WorkspaceName
		,BKReportID
		,ReportName
		,FolderDisplayName
		,ImportSource
		,ImportType
		,ImportDisplayName
		,DataConnectivityMode
		,BKDatasetID
		,DatasetName
		,RefreshType
		,BKGatewayClusterID
		,GatewayClusterName
		,GatewayClusterType
		,BKArtifactID
		,ArtifactName
		,CapacityName
		,CapacityUsers
		,CapacityState
		,ExportedArtifactExportType
		,ExportedArtifactType
		,AuditedArtifactName
		,AuditedArtifactItemType
		,OtherDatasetNames
		,OtherDatasourceTypes
		,OtherDatasourceConnectionDetails
		,SharingRecipientEmails
		,SharingResharePermissions
		,SubscribeeRecipientEmails
		,SubscribeeRecipientNames
	)

	SELECT 
		al.ActivityLogInternalID AS BKActivityLogInternalID
		,ISNULL(cd.DateKey, -1) AS CreateDateKey
		,ISNULL(ct.TimeKey, -1) AS CreateTimeKey
		,CAST(al.CreationTime AS DATETIME) AS CreateDateTime
		,ISNULL(ad.ActivityTypeKey, -1) AS ActivityTypeKey
		,al.Activity
		,CAST(al.IsSuccess AS BIT) AS IsSuccess
		,CASE 
			WHEN al.ItemName = '' AND al.Activity = 'PostComment' THEN al.AuditedArtifactName 
			ELSE REPLACE(al.ItemName, '.pbix', '')
			END AS ItemName
		,CASE 
			WHEN al.Activity LIKE '%Report%' THEN 'Report'
			WHEN al.Activity LIKE '%Subscription%' THEN 'Report'
			WHEN al.Activity LIKE '%Import%' THEN 'Report'
			WHEN al.Activity LIKE '%ExportArtifact%' THEN 'Report'
			WHEN al.Activity LIKE '%UsageMetrics%' THEN 'Report'
			WHEN al.Activity LIKE '%Dashboard%' THEN 'Dashboard'
			WHEN al.Activity LIKE '%ExportTile%' THEN 'Dashboard'
			WHEN al.Activity LIKE '%App%' THEN 'App'
			WHEN al.Activity LIKE '%Gateway%' THEN 'Gateway'
			WHEN al.Activity LIKE '%Dataset%' THEN 'Dataset'
			WHEN al.Activity LIKE '%ScheduledRefresh%' THEN 'Dataset'
			WHEN al.Activity LIKE '%Datasource%' THEN 'Datasource'
			WHEN al.Activity LIKE '%Dataflow%' THEN 'Datasource'
			WHEN al.Activity LIKE '%Capacity%' THEN 'Capacity'
			WHEN al.Activity LIKE '%Workspace%' THEN 'Workspace'
			WHEN al.Activity LIKE '%Group%' THEN 'Workspace'
			WHEN al.Activity LIKE '%Folder%' THEN 'Workspace'
			ELSE 'Other'
			END AS ItemType
		,al.UserType
		,al.UserID
		,al.ClientIP
		,al.UserAgent
		,al.DistributionMethod
		,al.ConsumptionMethod
		,al.WorkspaceID AS BKWorkspaceID
		,al.WorkspaceName
		,al.ReportID AS BKReportID
		,al.ReportName
		,al.FolderDisplayName
		,al.ImportSource
		,al.ImportType
		,al.ImportDisplayName
		,al.DataConnectivityMode
		,al.DatasetID AS BKDatasetID
		,al.DatasetName
		,al.RefreshType
		,al.GatewayID AS BKGatewayClusterID
		,al.GatewayName AS GatewayClusterName
		,al.GatewayType AS GatewayClusterType
		,al.ArtifactID AS BKArtifactID
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
	FROM tmp.PBIActivityLog al
		LEFT OUTER JOIN it.PBIActivityLogActivityType ad ON (al.Activity = ad.Activity)
		LEFT OUTER JOIN dbo.DimDate cd ON (CAST(al.CreationTime AS DATE) = cd.[Date])
		LEFT OUTER JOIN dbo.DimTime ct ON (CAST(al.CreationTime AS TIME) = ct.[Time])
	WHERE UserAgent NOT LIKE 'PowerBIServiceInventory%'
	ORDER BY 
		al.CreationTime



END
GO


-------------------------------------------------------------------------------------------------------

DROP PROCEDURE IF EXISTS etl.usp_PBI_ExtractActivityLog


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		jeff@jpries.com
-- Create date: 10/14/2019
-- Description:	Extract for Power BI Workspaces
-- =============================================
CREATE PROCEDURE [etl].[usp_PBI_ExtractActivityLog] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	BEGIN TRAN

		MERGE INTO tmp.PBIActivityLog AS TARGET
		USING
		(
			SELECT DISTINCT
				ActivityLogInternalID
				,RecordType
				,CreationTime
				,Operation
				,OrganizationID
				,UserType
				,UserKey
				,[Workload]
				,UserID
				,ClientIP
				,UserAgent
				,Activity
				,ItemName
				,ObjectID
				,RequestID
				,ActivityID
				,IsSuccess

				,WorkspaceName
				,WorkspaceID
				,ImportID
				,ImportSource
				,ImportType
				,ImportDisplayName
				,DatasetName
				,DatasetID
				,DataConnectivityMode
				,GatewayID
				,GatewayName
				,GatewayType
				,ReportName
				,ReportID
				,ReportType
				,FolderObjectID
				,FolderDisplayName
				,ArtifactName
				,ArtifactID
				,CapacityName
				,CapacityUsers
				,CapacityState
				,DistributionMethod
				,ConsumptionMethod
				,RefreshType
				,ExportEventStartDateTimeParameter
				,ExportEventEndDateTimeParameter

				,ExportedArtifactExportType
				,ExportedArtifactType
				,AuditedArtifactName
				,AuditedArtifactObjectID
				,AuditedArtifactItemType
				,OtherDatasetIDs
				,OtherDatasetNames
				,OtherDatasourceTypes
				,OtherDatasourceConnectionDetails
				,SharingRecipientEmails
				,SharingResharePermissions
				,SubscribeeRecipientEmails
				,SubscribeeRecipientNames
				,SubscribeeObjectIDs

                ,GETDATE() AS AddedDateTime
			FROM tmp.PBIActivityLogExtract
			WHERE Operation <> 'ExportActivityEvents'
		) AS SOURCE ON (TARGET.ActivityLogInternalID = SOURCE.ActivityLogInternalID)

		-- Matched but old, update
		WHEN MATCHED THEN
			UPDATE
				SET
					TARGET.RecordType = SOURCE.RecordType
					,TARGET.CreationTime = SOURCE.CreationTime
					,TARGET.Operation = SOURCE.Operation
					,TARGET.OrganizationID = SOURCE.OrganizationID
					,TARGET.UserType = SOURCE.UserType
					,TARGET.UserKey = SOURCE.UserKey
					,TARGET.[Workload] = SOURCE.[Workload]
					,TARGET.UserID = SOURCE.UserID
					,TARGET.ClientIP = SOURCE.ClientIP
					,TARGET.UserAgent = SOURCE.UserAgent
					,TARGET.Activity = SOURCE.Activity
					,TARGET.ItemName = SOURCE.ItemName
					,TARGET.ObjectID = SOURCE.ObjectID
					,TARGET.RequestID = SOURCE.RequestID
					,TARGET.ActivityID = SOURCE.ActivityID
					,TARGET.IsSuccess = SOURCE.IsSuccess					
					
					,TARGET.WorkspaceName = SOURCE.WorkspaceName
					,TARGET.WorkspaceID = SOURCE.WorkspaceID
					,TARGET.ImportID = SOURCE.ImportID
					,TARGET.ImportSource = SOURCE.ImportSource
					,TARGET.ImportType = SOURCE.ImportType
					,TARGET.ImportDisplayName = SOURCE.ImportDisplayName
					,TARGET.DatasetName = SOURCE.DatasetName
					,TARGET.DatasetID = SOURCE.DatasetID
					,TARGET.DataConnectivityMode = SOURCE.DataConnectivityMode
					,TARGET.GatewayID = SOURCE.GatewayID
					,TARGET.GatewayName = SOURCE.GatewayName
					,TARGET.GatewayType = SOURCE.GatewayType
					,TARGET.ReportName = SOURCE.ReportName
					,TARGET.ReportID = SOURCE.ReportID
					,TARGET.ReportType = SOURCE.ReportType
					,TARGET.FolderObjectID = SOURCE.FolderObjectID
					,TARGET.FolderDisplayName = SOURCE.FolderDisplayName
					,TARGET.ArtifactName = SOURCE.ArtifactName
					,TARGET.ArtifactID = SOURCE.ArtifactID
					,TARGET.CapacityName = SOURCE.CapacityName
					,TARGET.CapacityUsers = SOURCE.CapacityUsers
					,TARGET.CapacityState = SOURCE.CapacityState
					,TARGET.DistributionMethod = SOURCE.DistributionMethod
					,TARGET.ConsumptionMethod = SOURCE.ConsumptionMethod
					,TARGET.RefreshType = SOURCE.RefreshType
					,TARGET.ExportEventStartDateTimeParameter = SOURCE.ExportEventStartDateTimeParameter
					,TARGET.ExportEventEndDateTimeParameter = SOURCE.ExportEventEndDateTimeParameter

					,TARGET.ExportedArtifactExportType = SOURCE.ExportedArtifactExportType
					,TARGET.ExportedArtifactType = SOURCE.ExportedArtifactType
					,TARGET.AuditedArtifactName = SOURCE.AuditedArtifactName
					,TARGET.AuditedArtifactObjectID = SOURCE.AuditedArtifactObjectID
					,TARGET.AuditedArtifactItemType = SOURCE.AuditedArtifactItemType
					,TARGET.OtherDatasetIDs = SOURCE.OtherDatasetIDs
					,TARGET.OtherDatasetNames = SOURCE.OtherDatasetNames
					,TARGET.OtherDatasourceTypes = SOURCE.OtherDatasourceTypes
					,TARGET.OtherDatasourceConnectionDetails = SOURCE.OtherDatasourceConnectionDetails
					,TARGET.SharingRecipientEmails = SOURCE.SharingRecipientEmails
					,TARGET.SharingResharePermissions = SOURCE.SharingResharePermissions
					,TARGET.SubscribeeRecipientEmails = SOURCE.SubscribeeRecipientEmails
					,TARGET.SubscribeeRecipientNames = SOURCE.SubscribeeRecipientNames
					,TARGET.SubscribeeObjectIDs = SOURCE.SubscribeeObjectIDs

					,TARGET.AddedDateTime = SOURCE.AddedDateTime

		-- No target match, insert new record
		WHEN NOT MATCHED BY TARGET THEN
			INSERT
			(
				ActivityLogInternalID
				,RecordType
				,CreationTime
				,Operation
				,OrganizationID
				,UserType
				,UserKey
				,[Workload]
				,UserID
				,ClientIP
				,UserAgent
				,Activity
				,ItemName
				,ObjectID
				,RequestID
				,ActivityID
				,IsSuccess

				,WorkspaceName
				,WorkspaceID
				,ImportID
				,ImportSource
				,ImportType
				,ImportDisplayName
				,DatasetName
				,DatasetID
				,DataConnectivityMode
				,GatewayID
				,GatewayName
				,GatewayType
				,ReportName
				,ReportID
				,ReportType
				,FolderObjectID
				,FolderDisplayName
				,ArtifactName
				,ArtifactID
				,CapacityName
				,CapacityUsers
				,CapacityState
				,DistributionMethod
				,ConsumptionMethod
				,RefreshType
				,ExportEventStartDateTimeParameter
				,ExportEventEndDateTimeParameter

				,ExportedArtifactExportType
				,ExportedArtifactType
				,AuditedArtifactName
				,AuditedArtifactObjectID
				,AuditedArtifactItemType
				,OtherDatasetIDs
				,OtherDatasetNames
				,OtherDatasourceTypes
				,OtherDatasourceConnectionDetails
				,SharingRecipientEmails
				,SharingResharePermissions
				,SubscribeeRecipientEmails
				,SubscribeeRecipientNames
				,SubscribeeObjectIDs

                ,AddedDateTime
			)

			VALUES
			(
				SOURCE.ActivityLogInternalID
				,SOURCE.RecordType
				,SOURCE.CreationTime
				,SOURCE.Operation
				,SOURCE.OrganizationID
				,SOURCE.UserType
				,SOURCE.UserKey
				,SOURCE.[Workload]
				,SOURCE.UserID
				,SOURCE.ClientIP
				,SOURCE.UserAgent
				,SOURCE.Activity
				,SOURCE.ItemName
				,SOURCE.ObjectID
				,SOURCE.RequestID
				,SOURCE.ActivityID
				,SOURCE.IsSuccess

				,SOURCE.WorkspaceName
				,SOURCE.WorkspaceID
				,SOURCE.ImportID
				,SOURCE.ImportSource
				,SOURCE.ImportType
				,SOURCE.ImportDisplayName
				,SOURCE.DatasetName
				,SOURCE.DatasetID
				,SOURCE.DataConnectivityMode
				,SOURCE.GatewayID
				,SOURCE.GatewayName
				,SOURCE.GatewayType
				,SOURCE.ReportName
				,SOURCE.ReportID
				,SOURCE.ReportType
				,SOURCE.FolderObjectID
				,SOURCE.FolderDisplayName
				,SOURCE.ArtifactName
				,SOURCE.ArtifactID
				,SOURCE.CapacityName
				,SOURCE.CapacityUsers
				,SOURCE.CapacityState
				,SOURCE.DistributionMethod
				,SOURCE.ConsumptionMethod
				,SOURCE.RefreshType
				,SOURCE.ExportEventStartDateTimeParameter
				,SOURCE.ExportEventEndDateTimeParameter

				,SOURCE.ExportedArtifactExportType
				,SOURCE.ExportedArtifactType
				,SOURCE.AuditedArtifactName
				,SOURCE.AuditedArtifactObjectID
				,SOURCE.AuditedArtifactItemType
				,SOURCE.OtherDatasetIDs
				,SOURCE.OtherDatasetNames
				,SOURCE.OtherDatasourceTypes
				,SOURCE.OtherDatasourceConnectionDetails
				,SOURCE.SharingRecipientEmails
				,SOURCE.SharingResharePermissions
				,SOURCE.SubscribeeRecipientEmails
				,SOURCE.SubscribeeRecipientNames
				,SOURCE.SubscribeeObjectIDs

                ,SOURCE.AddedDateTime
			)
		
		OUTPUT
			$action AS MergeAction
			,inserted.ActivityLogInternalID AS Inserted
			,deleted.ActivityLogInternalID AS Deleted
		;

	COMMIT TRAN

END
GO

