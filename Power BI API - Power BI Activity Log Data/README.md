# Power BI API - Power BI Activity Log Data
A sample C# application to authenticate to Azure Active Directory and the Power BI REST API to perform the Get Activity Events function to get all Power BI usage data (Activity Log aka Activity Events) for a given date range. The program converts to the necessary UTC format and breaks requests down to single day pieces before performing the request, de-serializing the response, and storing it in a SQL database table. 

The application is a simple console application which makes uses of native .NET libraries as well as the Microsoft Identity Model Client library for authenticating to Azure Active Directory.

This application is only a very basic sample with minimal functionality and not intended for any sort of production use. The basic concepts for authentication, querying the API, and parsing the resposne could be taken and turned into a much more robust application.

Before running the application, you will need to have a SQL Server database with the appropriate objects created.  See the Misc subfolder for scripts to create these objects.

Make sure to review the .config file for the application and to set the database connection string if using the save to database option.  Note that the password which can be saved in the .config file is done so via an encrypted value, stored by the application.  See its /help option for command line switches.

For more information, see: <link tbd>
and https://blog.jpries.com/2020/01/03/getting-started-with-the-power-bi-api-querying-the-power-bi-rest-api-directly-with-c/
