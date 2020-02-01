///===================================================================================
///   PowerBIServiceActivityLogExport.cs
///===================================================================================
/// -- Author:       Jeff Pries (jeff@jpries.com)
/// -- Create date:  1/5/2020
/// -- Description:  Main driver class for Power BI Service Activity Log Export application

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

namespace PowerBIServiceActivityLogExport
{
    class PowerBIServiceActivityLogExport
    {
        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// Global Constants and Variables
        ///    

        // Constants
        const string VERSION = "1.0";
        const string DBDESTTABLEACTIVITYLOG = "tmp.PBIActivityLog";
        const string DBDESTTABLEACTIVITYLOGEXTRACT = "tmp.PBIActivityLogExtract";
        const string ACTIVITYEXTRACTQUERY = "etl.usp_PBI_ExtractActivityLog";
        const string ACTIVITYTRANSFORMQUERY = "etl.usp_PBI_TransformActivityLog";
        const string ACTIVITYLOADQUERY = "etl.usp_PBI_LoadActivityLog";

        // Variables 
        string HttpHeadUserAgent = "";
        HttpClient client = null;
        string destDBConnString;
        SqlConnectionStringBuilder destDBCSB;
        string connString;

        string AuthorityURL = "";
        string PowerBIResourceURL = "";
        string PowerBIRedirectURL = "";
        string GlobalServiceEndpoint = "";
        string PowerBIAPIPath = "";
        string PowerBIAPIURLBase = "";
        string ApplicationID = "";
        string UserName = "";
        SecureString Password = new SecureString();
        byte[] encKey = null;
        byte[] encIV = null;

        bool isStop = false;
        bool isInteractive = false;
        bool isActivityExactTimeRefresh = false;
        bool isUpdatePassword = false;
        bool isUpdateUsername = false;
        bool isUpdateApplicationID = false;
        bool isUpdateInteractive = false;

        DateTime parsedActivityStartDateTime = DateTime.MinValue;
        DateTime parsedActivityEndDateTime = DateTime.MinValue;

        DataTable ActivityLogs = null;

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
        public PowerBIServiceActivityLogExport()
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
            string activityRefreshUseExactTime = "";

            encKey = Encoding.UTF8.GetBytes("437f04670d46c2a7");
            encIV = Encoding.UTF8.GetBytes("bb336a57c64f86f4");

            // Read DB connection string - DestDB
            var destDBConnStringObj = ConfigurationManager.ConnectionStrings["DestinationDB"];
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

            activityRefreshUseExactTime = ConfigurationManager.AppSettings["ActivityRefreshIncrementalUseExactTime"];
            if (activityRefreshUseExactTime == "1" || activityRefreshUseExactTime.ToLower() == "true" || activityRefreshUseExactTime.ToLower() == "yes")
            {
                isActivityExactTimeRefresh = true;
            }

            AuthorityURL = ConfigurationManager.AppSettings["AzureADAuthorityURL"];
            GlobalServiceEndpoint = ConfigurationManager.AppSettings["GlobalServiceEndpoint"];
            PowerBIAPIPath = ConfigurationManager.AppSettings["PowerBIAPIPath"];

            PowerBIAPIURLBase = GlobalServiceEndpoint + PowerBIAPIPath;

            PowerBIResourceURL = ConfigurationManager.AppSettings["PowerBIResourceURL"];
            PowerBIRedirectURL = ConfigurationManager.AppSettings["PowerBIRedirectURL"];

            ApplicationID = ConfigurationManager.AppSettings["ApplicationID"];

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
            Console.WriteLine("||         Power BI Service Activity Log Export v" + VERSION + "          ||");
            Console.WriteLine("||               by Jeff Pries (jeff@jpries.com)              ||");
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
            isUpdatePassword = false;
            isUpdateUsername = false;
            isUpdateApplicationID = false;

