# RaceDay

## South African Road Event Management System

RaceDay is a full-stack web-based event management platform designed for the South African road running, walking and cycling community.

The project is being developed progressively across three parts:

* **Part 1 — System Planning and Database**
* **Part 2 — RESTful API**
* **Part 3 — MVC Web Application**

RaceDay allows Event Organisers to manage race events, categories, routes, participant enrolments and results, while Participants can discover events, enter race categories and track their personal race results.

## Project Purpose

The purpose of RaceDay is to provide a centralised digital system for managing road running, walking and cycling events.

The system is designed around two primary user roles:

* **Organiser**
* **Participant**

The application is being designed using a relational SQL Server database, a RESTful C# API and an MVC web application.

## Current Part 1 Deliverables

Part 1 establishes the system blueprint before application implementation begins.

The `/docs` folder contains:

```text
docs/
├── RaceDay_ERD.png
├── API_Endpoint_Plan.md
└── RaceDay_Database.sql
```

### Entity Relationship Diagram

The RaceDay ERD defines the six core entities:

```text
Users
Events
Categories
Routes
Enrolments
Results
```

It defines their attributes, primary keys, foreign keys, relationships and cardinality.

### API Endpoint Plan

The API Endpoint Plan defines the RESTful API that will be implemented during Part 2.

The plan contains **26 endpoints** covering:

* Authentication
* User profiles
* Events
* Categories
* Routes
* Enrolments
* Results
* Role-based access control
* Validation
* Error handling

### SQL Database

The SQL script creates and populates the RaceDay database using Microsoft SQL Server.

It contains:

* Six tables
* Primary keys
* Foreign keys
* `NOT NULL` constraints
* `UNIQUE` constraints
* `DEFAULT` constraints
* Validation constraints
* Realistic seed data
* Verification queries

The script has been tested successfully against a local SQL Server LocalDB instance.

## Project Architecture

```text
MVC Web Application
        │
        │ HTTP / JSON
        ▼
RaceDay REST API
        │
        │ Data Access
        ▼
RaceDay SQL Server Database
```

Part 1 establishes the database and API blueprint.

Part 2 will implement the RESTful API.

Part 3 will implement the MVC application that consumes the API.
## User Roles

### Organiser

Organisers are responsible for managing RaceDay events.

An Organiser can:

* Create events
* Edit their own events
* Delete their own events
* Create event categories
* Update event categories
* Delete event categories
* Create race routes
* Update race routes
* Delete race routes
* View participant enrolments for their events
* Capture participant results
* Update race results
* View results for their events

Organiser permissions will be enforced at the API level during Part 2.

### Participant

Participants use RaceDay to discover and enter events.

A Participant can:

* Create an account
* Log in
* View their own profile
* Update their own profile
* Browse available events
* View event categories
* View event routes
* Enrol in a category
* View their own enrolments
* Cancel their own enrolments
* View their own race results

Participants cannot create or manage events or record official race results.

### Access Control

RaceDay separates authentication from authorisation.

**Authentication** determines who the user is.

**Authorisation** determines what the user is allowed to do.

For example:

```text
Participant
    │
    └── POST /api/enrolments
             │
             └── Allowed

Participant
    │
    └── POST /api/events
             │
             └── 403 Forbidden

Organiser
    │
    └── POST /api/events
             │
             └── Allowed
```
## SQL Database Setup

The RaceDay database is defined by:

```text
docs/RaceDay_Database.sql
```

The script is written for Microsoft SQL Server and has been tested against:

```text
(localdb)\MSSQLLocalDB
```

### Running the database script

From the project root, the script can be executed with SQL Server tools using:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "docs/RaceDay_Database.sql"
```

The script creates the `RaceDay` database and the following tables:

```text
Users
Events
Categories
Routes
Enrolments
Results
```

### Seed Data Verification

The script includes verification queries confirming the expected records.

The current test data contains:

| Table      | Records |
| ---------- | ------: |
| Users      |       4 |
| Events     |       3 |
| Categories |       7 |
| Routes     |       3 |
| Enrolments |       4 |
| Results    |       2 |

The database script is designed to recreate the database during development so that it can be tested repeatedly from a clean state.

## GitHub Actions CI/CD

RaceDay uses GitHub Actions to validate the Part 1 repository structure.

The workflow is stored in:

```text
.github/workflows/
```

The workflow checks that the required Part 1 files are present, including:

```text
docs/RaceDay_ERD.png
docs/API_Endpoint_Plan.md
docs/RaceDay_Database.sql
README.md
```

The workflow runs automatically when changes are pushed to the `main` branch.

A successful workflow is represented by a green check mark in GitHub Actions.

### CI/CD Validation Flow

```text
Developer
    │
    │ git push
    ▼
GitHub Repository
    │
    ▼
GitHub Actions
    │
    ├── Check repository structure
    ├── Check required documentation
    └── Validate Part 1 files
    │
    ▼
Successful Build
    │
    ▼
Green Check Mark
```

A screenshot of the successful workflow run will be included below as submission evidence.
