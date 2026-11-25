namespace Backend.Models;

public enum HouseholdRole { Admin, Member }

public class Household {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? InvitationCode { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class HouseholdMember {
    public Guid HouseholdId { get; set; }
    public Guid UserId { get; set; }
    public HouseholdRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}
