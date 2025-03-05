public record Users(
    int id,
    string name,
    string email,
    string password,
    string phonenumber,
    int role_id,
    int companyId);

public record LoginRequest(string email, string password);

public record Employees(int UserId, int CompanyId);
