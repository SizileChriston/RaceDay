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

9. Route Endpoints

Routes represent the physical routes associated with RaceDay events.

#	Method	Route	Description	Role Required	Request Body	Expected Response
14	GET	/api/events/{eventId}/routes	Retrieves routes associated with an event.	Public	None	200 OK, 404 Not Found
15	POST	/api/events/{eventId}/routes	Creates a route for an event owned by the organiser.	Organiser	routeName, distanceKm, description	201 Created, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found
16	PUT	/api/routes/{routeId}	Updates a route belonging to the organiser's event.	Organiser	routeName, distanceKm, description	200 OK, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found
17	DELETE	/api/routes/{routeId}	Deletes a route belonging to the organiser's event.	Organiser	None	204 No Content, 401 Unauthorized, 403 Forbidden, 404 Not Found

The API will determine route ownership through:

Route
  |
  v
Route.EventID
  |
  v
Event.OrganiserID
  |
  v
Authenticated UserID

This prevents an organiser from modifying another organiser's route.

10. Enrolment Endpoints
#	Method	Route	Description	Role Required	Request Body	Expected Response
18	POST	/api/enrolments	Enrols the authenticated participant in an event category.	Participant	categoryId	201 Created, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 409 Conflict
19	GET	/api/enrolments/my	Retrieves all enrolments belonging to the authenticated participant.	Participant	None	200 OK, 401 Unauthorized
20	GET	/api/enrolments	Retrieves enrolments associated with events owned by the authenticated organiser.	Organiser	None	200 OK, 401 Unauthorized, 403 Forbidden
21	DELETE	/api/enrolments/{enrolmentId}	Cancels an enrolment belonging to the authenticated participant.	Participant	None	204 No Content, 401 Unauthorized, 403 Forbidden, 404 Not Found
10.1 Enrolment Business Rules

The API should prevent:

Unauthenticated users from creating enrolments.
Organisers from enrolling as participants through this endpoint.
Participants from enrolling in the same category more than once.
Enrolment into a non-existent category.
Enrolment when the category's maximum participant capacity has been reached.
A participant from cancelling another participant's enrolment.

The API should also verify that the authenticated participant matches the UserID associated with the enrolment before allowing a cancellation.

The database supports duplicate prevention through the unique constraint on:

(UserID, CategoryID)
11. Result Endpoints
#	Method	Route	Description	Role Required	Request Body	Expected Response
22	POST	/api/enrolments/{enrolmentId}/result	Records a race result for an enrolled participant.	Organiser	finishTime, position, resultStatus	201 Created, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 409 Conflict
23	PUT	/api/results/{resultId}	Updates an existing race result.	Organiser	finishTime, position, resultStatus	200 OK, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found
24	GET	/api/results/my	Retrieves results belonging to the authenticated participant.	Participant	None	200 OK, 401 Unauthorized
25	GET	/api/results	Retrieves results associated with events owned by the authenticated organiser.	Organiser	None	200 OK, 401 Unauthorized, 403 Forbidden
26	GET	/api/events/{eventId}/results	Retrieves public results and leaderboard information for an event.	Public	None	200 OK, 404 Not Found
11.1 Result Status Values

The API will use the following result statuses:

Official
Pending
Disqualified

These values correspond directly with the database validation rules defined in the RaceDay SQL database.

Each enrolment can have a maximum of one result because the database defines a unique relationship between Results.EnrolmentID and the associated enrolment.

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
19. Consistency with the RaceDay Database

The API design has been aligned with the database schema defined in RaceDay_Database.sql.

API Concept	Database Implementation
User account	Users
Event ownership	Events.OrganiserID → Users.UserID
Event categories	Categories.EventID → Events.EventID
Event routes	Routes.EventID → Events.EventID
Participant enrolment	Enrolments.UserID → Users.UserID
Category enrolment	Enrolments.CategoryID → Categories.CategoryID
Race result	Results.EnrolmentID → Enrolments.EnrolmentID
Result status	Official, Pending, Disqualified

The API endpoint design is therefore based on the same six entities represented in the RaceDay ERD and database.

20. Part 2 Implementation Note

This document represents the planned API design for Part 1.

The endpoints described above will be implemented using C# and ASP.NET Core Web API during Part 2.

The implementation will follow the routes, roles, request structures, database relationships, validation rules and expected responses defined in this specification.

Minor technical changes may be introduced during implementation where required by the final architecture or framework, but the implementation should preserve the functionality, relationships and access-control principles established in this Part 1 plan.

No API implementation code is included in this Part 1 document.