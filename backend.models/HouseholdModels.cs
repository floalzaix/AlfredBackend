using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public enum HouseholdRole
{
    Admin,
    Member
}

public class Household
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? InvitationCode { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<HouseholdMember> Members { get; set; } = new List<HouseholdMember>();
}

public class HouseholdMember
{
    public Guid HouseholdId { get; set; }

    public Guid UserId { get; set; }

    public HouseholdRole Role { get; set; }

    public DateTime JoinedAt { get; set; }

    public Household Household { get; set; } = null!;

    public User User { get; set; } = null!;
}
