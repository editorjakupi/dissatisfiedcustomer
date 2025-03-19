# Dissatisfied Customer

# API Documentation

## Tickets

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