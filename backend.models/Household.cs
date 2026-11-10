namespace Backend.Models;

public class Household {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InvitationCode { get; set; } = string.Empty;
}

public class HouseholdMember {
    public Guid HouseholdId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Household Household { get; set; } = null!;
}
