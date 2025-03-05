namespace server;

#region Message Records
public record Message(
  int UserId,
  string Content);

public record MessageDTO(string Email,
  string Name,
  string Content,
  int CompanyID);

public record MessageDetails(
  int MessageId,
  int UserId,
  string Content);

#endregion

#region Case Records
public record CaseDetails(
  int CaseId,
  string Title,
  string Description,
  string CaseNumber,
  string Status,
  DateTime CreatedAt);
#endregion

#region Company Records
public record Company(
  int id,
  string name,
  string phone,
  string email
  );

public record CompanyDTO(
  string name,
  string phone,
  string email);
#endregion

#region Category Records
public record Category(
  int id,
  string name);
#endregion

#region Product Records
public record Products(
    int id,
    string name,
    string description,
    int company_id);

public record PostProductDTO(
  string Name,
  string Description,
  int companyId);

public record PutProductDTO(
  string Name,
  string Description);
#endregion

#region User & Employee Records
public record Users(
  int id,
  string name,
  string email,
  string password,
  string phonenumber,
  int role_id);

public record PostUserDTO(
  string Name,
  string Email,
  string Password,
  string Phonenumber);

public record LoginRequest(
  string email,
  string password);

public record Employees(
  int id,
  int userId,
  int companyId);
#endregion

#region Ticket Records
public record TicketForm(
      int ticket_id,
      int company_id,
      string title,
      string description,
      List<Products> company_products,
      List<Category> categories);

public record TicketFormDTO(
  int TicketId,
  int ProductId,
  int CategoryId,
  string Content);

public record Ticket(
  int id,
  string date,
  string title,
  string categoryname,
  string email,
  string status,
  string caseNumber,
  string description);

public record TicketStatus(
  int id,
  string statusName);
#endregion