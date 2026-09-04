RaceDay RESTful API Specification & Endpoint Plan
1. System Overview

The RaceDay RESTful API will provide the backend services for the RaceDay event management system.

The API will support:

User registration and authentication
User profile management
Race event management
Event category management
Race route management
Participant enrolments
Race result management
Public event results and leaderboards
Role-based access control

The API will be developed using C# and ASP.NET Core Web API during Part 2 and will be consumed by the MVC web application during Part 3.

The API is designed to act as the middle layer between the RaceDay database and the MVC application. The MVC application will not directly access the database.

2. API Standards
Specification	Standard
API Architecture	RESTful Web API
Base Route	/api
Data Format	JSON (application/json)
Authentication	JWT Bearer Authentication
Database	Microsoft SQL Server
Programming Language	C#
Framework	ASP.NET Core Web API

Protected endpoints will require a valid JWT access token using the following HTTP header:

Authorization: Bearer <token>
3. User Roles and Access Control

RaceDay contains two authenticated user roles and a public access level.

3.1 Public Access

Unauthenticated users can:

Register for a RaceDay account
Log in
Browse available events
View individual event details
View event categories
View event routes
View public event results and leaderboards

Public users cannot create events, manage categories, create routes, create enrolments, or record results.

3.2 Participant

Authenticated participants can:

View their own profile
Update their own profile
Browse events
View event details
View categories and routes
Enrol in event categories
View their own enrolments
Cancel their own enrolments
View their own race results

Participants cannot:

Create events
Modify events
Delete events
Manage event categories
Manage event routes
Record official race results
3.3 Organiser

Authenticated organisers can:

Create events
Update their own events
Delete their own events
Create categories for their events
Update categories belonging to their events
Delete categories belonging to their events
Create routes for their events
Update routes belonging to their events
Delete routes belonging to their events
View participant enrolments for their events
Record race results
Update race results
View results for their events
3.4 Access Control Principle

Role-based authorisation will be enforced at the API level during Part 2.

Ownership will also be enforced.

An organiser may only modify resources belonging to events that they own.

A participant may only access and modify their own enrolments and personal results.

The API will determine ownership from the authenticated user's identity rather than trusting a user-supplied UserID or OrganiserID.

4. HTTP Methods
Method	Purpose
GET	Retrieves an existing resource or collection of resources.
POST	Creates a new resource or performs an authentication action.
PUT	Updates an existing resource.
DELETE	Deletes a resource or cancels a registration.

These methods follow the standard CRUD approach:

Create → POST
Read → GET
Update → PUT
Delete → DELETE
5. Authentication Endpoints
5.1 Endpoint Table
#	Method	Route	Description	Role Required	Request Body	Expected Response
1	POST	/api/auth/register	Creates a new RaceDay Participant account.	Public	firstName, lastName, email, password	201 Created, 400 Bad Request, 409 Conflict
2	POST	/api/auth/login	Authenticates a registered user and returns a JWT access token.	Public	email, password	200 OK, 400 Bad Request, 401 Unauthorized
5.2 Registration Rules

Public registration will create a Participant account by default.

The public registration request will not allow a client to choose the Organiser role.

The API should validate:

First name is provided.
Last name is provided.
Email address is provided.
Email address is correctly formatted.
Email address is unique.
Password satisfies the application's security requirements.

Organiser accounts will be provisioned through a controlled process rather than allowing an unauthenticated user to assign themselves the Organiser role.

This prevents a public user from submitting:

{
    "role": "Organiser"
}

and gaining organiser privileges.

5.3 Registration Failure Responses

400 Bad Request — Required registration information is missing or invalid.

409 Conflict — The email address is already registered.

5.4 Login Failure Responses

400 Bad Request — Required login information is missing.

401 Unauthorized — Email address or password is incorrect.

