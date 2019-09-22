# Power BI Data Export
# 
# A simple script to access the Power BI API and extract basic information about workspaces, reports, datasets, and datasources
# and export the results to CSV files
#
# jeff@jpries.com


# Authenticate to the Power BI Service API
Connect-PowerBIServiceAccount

# Arrays to hold retrieved data
$workspaceReports = @()
$workspaceDatasets = @()
$datasetDatasources = @()

# Get the workspace data
$workspaces = Get-PowerBIWorkspace -All

# Get the report and dataset data for each workspace
foreach ($workspace in $workspaces) {
   $reports = Get-PowerBIReport -WorkspaceId  $workspace.Id
   $datasets = Get-PowerBIDataset -WorkspaceId  $workspace.Id

   $reports | Add-Member -NotePropertyName WorkspaceId -NotePropertyValue $workspace.Id 
   $datasets | Add-Member -NotePropertyName WorkspaceId -NotePropertyValue $workspace.Id 

   $workspaceReports += $reports
   $workspaceDatasets += $datasets
}

# Get the datasource data for each workspace dataset
foreach ($dataset in $workspaceDatasets) {
   $datasources = Get-PowerBIDatasource -DatasetId $dataset.Id -WorkspaceId $dataset.WorkspaceId

   $datasources | Add-Member -NotePropertyName DatasetId -NotePropertyValue $dataset.Id
   $datasources | Add-Member -NotePropertyName WorkspaceId -NotePropertyValue $dataset.WorkspaceId

   $datasetDatasources += $datasources
}

# Output the results for each type to CSV files
$workspaces | Export-CSV -Force -Path .\PowerBI_Workspaces.csv
$workspaceReports | Export-CSV -Force -Path .\PowerBI_Reports.csv
$workspaceDatasets | Export-CSV -Force -Path .\PowerBI_Datasets.csv
$datasetDatasources | Export-CSV -Force -Path .\PowerBI_Datasources.csv