# This is a .Net 7.0 Template for Linq2Db

This is the basic MVC template for **.Net 7.0** with a few tweaks: -

 - The ORM tool is **Linq2DB**
 - The Identity UI is **fully scaffolded**.
	 - **Identity tables don't have the default names** - these can be changed.
	 - Two-Factor Authentication works (**with the QR Code**).
	 - To change the app name in the QR Code: - Edit EnableAuthenticator.cshtml.cs - change the line with **`_urlEncoder.Encode("LINQ2DB_MVC_Core_5")`**.
 - **External Authentication** is easy to setup (documented in Startup.cs). These 3 integrations are coded already: -
	 - Google Authentication
	 - Microsoft Authentication
	 - Facebook Authentication
 
	 Additional External Authentication can be set up through following the comments in Startup.cs
 - Linq2DB is fully integrated into the project : -
	 - the default appsettings.json connection string is used to configure Linq2DB
	 - instructions to generate Linq2DB POCO Classes included
	 - Database is SQL Server

# Instructions

 1. Clone or download the project.
 2. Run the "Create Database" script on you local SQL Server  ("20200415 Create MVC Linq2DB Template Tables.sql").
 3. Edit appsettings.json to point to the same server instance where you created the database.
 The project should now run (if the database connections are correctly set-up) .

##Setting up Linq2DB POCO Class generation

 1. To install the generation tool - open the `Developer Command Prompt` and execute this command: -

        dotnet tool install -g linq2db.cli

     1.1 Note - you should update the tool every time you update the Linq2DB component. The following command will update the tool: -
 
        dotnet tool update -g linq2db.cli

 2. After confirming that the project runs, you can edit the POCO generation config file `DB\_dbGenerateParams.json` to point to the new database.
 3. run the DOS batch file `_make_linq2db.bat` from within the `DB` folder to generate the `Linq2DB.cs` file.
 4. Now you can continue with the Angular - API project developing whatever you had in mind in the first place (See Startup.cs and DemoController for tips on accessing the database).
