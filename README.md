
<![endif]-->

**This guide will help you set up and run the PetQuestV1 Blazor application on your local machine.**

----------

**Prerequisites**

**Before you begin, ensure you have the following installed:**

-   **.NET 8.0 SDK: Download from the official Microsoft website.**
-   **SQL Server or SQL Server Express LocalDB**
-   **SQL Server Management Studio (SSMS): Optional, but recommended for database verification.**
-   **IDE: Visual Studio 2022 (recommended).**

----------

**Step 1: Download the Repository**

**Download the PetQuestV1 project by visiting this link in your web browser:** [**https://github.com/MattiasGijsels/PetQuestV1**](https://github.com/MattiasGijsels/PetQuestV1)

**On the GitHub page, click the green "Code" button and then select "Download ZIP." Once downloaded, extract the contents of the ZIP file to a directory on your local machine where you want to keep the project.**

----------

**Step 2: Open the Project in Your IDE**

**Visual Studio 2022**

1.  **Open Visual Studio 2022.**
2.  **Click "Open a project or solution."**
3.  **Navigate to the extracted PetQuestV1 folder.**
4.  **Select the PetQuestV1.sln solution file and click "Open."**

**Other IDEs (Rider, etc.)**

1.  **Open your IDE.**
2.  **Import or open the project/solution from the extracted PetQuestV1 folder.**

----------

**Step 3: Configure Connection Strings**

**⚠️ IMPORTANT: You must update the connection strings to match your local database setup.** **⚠️**

**3.1 Update CLIDatabaseSetupTool Connection String**

1.  **In your project's file explorer (within your IDE), navigate to the CLIDatabaseSetupTool folder.**
2.  **Open the appsettings.json file located in this folder.**
3.  **Update the DefaultConnection string to match your SQL Server setup.**

**Example connection string (LocalDB):**

**JSON**

**{**

**"ConnectionStrings": {**

**"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PetQuestV1Identity;Trusted_Connection=True;TrustServerCertificate=True;"**

**}**

**}**

**Example connection string (SQL Server Express):**

**JSON**

**{**

**"ConnectionStrings": {**

**"DefaultConnection": "Server=.\\SQLEXPRESS;Database=PetQuestV1Identity;Trusted_Connection=True;TrustServerCertificate=True;"**

**}**

**}**

**3.2 Update PetQuestV1 Connection String**

1.  **Navigate to the PetQuestV1 folder in your project (the main application folder).**
2.  **Open the appsettings.json file in this folder.**
3.  **Ensure that the DefaultConnection string in this file is EXACTLY the same as the one you set in CLIDatabaseSetupTool/appsettings.json.**

**⚠️ Critical: Both projects must use the same connection string as they share the same database!** **⚠️**

----------

**Step 4: Set Up the Database**

**To set up the database, you will use the CLIDatabaseSetupTool.**

**4.1 Run the CLI Database Setup Tool**

**The CLI tool will guide you through the database setup process:**

-   **Connection String Check: If no connection string is found or is incorrect, you'll receive a warning. Double-check Step 3 if this happens.**
-   **⚠️⚠️Confirmation Prompt: The tool will ask for confirmation to proceed. Type y to continue.** **!! Warning: This will delete any existing database with the specified name and create a fresh one!!****⚠️⚠️**
-   **Database Operations: The tool will perform the following actions:**

-   **Delete the existing database (if it exists).**
-   **Create a new database.**
-   **Run Entity Framework migrations.**
-   **Seed the database with data from the included JSON files.**

-   **Success Confirmation: The tool will inform you if the database seeding was successful.**

----------

**Step 5: Run the Blazor Application**

**5.1 Set PetQuestV1 as Startup Project (Visual Studio)**

1.  **In the Solution Explorer within Visual Studio, right-click on the PetQuestV1 project (the one in the main application folder).**
2.  **Select "Set as Startup Project."**

**5.2 Run the Application**

-   **In Visual Studio: Press F5 or click the green "Run" button.**

**The application should now start and open in your default web browser!**

----------

**Verification Steps**

**If you have SQL Server Management Studio (SSMS):**

1.  **Open SSMS.**
2.  **Connect to your SQL Server instance.**
3.  **Look for the PetQuestV1Identity database.**
4.  **Expand the database to verify tables and data were created successfully.**

**Application Verification**

-   **The Blazor application should load in your browser.**
-   **Test the basic functionality to ensure everything is working correctly.**

----------

**User Accounts for Educational Purposes**

**Normally it is a no-go to share accounts and passwords, but for educational purposes, the following accounts are provided:**

-   **Email: viking@thor.com Password: Password005- Role: User**
-   **Email: clint@barton.com Password: Password005- Role: SuperUser**
-   **Email: tony@stark.com Password: Password001- Role: Admin**
