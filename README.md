# CheckingSupplierEmail

This ASP.NET Core MVC application is designed to help purchasing and IT teams ensure that supplier (vendor) contact information is accurate and actionable. It focuses on validating supplier email addresses within the ERP system to prevent communication failures during the Purchase Order (PO) process.

## 🎯 Objectives

- **Prevent PO Delivery Failures:** Identify active vendors who lack a valid email address before a PO is sent.
- **Data Integrity:** Ensure that the `VEN_POEmail` field in the ERP database conforms to standard email formats.
- **Internal Access Control:** Manage internal user mapping (Employee No. & Email) for related purchasing notifications or CC lists.

## ✨ Key Features

### 1. Vendor Email Validation (Monitor)

The system automatically scans all vendors marked as **"Active"** and verifies their email configurations:

- **Missing Emails:** Identifies vendors with empty email fields.
- **Format Validation:** Checks if the email address follows standard formats (e.g., `user@domain.com`).
- **Multiple Emails:** Supports validating multiple emails separated by semicolons (`;`).
- **Reporting:** Displays a list of vendors with invalid or missing emails, along with the specific reason for failure.

### 2. Internal Email Management (PurCCEmail)

A dedicated interface for managing internal users associated with the purchasing process:

- **CRUD Operations:** Add, Edit, Delete internal email configurations.
- **Employee Integration:** Links usernames to specific Employee Numbers (EmpNo).
- **Validation:**
  - Verifies that the Employee Number exists.
  - Ensures the employee has not resigned (`empstatusno != "R"`).
  - Prevents duplicate email entries.

## 🛠️ Technology Stack

- **Framework:** ASP.NET Core MVC
- **Database:** SQL Server (via Entity Framework Core)
- **Frontend:** Razor Views, JavaScript (w/ jQuery), Bootstrap
- **Authentication:** Custom Identity/Claims-based system

## 🚀 Getting Started

1.  **Clone the repository**
    ```bash
    git clone https://github.com/wirat0155/CheckingSupplierEmail.git
    ```
2.  **Configure Database**
    - Ensure the `appsettings.json` connection strings point to your ERP and valid databases.
3.  **Run the Application**
    - Open the solution in Visual Studio.
    - Build and Run (F5).
