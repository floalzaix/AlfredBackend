/**
 * Household models.
 *
 * A household groups users (through HouseholdMember) and
 * owns the rooms. A user belongs to a household only through
 * a HouseholdMember row, which also carries the user's role
 * inside that household (not to be confused with the global
 * User.Role used by the JWT policies).
 */

//
//  Imports
//

namespace Backend.Models;

//
//  Enum
//

public enum HouseholdRole {
    Admin,
    Member
}

//
//  Classes
//

public class Household {
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // Null when no invitation code is active (see MEM-06).
    public string? InvitationCode { get; set; }

    public DateTime CreatedAt { get; set; }
}

/**
 * Join entity between a User and a Household.
 *
 * The primary key is (HouseholdId, UserId) so a user
 * cannot join the same household twice.
 */
public class HouseholdMember {
    public Guid HouseholdId { get; set; }

    public Guid UserId { get; set; }

    public HouseholdRole Role { get; set; }

    public DateTime JoinedAt { get; set; }
}
