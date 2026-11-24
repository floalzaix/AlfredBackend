namespace Backend.Models.Exceptions;

public class ForbiddenException(string message) : AppException(message, 403, "Accès interdit");
