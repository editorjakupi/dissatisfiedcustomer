# Dissatisfied Customer
CRM-system (Customer Relationship Management) är en mjukvara som hjälper företag att hantera kundrelationer, organisera försäljningsprocesser och förbättra kundservice. Det fungerar som en central databas där all kundrelaterad information lagras och görs tillgänglig för olika avdelningar såsom försäljning, marknadsföring och support.

## Usage

in order to use ...

### Installation

to install this you need to set up the server and client parts of the application...



# API Documentation
## Users

### Get User by ID
**Endpoint:** `GET /api/users/{id}`  
**Description:** Hämtar en användare baserat på ID.  
**Parameters:**  
- `id` (int, required) - Användarens unika ID.  
**Response:** JSON med användarinformation.  

### Get Users from Company
**Endpoint:** `GET /api/usersfromcompany`  
**Description:** Hämtar alla användare kopplade till ett företag.  
**Response:** Lista av användare i JSON-format.  

### Get All Users
**Endpoint:** `GET /api/users`  
**Description:** Hämtar alla användare.  
**Response:** Lista av användare i JSON-format.  

### Create User
**Endpoint:** `POST /api/users`  
**Description:** Skapar en ny användare.  
**Request Body:** JSON med användardata.  
**Response:** Skapad användare i JSON-format.  

### Delete User
**Endpoint:** `DELETE /api/users/{id}`  
**Description:** Raderar en användare baserat på ID.  
**Parameters:**  
- `id` (int, required) - Användarens unika ID.  
**Response:** Bekräftelsemeddelande.  

### Update User
**Endpoint:** `PUT /api/users/{userId}`  
**Description:** Uppdaterar information för en användare.  
**Parameters:**  
- `userId` (int, required) - Användarens unika ID.  
**Request Body:** JSON med uppdaterad användardata.  
**Response:** Bekräftelsemeddelande.  

### Promote User to Admin
**Endpoint:** `PUT /api/promoteuser/{userId}`  
**Description:** Uppgraderar en användare till administratör.  
**Parameters:**  
- `userId` (int, required) - Användarens unika ID.  
**Response:** Bekräftelsemeddelande.  

## Employees

### Get Employees by User ID
**Endpoint:** `GET /api/employees/{userId}`  
**Description:** Hämtar anställda kopplade till ett användar-ID.  
**Parameters:**  
- `userId` (int, required) - Användarens unika ID.  
**Response:** Lista av anställda i JSON-format.  

### Get Employees by Company ID
**Endpoint:** `GET /api/employee/{companyId}`  
**Description:** Hämtar anställda kopplade till ett företags-ID.  
**Parameters:**  
- `companyId` (int, required) - Företagets unika ID.  
**Response:** Lista av anställda i JSON-format.  

### Create Employee
**Endpoint:** `POST /api/employees`  
**Description:** Skapar en ny anställd.  
**Request Body:** JSON med anställd-data.  
**Response:** Bekräftelsemeddelande.  

### Delete Employee
**Endpoint:** `DELETE /api/employees/{userId}`  
**Description:** Raderar en anställd baserat på användar-ID.  
**Parameters:**  
- `userId` (int, required) - Användarens unika ID.  
**Response:** Bekräftelsemeddelande.  

## Authentication

### Login
**Endpoint:** `POST /api/login`  
**Description:** Hanterar inloggning.  
**Request Body:** JSON med användaruppgifter.  
**Response:** Sessionsdata.  

### Get Session User
**Endpoint:** `GET /api/session`  
**Description:** Hämtar information om den aktuella sessionens användare.  
**Response:** JSON med användarinformation.  

### Logout
**Endpoint:** `POST /api/logout`  
**Description:** Hanterar utloggning.  
**Response:** Bekräftelsemeddelande.  

## Super Admin Management

### Get All Admins
**Endpoint:** `GET /api/adminlist`  
**Description:** Hämtar alla administratörer.  
**Response:** Lista av administratörer i JSON-format.  

### Get Admin by User ID
**Endpoint:** `GET /api/adminlist/{userId}`  
**Description:** Hämtar en administratör baserat på användar-ID.  
**Parameters:**  
- `userId` (int, required) - Användarens unika ID.  
**Response:** JSON med administratörsdata.  

### Update Admin
**Endpoint:** `PUT /api/adminlist/{userId}`  
**Description:** Uppdaterar information för en administratör.  
**Parameters:**  
- `userId` (int, required) - Användarens unika ID.  
**Request Body:** JSON med uppdaterad administratörsdata.  
**Response:** Bekräftelsemeddelande.  

### Delete Admin
**Endpoint:** `DELETE /api/company/admins/{userId}`  
**Description:** Raderar en administratör.  
**Parameters:**  
- `userId` (int, required) - Användarens unika ID.  
**Response:** Bekräftelsemeddelande.  

### Update User for Super Admin
**Endpoint:** `PUT /api/putuser/{userId}`  
**Description:** Uppdaterar användarinformation för superadmin.  
**Parameters:**  
- `userId` (int, required) - Användarens unika ID.  
**Request Body:** JSON med uppdaterad användardata.  
**Response:** Bekräftelsemeddelande.  

## Tickets & Feedback

### Get Ticket by ID
**Endpoint:** `GET /api/tickets/{id}`
**Description:** Gets a ticket based on ticket ID.
**Parameters:**
- `id` (int, required) - Unique ID for a ticket.
**Response:** JSON containing ticket information

### Update Category for a Ticket
**Endpoint:** `PUT /api/ticketscategory`
**Description:** Updates the category of a ticket.
**Parameters:**
- `ticketId` (int, required) - Unique ID for a ticket.
- `categoryName` (string, required) - String containing category name.
**Request Body:** JSON with updated category data.
**Response:** Confirmaiton message.

### Update Ticket status
**Endpoint:** `Put /api/ticketstatus`
**Description:** Updates ticket status based on ticket ID and status ID.
**Parameters:**
- `ticketID` (int, required) - Unique ID for a ticket.
- `status` (int, required) - ID for status.
**Response:** Confirmation message.

### Update Product for Ticket
**Endpoint:** `Put /api/ticketsproduct`
**Description:** Updates product for ticket based on ticket ID and product ID.
**Parameters:**
- `ticketID` (int, required) - Unique ID for a ticket.
- `productID` (int, requred) - Unique ID for a product.
**Response:** Confirmation message.

### Get Ticket status
**Endpoint:** `Put /api/ticketstatus`
**Description:** Get's all ticket statuses.
**Response:** List with all ticket statuses in JSON-format.

### Reset Ticket status
**Endpoint:** `Put /api/tickets/{id}` 
**Description:** Updates and resets ticket status.
**Parameters:**
- `ticketID` (int, required) - Unique ID for a ticket.
**Response:** Confirmation message.

### Get Ticket Feedback
**Endpoint:** `GET /api/tickets/feedback`  
**Description:** Hämtar feedback kopplat till biljetter.  
**Response:** Lista av feedback i JSON-format.  

## Security

### Hash Password
**Endpoint:** `POST /api/password/hash`  
**Description:** Genererar en hash för lösenord.  
**Request Body:** JSON med lösenord.  
**Response:** Hashed lösenord.  