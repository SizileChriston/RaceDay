# RaceDay API Endpoint Plan

## 1. Purpose
The RaceDay API will provide backend services for authentication, user profiles, events, categories, enrolments and results. It will be implemented in C# in Part 2 and consumed by the MVC application in Part 3.

## 2. Roles
- **Public:** Can register, log in and browse events/categories.
- **Participant:** Can manage their profile, enrol in categories, view/cancel their own enrolments and view their own results.
- **Organiser:** Can create/edit/delete their events, manage categories, view enrolments for their events and capture/update results.

## 3. HTTP Methods
| Method | Purpose |
|---|---|
| GET | Retrieve information |
| POST | Create a resource or perform an action |
| PUT | Update an existing resource |
| DELETE | Remove/cancel a resource |

## 4. Endpoint Plan

| # | HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---:|---|---|---|---|---|---|
| 1 | POST | `/api/auth/register` | Register a new participant account. | Public | `firstName`, `lastName`, `email`, `password` | **201 Created**; **400 Bad Request** for invalid data; **409 Conflict** if email exists. |
| 2 | POST | `/api/auth/login` | Authenticate a user and return a token and role. | Public | `email`, `password` | **200 OK**; **400 Bad Request**; **401 Unauthorized** for incorrect credentials. |
| 3 | GET | `/api/users/me` | Retrieve the authenticated user's profile. | Participant / Organiser | None | **200 OK**; **401 Unauthorized**. |
| 4 | PUT | `/api/users/me` | Update the authenticated user's profile. | Participant / Organiser | `firstName`, `lastName`, `email` | **200 OK**; **400 Bad Request**; **401 Unauthorized**; **409 Conflict** if email is already used. |
| 5 | GET | `/api/events` | Retrieve available RaceDay events. | Public | None | **200 OK** with events. |
| 6 | GET | `/api/events/{eventId}` | Retrieve one event by ID. | Public | None | **200 OK**; **404 Not Found**. |
| 7 | POST | `/api/events` | Create a new event. The authenticated organiser is assigned as owner. | Organiser | `eventName`, `description`, `eventDate`, `location`, `eventType` | **201 Created**; **400 Bad Request**; **401 Unauthorized**; **403 Forbidden**. |
| 8 | PUT | `/api/events/{eventId}` | Update an event managed by the organiser. | Organiser | `eventName`, `description`, `eventDate`, `location`, `eventType` | **200 OK**; **400 Bad Request**; **401**; **403**; **404 Not Found**. |
| 9 | DELETE | `/api/events/{eventId}` | Delete an event managed by the organiser. | Organiser | None | **204 No Content**; **401**; **403**; **404**. |
| 10 | GET | `/api/events/{eventId}/categories` | Retrieve all categories for an event. | Public | None | **200 OK**; **404 Not Found**. |
| 11 | POST | `/api/events/{eventId}/categories` | Create a category for an organiser's event. | Organiser | `categoryName`, `distanceKm`, `maxParticipants`, `entryFee` | **201 Created**; **400**; **401**; **403**; **404**. |
| 12 | PUT | `/api/categories/{categoryId}` | Update an event category. | Organiser | `categoryName`, `distanceKm`, `maxParticipants`, `entryFee` | **200 OK**; **400**; **401**; **403**; **404**. |
| 13 | DELETE | `/api/categories/{categoryId}` | Delete a category when it has no dependent enrolments. | Organiser | None | **204 No Content**; **401**; **403**; **404**; **409 Conflict** if dependent enrolments exist. |
| 14 | POST | `/api/enrolments` | Enrol the authenticated participant in a category. | Participant | `categoryId` | **201 Created**; **400**; **401**; **403**; **404**; **409 Conflict** for duplicate/full enrolment. |
| 15 | GET | `/api/enrolments/my` | Retrieve the authenticated participant's enrolments. | Participant | None | **200 OK**; **401 Unauthorized**. |
| 16 | GET | `/api/enrolments` | Retrieve enrolments for events managed by the organiser. | Organiser | None | **200 OK**; **401**; **403**. |
| 17 | DELETE | `/api/enrolments/{enrolmentId}` | Cancel the authenticated participant's own enrolment. | Participant | None | **204 No Content**; **401**; **403**; **404**. |
| 18 | POST | `/api/enrolments/{enrolmentId}/result` | Capture a result for an enrolment. | Organiser | `finishTime`, `position`, `resultStatus` | **201 Created**; **400**; **401**; **403**; **404**; **409 Conflict** if a result already exists. |
| 19 | PUT | `/api/results/{resultId}` | Correct/update an existing result. | Organiser | `finishTime`, `position`, `resultStatus` | **200 OK**; **400**; **401**; **403**; **404**. |
| 20 | GET | `/api/results/my` | Retrieve results belonging to the authenticated participant. | Participant | None | **200 OK**; **401 Unauthorized**. |
| 21 | GET | `/api/results` | Retrieve results associated with events managed by the organiser. | Organiser | None | **200 OK**; **401**; **403**. |

## 5. Request Body Examples

### Register
```json
{
  "firstName": "Thabo",
  "lastName": "Mokoena",
  "email": "thabo@example.com",
  "password": "Password123!"
}
```

### Login
```json
{
  "email": "thabo@example.com",
  "password": "Password123!"
}
```

### Create Event
```json
{
  "eventName": "Soweto 10K Run",
  "description": "Annual 10 kilometre road race",
  "eventDate": "2026-10-10T08:00:00",
  "location": "Soweto",
  "eventType": "Running"
}
```

### Create Category
```json
{
  "categoryName": "10 KM Men",
  "distanceKm": 10.00,
  "maxParticipants": 500,
  "entryFee": 150.00
}
```

### Create Enrolment
```json
{
  "categoryId": 1
}
```

### Capture Result
```json
{
  "finishTime": "00:52:31",
  "position": 15,
  "resultStatus": "Finished"
}
```

## 6. Authorisation Rules
1. Registration is public and creates a participant account.
2. Protected endpoints require authentication.
3. Organiser-only actions are event/category management, organiser enrolment viewing, and result management.
4. Participant-only actions are enrolment creation/cancellation and viewing their own results.
5. Participants cannot access or modify another participant's records.
6. Organisers can only manage events and related records belonging to them.
7. Protected operations should obtain the authenticated user's ID from the authentication token rather than trusting a client-supplied UserID.
8. Role-based access control will be implemented in Part 2.

## 7. Relationship to the ERD
The endpoint plan follows the RaceDay ERD relationships:
- `Users 1:N Events` → event management.
- `Users 1:N Enrolments` → participant enrolments.
- `Events 1:N Categories` → category management.
- `Events 1:N Routes` → route information associated with events.
- `Categories 1:N Enrolments` → category enrolments.
- `Enrolments 1:0..1 Results` → result creation/update.

## 8. Scope
This is a Part 1 planning document. The endpoints will be implemented as a RESTful C# API in Part 2. The Part 3 MVC application will consume the API rather than connect directly to SQL Server.