6. User Profile Endpoints
#	Method	Route	Description	Role Required	Request Body	Expected Response
3	GET	/api/users/me	Retrieves the profile of the currently authenticated user.	Participant / Organiser	None	200 OK, 401 Unauthorized
4	PUT	/api/users/me	Updates the profile of the currently authenticated user.	Participant / Organiser	firstName, lastName, email	200 OK, 400 Bad Request, 401 Unauthorized, 409 Conflict

The authenticated user's identity will be obtained from the JWT rather than accepting a UserID from the client.

The /me approach prevents a user from attempting to access another user's profile by changing an ID in the URL.

7. Event Endpoints
#	Method	Route	Description	Role Required	Request Body	Expected Response
5	GET	/api/events	Retrieves available RaceDay events.	Public	None	200 OK
6	GET	/api/events/{eventId}	Retrieves detailed information for a specific event.	Public	None	200 OK, 404 Not Found
7	POST	/api/events	Creates a new race event. The authenticated organiser becomes the owner.	Organiser	eventName, description, eventDate, location, eventType	201 Created, 400 Bad Request, 401 Unauthorized, 403 Forbidden
8	PUT	/api/events/{eventId}	Updates an event owned by the authenticated organiser.	Organiser	eventName, description, eventDate, location, eventType	200 OK, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found
9	DELETE	/api/events/{eventId}	Deletes an event owned by the authenticated organiser.	Organiser	None	204 No Content, 401 Unauthorized, 403 Forbidden, 404 Not Found, 409 Conflict
7.1 Event Ownership

When an organiser creates an event, the authenticated user's UserID will be stored in the event's OrganiserID foreign key.

Conceptually:

Authenticated User
       |
       v
     UserID
       |
       v
Events.OrganiserID
       |
       v
Ownership confirmed
       |
       v
Action permitted

An organiser cannot modify or delete another organiser's event.

8. Category Endpoints
#	Method	Route	Description	Role Required	Request Body	Expected Response
10	GET	/api/events/{eventId}/categories	Retrieves all categories belonging to an event.	Public	None	200 OK, 404 Not Found
11	POST	/api/events/{eventId}/categories	Creates a category for an event owned by the organiser.	Organiser	categoryName, distanceKm, maxParticipants, entryFee	201 Created, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 409 Conflict
12	PUT	/api/categories/{categoryId}	Updates an existing category belonging to the organiser's event.	Organiser	categoryName, distanceKm, maxParticipants, entryFee	200 OK, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found
13	DELETE	/api/categories/{categoryId}	Deletes a category where dependent enrolments do not prevent deletion.	Organiser	None	204 No Content, 401 Unauthorized, 403 Forbidden, 404 Not Found, 409 Conflict
8.1 Category Validation

The API should ensure:

distanceKm is greater than zero.
maxParticipants is greater than zero.
entryFee is zero or greater.
The event exists.
The authenticated organiser owns the event.
A duplicate category is not created for the same event.
Existing enrolments are considered before deleting a category.

The database also enforces important category rules through CHECK and UNIQUE constraints.

# 9. Enrolment Endpoints

Enrolments represent a participant's registration for a specific event category.

A participant does not enrol directly in an event. Instead, the participant selects a category belonging to an event and creates an enrolment for that category.

The authenticated user's identity will be obtained from the JWT. The client will not submit an arbitrary `UserID`.

|  # | Method | Route                           | Description                                                                       | Role Required | Request Body | Expected Response                                                                                      |
| -: | ------ | ------------------------------- | --------------------------------------------------------------------------------- | ------------- | ------------ | ------------------------------------------------------------------------------------------------------ |
| 18 | POST   | `/api/enrolments`               | Enrols the authenticated participant in an event category.                        | Participant   | `categoryId` | `201 Created`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict` |
| 19 | GET    | `/api/enrolments/my`            | Retrieves all enrolments belonging to the authenticated participant.              | Participant   | None         | `200 OK`, `401 Unauthorized`                                                                           |
| 20 | GET    | `/api/enrolments`               | Retrieves enrolments associated with events owned by the authenticated organiser. | Organiser     | None         | `200 OK`, `401 Unauthorized`, `403 Forbidden`                                                          |
| 21 | DELETE | `/api/enrolments/{enrolmentId}` | Cancels an enrolment belonging to the authenticated participant.                  | Participant   | None         | `204 No Content`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`                                 |

