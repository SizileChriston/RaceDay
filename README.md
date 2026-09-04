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