            // Check for args, if none, set default settings
            if (args.Length == 0)
            {
                Console.WriteLine("- No arguments specified, running in default mode.  Run with /help to see available options.");
                Console.WriteLine("");

                // Default settings
                isStop = false;
                isUpdatePassword = false;
                isUpdateUsername = false;
                isUpdateApplicationID = false;
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
                        Console.WriteLine("This is a sample program to query Power API for ActivityLog (Usage) information.");
                        Console.WriteLine("");
                        Console.WriteLine("By default, it will attempt to use credentials saved in the .config file and if those fail, it will prompt for credentials.");
                        Console.WriteLine("If you wish, instead, to only prompt for credentials, run with the /interactive option.");
                        Console.WriteLine("");
                        Console.WriteLine("Also by default, it will attempt to get the date of the last saved activity and then get all subsequent days between then and today.");
                        Console.WriteLine("If no prior activity is found (first time run) then it will use its default period today through 14 days ago.");
                        Console.WriteLine("");
                        Console.WriteLine("The password saved in the .config file is encrypted.  To save a new password in the file, use the /savepassword:<password> command (without <>'s)");
                        Console.WriteLine("Note that the .config file should be treated as sensitive and secured as it is possible to decrypt a password from this file.");
                        Console.WriteLine("");
                        Console.WriteLine("Options:");
                        Console.WriteLine("     /interactive                    - Login with an interactive login prompt");
                        Console.WriteLine("     /activitystart:<mm/DD/yyyy>     - Start at a particular date (local time zone)");
                        Console.WriteLine("     /activityend:<mm/DD/yyyy>       - End at a particular date (local time zone)");
                        Console.WriteLine("     /saveapplicationid:             - Save the application id to the config file (from an interactive prompt)");
                        Console.WriteLine("     /saveapplicationid:<app id>     - Save the application id to the config file");
                        Console.WriteLine("     /saveusername                   - Save a username value for non-interactive login (from an interactive prompt)");
                        Console.WriteLine("     /saveusername:<username>        - Save a username value for non-interactive login");
                        Console.WriteLine("     /savepassword                   - Save an encrypted password value for non-interactive login (from an interactive prompt)");
                        Console.WriteLine("     /savepassword:<password>        - Save an encrypted password value for non-interactive login");

                        isStop = true;
                        break;
                    }
                    else if (argStr.Contains("/interactive"))
                    {
                        isInteractive = true;
                    }
                    if (argStr.Contains("/activitystart"))
                    {
                        // Start DateTime
                        if (argStr.Substring(0, 14) == "/activitystart" && argStr.Length == 14)
                        {
                            Console.WriteLine("To specify a start date/time, it must be in format:  /start:<mm/DD/yyyy>");
                            isStop = true;
                            break;
                        }
                        else if (argStr.Substring(0, 14) == "/activitystart" && argStr.Length >= 15)
                        {
                            if (argStr.Substring(14, 1) == ":" && argStr.Length < 16)
                            {
                                Console.WriteLine("To specify a start date/time, it must be in format:  /activitystart:<mm/DD/yyyy>");
                                isStop = true;
                                break;
                            }
                            else
                            {
                                if (!DateTime.TryParse(argStr.Substring(15), out parsedActivityStartDateTime))
                                {
                                    // Invalid value
                                    Console.WriteLine("To specify a start date/time, it must be in format:  /activitystart:<mm/DD/yyyy>");
                                    isStop = true;
                                    break;
                                }
                            }
                        }
                    }
                    else if (argStr.Contains("/activityend"))
                    {
                        // End DateTime
                        if (argStr.Substring(0, 12) == "/activityend" && argStr.Length == 12)
                        {
                            Console.WriteLine("To specify an end date/time, it must be in format:  /activityend:<mm/DD/yyyy>");
                            isStop = true;
                            break;
                        }
                        else if (argStr.Substring(0, 12) == "/activityend" && argStr.Length >= 13)
                        {
                            if (argStr.Substring(13, 1) == ":" && argStr.Length < 14)
                            {
                                Console.WriteLine("To specify an end date/time, it must be in format:  /activityend:<mm/DD/yyyy>");
                                isStop = true;
                                break;
                            }
                            else
                            {
                                if (!DateTime.TryParse(argStr.Substring(13), out parsedActivityEndDateTime))
                                {
                                    // Invalid value
                                    Console.WriteLine("To specify an end date/time, it must be in format:  /activityend:<mm/DD/yyyy>");
                                    isStop = true;
                                    break;
                                }
                            }
                        }
                    }
                    // Save config settings
                    else if (argStr.Contains("/saveapplicationid") || argStr.Contains("/applicationid"))
                    {
                        // Application ID
                        if (argStr == "/saveapplicationid:" || argStr == "/applicationid:" || !argStr.Contains(":"))
                        {
                            // Interactive
                            isUpdateApplicationID = true;
                            isUpdateInteractive = true;
                        }
                        else if (argStr.Contains("/saveapplicationid:") && argStr.Length > 19)
                        {
                            // Command line
                            isUpdateApplicationID = true;
                            isUpdateInteractive = false;
                            ApplicationID = arg.Substring(19);
                        }
                        else
                        {
                            Console.WriteLine("To save an Application ID to config, use /saveapplicationid with no other options.");
                            isStop = true;
                            break;
                        }
                    }
                    else if (argStr.Contains("/saveusername") || argStr.Contains("/saveusername"))
                    {
                        // User Name
                        if (argStr == "/saveusername:" || argStr == "/username:" || !argStr.Contains(":"))
                        {
                            // Interactive
                            isUpdateUsername = true;
                            isUpdateInteractive = true;
                            isStop = true;
                            break;
                        }
                        else if (argStr.Contains("/saveusername:") && argStr.Length > 14)
                        {
                            // Command line
                            isUpdateUsername = true;
                            isUpdateInteractive = false;
                            UserName = arg.Substring(14);
                            isStop = true;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("To save a Username to config, use /saveusername with no other options.");
                            isStop = true;
                            break;
                        }
                    }
                    else if (argStr.Contains("/savepassword") || argStr.Contains("/password"))
                    {
                        // Password
                        if (argStr == "/savepassword:" || argStr == "/password:" || !argStr.Contains(":"))
                        {
                            // Interactive
                            isUpdatePassword = true;
                            isUpdateInteractive = true;
                            isStop = true;
                            break;
                        }
                        else if (argStr.Contains("/savepassword:") && argStr.Length > 14)
                        {
                            // Command line
                            isUpdatePassword = true;
                            isUpdateInteractive = false;
                            Password = new NetworkCredential("", arg.Substring(14)).SecurePassword;
                            isStop = true;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("To save a Password to config, use /savepassword with no other options.");
                            isStop = true;
                            break;
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
            string authToken = "";

            PerformWelcome();

            ParseArgs(args);

            // Application ID Update
            if (isUpdateApplicationID)
            {
                if (!isUpdateInteractive)
                {
                    SaveSetting("ApplicationID", ApplicationID);
                }
                else
                {
                    SaveSettingInteractive("ApplicationID");
                }

                isStop = true;
            }

            // Username Update
            if (isUpdateUsername)
            {
                if (!isUpdateInteractive)
                {
                    SaveSetting("UserName", UserName);
                }
                else
                {
                    SaveSettingInteractive("UserName");
                }

                isStop = true;
            }

            // Password Update
            if (isUpdatePassword)
            {
                if (!isUpdateInteractive)
                {
                    SaveSetting("Password", "");
                }
                else
                {
                    SaveSettingInteractive("Password");
                }

                isStop = true;
            }

            // Main data api query execution
            if (!isStop)
            {
                // Get an authentication token for Power BI API
                
                authToken = GetAuthTokenUser();  // Uses native AD auth

                if (String.IsNullOrEmpty(authToken))
                {
                    isStop = true;
                }
                
                if (!isStop)
                {
                    // Initialize the client with the token
                    InitHttpClient(authToken);

                    DateTime queryStartDate = GetActivityEventStartDate();
                    DateTime queryEndDate = GetActivityEventEndDate();

                    Console.WriteLine("   - Get Activity Events for UTC dates from " + queryStartDate.ToString() + " to " + queryEndDate.ToString() + ".");


                    while (queryStartDate <= queryEndDate)
                    {
                        //Console.WriteLine("      GetActivityEvents('" + queryStartDate.ToString("s") + "', '" + queryStartDate.Date.AddDays(1).AddSeconds(-1).ToString("s") + "')");
                        GetActivityLog(queryStartDate.ToString("s"), queryStartDate.Date.AddDays(1).AddSeconds(-1).ToString("s"));

                        queryStartDate = queryStartDate.Date.AddDays(1);
                    } // while

                    // Load data to stage table
                    Console.WriteLine("");
                    Console.WriteLine("- Loading stage table (" + DBDESTTABLEACTIVITYLOG.Replace("tmp.", "stage.") + ") from tmp table...");
                    ExecuteSQLProc(ACTIVITYTRANSFORMQUERY);

                    // Load data to final table
                    Console.WriteLine("");
                    Console.WriteLine("- Loading prod table (" + DBDESTTABLEACTIVITYLOG.Replace("tmp.", "it.") + ") from stage table...");
                    ExecuteSQLProc(ACTIVITYLOADQUERY);

                } // if (!isStop)
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

            config.Save(ConfigurationSaveMode.Minimal, true);
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
        /// SaveSettingInteractive Method
        /// 
        public void SaveSettingInteractive(string settingName)
        {
            string settingValue = "";

            if (settingName == "ApplicationID")
            {
                Console.WriteLine("Enter the Application ID (GUID) which was assigned when the application was created for Power BI API access at https://portal.azure.com");
                Console.WriteLine("");
                Console.Write("Application ID: ");
                settingValue = Console.ReadLine();

                SaveSetting(settingName, settingValue);
            }
            else if (settingName == "UserName")
            {
                Console.WriteLine("Enter your Power BI Username, which will be used to authenticate to the Power BI API");
                Console.WriteLine("");
                Console.Write("Username: ");
                settingValue = Console.ReadLine();

                SaveSetting(settingName, settingValue);
            }
            else if (settingName == "Password")
            {
                Console.WriteLine("Enter your Power BI Password, which will be saved encrypted to the application's .config file and used to authenticate to the Power BI API");
                Console.WriteLine("");
                Console.Write("Password: ");
                do
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    // Disallow backspace and enter from password value
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        settingValue += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && settingValue.Length > 0)
                        {
                            settingValue = settingValue.Substring(0, (settingValue.Length - 1));
                            Console.Write("\b \b");
                        }
                        else if (key.Key == ConsoleKey.Enter)
                        {
                            break;
                        }
                    }
                } while (true);

                Password = new NetworkCredential("", settingValue).SecurePassword;
                SaveSetting(settingName, "");
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
                Console.WriteLine("        Application ID: " + ApplicationID);
                Console.WriteLine("        Redirect URL: " + PowerBIRedirectURL);

                authResult = await authContext.AcquireTokenAsync(PowerBIResourceURL, ApplicationID, new Uri(PowerBIRedirectURL), parameters).ConfigureAwait(false);

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
                // Query Azure AD for an interactive login prompt and subsequent Power BI auth token
                AuthenticationContext authContext = new AuthenticationContext(AuthorityURL);

                UserPasswordCredential userPasswordCredential = new UserPasswordCredential(UserName, Password);

                Console.WriteLine("        Authority URL: " + AuthorityURL);
                Console.WriteLine("        Resource URL: " + PowerBIResourceURL);
                Console.WriteLine("        Application ID: " + ApplicationID);
                Console.WriteLine("        Username: " + UserName);

                authResult = await authContext.AcquireTokenAsync(PowerBIResourceURL, ApplicationID, userPasswordCredential).ConfigureAwait(false);
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
            if ((String.IsNullOrEmpty(UserName) && Password == null) || authResult.Result == null || isInteractive == true)
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
        /// InitHttpClient Method
        ///    
        public void InitHttpClient(string authToken)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            Console.WriteLine("");
            Console.Write("- Initializing Power BI Service API client with generated auth token [");
            if (authToken.Length >= 40)
            {
                Console.WriteLine(authToken.Substring(0, 40) + "...]");
            }
            else
            {
                Console.WriteLine("None]");
            }

            // Create the gateway management web client connection
            client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(HttpHeadUserAgent);
            client.DefaultRequestHeaders.Add("Authorization", authToken);

            // Clear out the save password object
            Password.Dispose();
        }


        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetActivityLog Method
        /// 
        public void GetActivityLog(string startDateTimeStr, string endDateTimeStr)
        {
            HttpResponseMessage response = null;
            HttpContent responseContent = null;
            string strContent = "";
            string serviceURL = "";
            bool isFirstRun = true;
            bool isFirstResult = true;
            int numItems = 0;
            int numRequests = 0;

            string continuationURI = "";
            string continuationToken = "";

            do
            {
                ActivityLogs = new DataTable();

                ActivityLogs.TableName = DBDESTTABLEACTIVITYLOGEXTRACT;
                ActivityLogs.Columns.Add("ActivityLogInternalID", typeof(string));
                ActivityLogs.Columns.Add("RecordType", typeof(int));
                ActivityLogs.Columns.Add("CreationTime", typeof(DateTime));
                ActivityLogs.Columns.Add("Operation", typeof(string));
                ActivityLogs.Columns.Add("OrganizationID", typeof(string));
                ActivityLogs.Columns.Add("UserType", typeof(int));
                ActivityLogs.Columns.Add("UserKey", typeof(string));
                ActivityLogs.Columns.Add("Workload", typeof(string));
                ActivityLogs.Columns.Add("UserID", typeof(string));
                ActivityLogs.Columns.Add("ClientIP", typeof(string));
                ActivityLogs.Columns.Add("UserAgent", typeof(string));
                ActivityLogs.Columns.Add("Activity", typeof(string));
                ActivityLogs.Columns.Add("ItemName", typeof(string));
                ActivityLogs.Columns.Add("ObjectID", typeof(string));
                ActivityLogs.Columns.Add("RequestID", typeof(string));
                ActivityLogs.Columns.Add("ActivityID", typeof(string));
                ActivityLogs.Columns.Add("IsSuccess", typeof(bool));
                ActivityLogs.Columns.Add("WorkspaceName", typeof(string));
                ActivityLogs.Columns.Add("WorkspaceID", typeof(string));
                ActivityLogs.Columns.Add("ImportID", typeof(string));
                ActivityLogs.Columns.Add("ImportSource", typeof(string));
                ActivityLogs.Columns.Add("ImportType", typeof(string));
                ActivityLogs.Columns.Add("ImportDisplayName", typeof(string));
                ActivityLogs.Columns.Add("DatasetName", typeof(string));
                ActivityLogs.Columns.Add("DatasetID", typeof(string));
                ActivityLogs.Columns.Add("DataConnectivityMode", typeof(string));
                ActivityLogs.Columns.Add("GatewayID", typeof(string));
                ActivityLogs.Columns.Add("GatewayName", typeof(string));
                ActivityLogs.Columns.Add("GatewayType", typeof(string));
                ActivityLogs.Columns.Add("ReportName", typeof(string));
                ActivityLogs.Columns.Add("ReportID", typeof(string));
                ActivityLogs.Columns.Add("ReportType", typeof(string));
                ActivityLogs.Columns.Add("FolderObjectID", typeof(string));
                ActivityLogs.Columns.Add("FolderDisplayName", typeof(string));
                ActivityLogs.Columns.Add("ArtifactName", typeof(string));
                ActivityLogs.Columns.Add("ArtifactID", typeof(string));
                ActivityLogs.Columns.Add("CapacityName", typeof(string));
                ActivityLogs.Columns.Add("CapacityUsers", typeof(string));
                ActivityLogs.Columns.Add("CapacityState", typeof(string));
                ActivityLogs.Columns.Add("DistributionMethod", typeof(string));
                ActivityLogs.Columns.Add("ConsumptionMethod", typeof(string));
                ActivityLogs.Columns.Add("RefreshType", typeof(string));
                ActivityLogs.Columns.Add("ExportEventStartDateTimeParameter", typeof(string));
                ActivityLogs.Columns.Add("ExportEventEndDateTimeParameter", typeof(string));

                ActivityLogs.Columns.Add("ExportedArtifactExportType", typeof(string));
                ActivityLogs.Columns.Add("ExportedArtifactType", typeof(string));
                ActivityLogs.Columns.Add("AuditedArtifactName", typeof(string));
                ActivityLogs.Columns.Add("AuditedArtifactObjectID", typeof(string));
                ActivityLogs.Columns.Add("AuditedArtifactItemType", typeof(string));
                ActivityLogs.Columns.Add("OtherDatasetIDs", typeof(string));
                ActivityLogs.Columns.Add("OtherDatasetNames", typeof(string));
                ActivityLogs.Columns.Add("OtherDatasourceTypes", typeof(string));
                ActivityLogs.Columns.Add("OtherDatasourceConnectionDetails", typeof(string));
                ActivityLogs.Columns.Add("SharingRecipientEmails", typeof(string));
                ActivityLogs.Columns.Add("SharingResharePermissions", typeof(string));
                ActivityLogs.Columns.Add("SubscribeeRecipientEmails", typeof(string));
                ActivityLogs.Columns.Add("SubscribeeRecipientNames", typeof(string));
                ActivityLogs.Columns.Add("SubscribeeObjectIDs", typeof(string));

                ActivityLogs.Columns.Add("AddedDateTime", typeof(DateTime));

                PowerBIActivityLog rc = null;

                if (isFirstRun && startDateTimeStr.Length > 0 && endDateTimeStr.Length > 0)
                {
                    serviceURL = String.Format(PowerBIAPIURLBase + "/admin/activityevents?startDateTime='{0}'&endDateTime='{1}'", startDateTimeStr, endDateTimeStr);
                }
                else if (!isFirstRun && continuationURI.Length > 0)
                {
                    serviceURL = continuationURI;
                }
                else
                {
                    serviceURL = "";
                }
                // ?startDateTime={startDateTime}&endDateTime={endDateTime}&continuationToken={continuationToken}
                // &$filter={$filter}

                /*
                First request:
                    https://api.powerbi.com/v1.0/myorg/admin/ActivityLogs?startDateTime='2019-08-31T00:00:00'&endDateTime='2019-08-31T23:59:59'

                Subsequent request:
                    https://api.powerbi.com/v1.0/myorg/admin/ActivityLogs?continuationToken='%2BRID%3ARthsAIwfWGcVAAAAAAAAAA%3D%3D%23RT%3A4%23TRC%3A20%23FPC%3AARUAAAAAAAAAFwAAAAAAAAA%3D'
                */
                

                if (serviceURL.Length > 0)
                {
                    try
                    {
                        if (isFirstRun)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("- Retrieving data from: " + serviceURL);
                        }

                        response = client.GetAsync(serviceURL).Result;
                        numRequests++;

                        if (isFirstRun)
                        {
                            Console.WriteLine("   - Response code received: " + response.StatusCode);
                        }

                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            Console.WriteLine("      - Unauthorized response received.");
                            Console.WriteLine("        Ensure you are logged in with an account that is granted the Power BI Administrator role, you have the correct Application ID (from https://portal.azure.com) and ensure the application has been granted the Tenant.Read.All permission by an administrator.");
                        }

                        try
                        {
                            responseContent = response.Content;
                            strContent = responseContent.ReadAsStringAsync().Result;

                            if (strContent.Length > 0)
                            {
                                //Console.WriteLine("   - De-serializing Activity Event Data...");

                                // Parse the JSON string into objects and store in DataTable
                                JavaScriptSerializer js = new JavaScriptSerializer();
                                js.MaxJsonLength = 2147483647;
                                rc = js.Deserialize<PowerBIActivityLog>(strContent);

                                // Get the Power BI Activity Events that have been de-serialized
                                if (rc != null)
                                {
                                    continuationURI = rc.continuationUri;
                                    continuationToken = rc.continuationToken;

                                    // Read all of the activity events and save to DataTable
                                    if (rc.activityEventEntities != null)
                                    {
                                        //Console.WriteLine("      - Activity Logs received: " + rc.activityEventEntities.Count);
                                        numItems = numItems + rc.activityEventEntities.Count;

                                        foreach (PowerBIActivityLogEntity item in rc.activityEventEntities)
                                        {
                                            string id = "";
                                            int recordType = 0;
                                            DateTime creationTime = DateTime.MinValue;
                                            string operation = "";
                                            string organizationID = "";
                                            int userType = 0;
                                            string userKey = "";
                                            string workload = "";
                                            string userID = "";
                                            string clientIP = "";
                                            string userAgent = "";
                                            string activity = "";
                                            string itemName = "";
                                            string objectID = "";
                                            string requestID = "";
                                            string activityID = "";
                                            bool isSuccess = true;

                                            string workspaceName = "";
                                            string workspaceID = "";
                                            string importID = "";
                                            string importSource = "";
                                            string importType = "";
                                            string importDisplayName = "";
                                            string datasetName = "";
                                            string datasetID = "";
                                            string dataConnectivityMode = "";
                                            string gatewayID = "";
                                            string gatewayName = "";
                                            string gatewayType = "";
                                            string reportName = "";
                                            string reportID = "";
                                            string reportType = "";
                                            string folderObjectID = "";
                                            string folderDisplayName = "";
                                            string artifactName = "";
                                            string artifactID = "";
                                            string capacityName = "";
                                            string capacityState = "";
                                            string capacityUsers = "";
                                            string distributionMethod = "";
                                            string consumptionMethod = "";
                                            string refreshType = "";
                                            string exportEventStartDateTimeParameter = "";
                                            string exportEventEndDateTimeParameter = "";

                                            // Embedded objects
                                            string exportedArtifactExportType = "";
                                            string exportedArtifactType = "";
                                            string auditedArtificatName = "";
                                            string auditedArtifactObjectID = "";
                                            string auditedArtifactItemType = "";
                                            string otherDatasetIDs = "";
                                            string otherDatasetNames = "";
                                            string otherDatasourceTypes = "";
                                            string otherDatasourceConnectionDetails = "";
                                            string sharingRecipientEmails = "";
                                            string sharingResharePermissions = "";
                                            string subscribeeRecipientEmails = "";
                                            string subscribeeReceipientNames = "";
                                            string subscribeeObjectIDs = "";

                                            if (item.Id != null)
                                            {
                                                id = item.Id;
                                            }

                                            recordType = item.RecordType;

                                            creationTime = item.CreationTime;
                                            creationTime = creationTime.ToLocalTime(); // Convert from UTC to local

                                            if (item.Operation != null)
                                            {
                                                operation = item.Operation;
                                            }

                                            if (item.OrganizationId != null)
                                            {
                                                organizationID = item.OrganizationId;
                                            }

                                            userType = item.UserType;

                                            if (item.UserKey != null)
                                            {
                                                userKey = item.UserKey;
                                            }

                                            if (item.Workload != null)
                                            {
                                                workload = item.Workload;
                                            }

                                            if (item.UserId != null)
                                            {
                                                userID = item.UserId;
                                            }

                                            if (item.ClientIP != null)
                                            {
                                                clientIP = item.ClientIP;
                                            }

                                            if (item.UserAgent != null)
                                            {
                                                userAgent = item.UserAgent;
                                            }

                                            if (item.Activity != null)
                                            {
                                                activity = item.Activity;
                                            }

                                            if (item.ItemName != null)
                                            {
                                                itemName = item.ItemName;
                                            }

                                            if (item.ObjectId != null)
                                            {
                                                objectID = item.ObjectId;
                                            }

                                            if (item.RequestId != null)
                                            {
                                                requestID = item.RequestId;
                                            }

                                            if (item.ActivityId != null)
                                            {
                                                activityID = item.ActivityId;
                                            }

                                            isSuccess = item.IsSuccess;

                                            if (item.WorkSpaceName != null)
                                            {
                                                workspaceName = item.WorkSpaceName;
                                            }

                                            if (item.WorkspaceId != null)
                                            {
                                                workspaceID = item.WorkspaceId;
                                            }

                                            if (item.ImportId != null)
                                            {
                                                importID = item.ImportId;
                                            }

                                            if (item.ImportSource != null)
                                            {
                                                importSource = item.ImportSource;
                                            }

                                            if (item.ImportType != null)
                                            {
                                                importType = item.ImportType;
                                            }

                                            if (item.ImportDisplayName != null)
                                            {
                                                importDisplayName = item.ImportDisplayName;
                                            }

                                            if (item.DatasetName != null)
                                            {
                                                datasetName = item.DatasetName;
                                            }

                                            if (item.DatasetId != null)
                                            {
                                                datasetID = item.DatasetId;
                                            }

                                            if (item.DataConnectivityMode != null)
                                            {
                                                dataConnectivityMode = item.DataConnectivityMode;
                                            }

                                            if (item.GatewayId != null)
                                            {
                                                gatewayID = item.GatewayId;
                                            }

                                            if (item.GatewayName != null)
                                            {
                                                gatewayName = item.GatewayName;
                                            }

                                            if (item.GatewayType != null)
                                            {
                                                gatewayType = item.GatewayType;
                                            }

                                            if (item.ReportName != null)
                                            {
                                                reportName = item.ReportName;
                                            }

                                            if (item.ReportId != null)
                                            {
                                                reportID = item.ReportId;
                                            }

                                            if (item.ReportType != null)
                                            {
                                                reportType = item.ReportType;
                                            }

                                            if (item.FolderObjectId != null)
                                            {
                                                folderObjectID = item.FolderObjectId;
                                            }

                                            if (item.FolderDisplayName != null)
                                            {
                                                folderDisplayName = item.FolderDisplayName;
                                            }

                                            if (item.ArtifactName != null)
                                            {
                                                artifactName = item.ArtifactName;
                                            }

                                            if (item.ArtifactId != null)
                                            {
                                                artifactID = item.ArtifactId;
                                            }

                                            if (item.CapacityName != null)
                                            {
                                                capacityName = item.CapacityName;
                                            }

                                            if (item.CapacityState != null)
                                            {
                                                capacityState = item.CapacityState;
                                            }

                                            if (item.CapacityUsers != null)
                                            {
                                                capacityUsers = item.CapacityUsers;
                                            }

                                            if (item.DistributionMethod != null)
                                            {
                                                distributionMethod = item.DistributionMethod;
                                            }

                                            if (item.ConsumptionMethod != null)
                                            {
                                                consumptionMethod = item.ConsumptionMethod;
                                            }

                                            if (item.RefreshType != null)
                                            {
                                                refreshType = item.RefreshType;
                                            }

                                            if (item.ExportEventStartDateTimeParameter != null)
                                            {
                                                exportEventStartDateTimeParameter = item.ExportEventStartDateTimeParameter;
                                            }

                                            if (item.ExportEventEndDateTimeParameter != null)
                                            {
                                                exportEventEndDateTimeParameter = item.ExportEventEndDateTimeParameter;
                                            }

                                            // Embedded objects (Datasets)
                                            if (item.Datasets != null)
                                            {
                                                foreach (PowerBIActivityLogDataset ds in item.Datasets)
                                                {
                                                    if (!String.IsNullOrEmpty(ds.DatasetId))
                                                    {
                                                        if (otherDatasetIDs.Length > 0)
                                                        {
                                                            otherDatasetIDs = otherDatasetIDs + ", ";
                                                        }

                                                        otherDatasetIDs = otherDatasetIDs + ds.DatasetId;
                                                    }

                                                    if (!String.IsNullOrEmpty(ds.DatasetName))
                                                    {
                                                        if (!String.IsNullOrEmpty(ds.DatasetName))
                                                        {
                                                            if (otherDatasetNames.Length > 0)
                                                            {
                                                                otherDatasetNames = otherDatasetNames + ", ";
                                                            }

                                                            otherDatasetNames = otherDatasetNames + ds.DatasetName;
                                                        }
                                                    }
                                                }

                                            }

                                            // Embedded objects (Datasources)
                                            if (item.Datasources != null)
                                            {
                                                foreach (PowerBIActivityLogDatasource ds in item.Datasources)
                                                {
                                                    if (!String.IsNullOrEmpty(ds.DatasourceType))
                                                    {
                                                        if (otherDatasourceTypes.Length > 0)
                                                        {
                                                            otherDatasourceTypes = otherDatasourceTypes + ", ";
                                                        }

                                                        otherDatasourceTypes = otherDatasourceTypes + ds.DatasourceType;
                                                    }

                                                    if (!String.IsNullOrEmpty(ds.ConnectionDetails))
                                                    {
                                                        if (otherDatasourceConnectionDetails.Length > 0)
                                                        {
                                                            otherDatasourceConnectionDetails = otherDatasourceConnectionDetails + ", ";
                                                        }

                                                        otherDatasourceConnectionDetails = otherDatasourceConnectionDetails + ds.ConnectionDetails;
                                                    }
                                                }
                                            }

                                            // Embedded objects (SharingInformation)
                                            if (item.SharingInformation != null)
                                            {
                                                foreach (PowerBIActivityLogSharingInformation si in item.SharingInformation)
                                                {
                                                    if (!String.IsNullOrEmpty(si.RecipientEmail))
                                                    {
                                                        if (sharingRecipientEmails.Length > 0)
                                                        {
                                                            sharingRecipientEmails = sharingRecipientEmails + ", ";
                                                        }

                                                        sharingRecipientEmails = sharingRecipientEmails + si.RecipientEmail;
                                                    }

                                                    if (!String.IsNullOrEmpty(si.ResharePermission))
                                                    {
                                                        if (sharingResharePermissions.Length > 0)
                                                        {
                                                            sharingResharePermissions = sharingResharePermissions + ", ";
                                                        }

                                                        sharingResharePermissions = sharingResharePermissions + si.ResharePermission;
                                                    }
                                                }
                                            }

                                            // Embedded objects (SubscribeeInformation)
                                            if (item.SubscribeeInformation != null)
                                            {
                                                foreach (PowerBIActivityLogSubscribeeInformation si in item.SubscribeeInformation)
                                                {
                                                    if (!String.IsNullOrEmpty(si.RecipientEmail))
                                                    {
                                                        if (subscribeeRecipientEmails.Length > 0)
                                                        {
                                                            subscribeeRecipientEmails = subscribeeRecipientEmails + ", ";
                                                        }

                                                        subscribeeRecipientEmails = subscribeeRecipientEmails + si.RecipientEmail;
                                                    }
                                                }

                                                foreach (PowerBIActivityLogSubscribeeInformation si in item.SubscribeeInformation)
                                                {
                                                    if (!String.IsNullOrEmpty(si.RecipientName))
                                                    {
                                                        if (subscribeeReceipientNames.Length > 0)
                                                        {
                                                            subscribeeReceipientNames = subscribeeReceipientNames + ", ";
                                                        }

                                                        subscribeeReceipientNames = subscribeeReceipientNames + si.RecipientName;
                                                    }
                                                }

                                                foreach (PowerBIActivityLogSubscribeeInformation si in item.SubscribeeInformation)
                                                {
                                                    if (!String.IsNullOrEmpty(si.ObjectId))
                                                    {
                                                        if (subscribeeObjectIDs.Length > 0)
                                                        {
                                                            subscribeeObjectIDs = subscribeeObjectIDs + ", ";
                                                        }

                                                        subscribeeObjectIDs = subscribeeObjectIDs + si.ObjectId;
                                                    }
                                                }
                                            }

                                            // Embedded objects (ExportedArtifactInfo)
                                            if (item.ExportedArtifactInfo != null)
                                            {
                                                if (item.ExportedArtifactInfo.ExportType != null)
                                                {
                                                    exportedArtifactExportType = item.ExportedArtifactInfo.ExportType;
                                                }

                                                if (item.ExportedArtifactInfo.ArtifactType != null)
                                                {
                                                    exportedArtifactType = item.ExportedArtifactInfo.ArtifactType;
                                                }
                                            }

                                            // Embedded objects (AuditedArtifactInformation)
                                            if (item.AuditedArtifactInformation != null)
                                            {
                                                if (item.AuditedArtifactInformation.Name != null)
                                                {
                                                    auditedArtificatName = item.AuditedArtifactInformation.Name;
                                                }

                                                if (item.AuditedArtifactInformation.ArtifactObjectId != null)
                                                {
                                                    auditedArtifactObjectID = item.AuditedArtifactInformation.ArtifactObjectId;
                                                }

                                                if (item.AuditedArtifactInformation.AnnotatedItemType != null)
                                                {
                                                    auditedArtifactItemType = item.AuditedArtifactInformation.AnnotatedItemType;
                                                }
                                            }

                                            ActivityLogs.Rows.Add(id, recordType, creationTime, operation, organizationID,
                                                userType, userKey, workload, userID, clientIP, userAgent, activity, itemName,
                                                objectID, requestID, activityID, isSuccess, workspaceName, workspaceID, importID,
                                                importSource, importType, importDisplayName, datasetName, datasetID, dataConnectivityMode,
                                                gatewayID, gatewayName, gatewayType, reportName, reportID, reportType, folderObjectID,
                                                folderDisplayName, artifactName, artifactID, capacityName, capacityState, capacityUsers,
                                                distributionMethod, consumptionMethod, refreshType, exportEventStartDateTimeParameter, exportEventEndDateTimeParameter,
                                                exportedArtifactExportType, exportedArtifactType, auditedArtificatName, auditedArtifactObjectID,
                                                auditedArtifactItemType, otherDatasetIDs, otherDatasetNames, otherDatasourceTypes, otherDatasetNames,
                                                sharingRecipientEmails, sharingResharePermissions, subscribeeRecipientEmails, subscribeeReceipientNames,
                                                subscribeeObjectIDs, DateTime.Now);
                                        } // foreach
                                    } // if rc.ActivityLogEntities
                                } // if rc

                                // Only write out if at least one event was retrieved (blank responses with just a continuation token are apparently normal)
                                if (rc.activityEventEntities.Count > 0)
                                {
                                    // Write out the data to the database
                                    try
                                    {
                                        WriteDBTable(DBDESTTABLEACTIVITYLOGEXTRACT, ActivityLogs, isFirstResult);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("   - Error writing data to database.  Detail: " + ex.ToString());
                                        Environment.ExitCode = (int)ExitCodes.Error;
                                    }

                                    isFirstResult = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("   - No content received (" + numRequests + " API requests).");
                                continuationToken = "";
                                continuationURI = "";
                                Environment.ExitCode = (int)ExitCodes.Error;
                            } // if strContent.Length > 0
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("   - API Access Error (" + numRequests + " API requests): " + ex.ToString());
                            continuationToken = "";
                            continuationURI = "";
                            Environment.ExitCode = (int)ExitCodes.Error;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("   - API Access Error (" + numRequests + " API requests): " + ex.ToString());
                        continuationToken = "";
                        continuationURI = "";
                        Environment.ExitCode = (int)ExitCodes.Error;
                    }

                    isFirstRun = false;
                } // if serviceURL
            } // do while
            while (!String.IsNullOrEmpty(continuationURI) && !String.IsNullOrEmpty(serviceURL)); // If the continuation token is valid, perform a new get

            Console.WriteLine("");
            Console.WriteLine("   - Items received: " + numItems + " in " + numRequests + " requests.");

            // Merge the Extracted ActivityLog data for the day into the main extract (output) table
            Console.WriteLine("");
            Console.WriteLine("- Extracting Activity Log API data from tmp extract table to tmp table...");
            ExecuteSQLProc(ACTIVITYEXTRACTQUERY);
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// ExecuteSQLProc Method
        ///

        public void ExecuteSQLProc(string sqlProc)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                try
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = sqlProc;

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("   - Unable to execute SQL Proc: " + sqlProc + ".  Error: " + e.ToString());
                    Environment.ExitCode = (int)ExitCodes.Error;
                }

                connection.Close();
            }

