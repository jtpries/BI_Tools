///===================================================================================
///   PowerBIServiceDataExportSample.cs
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
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace PowerBIDataExportSampleSPN
{
    class PowerBIDataExportSampleSPN
    {
        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Global Constants and Variables
        ///    

        // Constants
        const string HTTPHEADUSERAGENT = "PowerBIDataExportSampleSPN/1.0";

        // Standard values for Power BI REST API Azure AD Authentication with MSAL
        const string PBI_API_URLBASE = "https://api.powerbi.com/v1.0/myorg/";
        const string MSALAuthorityURIBase = "https://login.microsoftonline.com/";
        const string PowerBIResourceURI = "https://analysis.windows.net/powerbi/api";
        const string PowerBIRedirectURI = "urn:ietf:wg:oauth:2.0:oob";
        const string PowerBIAccessScope = "https://analysis.windows.net/powerbi/api/.default";

        // User specific configuration values (hardcoded in plaintext for demo only)
        const string AzureTenantID = "11111111-1111-1111-1111-111111111111";     // Azure Tenant ID -- Put your Azure Tenant ID here (for demo only)
        const string ApplicationID = "22222222-2222-2222-2222-222222222222";     // Azure AD Application  -- Put your Application (Client ID) here (for demo only)
        const string ClientSecret = "abcdef.ghijkl.mnoPqRsTuvWxYZ";              // Application Client Secret  -- Put your Client Secret here (for demo only) 


        // Variables 
        HttpClient client = null;
        string MSALAuthorityURI = MSALAuthorityURIBase + AzureTenantID;


        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Default Class constructor
        ///    
        public PowerBIDataExportSampleSPN()
        {
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Execute Method
        ///    
        public void Execute()
        {
            string authToken = "";

            // Get an authentication token
            authToken = GetAuthToken();  

            // Initialize the client with the token
            if (!String.IsNullOrEmpty(authToken))
            {
                InitHttpClient(authToken);

                GetWorkspaces();

                Console.WriteLine("");
                Console.WriteLine("- Done!");
            }
        }


        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetAuthUserLoginSavedSPN Method (Saved SPN Auth)
        /// 
        public async Task<Microsoft.Identity.Client.AuthenticationResult> GetAuthUserLoginSavedSPN()
        {
            Microsoft.Identity.Client.AuthenticationResult authResult = null;

            try
            {
                Console.WriteLine("        Authority URI: " + MSALAuthorityURI);
                Console.WriteLine("        Resource URI: " + PowerBIResourceURI);
                Console.WriteLine("        Application ID: " + ApplicationID);
                Console.WriteLine("        Access Scope: " + PowerBIAccessScope);
                //Console.WriteLine("        Client Secret: " + ClientSecret);

                // Build the access scope list
                List<string> scopes = new List<string>();
                scopes.Add(PowerBIAccessScope);

                // Build the authentication request client
                var confidentialClient = ConfidentialClientApplicationBuilder
                        .Create(ApplicationID)
                        .WithClientSecret(ClientSecret)
                        .WithAuthority(new Uri(MSALAuthorityURI))
                        .WithRedirectUri(PowerBIRedirectURI)
                        .Build();

                // Request an access token with the request client and scope
                var accessTokenRequest = confidentialClient.AcquireTokenForClient(scopes);

                authResult = await accessTokenRequest.ExecuteAsync();
            }
            catch (AggregateException ex)
            {
                MsalServiceException ex1 = (MsalServiceException)ex.GetBaseException();

                Console.WriteLine("   - Error acquiring token with ApplicationID/Secret (SPN).");
                Console.WriteLine("     Usually this is due to an invalid Application (client) ID or Client Secret");
                Console.WriteLine("");
                Console.WriteLine("     Details: " + ex1.StatusCode + ": " + ex1.ResponseBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - Error acquiring token with ApplicationID/Secret (SPN).");
                Console.WriteLine("     Usually this is due to an invalid Application (client) ID or Client Secret");
                Console.WriteLine("");
                Console.WriteLine("     Details: " + ex.Message.ToString());
            }

            return authResult;
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetAuthToken Method - Call the appropriate methods to get a token and return an auth string
        /// 
        public string GetAuthToken()
        {
            Task<Microsoft.Identity.Client.AuthenticationResult> authResultMSAL = null;

            string authToken = "";

            Console.WriteLine("");
            Console.WriteLine("- Performing App authentication to request API access token [SPN]...");

            // Use SPN authentication
            Console.WriteLine("   - Attempting login with Application SPN (Client Secret) (MSAL).");

            authResultMSAL = GetAuthUserLoginSavedSPN();

            // Wait for the authentication to complete to check the result
            if (authResultMSAL != null)
            {
                authResultMSAL.Wait();
            }

            // If authentication result received, get the token
            if (authResultMSAL != null)
            {
                if (authResultMSAL.Result != null)
                {
                    // Create the bearer token to be used for API methods
                    authToken = authResultMSAL.Result.CreateAuthorizationHeader();
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
            

            // AuthToken will be populated if authentication was successful and blank of not.  Return value.
            return authToken;
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
            Console.WriteLine("- Initializing Power BI API client with received auth token [" + authTokenDisplay + "]");
            Console.WriteLine("");

            // Create the web client connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(HTTPHEADUSERAGENT);
            client.DefaultRequestHeaders.Add("Authorization", authToken);
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetWorkspaces Method
        /// 
        public void GetWorkspaces()
        {
            HttpResponseMessage response = null;
            HttpContent responseContent = null;
            string strContent = "";

            PowerBIWorkspace rc = null;

            string serviceURL = PBI_API_URLBASE + "/admin/groups?$top=5";  // actual max is 5000

            try
            {
                Console.WriteLine("- Getting a sampling of up to 5 Workspaces from the Power BI Admin API.");
                Console.WriteLine("- Retrieving data from: " + serviceURL);

                response = client.GetAsync(serviceURL).Result;

                Console.WriteLine("   - Response code received: " + response.StatusCode);
                //Console.WriteLine(response);  // debug
                try
                {
                    responseContent = response.Content;
                    strContent = responseContent.ReadAsStringAsync().Result;

                    if (strContent.Length > 0)
                    {
                        Console.WriteLine("   - De-serializing Workspace Data...");

                        // Parse the JSON string into objects and store in DataTable
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        js.MaxJsonLength = 2147483647;  // Set the maximum json document size to the max
                        rc = js.Deserialize<PowerBIWorkspace>(strContent);

                        if (rc != null)
                        {
                            if (rc.value != null)
                            {
                                Console.WriteLine("      - Workspaces received: " + rc.value.Count);
                                foreach (PowerBIWorkspaceValue item in rc.value)
                                {
                                    string workspaceID = "";
                                    string workspaceName = "";
                                    string workspaceDescription = "";
                                    string capacityID = "";
                                    string dataflowStorageID = "";
                                    bool isOnDedicatedCapacity = false;
                                    bool isReadOnly = false;
                                    bool isOrphaned = false;
                                    string workspaceState = "";
                                    string workspaceType = "";

                                    if (item.id != null)
                                    {
                                        workspaceID = item.id;
                                    }

                                    if (item.name != null)
                                    {
                                        workspaceName = item.name;
                                    }

                                    if (item.description != null)
                                    {
                                        workspaceDescription = item.description;
                                    }

                                    if (item.capacityId != null)
                                    {
                                        capacityID = item.capacityId;
                                    }

                                    if (item.dataflowStorageId != null)
                                    {
                                        dataflowStorageID = item.dataflowStorageId;
                                    }

                                    if (item.type != null)
                                    {
                                        workspaceType = item.type;
                                    }

                                    isOnDedicatedCapacity = item.isOnDedicatedCapacity;
                                    isReadOnly = item.isReadOnly;
                                    isOrphaned = item.isOrphaned;

                                    if (item.state != null)
                                    {
                                        workspaceState = item.state;
                                    }

                                    if (item.type != null)
                                    {
                                        workspaceState = item.type;
                                    }

                                    // Output the Workspace Data
                                    Console.WriteLine("");
                                    Console.WriteLine("----------------------------------------------------------------------------------");
                                    Console.WriteLine("");
                                    Console.WriteLine("Workspace ID: " + workspaceID);
                                    Console.WriteLine("Workspace Name: " + workspaceName);
                                    Console.WriteLine("Workspace Description: " + workspaceDescription);
                                    Console.WriteLine("Capacity ID: " + capacityID);
                                    Console.WriteLine("Dataflow Storage ID: " + dataflowStorageID);
                                    Console.WriteLine("On Dedicated Capacity: " + isOnDedicatedCapacity);
                                    Console.WriteLine("Read Only: " + isReadOnly);
                                    Console.WriteLine("Orphaned: " + isOrphaned);
                                    Console.WriteLine("WorkspaceState: " + workspaceState);
                                    Console.WriteLine("WorkspaceType: " + workspaceType);

                                } // foreach
                            } // rc.value
                        } // rc

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
        /// Main Method
        ///    
        static void Main(string[] args)
        {
            PowerBIDataExportSampleSPN pbi = new PowerBIDataExportSampleSPN();

            pbi.Execute();
            
        }
    }
}
