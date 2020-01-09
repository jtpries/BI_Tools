///===================================================================================
///   PowerBIServiceGateways.cs
///===================================================================================
/// -- Author:       Jeff Pries (jeff@jpries.com)
/// -- Create date:  10/9/2019
/// -- Description:  Main driver class for Power BI Service Gateway Export application

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace PowerBIServiceGatewayExport
{
    class PowerBIServiceGatewayExport
    {
        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Global Constants and Variables
        ///    

        // Constants
        const string VERSION = "0.1";

        // Variables 
        string HttpHeadUserAgent = "";
        HttpClient noAuthClient = null;
        HttpClient gatewayClient = null;
        string destDBConnString;
        SqlConnectionStringBuilder destDBCSB;
        string connString;

        string AuthorityURL = "";
        string PowerBIResourceURL = "";
        string PowerBIRedirectURL = "";
        string GlobalServiceEndpoint = "";
        string GatewayMgmtEndpoint = "";
        string ServiceConfigPath = "";
        string ServiceBackendPath = "";
        string GatewayMgmtApplicationID = "";
        string OutputTableName = "";
        string UserName = "";
        SecureString Password = new SecureString();
        byte[] encKey = null;
        byte[] encIV = null;

        bool isStop = false;
        bool isInteractive = false;
        bool isUpdatePassword = false;
        bool isUpdateUsername = false;
        bool isGatewayAgents = false;
        bool isDBOutput = false;

        DataTable GatewayAgents = null;

        enum ExitCodes : int
        {
            Success = 0,
            Warning = 100,
            Warning_Authentication = 101,
            Error = 1000,
            Error_Authentication = 1001
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Default Class constructor
        ///  
        public PowerBIServiceGatewayExport()
        {
            Environment.ExitCode = (int)ExitCodes.Success; // Success unless specified otherwise elsewhere
            ReadSettings();
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// ReadSettings Method
        ///    
        public void ReadSettings()
        {
            encKey = Encoding.UTF8.GetBytes("b15ccb449ae879e8");
            encIV = Encoding.UTF8.GetBytes("4b1f9e8fb137fc01");

            // Read DB connection string - DestDB
            var destDBConnStringObj = ConfigurationManager.ConnectionStrings["DestDB"];
            if (destDBConnStringObj != null)
            {
                destDBConnString = destDBConnStringObj.ConnectionString;
                destDBCSB = new SqlConnectionStringBuilder(destDBConnString);
            }

            if (destDBCSB != null)
            {
                connString = destDBCSB.ConnectionString;
            }

            // Read the App Settings
            HttpHeadUserAgent = ConfigurationManager.AppSettings["HttpUserAgentString"];
            AuthorityURL = ConfigurationManager.AppSettings["AzureADAuthorityURL"];
            GlobalServiceEndpoint = ConfigurationManager.AppSettings["GlobalServiceEndpoint"];
            ServiceConfigPath = ConfigurationManager.AppSettings["ServiceConfigPath"];
            ServiceBackendPath = ConfigurationManager.AppSettings["ServiceBackendPath"];

            PowerBIResourceURL = ConfigurationManager.AppSettings["PowerBIResourceURL"];
            PowerBIRedirectURL = ConfigurationManager.AppSettings["PowerBIRedirectURL"];
            GatewayMgmtApplicationID = ConfigurationManager.AppSettings["GatewayMgmtApplicationID"];

            OutputTableName = ConfigurationManager.AppSettings["OutputTableName"];
            UserName = ConfigurationManager.AppSettings["UserName"];

            string pwRead = ConfigurationManager.AppSettings["Password"];
            if (pwRead.Length > 0)
            {
                Password = DecryptString(pwRead);
                if (Password == null)
                {
                    Console.WriteLine("");
                    Console.WriteLine("");
                    Console.WriteLine("- Password not read from file (blank value or incorrect encryption stored.");
                }
            }
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// PerformWelcome Method
        ///    

        public void PerformWelcome()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("================================================================");
            Console.WriteLine("||             Power BI Service Gateway Export v" + VERSION + "              ||");
            Console.WriteLine("||                by Jeff Pries (jeff@jpries.com)                ||");
            Console.WriteLine("================================================================");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// ParseArgs Method
        ///    
        public void ParseArgs(string[] args)
        {
            // Initial settings
            isStop = false;
            isInteractive = false;
            isDBOutput = false;
            isUpdatePassword = false;
            isUpdateUsername = false;
            isGatewayAgents = false;

            // Check for args, if none, set default settings
            if (args.Length == 0)
            {
                Console.WriteLine("- No arguments specified, running in default mode.  Run with /help to see available options.");
                Console.WriteLine("");

                // Default settings
                isStop = false;
                isUpdatePassword = false;
                isUpdateUsername = false;
                isGatewayAgents = true;
            }
            else
            {
                // Search the args array and set the option flags accordingly
                foreach (string arg in args)
                {
                    string argStr = arg.ToLower();

                    if (argStr == "/?" || argStr == "/help")
                    {
                        Console.WriteLine("");
                        Console.WriteLine("This is a sample program to query Power BI Gateway information from the Power Platform API.");
                        Console.WriteLine("");
                        Console.WriteLine("By default, it will attempt to use credentials saved in the .config file and if those fail, it will prompt for credentials.");
                        Console.WriteLine("If you wish, instead, to only prompt for credentials, run with the /interactive option.");
                        Console.WriteLine("");
                        Console.WriteLine("The password saved in the .config file is encrypted.  To save a new password in the file, use the /savepassword:<password> command (without <>'s)");
                        Console.WriteLine("Note that the .config file should be treated as sensitive and secured as it is possible to decrypt a password from this file.");
                        Console.WriteLine("");
                        Console.WriteLine("Options:");
                        Console.WriteLine("     /interactive                    - Login with an interactive login prompt");
                        Console.WriteLine("     /dboutput                       - Output the Gateway Agent query results to database table (must already exist)");
                        Console.WriteLine("     /saveusername:<username>        - Save a username value for non-interactive login");
                        Console.WriteLine("     /savepassword:<password>        - Save an encrypted password value for non-interactive login");

                        isStop = true;
                        break;
                    }
                    else if (argStr.Contains("/interactive"))
                    {
                        isInteractive = true;
                        isGatewayAgents = true;
                    }
                    else if (argStr.Contains("/dboutput"))
                    {
                        isDBOutput = true;
                        isGatewayAgents = true;
                    }
                    else if (argStr.Contains("/saveusername"))
                    {
                        // Username
                        if (argStr.Substring(0, 13) == "/saveusername" && argStr.Length == 13)
                        {
                            Console.WriteLine("To save a username to config, it must be in format:  /saveusername:<username value>");
                            isStop = true;
                            break;
                        }
                        else if (argStr.Substring(0, 13) == "/saveusername" && argStr.Length >= 14)
                        {
                            if (argStr.Substring(13, 1) == ":" && argStr.Length < 15)
                            {
                                Console.WriteLine("To save a username to config, it must be in format:  /saveusername:<username value>");
                                isStop = true;
                                break;
                            }
                            else
                            {
                                isUpdateUsername = true;
                                UserName = arg.Substring(14);
                                break;
                            }
                        }
                    }
                    else if (argStr.Contains("/savepassword"))
                    {
                        // Password
                        if (argStr.Substring(0, 13) == "/savepassword" && argStr.Length == 13)
                        {
                            Console.WriteLine("To save a password to config, it must be in format:  /savepassword:<password value>");
                            isStop = true;
                            break;
                        }
                        else if (argStr.Substring(0, 13) == "/savepassword" && argStr.Length >= 14)
                        {
                            if (argStr.Substring(13, 1) == ":" && argStr.Length < 15)
                            {
                                Console.WriteLine("To save a password to config, it must be in format:  /savepassword:<password value>");
                                isStop = true;
                                break;
                            }
                            else
                            {
                                isUpdatePassword = true;
                                Password = new NetworkCredential("", arg.Substring(14)).SecurePassword;
                                break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid command option entered: " + arg);
                        isStop = true;
                        break;
                    }
                } // foreach
            }
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Execute Method
        ///    
        public void Execute(string[] args)
        {
            string gatewayAuthToken = "";

            PerformWelcome();

            ParseArgs(args);

            if (isUpdatePassword)
            {
                SaveSetting("Password", "");
                isStop = true;
            }

            if (isUpdateUsername)
            {
                SaveSetting("UserName", UserName);
                isStop = true;
            }

            // Main data api query execution
            if (!isStop)
            {
                // Initialize the No Auth client for getting basic Power BI settings
                InitNoAuthHttpClient();

                // Get the generic API configuration objects from the MS directory using unauthenticated client
                GetServiceConfig();

                // Get an authentication token for Gateway Mgmt
                gatewayAuthToken = GetAuthTokenUser();  // Uses native AD auth

                if (String.IsNullOrEmpty(gatewayAuthToken))
                {
                    isStop = true;
                }

                if (!isStop)
                {
                    // Initialize the client with the token
                    InitHttpClient(gatewayAuthToken);

                    // Get the PBI Gateway backend config info using authenticated gatewayClient
                    GetServiceBackend();

                    Console.WriteLine("");
                    Console.WriteLine("     --------------------------------------------------------------------------------------");
                    Console.WriteLine("     Power BI Gateway Mgmt Base: " + GatewayMgmtEndpoint);
                    Console.WriteLine("     --------------------------------------------------------------------------------------");
                    Console.WriteLine("");
                    

                    if (isGatewayAgents)
                    {
                        GetGatewayAgents();
                    }
                }
            }

            Console.WriteLine("");
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// SaveSetting Method
        /// 
        public void SaveSetting(string settingName, string settingValue)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (settingName == "Password")
            {
                config.AppSettings.Settings[settingName].Value = EncryptString(Password);
                Password.Dispose();
            }
            else
            {
                config.AppSettings.Settings[settingName].Value = settingValue;
            }

            config.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");

            if (settingName == "Password")
            {
                Console.WriteLine("   - Encrypted password saved to configuration file.");
            }
            else
            {
                Console.WriteLine("   - " + settingName + " saved to configuration file.");
            }
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// EncryptString Method
        /// 
        public string EncryptString(SecureString inputString)
        {
            string encStr = "";
            string retVal = "";

            // Unpack the SecureString to a string for handling
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(inputString);
            try
            {
                encStr = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }

            // Encrypt the string
            try
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;
                    aes.Key = encKey;
                    aes.IV = encIV;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            // Create StreamWriter and write data to a stream    
                            using (StreamWriter sw = new StreamWriter(cs))
                            {
                                sw.Write(encStr);
                            }
                            retVal = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                }
            }
            catch
            {
                retVal = null;
                Environment.ExitCode = (int)ExitCodes.Warning_Authentication;
            }

            return (retVal);
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// DecryptString Method
        /// 
        public SecureString DecryptString(string inputString)
        {
            SecureString retVal = null;

            // Decrypt the string
            try
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;
                    aes.Key = encKey;
                    aes.IV = encIV;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(inputString)))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            // Create StreamReader to read data    
                            using (StreamReader reader = new StreamReader(cs))
                            {
                                retVal = new NetworkCredential("", reader.ReadToEnd()).SecurePassword;
                            }
                        }
                    }
                }
            }
            catch
            {
                retVal = null;
                Console.WriteLine("");
                Console.WriteLine("- Stored password blank or unable to be decrypted.");
            }

            return (retVal);
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

                Console.WriteLine("        Authority URL: " + AuthorityURL);
                Console.WriteLine("        Resource URL: " + PowerBIResourceURL);
                Console.WriteLine("        Application ID: " + GatewayMgmtApplicationID);
                Console.WriteLine("        Redirect URL: " + PowerBIRedirectURL);

                authResult = await authContext.AcquireTokenAsync(PowerBIResourceURL, GatewayMgmtApplicationID, new Uri(PowerBIRedirectURL), parameters);

            }
            catch (Exception ex)
            {
                Console.WriteLine("   - Error acquiring token with interactive credentials.  ");
                Console.WriteLine("     Usually this is due to an invalid username or password.");
                Console.WriteLine("");
                Console.WriteLine("     Details: " + ex.Message);
                Environment.ExitCode = (int)ExitCodes.Warning_Authentication;
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
                // Query Azure AD for an interactive login prompt and provide it with credentials and get subsequent Power BI auth token
                AuthenticationContext authContext = new AuthenticationContext(AuthorityURL);

                UserPasswordCredential userPasswordCredential = new UserPasswordCredential(UserName, Password);

                Console.WriteLine("        Authority URL: " + AuthorityURL);
                Console.WriteLine("        Resource URL: " + PowerBIResourceURL);
                Console.WriteLine("        Application ID: " + GatewayMgmtApplicationID);
                Console.WriteLine("        Username: " + UserName);

                authResult = await authContext.AcquireTokenAsync(PowerBIResourceURL, GatewayMgmtApplicationID, userPasswordCredential);
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - Error acquiring token with saved credentials.  ");
                Console.WriteLine("     Usually this is due to an invalid username or password.");
                Console.WriteLine("");
                Console.WriteLine("     Details: " + ex.Message);
                Environment.ExitCode = (int)ExitCodes.Warning_Authentication;
            }

            return authResult;
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetAuthToken Method 
        /// 
        public string GetAuthTokenUser()
        {
            Task<AuthenticationResult> authResult = null;
            string authToken = "";

            Console.WriteLine("");
            Console.WriteLine("- Performing App authentication to request API access token...");

            // If saved credentials in use, attempt to login with them
            if ((!String.IsNullOrEmpty(UserName) && Password != null && Password.Length > 0) && isInteractive == false)
            {
                Console.WriteLine("   - Attempting login with saved credentials.");
                authResult = GetAuthUserLoginSavedCredential();

                // Wait for the authentication to complete to check the result
                if (authResult != null)
                {
                    authResult.Wait();
                }
            }

            // If saved credentials not in use or failed, prompt for interactive login
            if ((String.IsNullOrEmpty(UserName) && Password == null) || authResult == null || authResult.Result == null || isInteractive == true)
            {
                Console.Write("   - Attempting login with interactive credentials");
                if (String.IsNullOrEmpty(UserName))
                {
                    Console.Write(" (Username empty)");
                }
                if (Password == null)
                {
                    Console.Write(" (Password empty or not decrypted)");
                }
                if (isInteractive == true)
                {
                    Console.Write(" (Interactive login requested)");
                }
                if (authResult == null || authResult.Result == null && !String.IsNullOrEmpty(UserName) && Password != null)
                {
                    Console.Write(" (Saved authentication failed/skipped)");
                }
                Console.WriteLine("");
                authResult = GetAuthUserLoginInteractive();

                // Wait for the authentication to complete to check the result
                if (authResult != null)
                {
                    authResult.Wait();
                }
            }

            // If authentication result received, get the token
            if (authResult != null)
            {
                if (authResult.Result != null)
                {
                    authToken = authResult.Result.CreateAuthorizationHeader();
                    if (authToken.Substring(0, 6) == "Bearer")
                    {
                        Console.WriteLine("   - API Authorization token received.");
                    }
                    else
                    {
                        Console.WriteLine("   - Unable to retrieve API Authorization token.");
                        Environment.ExitCode = (int)ExitCodes.Error_Authentication;
                    }
                }
                else
                {
                    Console.WriteLine("   - Unable to retrieve API Authorization token.");
                    Environment.ExitCode = (int)ExitCodes.Error_Authentication;
                }
            }
            else
            {
                Console.WriteLine("   - Unable to retrieve API Authorization token.");
                Environment.ExitCode = (int)ExitCodes.Error_Authentication;
            }

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
        public void InitHttpClient(string gatewayAuthToken)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            Console.WriteLine("");
            Console.Write("- Initialzing Gateway Mgmt API client with generated auth token [");
            if (gatewayAuthToken.Length >= 40)
            {
                Console.WriteLine(gatewayAuthToken.Substring(0, 40) + "...]");
            }
            else
            {
                Console.WriteLine("None]");
            }

            // Create the gateway management web client connection
            gatewayClient = new HttpClient();
            gatewayClient.DefaultRequestHeaders.UserAgent.ParseAdd(HttpHeadUserAgent);
            gatewayClient.DefaultRequestHeaders.Add("Authorization", gatewayAuthToken);

            // Clear out the save password object
            Password.Dispose();
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

            string defaultAuthorityURL = "https://login.windows.net/common/oauth2/authorize";
            string defaultPowerBIResourceURL = "https://analysis.windows.net/powerbi/api";
            string defaultGatewayMgmtApplicationID = "ea0616ba-638b-4df5-95b9-636659ae5121";
            string defaultPowerBIRedirectURL = "urn:ietf:wg:oauth:2.0:oob";

            PowerBIConfig rc = null;

            string serviceURL = "";

            // Set initial defaults
            AuthorityURL = defaultAuthorityURL;
            PowerBIResourceURL = defaultPowerBIResourceURL;
            GatewayMgmtApplicationID = defaultGatewayMgmtApplicationID;
            PowerBIRedirectURL = defaultPowerBIRedirectURL;


            if (String.IsNullOrEmpty(UserName) || isInteractive == true)
            {
                Console.Write("Power BI Service User Name (user@domain.com):  ");
                UserName = Console.ReadLine();
            }

            serviceURL = GlobalServiceEndpoint + ServiceConfigPath + UserName;

            try
            {
                Console.WriteLine("");
                Console.WriteLine("- Retrieving configuration data from: " + serviceURL);

                response = noAuthClient.PostAsync(serviceURL, new StringContent("")).Result;

                Console.WriteLine("   - Response code received: " + response.StatusCode);
                //Console.WriteLine(response);  // debug

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

                                    // Get the Azure Authority URL from the MS Backend
                                    if (serviceName == "aad")
                                    {
                                        // If the value read from config is empty
                                        if (String.IsNullOrEmpty(AuthorityURL))
                                        {
                                            // If the value read from the MS backend (which is typicaly best for this) is empty
                                            if (!String.IsNullOrEmpty(serviceEndpoint))
                                            {
                                                // Use a static default
                                                AuthorityURL = defaultAuthorityURL;
                                            }
                                            else
                                            {
                                                // Use the value received from the MS backend
                                                // "endpoint":"https://login.microsoftonline.com/common",
                                                AuthorityURL = serviceEndpoint;
                                            }
                                        }
                                    }

                                    // Get the Power BI Resource URL
                                    if (serviceName == "powerbi-backend")
                                    {
                                        // If the value read from config is empty
                                        if (String.IsNullOrEmpty(PowerBIResourceURL))
                                        {
                                            // If the value read from the MS backendis empty
                                            if (!String.IsNullOrEmpty(serviceResourceID))
                                            {
                                                PowerBIResourceURL = defaultPowerBIResourceURL;
                                            }
                                            else
                                            {
                                                // Use the value received from the MS backend
                                                // "resourceId":"https://analysis.windows.net/powerbi/api",
                                                PowerBIResourceURL = serviceResourceID;
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

                                    // Get the Power BI Gateway Mgmt URL and Redirect URI
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
                                                // "appId":"ea0616ba-638b-4df5-95b9-636659ae5121",
                                                GatewayMgmtApplicationID = clientAppID;
                                            }
                                        }

                                        // If the value read from config is empty
                                        if (String.IsNullOrEmpty(PowerBIRedirectURL))
                                        {
                                            // If the value read from the MS backend is empty
                                            if (!String.IsNullOrEmpty(clientRedirectURI))
                                            {
                                                PowerBIRedirectURL = defaultPowerBIRedirectURL;
                                            }
                                            else
                                            {
                                                // Use the value received from the MS backend
                                                // "redirectUri":"urn:ietf:wg:oauth:2.0:oob",
                                                PowerBIRedirectURL = clientRedirectURI;
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
                        Environment.ExitCode = (int)ExitCodes.Warning;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("   - Power Platform API Access Error (Service Configuration): " + ex.ToString());
                    Environment.ExitCode = (int)ExitCodes.Warning;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - Power Platform API Access Error (Service Configuration): " + ex.ToString());
                Environment.ExitCode = (int)ExitCodes.Warning;
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

            string defaultGatewayMgmtEndpoint = "https://wabi-west-us-redirect.analysis.windows.net/";

            PowerBIBackend rc = null;

            string serviceURL = GlobalServiceEndpoint + ServiceBackendPath;

            // Set initial defaults
            GatewayMgmtEndpoint = defaultGatewayMgmtEndpoint;


            try
            {
                Console.WriteLine("");
                Console.WriteLine("- Retrieving service backend data from: " + serviceURL);

                response = gatewayClient.PutAsync(serviceURL, new StringContent("")).Result;

                Console.WriteLine("   - Response code received: " + response.StatusCode);
                //Console.WriteLine(response);  // debug
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
                        Console.WriteLine("   - No response content received for Power Platform Service Backend request");
                        Environment.ExitCode = (int)ExitCodes.Warning;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("   - Power Platform API Access Error (Service Backend): " + ex.ToString());
                    Environment.ExitCode = (int)ExitCodes.Warning;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - Power Platform API Access Error (Service Backend): " + ex.ToString());
                Environment.ExitCode = (int)ExitCodes.Warning;
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
            GatewayAgents = new DataTable();

            GatewayAgents.TableName = OutputTableName;
            GatewayAgents.Columns.Add("GatewayAgentInternalID", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentID", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentName", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentAnnotation", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentStatus", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentIsAnchor", typeof(bool));
            GatewayAgents.Columns.Add("GatewayAgentClusterStatus", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentLoadBalancingSettings", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentStaticCapabilities", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentPublicKey", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentVersion", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentVersionStatus", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentEmail", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentMachineName", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentDepartment", typeof(string));
            GatewayAgents.Columns.Add("GatewayAgentExpiryDate", typeof(string));
            GatewayAgents.Columns.Add("GatewayClusterInternalID", typeof(string));
            GatewayAgents.Columns.Add("GatewayClusterID", typeof(string));
            GatewayAgents.Columns.Add("GateawyClusterName", typeof(string));

            PowerBIGateway[] rcArr = null;

            string serviceURL = GatewayMgmtEndpoint + "unifiedgateway/gatewayclusters";

            try
            {
                Console.WriteLine("");
                Console.WriteLine("- Retrieving data from: " + serviceURL);

                response = gatewayClient.GetAsync(serviceURL).Result;

                Console.WriteLine("   - Response code received: " + response.StatusCode);
                //Console.WriteLine(response);  // debug
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

                                            GatewayAgents.Rows.Add(gwGatewayID, gwGatewayObjectID, gwGatewayName, gwGatewayAnnotation, gwGatewayStatus, isAnchorGateway, gatewayClusterStatus, gatewayLoadBalancingSettings, gatewayStaticCapabilities, gatewayPublicKey, gatewayVersion, gatewayVersionStatus, gwEmail, gwMachineName, gwDepartment, gwExpiryDate, gatewayID, objectID, gatewayName);

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


                        // Write out the data to the database
                        if (isDBOutput)
                        {
                            try
                            {
                                WriteDBTable(OutputTableName, GatewayAgents);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("   - Error writing data to database.  Detail: " + ex.ToString());
                                Environment.ExitCode = (int)ExitCodes.Error;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("   - No content received.");
                        Environment.ExitCode = (int)ExitCodes.Error;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("   - API Access Error: " + ex.ToString());
                    Environment.ExitCode = (int)ExitCodes.Error;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("   - API Access Error: " + ex.ToString());
                Environment.ExitCode = (int)ExitCodes.Error;
            }
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// WriteDBTable Method
        /// 
        public void WriteDBTable(string destTableName, DataTable outputTable)
        {
            // Write out the DataTables to SQL database
            using (SqlConnection connection = new SqlConnection(connString))
            {
                Console.WriteLine("      - Writing data to database (" + destTableName + ")");

                connection.Open();

                TruncateTable(destTableName);

                // Write the DataTable to database
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    foreach (DataColumn c in outputTable.Columns)
                    {
                        //Console.WriteLine("      (Mapping) Table: " + outputTable.TableName + "   Column: " + c.ColumnName);
                        bulkCopy.ColumnMappings.Add(c.ColumnName, c.ColumnName);
                    }

                    bulkCopy.DestinationTableName = outputTable.TableName;
                    try
                    {
                        bulkCopy.WriteToServer(outputTable);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("      - Unable to load data to database (" + destTableName + ").  Error:" + e.ToString());
                    }
                }
            }
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// TruncateTable Method
        ///    

        public void TruncateTable(string sqltable)
        {
            string truncateSQL = String.Format("TRUNCATE TABLE {0}", sqltable);

            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                try
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = truncateSQL;

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("      - Unable to truncate temp table: " + sqltable + "  Error:" + e.ToString());
                    Environment.ExitCode = (int)ExitCodes.Error;
                }

                connection.Close();
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
            PowerBIServiceGatewayExport pbi = new PowerBIServiceGatewayExport();

            pbi.Execute(args);
        }
    }
}