            Console.WriteLine("      - Done!");
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetActivityEventStartDate Method
        /// 
        public DateTime GetActivityEventStartDate()
        {
            DateTime retVal = DateTime.MinValue;
            DateTime defaultVal = DateTime.Now.ToUniversalTime().AddDays(-30).Date; // Default amount of activity (30 days)

            if (parsedActivityStartDateTime == DateTime.MinValue)
            {
                // Read from DB to get date of most recent activity and use for start date
                string truncateSQL = String.Format(@"
			    SELECT
				    MAX(CreationTime) AS LastCreationTime
			    FROM {0}
			    ", DBDESTTABLEACTIVITYLOG);

                using (SqlConnection connection = new SqlConnection(connString))
                {
                    connection.Open();
                    try
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = truncateSQL;

                            var result = command.ExecuteScalar();
                            DateTime.TryParse(result.ToString(), out retVal);

                            if (!isActivityExactTimeRefresh)
                            {
                                retVal = retVal.Date; // Strip off the time
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("   - Previous export data not found, using default start date (" + retVal + ")");
                    }

                    connection.Close();

                    if (retVal == DateTime.MinValue)
                    {
                        retVal = defaultVal;
                    }
                }
            }
            else
            {
                // Use the parsed value
                retVal = parsedActivityStartDateTime.ToUniversalTime();
            }

            return (retVal);
        }


        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// GetActivityEventEndDate Method
        /// 
        public DateTime GetActivityEventEndDate()
        {
            DateTime retVal = DateTime.MinValue;
            DateTime defaultVal = DateTime.Now.ToUniversalTime().Date;  // Default end date (today)

            if (parsedActivityEndDateTime == DateTime.MinValue)
            {
                retVal = defaultVal;
            }
            else
            {
                retVal = parsedActivityEndDateTime;
            }

            retVal = retVal.AddDays(1).AddSeconds(-1);

            return (retVal);
        }

        /// ----------------------------------------------------------------------------------------------------------------------------------------------------------------- ///
        ///
        /// WriteDBTable Method
        /// 
        public void WriteDBTable(string destTableName, DataTable outputTable, bool isTruncate)
        {
            // Write out the DataTables to SQL database
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();

                if (isTruncate)
                {
                    TruncateTable(destTableName);
                }

                Console.WriteLine("      - Writing data to database (" + destTableName + ")");

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
                        Environment.ExitCode = (int)ExitCodes.Error;
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
                        Console.WriteLine("      - Cleared table: " + sqltable + "");
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
        /// Main Method
        ///   
        static void Main(string[] args)
        {
            PowerBIServiceActivityLogExport pbi = new PowerBIServiceActivityLogExport();

            pbi.Execute(args);
        }
    }
}