## 9.1 Create Enrolment Process

The enrolment process is:

```text
Participant logs in
       |
       v
JWT identifies UserID
       |
       v
Participant selects Category
       |
       v
POST /api/enrolments
       |
       v
API verifies Category exists
       |
       v
API identifies associated Event
       |
       v
API checks participant capacity
       |
       v
API checks duplicate enrolment
       |
       v
Enrolment created
```

The client only needs to submit the category being selected.

Example:

```json
{
    "categoryId": 2
}
```

The API determines the participant from the authenticated token.

## 9.2 Enrolment Business Rules

The API should prevent:

* Unauthenticated users from creating enrolments.
* Organisers from enrolling as participants through this endpoint.
* Participants from enrolling in the same category more than once.
* Enrolment into a category that does not exist.
* Enrolment when the category's participant capacity has been reached.
* Participants from cancelling another participant's enrolment.

The API should also verify that the selected category belongs to a valid event.

## 9.3 Database Relationship

The enrolment is represented in the database using:

```text
Users.UserID
      |
      v
Enrolments.UserID

Categories.CategoryID
      |
      v
Enrolments.CategoryID
```

This means one participant can have multiple enrolments and one category can contain multiple participant enrolments.

The database contains a unique constraint on:

```text
(UserID, CategoryID)
```

This prevents the same participant from being enrolled in the same category more than once.

## 9.4 Enrolment Capacity

The API should compare the number of active/confirmed enrolments for a category against:

```text
Categories.MaxParticipants
```

If the category has reached capacity, the API should return:

`409 Conflict` — The category has reached its maximum participant capacity.

## 9.5 Participant Enrolment Ownership

The participant's authenticated identity must be checked before a cancellation is permitted.

Conceptually:

```text
Authenticated User
       |
       v
     UserID
       |
       v
Enrolments.UserID
       |
       v
Ownership confirmed
       |
       v
Cancellation permitted
```

A participant must never be able to cancel another participant's enrolment by changing the `enrolmentId` in the request URL.

## 9.6 Organiser Enrolment Access

The organiser endpoint:

```text
GET /api/enrolments
```

will return enrolments associated with events owned by the authenticated organiser.

The organiser should therefore only see enrolments connected to their own events rather than unrestricted enrolments from every organiser in the system.

The ownership path is:

```text
Enrolment
    |
    v
CategoryID
    |
    v
Category.EventID
    |
    v
Event.EventID
    |
    v
Event.OrganiserID
    |
    v
Authenticated UserID
```

---

# 10. Result Endpoints

Results represent the official performance information associated with a participant's enrolment.

A result belongs to an enrolment rather than directly to a user or event.

|  # | Method | Route                                  | Description                                                                    | Role Required | Request Body                             | Expected Response                                                                                      |
| -: | ------ | -------------------------------------- | ------------------------------------------------------------------------------ | ------------- | ---------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| 22 | POST   | `/api/enrolments/{enrolmentId}/result` | Records a race result for an enrolled participant.                             | Organiser     | `finishTime`, `position`, `resultStatus` | `201 Created`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict` |
| 23 | PUT    | `/api/results/{resultId}`              | Updates an existing race result.                                               | Organiser     | `finishTime`, `position`, `resultStatus` | `200 OK`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`                      |
| 24 | GET    | `/api/results/my`                      | Retrieves results belonging to the authenticated participant.                  | Participant   | None                                     | `200 OK`, `401 Unauthorized`                                                                           |
| 25 | GET    | `/api/results`                         | Retrieves results associated with events owned by the authenticated organiser. | Organiser     | None                                     | `200 OK`, `401 Unauthorized`, `403 Forbidden`                                                          |
| 26 | GET    | `/api/events/{eventId}/results`        | Retrieves public results and leaderboard information for an event.             | Public        | None                                     | `200 OK`, `404 Not Found`                                                                              |

