namespace server.Records;

public record Products(
    int id,
    string name,
    string description,
    int company_id);