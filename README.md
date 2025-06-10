PetQuestV1 Installation Guide
This guide will help you set up and run the PetQuestV1 Blazor application on your local machine.

Prerequisites
Before you begin, ensure you have the following installed:

.NET 8.0 SDK – Download here

SQL Server or SQL Server Express LocalDB

SQL Server Management Studio (SSMS) (optional but recommended)

IDE – Visual Studio 2022 (recommended)

Step 1: Clone or Download the Repository
Download the PetQuestV1 project from GitHub:

Visit this link.

Click the green "Code" button, then select "Download ZIP".

Extract the ZIP file to a suitable directory on your local machine.

Step 2: Open the Project in Your IDE
Visual Studio 2022
Open Visual Studio 2022.

Click "Open a project or solution".

Navigate to the extracted PetQuestV1 folder.

Select PetQuestV1.sln and click "Open".

Other IDEs (Rider, etc.)
Open your IDE.

Import or open the project from the extracted PetQuestV1 folder.

Step 3: Configure Connection Strings
⚠️ IMPORTANT: You must update the connection strings to match your local database setup.

Update CLIDatabaseSetupTool Connection String
Navigate to CLIDatabaseSetupTool/appsettings.json.

Update the DefaultConnection string:

Example (LocalDB):
json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PetQuestV1Identity;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
Example (SQL Server Express):
json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=PetQuestV1Identity;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
Update PetQuestV1 Connection String
Navigate to PetQuestV1/appsettings.json.

Ensure this connection string matches exactly the one in CLIDatabaseSetupTool/appsettings.json.

Step 4: Set Up the Database
Run the CLI Database Setup Tool by following these steps:

Open the project containing the tool.

Execute the tool within your development environment.

Follow the interactive prompts:

If the connection string is incorrect, a warning will appear—double-check Step 3 if this happens.

Confirm to proceed (Warning: this will delete any existing database with the specified name).

The tool will:

Delete any existing database.

Create a new database.

Apply Entity Framework migrations.

Seed the database with initial data.

Step 5: Run the Blazor Application
Set PetQuestV1 as Startup Project
In Solution Explorer, right-click PetQuestV1.

Select "Set as Startup Project".

Start the Application
In Visual Studio, press F5 or click the "Run" button.

Verification
SQL Database Check (SSMS)
Open SSMS.

Connect to your SQL Server instance.

Verify that the PetQuestV1Identity database is created.

Expand the database to check if tables and data exist.

Application Check
Ensure the Blazor app loads in your browser.

Test its core functionality.

Educational User Accounts
For testing purposes, the following user accounts are available:

Email	Password	Role
viking@thor.com	Password005-	User
clint@barton.com	Password005-	SuperUser
tony@stark.com	Password001-	Admin
