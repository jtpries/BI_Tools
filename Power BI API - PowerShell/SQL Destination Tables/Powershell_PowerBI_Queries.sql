USE BITools

-- What are the workspaces?
SELECT
	w.[Name] AS WorkspaceName
FROM dbo.PowerShell_PBIWorkspace w
ORDER BY WorkspaceName

-- What are the deployed reports?
SELECT
	r.[Name] AS ReportName
	,r.WebUrl AS ReportURL
	,w.[Name] AS WorkspaceName
FROM dbo.PowerShell_PBIReport r
	INNER JOIN dbo.PowerShell_PBIWorkspace w ON (r.WorkspaceID = w.ID)
ORDER BY
	ReportName
	,WorkspaceName

-- What datasets are configured for which reports / workspaces
SELECT
	se.[Name] AS DatasetName
	,se.ConfiguredBy AS DatasetConfiguredBy
	,se.IsRefreshable
	,se.IsOnPremGatewayRequired
	,r.[Name] AS ReportName
	,r.WebUrl AS ReportURL
	,w.[Name] AS WorkspaceName
FROM dbo.PowerShell_PBIDataset se
	INNER JOIN dbo.PowerShell_PBIReport r ON (se.Id = r.DatasetId)
	INNER JOIN dbo.PowerShell_PBIWorkspace w ON (r.WorkspaceID = w.ID)
ORDER BY
	DatasetName
	,ReportName
	,WorkspaceName

-- What are the Datasources?
SELECT
	so.DatasourceType
	,so.ConnectionDetails
	,so.GatewayId
	,se.[Name] AS DatasetName
	,r.[Name] AS ReportName
	,r.WebUrl AS ReportURL
	,w.[Name] AS WorkspaceName
FROM dbo.PowerShell_PBIDatasource so
	INNER JOIN dbo.PowerShell_PBIDataset se ON (so.DatasetId = se.Id)
	INNER JOIN dbo.PowerShell_PBIReport r ON (se.Id = r.DatasetId)
	INNER JOIN dbo.PowerShell_PBIWorkspace w ON (r.WorkspaceID = w.ID)
ORDER BY
	DatasetName
	,ReportName
	,WorkspaceName