## 10.1 Record Result Process

The intended result workflow is:

```text
Participant
     |
     v
Enrolment
     |
     v
Race takes place
     |
     v
Organiser records result
     |
     v
Results record created
     |
     v
Participant views own result
     |
     v
Public event leaderboard available
```

## 10.2 Result Validation

When an organiser records a result, the API should validate:

* The enrolment exists.
* The enrolment belongs to a valid category.
* The category belongs to a valid event.
* The authenticated organiser owns the event.
* A result does not already exist for the enrolment.
* `position`, when supplied, is greater than zero.
* `finishTime`, when supplied, is in a valid time format.
* `resultStatus` is one of the permitted values.

The permitted result statuses are:

```text
Official
Pending
Disqualified
```

## 10.3 Result Ownership

An organiser's authority to record or update a result is determined through the enrolment's relationship with the event.

The ownership chain is:

```text
Result
   |
   v
EnrolmentID
   |
   v
Enrolment
   |
   v
CategoryID
   |
   v
Category
   |
   v
EventID
   |
   v
Event.OrganiserID
   |
   v
Authenticated Organiser
```

This prevents an organiser from recording or editing results for another organiser's event.

## 10.4 One Result Per Enrolment

The database defines `Results.EnrolmentID` as unique.

Therefore:

```text
One Enrolment
      |
      └──── 0 or 1 Result
```

An organiser attempting to create a second result for the same enrolment should receive:

`409 Conflict` — A result already exists for this enrolment.

## 10.5 Participant Result Access

The participant endpoint:

```text
GET /api/results/my
```

must return only results belonging to the authenticated participant.

The API determines this relationship through:

```text
Authenticated UserID
       |
       v
Enrolments.UserID
       |
       v
Enrolments.EnrolmentID
       |
       v
Results.EnrolmentID
```

The participant cannot supply another user's ID to retrieve their results.

## 10.6 Organiser Result Access

The organiser endpoint:

```text
GET /api/results
```

will return results associated with events owned by the authenticated organiser.

This means an organiser can manage their own event results without receiving unrestricted access to results belonging to other organisers.

## 10.7 Public Results

The endpoint:

```text
GET /api/events/{eventId}/results
```

allows public users to view event results and leaderboard information.

This supports the RaceDay requirement for participants and visitors to track event performance without exposing private user account information.

Public result responses should expose only the information required for event performance reporting.

## 10.8 Result Update

The organiser may update an existing result using:

```text
PUT /api/results/{resultId}
```

This can be used to correct:

* Finish time
* Position
* Result status

An update is permitted only when the authenticated organiser owns the event connected to the result.

---

# 11. Participant-to-Result Workflow

The combined enrolment and result workflow is:

```text
┌───────────────────┐
│     Participant   │
└─────────┬─────────┘
          │
          │ Browse Events
          ▼
┌───────────────────┐
│       Event       │
└─────────┬─────────┘
          │
          │ Select Category
          ▼
┌───────────────────┐
│     Category      │
└─────────┬─────────┘
          │
          │ POST /api/enrolments
          ▼
┌───────────────────┐
│     Enrolment     │
└─────────┬─────────┘
          │
          │ Race takes place
          ▼
┌───────────────────┐
│      Result       │
└─────────┬─────────┘
          │
          ├──────────────► Participant views /api/results/my
          │
          └──────────────► Public views /api/events/{eventId}/results
```

Organiser control operates across the same relationship:

```text
Organiser
    |
    v
Event
    |
    v
Category
    |
    v
Enrolment
    |
    v
Result
```

The API will verify organiser ownership before allowing organiser operations at each stage.

12. Sample Request Payloads
12.1 Register Participant
Endpoint
POST /api/auth/register
Request Body
{
    "firstName": "Thabo",
    "lastName": "Mokoena",
    "email": "thabo.mokoena@example.com",
    "password": "SecurePassword123!"
}

The account created through public registration will have the Participant role.

