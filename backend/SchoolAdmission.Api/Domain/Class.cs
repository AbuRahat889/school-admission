namespace SchoolAdmission.Api.Domain;

public class Class
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<Hold> Holds { get; set; } = new();
}
