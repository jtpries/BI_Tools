/*
CREATE SCHEMA tmp
GO
*/


DROP TABLE IF EXISTS tmp.PBIGatewayAgent

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [tmp].[PBIGatewayAgent](
	[GatewayAgentInternalID] [nvarchar](40) NULL,
	[GatewayAgentID] [nvarchar](40) NULL,
	[GatewayAgentName] [nvarchar](200) NULL,
	[GatewayAgentAnnotation] [nvarchar](4000) NULL,
	[GatewayAgentStatus] [nvarchar](50) NULL,
	[GatewayAgentIsAnchor] [bit] NULL,
	[GatewayAgentClusterStatus] [nvarchar](50) NULL,
	[GatewayAgentLoadBalancingSettings] [nvarchar](500) NULL,
	[GatewayAgentStaticCapabilities] [nvarchar](50) NULL,
	[GatewayAgentPublicKey] [nvarchar](4000) NULL,
	[GatewayAgentVersion] [nvarchar](50) NULL,
	[GatewayAgentVersionStatus] [nvarchar](50) NULL,
	[GatewayAgentEmail] [nvarchar](200) NULL,
	[GatewayAgentMachineName] [nvarchar](200) NULL,
	[GatewayAgentDepartment] [nvarchar](200) NULL,
	[GatewayAgentExpiryDate] [nvarchar](50) NULL,
	[GatewayClusterInternalID] [nvarchar](40) NULL,
	[GatewayClusterID] [nvarchar](40) NULL,
	[GateawyClusterName] [nvarchar](200) NULL
) 