12.2 Login
Endpoint
POST /api/auth/login
Request Body
{
    "email": "thabo.mokoena@example.com",
    "password": "SecurePassword123!"
}
Example Response
{
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "expiresIn": 3600,
    "user": {
        "userId": 5,
        "firstName": "Thabo",
        "lastName": "Mokoena",
        "role": "Participant"
    }
}
12.3 Create Event
Endpoint
POST /api/events
Request Body
{
    "eventName": "Soweto Marathon & Community Run",
    "description": "A road running event passing through historical landmarks in Soweto.",
    "eventDate": "2026-11-01T06:00:00",
    "location": "Soweto, Johannesburg",
    "eventType": "Running"
}

The API will obtain the organiser identity from the authenticated JWT.

The client will not provide organiserId.

12.4 Create Event Category
Endpoint
POST /api/events/1/categories
Request Body
{
    "categoryName": "Half Marathon",
    "distanceKm": 21.10,
    "maxParticipants": 8000,
    "entryFee": 250.00
}
12.5 Create Event Route
Endpoint
POST /api/events/1/routes
Request Body
{
    "routeName": "Soweto Heritage Route",
    "distanceKm": 21.10,
    "description": "Route passing through major historical landmarks in Soweto."
}
12.6 Participant Enrolment
Endpoint
POST /api/enrolments
Request Body
{
    "categoryId": 2
}
Example Response
{
    "enrolmentId": 5,
    "categoryId": 2,
    "status": "Confirmed",
    "message": "Participant successfully enrolled."
}

The participant's UserID will be obtained from the authenticated JWT.

12.7 Record Race Result
Endpoint
POST /api/enrolments/1/result
Request Body
{
    "finishTime": "01:45:30",
    "position": 142,
    "resultStatus": "Official"
}
Example Response
{
    "resultId": 3,
    "enrolmentId": 1,
    "finishTime": "01:45:30",
    "position": 142,
    "resultStatus": "Official",
    "message": "Race result recorded successfully."
}
13. Standard HTTP Response Codes
Status Code	Meaning	RaceDay Example
200 OK	Request completed successfully.	Event, profile or result retrieved.
201 Created	A new resource was successfully created.	User, event, category, route or enrolment created.
204 No Content	Request succeeded without returning a response body.	Event, route, category or enrolment deleted.
400 Bad Request	Submitted information is missing or invalid.	Invalid event date or category information.
401 Unauthorized	Authentication is missing or invalid.	Invalid or expired JWT token.
403 Forbidden	User is authenticated but does not have permission.	Participant attempts to create an event.
404 Not Found	Requested resource does not exist.	Event or category ID does not exist.
409 Conflict	Request conflicts with existing data or a business rule.	Duplicate email, category, enrolment or result.
14. Database Entity Mapping

The API endpoints correspond directly to the six entities in the RaceDay database.

Database Entity	API Responsibility
Users	Registration, authentication and user profiles
Events	Race event creation and management
Categories	Race category definitions and entry information
Routes	Physical race route information
Enrolments	Participant registrations for race categories
Results	Participant race performance and finishing results
14.1 Entity Relationships
Users
 │
 ├──────────< Events
 │              │
 │              ├──────────< Categories
 │              │                │
 │              │                └──────────< Enrolments
 │              │                                  │
 │              │                                  └────────── Results
 │              │
 │              └──────────< Routes
 │
 └──────────< Enrolments

The API design follows these database relationships rather than treating each endpoint as an unrelated operation.

15. Planned API Controllers

The following controllers are planned for implementation during Part 2.

Controller	Responsibility
AuthController	User registration and authentication
UsersController	Authenticated user profile management
EventsController	Event creation and management
CategoriesController	Event category management
RoutesController	Event route management
EnrolmentsController	Participant enrolments
ResultsController	Race results and public leaderboards
16. Authentication and Authorisation

JWT authentication will be used to protect authenticated API endpoints.

After successful login, the API will issue a JWT containing relevant authentication claims, including:

User identity
User role

The client will include the token in subsequent protected requests:

