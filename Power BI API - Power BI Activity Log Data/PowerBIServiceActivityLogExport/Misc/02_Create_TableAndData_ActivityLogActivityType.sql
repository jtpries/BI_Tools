USE [BITools]
GO

--------------------------------------------------------------------------------------------------

DROP TABLE IF EXISTS it.PBIActivityLogActivityType



SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [it].[PBIActivityLogActivityType](
	[ActivityTypeKey] [int] IDENTITY(1,1) NOT NULL,
	[Activity] [varchar](200) NULL,
	[ActivityGroup] [varchar](100) NULL,
	[ActivityDescription] [varchar](1000) NULL,
	[ActivityNotes] [varchar](4000) NULL,
 CONSTRAINT [PK_PBIActivityLogActivityType] PRIMARY KEY CLUSTERED 
(
	[ActivityTypeKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [it].[PBIActivityLogActivityType] ON 
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (-1, N'Unknown', N'', N'', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (1, N'AddDatasourceToGateway', N'Gateway', N'Added data source to Power BI gateway', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (2, N'AddFolderAccess', N'Admin / Security', N'Added Power BI folder access', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (3, N'AddGroupMembers', N'Admin / Security', N'Added Power BI group members', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (4, N'AdminAttachedDataflowStorageAccountToTenant', N'Admin / Security', N'Admin attached dataflow storage account to tenant', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (5, N'AnalyzedByExternalApplication', N'View (Report / Dashboard / App)', N'Analyzed Power BI dataset', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (6, N'AnalyzeInExcel', N'View (Report / Dashboard / App)', N'Analyzed Power BI report', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (7, N'AttachedDataflowStorageAccount', N'Dataflow', N'Attached dataflow storage account', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (8, N'BindToGateway', N'Gateway', N'Binded Power BI dataset to gateway', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (9, N'CancelDataflowRefresh', N'Dataflow', N'Canceled dataflow refresh', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (10, N'ChangeCapacityState', N'Capacity', N'Changed capacity state', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (11, N'UpdateCapacityUsersAssignment', N'Capacity', N'Changed capacity user assignment', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (12, N'SetAllConnections', N'Dataset', N'Changed Power BI dataset connections', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (13, N'ChangeGatewayAdministrators', N'Gateway', N'Changed Power BI gateway admins', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (14, N'ChangeGatewayDatasourceUsers', N'Gateway', N'Changed Power BI gateway data source users', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (15, N'CreateOrgApp', N'Admin / Security', N'Created organizational Power BI content pack', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (16, N'CreateApp', N'Create (Report / Dashboard / App)', N'Created Power BI app', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (17, N'CreateDashboard', N'Create (Report / Dashboard / App)', N'Created Power BI dashboard', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (18, N'CreateDataflow', N'Dataflow', N'Created Power BI dataflow', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (19, N'CreateDataset', N'Dataset', N'Created Power BI dataset', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (20, N'CreateEmailSubscription', N'Subscription', N'Created Power BI email subscription', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (21, N'CreateFolder', N'Admin / Security', N'Created Power BI folder', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (22, N'CreateGateway', N'Gateway', N'Created Power BI gateway', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (23, N'CreateGroup', N'Admin / Security', N'Created Power BI group', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (24, N'CreateReport', N'Create (Report / Dashboard / App)', N'Created Power BI report', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (25, N'DataflowMigratedToExternalStorageAccount', N'Dataflow', N'Dataflow migrated to external storage account', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (26, N'DataflowPermissionsAdded', N'Dataflow', N'Dataflow permissions added', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (27, N'DataflowPermissionsRemoved', N'Dataflow', N'Dataflow permissions removed', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (28, N'DeleteOrgApp', N'Admin / Security', N'Deleted organizational Power BI content pack', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (29, N'DeleteComment', N'Delete (Report / Dashboard / App)', N'Deleted Power BI comment', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (30, N'DeleteDashboard', N'Delete (Report / Dashboard / App)', N'Deleted Power BI dashboard', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (31, N'DeleteDataflow', N'Dataflow', N'Deleted Power BI dataflow', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (32, N'DeleteDataset', N'Dataset', N'Deleted Power BI dataset', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (33, N'DeleteEmailSubscription', N'Subscription', N'Deleted Power BI email subscription', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (34, N'DeleteFolder', N'Admin / Security', N'Deleted Power BI folder', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (35, N'DeleteFolderAccess', N'Admin / Security', N'Deleted Power BI folder access', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (36, N'DeleteGateway', N'Gateway', N'Deleted Power BI gateway', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (37, N'DeleteGroup', N'Admin / Security', N'Deleted Power BI group', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (38, N'DeleteReport', N'Delete (Report / Dashboard / App)', N'Deleted Power BI report', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (39, N'GetDatasources', N'Dataset', N'Discovered Power BI dataset data sources', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (40, N'DownloadReport', N'Export / Print / Download', N'Downloaded Power BI report', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (41, N'EditDataflowProperties', N'Dataflow', N'Edited dataflow properties', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (42, N'EditCertificationPermission', N'Admin / Security', N'Edited Power BI certification permission', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (43, N'EditDashboard', N'Edit / Update (Report / Dashboard / App)', N'Edited Power BI dashboard', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (44, N'EditDataset', N'Dataset', N'Edited Power BI dataset', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (45, N'EditDatasetProperties', N'Dataset', N'Edited Power BI dataset properties', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (46, N'EditReport', N'Edit / Update (Report / Dashboard / App)', N'Edited Power BI report', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (47, N'ExportDataflow', N'Dataflow', N'Exported Power BI dataflow', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (48, N'ExportReport', N'Export / Print / Download', N'Exported Power BI report visual data', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (49, N'ExportTile', N'Export / Print / Download', N'Exported Power BI tile data', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (50, N'FailedToAddDataflowPermissions', N'Dataflow', N'Failed to add dataflow permissions', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (51, N'FailedToRemoveDataflowPermissions', N'Dataflow', N'Failed to remove dataflow permissions', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (52, N'GenerateDataflowSasToken', N'Dataflow', N'Generated Power BI dataflow SAS token', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (53, N'GenerateEmbedToken', N'Admin / Security', N'Generated Power BI Embed Token', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (54, N'Import', N'Import', N'Imported file to Power BI', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (55, N'InstallApp', N'Licensing / Account', N'Installed Power BI app', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (56, N'MigrateWorkspaceIntoCapacity', N'Capacity', N'Migrated workspace to a capacity', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (57, N'PostComment', N'Edit / Update (Report / Dashboard / App)', N'Posted Power BI comment', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (58, N'PrintDashboard', N'Export / Print / Download', N'Printed Power BI dashboard', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (59, N'PrintReport', N'Export / Print / Download', N'Printed Power BI report page', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (60, N'PublishToWebReport', N'Export / Print / Download', N'Published Power BI report to web', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (61, N'ReceiveDataflowSecretFromKeyVault', N'Dataflow', N'Received Power BI dataflow secret from Key Vault', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (62, N'RemoveDatasourceFromGateway', N'Gateway', N'Removed data source from Power BI gateway', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (63, N'DeleteGroupMembers', N'Admin / Security', N'Removed Power BI group members', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (64, N'RemoveWorkspacesFromCapacity', N'Capacity', N'Removed workspace from a capacity', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (65, N'RenameDashboard', N'Edit / Update (Report / Dashboard / App)', N'Renamed Power BI dashboard', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (66, N'RequestDataflowRefresh', N'Dataflow', N'Requested Power BI dataflow refresh', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (67, N'RefreshDataset', N'Dataset', N'Requested Power BI dataset refresh', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (68, N'GetWorkspaces', N'View (Report / Dashboard / App)', N'Retrieved Power BI workspaces', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (69, N'SetDataflowStorageLocationForWorkspace', N'Dataflow', N'Set dataflow storage location for a workspace', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (70, N'SetScheduledRefreshOnDataflow', N'Dataflow', N'Set scheduled refresh on Power BI dataflow', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (71, N'SetScheduledRefresh', N'Dataset', N'Set scheduled refresh on Power BI dataset', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (72, N'ShareDashboard', N'Sharing', N'Shared Power BI dashboard', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (73, N'ShareReport', N'Sharing', N'Shared Power BI report', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (74, N'OptInForExtendedProTrial', N'Licensing / Account', N'Started Power BI extended trial', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (75, N'OptInForProTrial', N'Licensing / Account', N'Started Power BI trial', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (76, N'TakeOverDatasource', N'Admin / Security', N'Took over a Power BI datasource', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (77, N'TakeOverDataset', N'Admin / Security', N'Took over Power BI dataset', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (78, N'TookOverDataflow', N'Admin / Security', N'Took over a Power BI dataflow', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (79, N'UnpublishApp', N'Delete (Report / Dashboard / App)', N'Unpublished Power BI app', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (80, N'UpdateCapacityResourceGovernanceSettings', N'Capacity', N'Update capacity resource governance settings', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (81, N'UpdateCapacityAdmins', N'Capacity', N'Updated capacity admin', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (82, N'UpdateCapacityDisplayName', N'Capacity', N'Updated capacity display name', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (83, N'UpdatedDataflowStorageAssignmentPermissions', N'Dataflow', N'Updated dataflow storage assignment permissions', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (84, N'UpdatedAdminFeatureSwitch', N'Admin / Security', N'Updated organization''s Power BI settings', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (85, N'UpdateApp', N'Edit / Update (Report / Dashboard / App)', N'Updated Power BI app', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (86, N'UpdateDataflow', N'Dataflow', N'Updated Power BI dataflow', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (87, N'UpdateDatasources', N'Dataset', N'Updated Power BI dataset data sources', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (88, N'UpdateDatasetParameters', N'Dataset', N'Updated Power BI dataset parameters', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (89, N'UpdateEmailSubscription', N'Subscription', N'Updated Power BI email subscription', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (90, N'UpdateFolder', N'Admin / Security', N'Updated Power BI folder', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (91, N'UpdateFolderAccess', N'Admin / Security', N'Updated Power BI folder access', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (92, N'UpdateDatasourceCredentials', N'Gateway', N'Updated Power BI gateway data source credentials', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (93, N'ViewDashboard', N'View (Report / Dashboard / App)', N'Viewed Power BI dashboard', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (94, N'ViewDataflow', N'Dataflow', N'Viewed Power BI dataflow', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (95, N'ViewReport', N'View (Report / Dashboard / App)', N'Viewed Power BI report', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (96, N'ViewTile', N'View (Report / Dashboard / App)', N'Viewed Power BI tile', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (97, N'ViewUsageMetrics', N'Admin / Security', N'Viewed Power BI usage metrics', N'')
GO
INSERT [it].[PBIActivityLogActivityType] ([ActivityTypeKey], [Activity], [ActivityGroup], [ActivityDescription], [ActivityNotes]) VALUES (98, N'ExportArtifact', N'Export / Print / Download', N'Export a Power BI Item', N'')
GO
SET IDENTITY_INSERT [it].[PBIActivityLogActivityType] OFF
GO
