/**
 * Room models.
 *
 * A room belongs to exactly one household and groups
 * the devices and sensors of that household (PIE stories).
 */

//
//  Imports
//

namespace Backend.Models;

//
//  Class
//

public class Room {
    public Guid Id { get; set; }

    public Guid HouseholdId { get; set; }

    public string Name { get; set; } = string.Empty;
}
