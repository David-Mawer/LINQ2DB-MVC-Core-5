# This is a .Net Core 5.0 Template for Linq2Db

This is the basic MVC template for **.Net Core 5.0** with a few tweaks: -

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
	 - The T4 class generation is installed and configured
	 - Database is SQL Server

# Instructions

 1. Clone or download the project.
 2. Run the "Create Database" script on you local SQL Server  ("20200415 Create MVC Linq2DB Template Tables.sql").
 3. Edit appsettings.json to point to the same server instance where you created the database.
 The project should now run (if the database connections are correctly set-up) .
 
 4. After confirming that the project runs, you can edit the T4 generation file `DB\LinqDB.tt` to point to the new database.
 5. (Make sure that you edit the connectionString at the end of "LinqDB.tt" to point at your database location)
 6. Now you can continue with the Angular - API project developing whatever you had in mind in the first place (See Startup.cs and DemoController for tips on accessing the database).