Authorization: Bearer <token>

The API will then use the authenticated user's identity and role to determine whether the requested operation is permitted.

Example
Participant
Participant
     │
     └── POST /api/enrolments
              │
              └── ALLOWED
Participant attempting organiser functionality
Participant
     │
     └── POST /api/events
              │
              └── 403 Forbidden
Organiser
Organiser
     │
     └── POST /api/events
              │
              └── ALLOWED

Role-based authorisation will be implemented in Part 2.

17. API-to-Database Relationship

The API is designed to act as the middle layer between the RaceDay database and the MVC web application.

┌──────────────────────────────┐
│       MVC Web Application    │
│            Part 3            │
└──────────────┬───────────────┘
               │
               │ HTTP / JSON
               ▼
┌──────────────────────────────┐
│       RaceDay REST API       │
│            Part 2            │
│                              │
│ Authentication               │
│ Authorisation                │
│ Business Rules               │
│ Validation                   │
└──────────────┬───────────────┘
               │
               │ SQL / Data Access
               ▼
┌──────────────────────────────┐
│        RaceDay Database      │
│                              │
│ Users                        │
│ Events                       │
│ Categories                   │
│ Routes                       │
│ Enrolments                   │
│ Results                      │
└──────────────────────────────┘

This separation ensures that the front-end application does not directly access the database.

All application data operations will be performed through the RESTful API.

18. Endpoint Coverage Summary

The RaceDay API plan contains 26 endpoints covering the major system requirements.

Functional Area	Number of Endpoints
Authentication	2
User Profiles	2
Events	5
Categories	4
Routes	4
Enrolments	4
Results	5
Total	26

The endpoint plan provides coverage for:

User registration and authentication
User profile management
Event creation and management
Category management
Route management
Participant enrolments
Participant result history
Organiser enrolment management
Organiser result management
Public event leaderboards
Role-based access control
Ownership validation
Business-rule validation
HTTP error handling
# 19. Database Entity and API Mapping

The RaceDay API has been designed directly from the six entities defined in the RaceDay Entity Relationship Diagram (ERD).

The purpose of this mapping is to ensure that the database structure, API resources, user roles and application functionality remain consistent throughout development.

Each database entity has a clearly defined responsibility within the API.

## 19.1 Entity-to-API Mapping

| ERD Entity | Primary Responsibility                                              | Main API Resource      | Main API Endpoints                                                 |
| ---------- | ------------------------------------------------------------------- | ---------------------- | ------------------------------------------------------------------ |
| Users      | Stores user identity, authentication information and roles.         | Users / Authentication | `/api/auth/register`, `/api/auth/login`, `/api/users/me`           |
| Events     | Stores race event information and organiser ownership.              | Events                 | `/api/events`, `/api/events/{eventId}`                             |
| Categories | Stores race categories and entry information belonging to an event. | Categories             | `/api/events/{eventId}/categories`, `/api/categories/{categoryId}` |
| Routes     | Stores physical race route information belonging to an event.       | Routes                 | `/api/events/{eventId}/routes`, `/api/routes/{routeId}`            |
| Enrolments | Stores participant registrations for event categories.              | Enrolments             | `/api/enrolments`, `/api/enrolments/{enrolmentId}`                 |
| Results    | Stores participant race performance associated with an enrolment.   | Results                | `/api/results`, `/api/results/my`, `/api/events/{eventId}/results` |

---

## 19.2 Users Entity

The `Users` entity stores the identity and authentication-related information for both RaceDay roles:

* Organiser
* Participant

The database fields include:

```text
UserID
FirstName
LastName
Email
PasswordHash
Role
CreatedAt
```

The API uses the `Users` entity for:

```text
POST /api/auth/register
POST /api/auth/login
GET  /api/users/me
PUT  /api/users/me
```

The authenticated user's `UserID` will be used by the API when applying ownership and access-control rules.

The client should not be trusted to provide an arbitrary user identity when performing protected operations.

---

## 19.3 Events Entity

The `Events` entity stores the main race event information.

