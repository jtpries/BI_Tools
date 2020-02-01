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

DROP VIEW IF EXISTS it.vPBIActivityLogActivityType

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [it].[vPBIActivityLogActivityType] AS 
(
	SELECT 
		ActivityTypeKey
		,Activity
		,ActivityGroup
		,ActivityDescription
		,ActivityNotes
	FROM it.PBIActivityLogActivityType
)

GO

--------------------------------------------------------------------------------------------------------------------------

DROP VIEW IF EXISTS dbo.vDimDate

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vDimDate] AS

	SELECT
		DateKey 
		,SeqDayNumber 
		,[Date]
		,DayNumber 
		,[DayName] 
		,DayAbbrv 

		,CalendarDayOfWeek
		,CalendarDayOfMonth 
		,CASE WHEN DateKey = -1 THEN 'Unknown' ELSE 'Day ' + CAST(CalendarDayOfMonth AS VARCHAR(3)) END AS CalendarDayOfMonthName
		,CalendarDayOfQuarter 
		,CalendarDayOfYear 
		,CASE WHEN DateKey = -1 THEN 'Unknown' ELSE 'Day ' + CAST(CalendarDayOfYear AS VARCHAR(3)) END AS CalendarDayOfYearName

		,CalendarWeekOfYear
		,CalendarWeekKey

		,CalendarMonthKey 
		,CalendarMonthNumber 
		,CalendarMonthName 
		,CalendarMonthNameShort 

		,CalendarQuarterKey 
		,CalendarQuarterNumber 
		,CalendarQuarterName 
		,CalendarYear 

		,SeqCalendarWeekNumber 
		,SeqCalendarMonthNumber 
		,SeqCalendarQuarterNumber 

		,CalendarMonthNumDays
		,IsUSHoliday 
		,USHolidayName 
		,IsBizDay 
	FROM dbo.DimDate


GO

--------------------------------------------------------------------------------------------------------------------------

DROP VIEW IF EXISTS dbo.vDimTime

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vDimTime] AS

	SELECT 
		TimeKey
		,[Time]
		,Hour12h
		,Hour24h
		,[Minute]
		,[Second]
		,AmPm
		,StandardTime
		,CAST(CAST(CASE WHEN Hour12h = '00' THEN '12' ELSE Hour12h END AS INT) AS VARCHAR(2)) + ' ' + AmPm AS StandardTimeHourOnly
		,Hour24h + ':' + [Minute] AS [Hour24TimeMinOnly]
		,Hour12h + ':' + [Minute] + ' ' + AmPm AS StandardTimeMinOnly
		,CAST([Hour24h] AS INT) * 60 + CAST([Minute] AS INT) AS SeqMinuteNumber
	 FROM dbo.DimTime

--------------------------------------------------------------------------------------------------------------------------
