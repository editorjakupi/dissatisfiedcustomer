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

## Products

### Get All Products connected to a Company
**Endpoint:** `GET api/products/{company_id}`
**Description:** Hämtar alla produkter kopplade till ett företag.
**Parameters:**
- `company_id` (int ,required) - företagets unika ID.
  **Response:** Lista med alla företags produkter i JSON-format.

## Get Products By Product ID
**Endpoint:** `GET /api/product/{product_id}`
**Description:** Hämtar alla produkter kopplade till ett product-ID.
**Parameters:**
- `product_id` (int, required) - produktens unika ID.
  **Response:** Lista av produkter i JSON-format.

## Create Product
**Endpoint:** `POST /api/products/`
**Description:** skapar en ny produkt.
**Request Body:** JSON med product data.
**Response:** Bekräftelsemeddelande.

## Delete Product
**Endpoint:** `DELETE /api/products/{id}`
**Description:** Raderar en produkt.
**Parameters:**
- `id` (int, required) - Produktens unika ID.
  **Response:** Bekräftelsemeddelande.

### Update Product
**Endpoint:** `PUT /api/products/`
**Description:** Uppdaterar information för en produkt.
**Parameters:**
- `id` (int, required) - Produktens unika ID.
  **Request Body:** JSON med uppdaterad produkt data.
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

### Create Company
**Endpoint:** `POST /api/company`
**Description:** Skapar ett nytt företag.
**Request Body:** JSON med företags data.
**Response:** Bekräftelsemeddelande.

### Delete Company
**Endpoint:** `DELETE /api/company/{id}`
**Description:** Raderar ett företag.
**Parameters:**
-`id` (int, required) - Företagets unika ID.
**Response:** Bekräftelsemeddelande.

### Get Company By ID
**Endpoint:** `GET /api/company/{id}`
**Description:** Hämtar information för ett företag baserat på företags-ID.
**Parameters:**
-`id` (int, required) - Företagets unika ID.
**Response:** Bekräftelsemeddelande.

### Get All Companies
**Endpoint:** `GET /api/company/`
**Description:** Hämtar alla företag.
**Response:** Lista av företag i JSON-format.

## Update Company
**Endpoint:** `PUT /api/company/`
**Description:** Uppdaterar information för ett företag.
**Request Body:** JSON med  uppdaterad företags data.
**Response:** Bekräftelsemeddelande.

## Get Unassigned Users
**Endpoint:** `GET api/company/admins/`
**Description:** Hämtar alla amvändare som inte är kopplade till ett företag.
**Response:** Lista av användarnamn och användar-ID i JSON-format.

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




## Case Endpoints

### Add Message to Ticket (Employee Dashboard)
**Endpoint:** `POST /api/tickets/handle/{ticketId}/messages`  
**Description:** Lägger till ett nytt meddelande i ett ärende via anställdas dashboard.  
**Parameters:**  
- `ticketId` (int, required) - ID för ärendet där meddelandet ska läggas till.  

**Request Body:**  
json
{
  "content": "This is a message from the employee."
}

**Response:
200 OK:Returnerar ett meddelande som bekräftar att operationen lyckades.
eller
404 Not Found: Returnerar ett felmeddelande som anger att ärendet inte hittades.



## Message Endpoints

### Add a New Message
**Endpoint:** `POST /api/messages`  
**Description:** Lägger till ett nytt meddelande anonymt.  
**Request Body:**  
json
{
  "email": "example@domain.com",
  "content": "This is a test message."
}

**Response:
200 OK: Returnerar en bekräftelse på att meddelandet har lagts till framgångsrikt.
eller
400 Bad Request: Returneras om något fel inträffar, till exempel ogiltiga inmatningar eller valideringsfel.




### Send Customer Message Using Token
**Endpoint:** `POST /api/tickets/view/{token}/messages`  
**Description:** Lägger till ett nytt meddelande i ett ärende från kunden med hjälp av en token.  
**Parameters:**  
- `token` (string, required) - Unik token som identifierar ärendet.  

**Request Body:**  
json
{
  "content": "Hej, jag behöver hjälp med en produkt."
}

**Response:
200 OK: Returnerar en bekräftelse på att meddelandet har lagts till framgångsrikt.
eller
404 Not Found: Returneras om det inte finns något ärende som matchar den angivna token.




### Get Messages by Ticket ID
**Endpoint:** `GET /api/tickets/{ticketId}/messages`  
**Description:** Hämtar alla meddelanden kopplade till ett specifikt ärende.  
**Parameters:**  
- `ticketId` (int, required) - ID för ärendet.  

**Response:**  
**200 OK:** Returnerar en lista över alla meddelanden kopplade till det angivna ärendet.
eller
404 Not Found: Returneras om det angivna ticketId inte hittas i systemet.




### Get Ticket by Token
**Endpoint:** `GET /api/tickets/view/{token}`  
**Description:** Hämtar information om ett specifikt ärende med hjälp av en token.  
**Parameters:**  
- `token` (string, required) - Unik token som identifierar ärendet.  

**Response:**  
- **200 OK:**  
  Returnerar information om det angivna ärendet.  
eller
404 Not Found: Returneras om inget ärende hittas som matchar den angivna token




### Get Tickets by View
**Endpoint:** `GET /api/tickets?view={view}`
**Descripiton:** Hämtar ticket baserat på view (måste ha ett company_id för att fungera)
**Parameters:**
- `view` (string) - view som identifierar (all, open, closed, pending)

- **Response:**  
- **200 OK:**  
  Returnerar information om det angivna ärendet.  
eller
404 Not Found: Returneras om inget ärende hittas som matchar den angivna token
400 Bad Request: Returneras om du inte skickar med company_id i requesten
