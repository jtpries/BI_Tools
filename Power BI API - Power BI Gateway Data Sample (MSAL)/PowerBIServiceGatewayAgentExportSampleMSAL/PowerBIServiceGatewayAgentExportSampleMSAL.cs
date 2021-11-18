///===================================================================================
///   PowerBIServiceGatewayAgentExportSampleMSAL.cs
///===================================================================================
/// -- Author:       Jeff Pries (jeff@jpries.com)
/// -- Create date:  11/16/2021
/// -- Description:  Sample application to export data from the Power BI API using saved SPN credentials and MSAL
/// 

using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace PowerBIServiceGatewayAgentExportSampleMSAL
{
    class PowerBIServiceGatewayAgentExportSampleMSAL
    {
        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Global Constants and Variables
        ///    

        // Constants
        const string HttpHeadUserAgent = "PowerBIServiceGatewayAgentExportSample/1.0";
        const string GlobalServiceEndpoint = "https://api.powerbi.com";
        const string ServiceConfigPath = "/powerbi/globalservice/v201606/environments/discover?user=";

        const string ServiceBackendPath = "/spglobalservice/GetOrInsertClusterUrisByTenantLocation";
        const string defaultGatewayMgmtEndpoint = "https://wabi-west-us-redirect.analysis.windows.net/";

        const string defaultAuthorityURI = "https://login.microsoftonline.com/"; // MSAL
        const string defaultPowerBIResourceURI = "https://analysis.windows.net/powerbi/api";
        const string defaultGatewayMgmtApplicationID = "ea0616ba-638b-4df5-95b9-636659ae5121";
        const string defaultPowerBIRedirectURI = "urn:ietf:wg:oauth:2.0:oob";
        const string PowerBIAccessScope = "https://analysis.windows.net/powerbi/api/.default";  // MSAL
        const string GatewayMgmtAccessScope = "https://analysis.windows.net/powerbi/api/.default";  // MSAL

        // User specific configuration values (hardcoded in plaintext for demo only)
        const string AzureTenantID = "11111111-1111-1111-1111-111111111111";
        const string UserName = "jeff@contoso.com";     // Username to authenticate as (for demo only)
        const string Password = "Password123";          // Password to authenticate with (for demo only)


        // Variables 
        HttpClient noAuthClient = null;
        HttpClient gatewayClient = null;

        string AuthorityURI = "";  // MSAL
        string PowerBIResourceURI = "";
        string GatewayMgmtApplicationID = "";
        string PowerBIRedirectURI = "";
        string GatewayMgmtEndpoint = "";



        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Default Class constructor
        ///    
        public PowerBIServiceGatewayAgentExportSampleMSAL()
        {
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Execute Method
        ///    
        public void Execute()
        {
            string authToken = "";

            // Initialize the No Auth client for getting basic Power BI settings
            InitNoAuthHttpClient();

            // Get the generic API configuration objects from the MS directory using unauthenticated client
            GetServiceConfig();

            // Get an authentication token for this API
            authToken = GetAuthToken();  // Uses native AD auth

            // Initialize the client with the token
            if (!String.IsNullOrEmpty(authToken))
            {
                // Initialize the client with the token
                InitHttpClient(authToken);

                // Get the PBI Gateway backend config info using authenticated gateway Client
                GetServiceBackend();

                Console.WriteLine("");
                Console.WriteLine("     --------------------------------------------------------------------------------------");
                Console.WriteLine("");
                Console.WriteLine("     Power BI Gateway Mgmt Base: " + GatewayMgmtEndpoint);
                Console.WriteLine("");
                Console.WriteLine("     --------------------------------------------------------------------------------------");
                Console.WriteLine("");

                GetGatewayAgents();  // Unified Gateawy API

                Console.WriteLine("");
                Console.WriteLine("- Done!");
            }
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetAuthGatewayUserLoginSavedCredential Method (Saved Credential)
        /// 
        public async Task<Microsoft.Identity.Client.AuthenticationResult> GetAuthGatewayUserLoginSavedCredential()
        {

            Microsoft.Identity.Client.AuthenticationResult authResult = null;
            AuthorityURI = defaultAuthorityURI + AzureTenantID;

            try
            {
                Console.WriteLine("        Authority URI: " + AuthorityURI);
                Console.WriteLine("        Resource URI: " + PowerBIResourceURI);
                Console.WriteLine("        Application ID: " + GatewayMgmtApplicationID);
                Console.WriteLine("        Access Scope: " + PowerBIAccessScope);
                Console.WriteLine("        Username: " + UserName);

                var publicClient = PublicClientApplicationBuilder
                                .Create(GatewayMgmtApplicationID)
                                .WithAuthority(new Uri(AuthorityURI))
                                .WithRedirectUri(PowerBIRedirectURI)
                                .Build();

                // Build the access scope list
                List<string> scopes = new List<string>();
                scopes.Add(GatewayMgmtAccessScope);

                // Request an access token with the request client and scope
                SecureString PasswordSecure = new NetworkCredential("", Password).SecurePassword;
                var accessTokenRequest = publicClient.AcquireTokenByUsernamePassword(scopes, UserName, PasswordSecure);

                authResult = await accessTokenRequest.ExecuteAsync();
            }
            catch (AggregateException ex)
            {
                MsalServiceException ex1 = (MsalServiceException)ex.GetBaseException();

                Console.WriteLine("   - Error acquiring token with ApplicationID/Secret (User/Pass) [GatewayMgmt].  ");
                Console.WriteLine("     Usually this is due to an invalid password");
                Console.WriteLine("");
                Console.WriteLine("     Details: " + ex1.StatusCode + ": " + ex1.ResponseBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - Error acquiring token with ApplicationID/Secret (User/pass) [GatewayMgmt].  ");
                Console.WriteLine("     Usually this is due to an invalid password");
                Console.WriteLine("");
                Console.WriteLine("     Details: " + ex.ToString());
            }

            return authResult;
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetAuthToken Method - Call the appropriate methods to get a token and return an auth string
        /// 
        public string GetAuthToken()
        {
            Task<Microsoft.Identity.Client.AuthenticationResult> authResult = null;

            string authToken = "";

            Console.WriteLine("");
            Console.WriteLine("- Performing authentication to request API access token [GatewayMgmt]...");


            // If not using client secret and password is present, use User/Pass authentication
            Console.WriteLine("   - Attempting login with saved credentials (User / Password).");

            authResult = GetAuthGatewayUserLoginSavedCredential();

            // Wait for the authentication to complete to check the result
            if (authResult != null)
            {
                authResult.Wait();
            }

            // If authentication result received, get the token
            if (authResult != null)
            {
                if (authResult.Result != null)
                {
                    // Create the bearer token to be used for API methods
                    authToken = authResult.Result.CreateAuthorizationHeader();
                    if (authToken.Substring(0, 6) == "Bearer")
                    {
                        Console.WriteLine("   - API Authorization token received.");
                    }
                    else
                    {
                        Console.WriteLine("   - Unable to retrieve API Authorization token.");
                    }
                }
                else
                {
                    Console.WriteLine("   - Unable to retrieve API Authorization token.");
                }
            }
            else
            {
                Console.WriteLine("   - Unable to retrieve API Authorization token.");
            }


            // AuthToken will be populated if authentication was successful and blank of not.  Return value.
            return authToken;
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// InitNoAuthHttpClient Method
        ///   
        public void InitNoAuthHttpClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            noAuthClient = new HttpClient();
            noAuthClient.DefaultRequestHeaders.UserAgent.ParseAdd(HttpHeadUserAgent);
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// InitHttpClient Method
        ///    
        public void InitHttpClient(string authToken)
        {
            string authTokenDisplay = "";

            if (authToken.Length >= 40)
            {
                authTokenDisplay = "..." + authToken.Substring(Math.Max(0, authToken.Length - 40));  // Last 40 chars
            }
            else
            {
                authTokenDisplay = "None";
            }

            Console.WriteLine("");
            Console.WriteLine("- Initializing Gateway Mgmt API API client with received auth token [" + authTokenDisplay + "]");
            Console.WriteLine("");

            // Create the web client connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            gatewayClient = new HttpClient();
            gatewayClient.DefaultRequestHeaders.UserAgent.ParseAdd(HttpHeadUserAgent);
            gatewayClient.DefaultRequestHeaders.Add("Authorization", authToken);
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetServiceConfig Method
        /// 
        public void GetServiceConfig()
        {
            HttpResponseMessage response = null;
            HttpContent responseContent = null;
            string strContent = "";
            PowerBIConfig rc = null;

            // Set initial defaults
            AuthorityURI = defaultAuthorityURI;
            PowerBIResourceURI = defaultPowerBIResourceURI;
            GatewayMgmtApplicationID = defaultGatewayMgmtApplicationID;
            PowerBIRedirectURI = defaultPowerBIRedirectURI;

            string serviceURI = GlobalServiceEndpoint + ServiceConfigPath + UserName;

            try
            {
                Console.WriteLine("");
                Console.WriteLine("- Retrieving configuration data from: " + serviceURI);

                response = noAuthClient.PostAsync(serviceURI, new StringContent("")).Result;

                Console.WriteLine("   - Response code received: " + response.StatusCode);

                try
                {
                    responseContent = response.Content;
                    strContent = responseContent.ReadAsStringAsync().Result;

                    if (strContent.Length > 0)
                    {
                        Console.WriteLine("   - De-serializing Service Config Data...");

                        // Parse the JSON string into objects and store in DataTable
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        rc = js.Deserialize<PowerBIConfig>(strContent);

                        // Parse through rc
                        if (rc != null)
                        {
                            if (rc.services != null)
                            {
                                Console.WriteLine("      - Services received: " + rc.services.Count);
                                foreach (PowerBIConfigService item in rc.services)
                                {
                                    string serviceName = "";
                                    string serviceEndpoint = "";
                                    string serviceResourceID = "";

                                    if (item.name != null)
                                    {
                                        serviceName = item.name;
                                    }

                                    if (item.endpoint != null)
                                    {
                                        serviceEndpoint = item.endpoint;
                                    }

                                    if (item.resourceId != null)
                                    {
                                        serviceResourceID = item.resourceId;
                                    }

                                    // Get the Azure Authority URI from the MS Backend
                                    if (serviceName == "aad")
                                    {
                                        // If the value read from config is empty
                                        if (String.IsNullOrEmpty(AuthorityURI))
                                        {
                                            // If the value read from the MS backend (which is typicaly best for this) is empty
                                            if (!String.IsNullOrEmpty(serviceEndpoint))
                                            {
                                                // Use a static default
                                                AuthorityURI = defaultAuthorityURI;
                                            }
                                            else
                                            {
                                                // Use the value received from the MS backend
                                                AuthorityURI = serviceEndpoint;
                                            }
                                        }
                                    }

                                    // Get the Power BI Resource URI
                                    if (serviceName == "powerbi-backend")
                                    {
                                        // If the value read from config is empty
                                        if (String.IsNullOrEmpty(PowerBIResourceURI))
                                        {
                                            // If the value read from the MS backendis empty
                                            if (!String.IsNullOrEmpty(serviceResourceID))
                                            {
                                                PowerBIResourceURI = defaultPowerBIResourceURI;
                                            }
                                            else
                                            {
                                                // Use the value received from the MS backend
                                                PowerBIResourceURI = serviceResourceID;
                                            }
                                        }
                                    }
                                } // foreach
                            } // rc.services

                            if (rc.clients != null)
                            {
                                Console.WriteLine("      - Clients received: " + rc.clients.Count);
                                foreach (PowerBIConfigClient item in rc.clients)
                                {
                                    string clientName = "";
                                    string clientAppID = "";
                                    string clientRedirectURI = "";
                                    string clientAppInsightsID = "";
                                    string clientLocalyticsID = "";

                                    if (item.name != null)
                                    {
                                        clientName = item.name;
                                    }

                                    if (item.appId != null)
                                    {
                                        clientAppID = item.appId;
                                    }

                                    if (item.redirectUri != null)
                                    {
                                        clientRedirectURI = item.redirectUri;
                                    }

                                    if (item.appInsightsId != null)
                                    {
                                        clientAppInsightsID = item.appInsightsId;
                                    }

                                    if (item.localyticsId != null)
                                    {
                                        clientLocalyticsID = item.localyticsId;
                                    }

                                    // Get the Power BI Gateway Mgmt URI and Redirect URI
                                    if (clientName == "powerbi-gateway")
                                    {
                                        // If the value read from config is empty
                                        if (String.IsNullOrEmpty(GatewayMgmtApplicationID))
                                        {
                                            // If the value read from the MS backend is empty
                                            if (!String.IsNullOrEmpty(clientAppID))
                                            {
                                                GatewayMgmtApplicationID = defaultGatewayMgmtApplicationID;
                                            }
                                            else
                                            {
                                                // Use the value received from the MS backend
                                                GatewayMgmtApplicationID = clientAppID;
                                            }
                                        }

                                        // If the value read from config is empty
                                        if (String.IsNullOrEmpty(PowerBIRedirectURI))
                                        {
                                            // If the value read from the MS backend is empty
                                            if (!String.IsNullOrEmpty(clientRedirectURI))
                                            {
                                                PowerBIRedirectURI = defaultPowerBIRedirectURI;
                                            }
                                            else
                                            {
                                                // Use the value received from the MS backend
                                                // "redirectUri":"urn:ietf:wg:oauth:2.0:oob",
                                                PowerBIRedirectURI = clientRedirectURI;
                                            }
                                        }
                                    }

                                } // foreach
                            } // rc.clients

                        } // rc
                    }
                    else
                    {
                        Console.WriteLine("   - No response content received for Power Platform Service Configuration request.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("   - Power Platform API Access Error (Service Configuration): " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - Power Platform API Access Error (Service Configuration): " + ex.ToString());
            }
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetServiceBackend Method
        /// 
        public void GetServiceBackend()
        {
            HttpResponseMessage response = null;
            HttpContent responseContent = null;
            string strContent = "";

            PowerBIBackend rc = null;

            string serviceURI = GlobalServiceEndpoint + ServiceBackendPath;

            // Set initial defaults
            GatewayMgmtEndpoint = defaultGatewayMgmtEndpoint;

            try
            {
                Console.WriteLine("");
                Console.WriteLine("- Retrieving service backend data from: " + serviceURI);

                response = gatewayClient.PutAsync(serviceURI, new StringContent("")).Result;

                Console.WriteLine("   - Response code received: " + response.StatusCode);

                try
                {
                    responseContent = response.Content;
                    strContent = responseContent.ReadAsStringAsync().Result;

                    if (strContent.Length > 0)
                    {
                        Console.WriteLine("   - De-serializing Service Backend Data...");

                        // Parse the JSON string into objects and store in DataTable
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        rc = js.Deserialize<PowerBIBackend>(strContent);

                        // Parse through rc
                        if (rc != null)
                        {
                            string fixedClusterURI = "";

                            if (rc.FixedClusterUri != null)
                            {
                                fixedClusterURI = rc.FixedClusterUri;

                                // Clean up the string
                                fixedClusterURI = fixedClusterURI.Replace("\\/\\/", "//");
                            }

                            if (!String.IsNullOrEmpty(fixedClusterURI))
                            {
                                GatewayMgmtEndpoint = fixedClusterURI;
                            }
                            else
                            {
                                GatewayMgmtEndpoint = defaultGatewayMgmtEndpoint;
                            }

                        } // rc
                    }
                    else
                    {
                        Console.WriteLine("   - No response content received for Power Platform Service Backend request.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("   - Power Platform API Access Error (Service Backend): " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - Power Platform API Access Error (Service Backend): " + ex.ToString());
            }
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetGatewayAgents Method
        /// 
        public void GetGatewayAgents()
        {
            HttpResponseMessage response = null;
            HttpContent responseContent = null;
            string strContent = "";

            PowerBIGateway[] rcArr = null;

            string serviceURL = GatewayMgmtEndpoint + "unifiedgateway/gatewayclusters";

            try
            {
                Console.WriteLine("");
                Console.WriteLine("- Retrieving data from: " + serviceURL);

                response = gatewayClient.GetAsync(serviceURL).Result;

                Console.WriteLine("   - Response code received: " + response.StatusCode);

                try
                {
                    responseContent = response.Content;
                    strContent = responseContent.ReadAsStringAsync().Result;

                    if (strContent.Length > 0)
                    {
                        Console.WriteLine("   - De-serializing Gateway Data...");

                        // Parse the JSON string into objects and store in DataTable
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        rcArr = js.Deserialize<PowerBIGateway[]>(strContent);

                        // Parse through rcArr
                        // Get the Gateways (GatewayAgent) for each and add some of these attributes to each to save
                        if (rcArr != null)
                        {
                            foreach (PowerBIGateway rc in rcArr)
                            {
                                if (rc != null)
                                {
                                    string gatewayID = "";
                                    string objectID = "";
                                    string gatewayName = "";
                                    string gatewayDescription = "";
                                    string publicKey = "";
                                    string keyword = "";
                                    string metadata = "";
                                    string loadBalancingSettings = "";
                                    string annotation = "";
                                    string versionStatus = "";
                                    string expiryDate = "";
                                    string type = "";
                                    string options = "";
                                    string email = "";
                                    string department = "";
                                    string primaryMachine = "";
                                    string primaryVersion = "";
                                    int agentNum = 1;


                                    if (rc.gatewayId != null)
                                    {
                                        gatewayID = rc.gatewayId;
                                    }

                                    if (rc.objectId != null)
                                    {
                                        objectID = rc.objectId;
                                    }

                                    if (rc.name != null)
                                    {
                                        gatewayName = rc.name;
                                    }

                                    if (rc.description != null)
                                    {
                                        gatewayDescription = rc.description;
                                    }

                                    if (rc.publicKey != null)
                                    {
                                        publicKey = rc.publicKey;
                                    }

                                    if (rc.keyword != null)
                                    {
                                        keyword = rc.keyword;
                                    }

                                    if (rc.metadata != null)
                                    {
                                        metadata = rc.metadata;
                                    }

                                    if (rc.loadBalancingSettings != null)
                                    {
                                        loadBalancingSettings = rc.loadBalancingSettings;
                                    }

                                    if (rc.annotation != null)
                                    {
                                        annotation = rc.annotation;
                                    }

                                    if (rc.versionStatus != null)
                                    {
                                        versionStatus = rc.versionStatus;
                                    }

                                    if (rc.expiryDate != null)
                                    {
                                        expiryDate = rc.expiryDate;
                                    }

                                    if (rc.type != null)
                                    {
                                        type = rc.type;
                                    }

                                    if (rc.options != null)
                                    {
                                        options = rc.options;
                                    }

                                    email = ParseContactInfoFromAnnotationString(annotation);
                                    primaryMachine = ParseMachineFromAnnotationString(annotation);
                                    primaryVersion = ParseVersionFromAnnotationString(annotation);
                                    department = ParseDepartmentFromAnnotationString(annotation);

                                    Console.WriteLine("");
                                    Console.WriteLine("-----------------------------------------------------------------");
                                    Console.WriteLine("Gateway Cluster:");
                                    Console.WriteLine("   Gateway Cluster Name: " + gatewayName);
                                    Console.WriteLine("   Gateway Cluster ID: " + objectID);
                                    Console.WriteLine("   Gateway Cluster Contact Email: " + email);
                                    Console.WriteLine("   Gateway Cluster Department: " + department);
                                    Console.WriteLine("   Gateway Cluster Primary Machine: " + primaryMachine);
                                    Console.WriteLine("   Gateway Cluster Original Version: " + primaryVersion);
                                    Console.WriteLine("");
                                    Console.WriteLine("   Installed Agents:");

                                    if (rc.gateways != null)
                                    {
                                        foreach (PowerBIGatewayAgent gw in rc.gateways)
                                        {
                                            string gwGatewayID = "";
                                            string gwGatewayObjectID = "";
                                            string gwGatewayName = "";
                                            string gwGatewayAnnotation = "";
                                            string gwGatewayStatus = "";
                                            bool isAnchorGateway = false;
                                            string gatewayClusterStatus = "";
                                            string gatewayLoadBalancingSettings = "";
                                            string gatewayStaticCapabilities = "";
                                            string gatewayPublicKey = "";
                                            string gatewayVersion = "";
                                            string gatewayVersionStatus = "";
                                            string gwEmail = "";
                                            string gwMachineName = "";
                                            string gwDepartment = "";
                                            string gwExpiryDate = "";

                                            isAnchorGateway = gw.isAnchorGateway;

                                            if (gw.gatewayId != null)
                                            {
                                                gwGatewayID = gw.gatewayId;
                                            }

                                            if (gw.gatewayObjectId != null)
                                            {
                                                gwGatewayObjectID = gw.gatewayObjectId;
                                            }

                                            if (gw.gatewayName != null)
                                            {
                                                gwGatewayName = gw.gatewayName;
                                            }

                                            if (gw.gatewayAnnotation != null)
                                            {
                                                gwGatewayAnnotation = gw.gatewayAnnotation;
                                            }

                                            if (gw.gatewayStatus != null)
                                            {
                                                gwGatewayStatus = gw.gatewayStatus;
                                            }

                                            if (gw.gatewayClusterStatus != null)
                                            {
                                                gatewayClusterStatus = gw.gatewayClusterStatus;
                                            }

                                            if (gw.gatewayLoadBalancingSettings != null)
                                            {
                                                gatewayLoadBalancingSettings = gw.gatewayLoadBalancingSettings;
                                            }

                                            if (gw.gatewayStaticCapabilities != null)
                                            {
                                                gatewayStaticCapabilities = gw.gatewayStaticCapabilities;
                                            }

                                            if (gw.gatewayPublicKey != null)
                                            {
                                                gatewayPublicKey = gw.gatewayPublicKey;
                                            }

                                            if (gw.gatewayVersion != null)
                                            {
                                                gatewayVersion = gw.gatewayVersion;
                                            }

                                            if (gw.gatewayVersionStatus != null)
                                            {
                                                gatewayVersionStatus = gw.gatewayVersionStatus;
                                            }

                                            if (gw.expiryDate != null)
                                            {
                                                gwExpiryDate = gw.expiryDate;
                                            }

                                            gwEmail = ParseContactInfoFromAnnotationString(gwGatewayAnnotation);
                                            gwMachineName = ParseMachineFromAnnotationString(gwGatewayAnnotation);
                                            gwDepartment = ParseDepartmentFromAnnotationString(gwGatewayAnnotation);

                                            if (agentNum > 1)
                                            {
                                                Console.WriteLine("      ---------------------------");

                                            }
                                            Console.WriteLine("      Agent #" + agentNum + " Name: " + gwGatewayName);
                                            Console.WriteLine("      Agent ID: " + gwGatewayObjectID);
                                            Console.WriteLine("      Machine Name: " + gwMachineName);
                                            Console.WriteLine("      Department: " + gwDepartment);
                                            Console.WriteLine("      Contact: " + gwEmail);
                                            Console.WriteLine("      Agent Version: " + gatewayVersion);
                                            Console.WriteLine("      Version Status: " + gatewayVersionStatus);
                                            Console.WriteLine("      Agent Status: " + gwGatewayStatus);
                                            Console.WriteLine("      Clustering Status: " + gatewayClusterStatus);
                                            Console.WriteLine("      Anchor (Primary) Gateway: " + isAnchorGateway);
                                            Console.WriteLine("");

                                            agentNum++;
                                        } // foreach
                                    }
                                    Console.WriteLine("-----------------------------------------------------------------");
                                    Console.WriteLine("");
                                }
                            } // foreach
                        } // rcArr
                    }
                    else
                    {
                        Console.WriteLine("   - No content received.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("   - API Access Error: " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - API Access Error: " + ex.ToString());
            }
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// ParseContactInfoFromAnnotationString Method
        ///
        public string ParseContactInfoFromAnnotationString(string inputString)
        {
            string retVal = "";

            int startIdx = 0;
            int endIdx = 0;
            string subStr = "";

            try
            {
                if (inputString.Contains("\"gatewayContactInformation\":"))
                {
                    startIdx = inputString.IndexOf("\"gatewayContactInformation\":");
                    endIdx = inputString.IndexOf("],");
                    subStr = inputString.Substring(startIdx, (endIdx - startIdx)).Replace("\"", "").Replace("[", "").Replace("]", "");

                    retVal = subStr.Substring(subStr.IndexOf(":") + 1, (subStr.Length - subStr.IndexOf(":")) - 1);
                }
            }
            catch
            {
            }

            return (retVal);
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// ParseVersionFromAnnotationString Method
        ///
        public string ParseVersionFromAnnotationString(string inputString)
        {
            string retVal = "";

            int startIdx = 0;
            int endIdx = 0;
            string subStr = "";

            try
            {
                if (inputString.Contains("\"gatewayVersion\":"))
                {
                    startIdx = inputString.IndexOf("\"gatewayVersion\":");
                    endIdx = inputString.Length - 1;

                    subStr = inputString.Substring(startIdx, (endIdx - startIdx));
                    subStr = subStr.Substring(0, (subStr.IndexOf("\",\""))).Replace("\"", "").Replace("[", "").Replace("]", "");

                    retVal = subStr.Substring(subStr.IndexOf(":") + 1, (subStr.Length - subStr.IndexOf(":")) - 1);
                }
            }
            catch
            {
            }

            return (retVal);
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// ParseMachineFromAnnotationString Method
        ///
        public string ParseMachineFromAnnotationString(string inputString)
        {
            string retVal = "";

            int startIdx = 0;
            int endIdx = 0;
            string subStr = "";

            try
            {
                if (inputString.Contains("\"gatewayMachine\":"))
                {
                    startIdx = inputString.IndexOf("\"gatewayMachine\":");
                    endIdx = inputString.Length - 1;

                    subStr = inputString.Substring(startIdx, (endIdx - startIdx));

                    if (subStr.Contains("\",\""))
                    {
                        endIdx = subStr.IndexOf("\",\"");
                    }
                    else
                    {
                        endIdx = subStr.Length - 1;
                    }

                    subStr = subStr.Substring(0, endIdx).Replace("\"", "").Replace("[", "").Replace("]", "");

                    retVal = subStr.Substring(subStr.IndexOf(":") + 1, (subStr.Length - subStr.IndexOf(":")) - 1);
                }
            }
            catch
            {
            }

            return (retVal);
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// ParseDepartmentFromAnnotationString Method
        ///
        public string ParseDepartmentFromAnnotationString(string inputString)
        {
            string retVal = "";

            int startIdx = 0;
            int endIdx = 0;
            string subStr = "";

            try
            {
                if (inputString.Contains("\"gatewayDepartment\":"))
                {
                    startIdx = inputString.IndexOf("\"gatewayDepartment\":");
                    endIdx = inputString.Length - 1;

                    subStr = inputString.Substring(startIdx, (endIdx - startIdx)).Replace("\"", "");

                    retVal = subStr.Substring(subStr.IndexOf(":") + 1, (subStr.Length - subStr.IndexOf(":")) - 1);
                }
            }
            catch
            {
            }

            return (retVal);
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Main Method
        ///    
        static void Main(string[] args)
        {
            PowerBIServiceGatewayAgentExportSampleMSAL pbi = new PowerBIServiceGatewayAgentExportSampleMSAL();

            pbi.Execute();

        }
    }
}
