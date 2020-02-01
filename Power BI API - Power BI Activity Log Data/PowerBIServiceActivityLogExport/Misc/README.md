#Miscellaneous Support Files

This files are used to create the database tables and other objects used by the PowerBIServiceActivityLogExport program.  Some files are zipped up to save space.  Files starting with a number should be executed in that order.  Files that do not start with a number are informational only and do not need to be run.

01_Create_Tables_ActivityLog.sql   -   SQL script to create the 4 PBIActivityLog tables that store log data and the it, stage, tmp, and etl schemas.
02_Create_TableAndData_ActivityLogActivityType.sql   -   SQL script to create the it.PBIActivityLogActivityType table and populate it with data (around 100 rows).
03_Create_Table_Date.zip   -   Compressed SQL script to create the dbo.DimDate date dimension and populate it with data.
04_Create_Table_Time.zip   -   Compressed SQL script to create the dbo.DimTime time dimension and populate it with data.
05_Create_StoredProcedures.sql   -   SQL script to create the Extract, Transform, and Load stored procedures used by the application to populate its tables.
06_Create_Function_DateBuckets.sql   -    SQL script to create the dbo.ufnDateBuckets function which populates the Date slicer in the report.
07_Create_Views.sql   -   SQL script to create the views which site between the database tables and the report
BITools_Demo.zip   -   SQL 2017 backup of the database with all of the above scripts run (restore this instead of running the scripts if you choose).
PBIActivityLogActivityType.xlsx   -   Excel spreadsheet of the Activity Types in the ActivityLogActivityType table.
Power BI Activity Log.pbix   -   Sample Power BI Report for Power BI Activity Log data.
