This is a professional and structured README.md designed for your repository. It highlights the core functionality you've built while being honest about its "work in progress" status, which is common for academic or learning projects.

---

# GabayForGood

**GabayForGood** is a full-stack donation platform designed to connect donors with local NGOs in the Philippines. This project was developed as a finals requirement for the **Enterprise Programming (ENTPROG)** course.

The application leverages the **ASP.NET MVC** architectural pattern and **Microsoft Entity Framework** for robust data management and enterprise-level scalability.

---

## 🏗️ Technical Stack

* **Framework:** ASP.NET Core MVC
* **ORM:** Microsoft Entity Framework (Code First)
* **Language:** C#
* **Frontend:** HTML5, CSS3, JavaScript (with Razor Pages)
* **Database:** SQL Server

## ✨ Key Features

* **User Authentication:** Integrated login and registration system for secure access.
* **NGO Donation Platform:** A centralized hub where users can browse and support various local NGOs.
* **Admin Dashboard:** Dedicated management interface for administrators to monitor platform activity (In Development).
* **Database Architecture:** Designed with structured Entity Relationship Diagrams (ERDs) to ensure data integrity and complex relationship mapping.

## 📊 Database Design (ERDs)

The project utilizes a relational database structure to handle:

* User profiles and roles (Donor vs. Admin).
* NGO details and verification status.
* Donation transaction logs and history.

## ⚠️ Project Status: Work in Progress

This repository serves as a snapshot of the development process. Please note the following:

* **Debugging:** The application is currently in a "non-final" state with several bugs yet to be resolved.
* **Incomplete Modules:** While the foundation for the Admin Dashboard and User Auth is present, some functional logic may be missing or require further testing.

## 🛠️ Setup & Installation

1. Clone the repository:
```bash
git clone https://github.com/mystiiii/GabayForGood.git

```


2. Open the `.sln` file in **Visual Studio 2022**.
3. Ensure the **Connection String** in `appsettings.json` is updated to match your local SQL Server instance.
4. Run the following command in the Package Manager Console to set up the database:
```bash
Update-Database

```


5. Build and Run the project.

---

**Developed by:** [Lei Villacorta](https://www.google.com/search?q=https://github.com/mystiiii) & Team

*Final Project for ENTPROG-FTIS3*
