# Power BI API - Power BI Gatewy Data
A sample C# application to authenticate to Azure Active Directory and the Power Platform Service to query Power BI Gateway Agent status for Power BI Gateways in an organization.

The application is a simple console application which makes uses of native .NET libraries as well as the Microsoft Identity Model Client library for authenticating to Azure Active Directory.

This application is only a very basic sample with minimal functionality and not intended for any sort of production use. The basic concepts for authentication, querying the API, and parsing the resposne could be taken and turned into a much more robust application.

If you wish to use the dboutput option of the program, make sure to first run the table scription script in the Misc subfolder to create the necessary SQL Server table to store the data in.

Make sure to review the .config file for the application and to set the database connection string if using the save to database option.  Note that the password which can be saved in the .config file is done so via an encrypted value, stored by the application.  See its /help option for command line switches.

Options:
     /interactive                    - Login with an interactive login prompt
     /dboutput                       - Output the Gateway Agent query results to database table (must already exist)
     /saveusername                   - Save a username value for non-interactive login (from an interactive prompt)
     /saveusername:<username>        - Save a username value for non-interactive login
     /savepassword                   - Save an encrypted password value for non-interactive login (from an interactive prompt)
     /savepassword:<password>        - Save an encrypted password value for non-interactive login

For more information, see: https://blog.jpries.com/2020/01/10/power-bi-on-premises-gateway-cluster-monitoring/
and https://blog.jpries.com/2020/01/03/getting-started-with-the-power-bi-api-querying-the-power-bi-rest-api-directly-with-c/
