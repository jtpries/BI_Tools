///===================================================================================
///   PowerBIServiceDataExportSample.cs
///===================================================================================
/// -- Author:       Jeff Pries (jeff@jpries.com)
/// -- Create date:  10/4/2019
/// -- Description:  Sample application to export data from the Power BI API using an interactive prompt for credentials
/// 

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace PowerBIDataExportSample
{
    class PowerBIDataExportSample
    {
        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Global Constants and Variables
        ///    

        // Constants
        const string HTTPHEADUSERAGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
        const string PBI_API_URLBASE = "https://api.powerbi.com/v1.0/myorg/";

        const string AuthorityURL = "https://login.windows.net/common/oauth2/authorize";  // use with 3.19.8

        const string ResourceURL = "https://analysis.windows.net/powerbi/api";
        const string RedirectURL = "https://dev.powerbi.com/Apps/SignInRedirect";
        const string ApplicationID = "11111111-1111-1111-1111-111111111111"; // Native Azure AD App ClientID  --  Put your Client ID here

        const string UserName = "jeff@contoso.com";  // Put your Active Directory / Power BI Username here (note this is not a secure place to store this!)
        const string Password = "mysecretpassword";  // Put your Active Directory / Power BI Password here (note this is not secure pace to store this!  this is a sample only)

        // Variables 
        HttpClient client = null;


        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Default Class constructor
        ///    
        public PowerBIDataExportSample()
        {
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Execute Method
        ///    
        public void Execute(string authType)
        {
            string authToken = "";

            // Get an authentication token
            authToken = GetAuthTokenUser(authType);  // Uses native AD auth

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
        /// GetAuthUserLogin Method (Interactive)
        /// 
        public async Task<AuthenticationResult> GetAuthUserLoginInteractive()
        {
            AuthenticationResult authResult = null;

            PlatformParameters parameters = new PlatformParameters(PromptBehavior.Always);

            try
            {
                // Query Azure AD for an interactive login prompt and subsequent Power BI auth token
                AuthenticationContext authContext = new AuthenticationContext(AuthorityURL);
                authResult = await authContext.AcquireTokenAsync(ResourceURL, ApplicationID, new Uri(RedirectURL), parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - Error acquiring token with interactive credentials.");
                Console.WriteLine("     Usually this is due to an invalid username or password.");
                Console.WriteLine("");
                Console.WriteLine("     Details: " + ex.Message);
                authResult = null;
            }

            return authResult;
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetAuthUserLogin Method (Saved Credential)
        /// 
        public async Task<AuthenticationResult> GetAuthUserLoginSavedCredential()
        {
            AuthenticationResult authResult = null;

            PlatformParameters parameters = new PlatformParameters(PromptBehavior.Always);

            try
            {
                // Query Azure AD for an interactive login prompt and subsequent Power BI auth token
                AuthenticationContext authContext = new AuthenticationContext(AuthorityURL);

                UserPasswordCredential userPasswordCredential = new UserPasswordCredential(UserName, Password);
                authResult = await authContext.AcquireTokenAsync(ResourceURL, ApplicationID, userPasswordCredential);
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - Error acquiring token with saved credentials.");
                Console.WriteLine("     Usually this is due to an invalid username or password.");
                Console.WriteLine("");
                Console.WriteLine("     Details: " + ex.Message);
                authResult = null;
            }

            return authResult;
        }


        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetAuthToken Method (Interactive and Saved Credential)
        /// 
        public string GetAuthTokenUser(string authType)
        {
            Task<AuthenticationResult> authResult = null;
            string authToken = "";

            Console.WriteLine("- Performing App authentication to request API access token...");
            if (authType == "SavedCredential")
            {
                authResult = GetAuthUserLoginSavedCredential();
            }
            else
            {
                authResult = GetAuthUserLoginInteractive();
            }

            if (authResult != null)
            {
                authResult.Wait();

                var auth = authResult.Result;

                if (auth != null)
                {
                    authToken = auth.CreateAuthorizationHeader();
                    if (authToken.Substring(0, 6) == "Bearer")
                    {
                        Console.WriteLine("   - API Authorization token received.");
                    }
                }
            }

            return authToken;
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// InitHttpClient Method
        ///    
        public void InitHttpClient(string authToken)
        {
            Console.WriteLine("");
            Console.WriteLine("- Initialzing client with generated auth token...");

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

            string serviceURL = PBI_API_URLBASE + "groups";

            try
            {
                Console.WriteLine("");
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
            PowerBIDataExportSample pbi = new PowerBIDataExportSample();
            string key = "";

            Console.WriteLine("Login method?");
            Console.WriteLine("[1] Interactive login prompt");
            Console.WriteLine("[2] Saved credential in program source file");
            Console.WriteLine("");

            key = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine("");


            if (key == "1")
            {
                Console.WriteLine("Executing using an interactive prompt for credentials.");
                pbi.Execute("InteractiveLogin");
            }
            else if (key == "2")
            {
                Console.WriteLine("Executing using saved credential.");
                pbi.Execute("SavedCredential");
            }
            else
            {
                Console.WriteLine("Please choose 1 or 2");
            }
            
        }
    }
}
