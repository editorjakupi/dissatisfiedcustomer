public record Users(
    int id,
    string name,
    string email,
    string password,
    string phonenumber,
    int role_id);

public record LoginRequest(string email, string password);