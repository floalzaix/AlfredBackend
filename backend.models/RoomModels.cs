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

/**
 * Read model for the room list (PIE-02): a room plus
 * the number of devices it contains.
 */
public class RoomSummary {
    public Guid Id { get; set; }

    public Guid HouseholdId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DeviceCount { get; set; }
}
