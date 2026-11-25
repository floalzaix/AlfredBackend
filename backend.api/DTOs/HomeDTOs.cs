namespace Backend.API.DTOs;

public sealed record HomeDto(Guid Id, string Name);

public sealed record HomeMemberDto(string Username, string Role);
