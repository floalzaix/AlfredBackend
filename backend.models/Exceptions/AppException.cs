/**
 * Base exception for expected application errors.
 *
 * Carries an HTTP status and a short title so the API
 * layer can map them to ProblemDetails without treating
 * them as unexpected 500 failures.
 */

//
//  Imports
//

namespace Backend.Models.Exceptions;

//
//  AppException
//  

public class AppException : Exception {
    public int StatusCode { get; }

    public string Title { get; }

    public AppException(
        string message,
        int statusCode,
        string title
    ) : base(message) {
        StatusCode = statusCode;
        Title = title;
    }
}

//
//  UnauthorizedException
//

public class UnauthorizedException : AppException {
    public UnauthorizedException(
        string message = "Non autorisé."
    ) : base(message, 401, "Non autorisé") {
    }
}

//
//  NotFoundException
//

public class NotFoundException : AppException {
    public NotFoundException(
        string message = "Ressource introuvable."
    ) : base(message, 404, "Introuvable") {
    }
}

//
//  ConflictException
//

public class ConflictException : AppException {
    public ConflictException(
        string message = "Conflit."
    ) : base(message, 409, "Conflit") {
    }
}

//
//  BadRequestException
//

public class BadRequestException : AppException {
    public BadRequestException(string message = "La requête est invalide.")
        : base(message, 400, "Requête invalide") {
    }
}
