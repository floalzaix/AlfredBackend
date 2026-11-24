/**
 * DTOs for the room routes.
 */

//
//  Imports
//

using System.ComponentModel.DataAnnotations;

namespace Backend.API.DTOs;

//
//  DTOs
//

/**
 * Body of POST /households/{householdId}/rooms.
 */
public sealed class CreateRoomRequestDTO {
    [Required(ErrorMessage = "Le nom de la pièce est requis !")]
    [MaxLength(100, ErrorMessage = "Le nom de la pièce ne doit pas dépasser 100 caractères !")]
    public string Name { get; set; } = string.Empty;
}

/**
 * Room as returned to the client.
 */
public sealed class RoomResponseDTO {
    public Guid Id { get; init; }
    public Guid HouseholdId { get; init; }
    public string Name { get; init; } = string.Empty;
}