The database fields include:

```text
EventID
OrganiserID
EventName
Description
EventDate
Location
EventType
CreatedAt
```

The API exposes Events through:

```text
GET    /api/events
GET    /api/events/{eventId}
POST   /api/events
PUT    /api/events/{eventId}
DELETE /api/events/{eventId}
```

The `OrganiserID` foreign key connects each event to its owning organiser.

The API therefore uses the relationship:

```text
Users.UserID
      |
      v
Events.OrganiserID
```

to enforce event ownership.

---

## 19.4 Categories Entity

The `Categories` entity stores the race options available within an event.

The database fields include:

```text
CategoryID
EventID
CategoryName
DistanceKm
MaxParticipants
EntryFee
```

Categories are exposed using:

```text
GET    /api/events/{eventId}/categories
POST   /api/events/{eventId}/categories
PUT    /api/categories/{categoryId}
DELETE /api/categories/{categoryId}
```

The category's `EventID` establishes its parent event.

The ownership path is:

```text
Category
    |
    v
Category.EventID
    |
    v
Event
    |
    v
Event.OrganiserID
    |
    v
Organiser
```

This allows the API to verify that an organiser owns the event before modifying one of its categories.

---

## 19.5 Routes Entity

The `Routes` entity stores information about the physical route associated with an event.

The database fields include:

```text
RouteID
EventID
RouteName
DistanceKm
Description
```

Routes are exposed through:

```text
GET    /api/events/{eventId}/routes
POST   /api/events/{eventId}/routes
PUT    /api/routes/{routeId}
DELETE /api/routes/{routeId}
```

The route belongs to an event through:

```text
Routes.EventID
        |
        v
Events.EventID
```

The event then determines the organiser who owns the route.

---

## 19.6 Enrolments Entity

The `Enrolments` entity records which participant has entered which race category.

The database fields include:

```text
EnrolmentID
UserID
CategoryID
EnrolmentDate
Status
```

The API exposes enrolment functionality through:

```text
POST   /api/enrolments
GET    /api/enrolments/my
GET    /api/enrolments
DELETE /api/enrolments/{enrolmentId}
```

The participant identity is determined from the JWT.

The database relationship is:

```text
Users.UserID
      |
      v
Enrolments.UserID
```

while the selected race category is connected through:

```text
Categories.CategoryID
      |
      v
Enrolments.CategoryID
```

The API can therefore determine the complete participant journey:

```text
Participant
    |
    v
Enrolment
    |
    v
Category
    |
    v
Event
```

The unique database constraint on `(UserID, CategoryID)` supports the API rule preventing a participant from enrolling in the same category more than once.

---

## 19.7 Results Entity

The `Results` entity stores the result associated with a participant's enrolment.

The database fields include:

```text
ResultID
EnrolmentID
FinishTime
Position
ResultStatus
RecordedAt
```

The API exposes result functionality through:

```text
POST /api/enrolments/{enrolmentId}/result
PUT  /api/results/{resultId}
GET  /api/results/my
GET  /api/results
GET  /api/events/{eventId}/results
```

The result does not directly reference a participant or organiser.

Instead, its ownership and context are determined through the relational chain:

```text
Result
   |
   v
EnrolmentID
   |
   v
Enrolment
   |
   v
CategoryID
   |
   v
Category
   |
   v
EventID
   |
   v
Event
   |
   v
OrganiserID
```

This allows the API to verify that an organiser is authorised to create or update a result for an event that they own.

---

# 20. API-to-ERD Relationship Map

The overall relationship between the ERD and API can be represented as:

```text
┌──────────────┐
│    Users     │
└──────┬───────┘
       │
       │ OrganiserID
       ▼
┌──────────────┐
│    Events    │
└───┬────┬─────┘
    │    │
    │    └──────────────┐
    │                   │
    ▼                   ▼
┌──────────────┐   ┌──────────────┐
│  Categories  │   │    Routes    │
└──────┬───────┘   └──────────────┘
       │
       │ CategoryID
       ▼
┌──────────────┐
│  Enrolments  │
└──────┬───────┘
       │
       │ EnrolmentID
       ▼
┌──────────────┐
│   Results    │
└──────────────┘
```

