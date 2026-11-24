namespace Backend.Models;

public sealed class Home {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public enum HomeRole {
    Member,
    Admin
}

public sealed class HomeMembership {
    public Guid HomeId { get; set; }
    public Guid UserId { get; set; }
    public HomeRole Role { get; set; }
}

public sealed record HomeMember(string Username, HomeRole Role);