The equivalent API structure is:

```text
/api/users/me
        │
        │
        ▼
/api/events
        │
        ├────────── /api/events/{eventId}/categories
        │
        └────────── /api/events/{eventId}/routes
                       │
                       │
                       ▼
                /api/enrolments
                       │
                       ▼
             /api/enrolments/{id}/result
                       │
                       ▼
                /api/results
```

The API therefore represents the same logical structure as the underlying relational database.

---

# 21. Functional Requirement to Endpoint Mapping

The endpoint plan is also directly mapped to the functional requirements of the RaceDay system.

| Functional Requirement           | Supporting Endpoint(s)                      |
| -------------------------------- | ------------------------------------------- |
| Register an account              | `POST /api/auth/register`                   |
| Log in                           | `POST /api/auth/login`                      |
| View own profile                 | `GET /api/users/me`                         |
| Update own profile               | `PUT /api/users/me`                         |
| Browse events                    | `GET /api/events`                           |
| View an event                    | `GET /api/events/{eventId}`                 |
| Create an event                  | `POST /api/events`                          |
| Edit an event                    | `PUT /api/events/{eventId}`                 |
| Delete an event                  | `DELETE /api/events/{eventId}`              |
| View categories                  | `GET /api/events/{eventId}/categories`      |
| Create category                  | `POST /api/events/{eventId}/categories`     |
| Update category                  | `PUT /api/categories/{categoryId}`          |
| Delete category                  | `DELETE /api/categories/{categoryId}`       |
| View routes                      | `GET /api/events/{eventId}/routes`          |
| Create route                     | `POST /api/events/{eventId}/routes`         |
| Update route                     | `PUT /api/routes/{routeId}`                 |
| Delete route                     | `DELETE /api/routes/{routeId}`              |
| Enter an event category          | `POST /api/enrolments`                      |
| View own enrolments              | `GET /api/enrolments/my`                    |
| Organiser views event enrolments | `GET /api/enrolments`                       |
| Cancel own enrolment             | `DELETE /api/enrolments/{enrolmentId}`      |
| Record participant result        | `POST /api/enrolments/{enrolmentId}/result` |
| Update result                    | `PUT /api/results/{resultId}`               |
| Participant views own results    | `GET /api/results/my`                       |
| Organiser views event results    | `GET /api/results`                          |
| Public views event results       | `GET /api/events/{eventId}/results`         |

This mapping demonstrates that the planned API covers the required RaceDay functionality before implementation begins.

---

# 22. Traceability from ERD to API to Database

The RaceDay design follows a traceability principle:

```text
ERD
 ↓
Database Table
 ↓
API Resource
 ↓
API Endpoint
 ↓
MVC Feature
```

For example:

```text
ERD:
Events
    ↓
Database:
Events table
    ↓
API:
Event endpoints
    ↓
MVC:
Event browsing and organiser event management
```

Another example:

```text
ERD:
Enrolments
    ↓
Database:
Enrolments table
    ↓
API:
POST /api/enrolments
    ↓
MVC:
Participant enters a race category
```

And:

```text
ERD:
Results
    ↓
Database:
Results table
    ↓
API:
POST /api/enrolments/{id}/result
    ↓
MVC:
Organiser captures race results
```

This traceability will help ensure that the implementation in Parts 2 and 3 remains consistent with the planning completed in Part 1.

20. Part 2 Implementation Note

This document represents the planned API design for Part 1.

The endpoints described above will be implemented using C# and ASP.NET Core Web API during Part 2.

The implementation will follow the routes, roles, request structures, database relationships, validation rules and expected responses defined in this specification.

Minor technical changes may be introduced during implementation where required by the final architecture or framework, but the implementation should preserve the functionality, relationships and access-control principles established in this Part 1 plan.

No API implementation code is included in this Part 1 document.